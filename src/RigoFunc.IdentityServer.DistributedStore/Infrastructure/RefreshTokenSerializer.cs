using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace RigoFunc.IdentityServer.DistributedStore
{
    public class RefreshTokenSerializer : IDataSerializer<RefreshToken> {
        private const string DefaultStringPlaceholder = "\0";
        private readonly IClientStore _clientStore;

        public RefreshTokenSerializer(IClientStore clientStore) {
            _clientStore = clientStore;
        }

        public virtual RefreshToken Deserialize(byte[] data) {
            using (var memory = new MemoryStream(data)) {
                using (var reader = new BinaryReader(memory)) {
                    return Read(reader);
                }
            }
        }

        public virtual byte[] Serialize(RefreshToken refreshToken) {
            using (var memory = new MemoryStream()) {
                using (var writer = new BinaryWriter(memory)) {
                    Write(writer, refreshToken);
                }
                return memory.ToArray();
            }
        }

        public virtual void Write(BinaryWriter writer, RefreshToken refreshToken) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (refreshToken == null) {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            writer.Write(refreshToken.Version);
            writer.Write(refreshToken.LifeTime);
            writer.Write(refreshToken.CreationTime.ToUnixTimeMilliseconds());

            // write token
            WriteToken(writer, refreshToken.AccessToken);

            // Write the number of identities contained in the principal.
            var principal = refreshToken.Subject;
            writer.Write(principal.Identities.Count());

            foreach (var identity in principal.Identities) {
                WriteIdentity(writer, identity);
            }
        }

        protected virtual void WriteToken(BinaryWriter writer, Token token) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (token == null) {
                throw new ArgumentNullException(nameof(token));
            }

            writer.Write(token.Version);
            writer.Write(token.Lifetime);
            writer.Write(token.Type);
            writer.Write(token.Issuer);
            writer.Write(token.Audience);
            writer.Write(token.CreationTime.ToUnixTimeMilliseconds());
            writer.Write(token.ClientId);

            writer.Write(token.Claims.Count);
            foreach (var claim in token.Claims) {
                WriteClaim(writer, claim);
            }
        }

        protected virtual void WriteIdentity(BinaryWriter writer, ClaimsIdentity identity) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (identity == null) {
                throw new ArgumentNullException(nameof(identity));
            }

            var authenticationType = identity.AuthenticationType ?? string.Empty;

            writer.Write(authenticationType);
            WriteWithDefault(writer, identity.NameClaimType, ClaimsIdentity.DefaultNameClaimType);
            WriteWithDefault(writer, identity.RoleClaimType, ClaimsIdentity.DefaultRoleClaimType);

            // Write the number of claims contained in the identity.
            writer.Write(identity.Claims.Count());

            foreach (var claim in identity.Claims) {
                WriteClaim(writer, claim);
            }

            var bootstrap = identity.BootstrapContext as string;
            if (!string.IsNullOrEmpty(bootstrap)) {
                writer.Write(true);
                writer.Write(bootstrap);
            }
            else {
                writer.Write(false);
            }

            if (identity.Actor != null) {
                writer.Write(true);
                WriteIdentity(writer, identity.Actor);
            }
            else {
                writer.Write(false);
            }
        }

        protected virtual void WriteClaim(BinaryWriter writer, Claim claim) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (claim == null) {
                throw new ArgumentNullException(nameof(claim));
            }

            WriteWithDefault(writer, claim.Type, claim.Subject?.NameClaimType ?? ClaimsIdentity.DefaultNameClaimType);
            writer.Write(claim.Value);
            WriteWithDefault(writer, claim.ValueType, ClaimValueTypes.String);
            WriteWithDefault(writer, claim.Issuer, ClaimsIdentity.DefaultIssuer);
            WriteWithDefault(writer, claim.OriginalIssuer, claim.Issuer);

            // Write the number of properties contained in the claim.
            writer.Write(claim.Properties.Count);

            foreach (var property in claim.Properties) {
                writer.Write(property.Key ?? string.Empty);
                writer.Write(property.Value ?? string.Empty);
            }
        }

        public virtual RefreshToken Read(BinaryReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            var refreshToken = new RefreshToken {
                Version = reader.ReadInt32(),
                LifeTime = reader.ReadInt32(),
                CreationTime = DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadInt64()),
            };

            refreshToken.AccessToken = ReadToken(reader);

            // Read the number of identities stored
            // in the serialized payload.
            var count = reader.ReadInt32();
            if (count < 0) {
                return null;
            }

            var identities = new ClaimsIdentity[count];
            for (var index = 0; index != count; ++index) {
                identities[index] = ReadIdentity(reader);
            }

            refreshToken.Subject = new ClaimsPrincipal(identities);

            return refreshToken;
        }

        protected virtual Token ReadToken(BinaryReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            var token = new Token {
                Version = reader.ReadInt32(),
                Lifetime = reader.ReadInt32(),
                Type = reader.ReadString(),
                Issuer = reader.ReadString(),
                Audience = reader.ReadString(),
                CreationTime = DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadInt64()),
            };

            // get the client
            var clientId = reader.ReadString();
            token.Client = _clientStore.FindClientByIdAsync(clientId).Result;

            // Read the number of claims contained
            var count = reader.ReadInt32();
            if (count > 0) {
                token.Claims = new List<Claim>(count);

                for (int index = 0; index != count; ++index) {
                    var claim = ReadClaim(reader);

                    token.Claims.Add(claim);
                }
            }

            return token;
        }

        protected virtual ClaimsIdentity ReadIdentity(BinaryReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            var authenticationType = reader.ReadString();
            var nameClaimType = ReadWithDefault(reader, ClaimsIdentity.DefaultNameClaimType);
            var roleClaimType = ReadWithDefault(reader, ClaimsIdentity.DefaultRoleClaimType);

            // Read the number of claims contained
            // in the serialized identity.
            var count = reader.ReadInt32();

            var identity = new ClaimsIdentity(authenticationType, nameClaimType, roleClaimType);

            for (int index = 0; index != count; ++index) {
                var claim = ReadClaim(reader, identity);

                identity.AddClaim(claim);
            }

            // Determine whether the identity
            // has a bootstrap context attached.
            if (reader.ReadBoolean()) {
                identity.BootstrapContext = reader.ReadString();
            }

            // Determine whether the identity
            // has an actor identity attached.
            if (reader.ReadBoolean()) {
                identity.Actor = ReadIdentity(reader);
            }

            return identity;
        }

        protected virtual Claim ReadClaim(BinaryReader reader, ClaimsIdentity identity = null) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            var type = ReadWithDefault(reader, identity?.NameClaimType ?? ClaimsIdentity.DefaultNameClaimType);
            var value = reader.ReadString();
            var valueType = ReadWithDefault(reader, ClaimValueTypes.String);
            var issuer = ReadWithDefault(reader, ClaimsIdentity.DefaultIssuer);
            var originalIssuer = ReadWithDefault(reader, issuer);

            Claim claim = null;
            if(identity == null) {
                claim = new Claim(type, value, valueType, issuer, originalIssuer);
            }
            else {
                claim = new Claim(type, value, valueType, issuer, originalIssuer, identity);
            }

            // Read the number of properties stored in the claim.
            var count = reader.ReadInt32();

            for (var index = 0; index != count; ++index) {
                var key = reader.ReadString();
                var propertyValue = reader.ReadString();

                claim.Properties.Add(key, propertyValue);
            }

            return claim;
        }

        private static void WriteWithDefault(BinaryWriter writer, string value, string defaultValue) {
            if (string.Equals(value, defaultValue, StringComparison.Ordinal)) {
                writer.Write(DefaultStringPlaceholder);
            }
            else {
                writer.Write(value);
            }
        }

        private static string ReadWithDefault(BinaryReader reader, string defaultValue) {
            var value = reader.ReadString();
            if (string.Equals(value, DefaultStringPlaceholder, StringComparison.Ordinal)) {
                return defaultValue;
            }
            return value;
        }
    }
}
