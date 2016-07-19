using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace RigoFunc.IdentityServer.DistributedStore {
    public class AuthorizationCodeSerializer : IDataSerializer<AuthorizationCode> {
        private const string DefaultStringPlaceholder = "\0";
        private readonly IClientStore _clientStore;
        private readonly IScopeStore _scopeStore;

        public AuthorizationCodeSerializer(IClientStore clientStore, IScopeStore scopeStore) {
            _clientStore = clientStore;
            _scopeStore = scopeStore;
        }

        public AuthorizationCode Deserialize(byte[] data) {
            using (var memory = new MemoryStream(data)) {
                using (var reader = new BinaryReader(memory)) {
                    return Read(reader);
                }
            }
        }

        public byte[] Serialize(AuthorizationCode code) {
            using (var memory = new MemoryStream()) {
                using (var writer = new BinaryWriter(memory)) {
                    Write(writer, code);
                }
                return memory.ToArray();
            }
        }

        public virtual void Write(BinaryWriter writer, AuthorizationCode code) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (code == null) {
                throw new ArgumentNullException(nameof(code));
            }

            writer.Write(code.IsOpenId);
            writer.Write(code.RedirectUri);
            writer.Write(code.Nonce);
            writer.Write(code.WasConsentShown);
            writer.Write(code.SessionId);
            writer.Write(code.CreationTime.ToUnixTimeMilliseconds());
            writer.Write(code.ClientId);

            // Write the number of identities contained in the principal.
            var principal = code.Subject;
            writer.Write(principal.Identities.Count());

            foreach (var identity in principal.Identities) {
                WriteIdentity(writer, identity);
            }

            // Write the number of scopes contained in code
            writer.Write(code.RequestedScopes.Count());
            foreach (var scope in code.RequestedScopes) {
                writer.Write(scope.Name);
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

        public virtual AuthorizationCode Read(BinaryReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            var code = new AuthorizationCode {
                IsOpenId = reader.ReadBoolean(),
                RedirectUri = reader.ReadString(),
                Nonce = reader.ReadString(),
                WasConsentShown = reader.ReadBoolean(),
                SessionId = reader.ReadString(),
                CreationTime = DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadInt64()),
            };

            // get the client
            var clientId = reader.ReadString();
            code.Client = _clientStore.FindClientByIdAsync(clientId).Result;

            // Read the number of identities stored
            // in the serialized payload.
            var count = reader.ReadInt32();
            if (count > 0) {
                var identities = new ClaimsIdentity[count];
                for (var index = 0; index != count; ++index) {
                    identities[index] = ReadIdentity(reader);
                }

                code.Subject = new ClaimsPrincipal(identities);
            }

            var scopeCount = reader.ReadInt32();
            if(scopeCount > 0) {
                var scopeNames = new List<string>(scopeCount);
                for (var index = 0; index != scopeCount; ++index) {
                    scopeNames.Add(reader.ReadString());
                }

                code.RequestedScopes = _scopeStore.FindScopesAsync(scopeNames).Result;
            }

            return code;
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

        protected virtual Claim ReadClaim(BinaryReader reader, ClaimsIdentity identity) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (identity == null) {
                throw new ArgumentNullException(nameof(identity));
            }

            var type = ReadWithDefault(reader, identity.NameClaimType);
            var value = reader.ReadString();
            var valueType = ReadWithDefault(reader, ClaimValueTypes.String);
            var issuer = ReadWithDefault(reader, ClaimsIdentity.DefaultIssuer);
            var originalIssuer = ReadWithDefault(reader, issuer);

            var claim = new Claim(type, value, valueType, issuer, originalIssuer, identity);

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
