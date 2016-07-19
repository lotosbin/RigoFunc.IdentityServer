using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace RigoFunc.IdentityServer.DistributedStore {
    public class TokenSerializer : IDataSerializer<Token> {
        private const string DefaultStringPlaceholder = "\0";
        private readonly IClientStore _clientStore;

        public TokenSerializer(IClientStore clientStore) {
            _clientStore = clientStore;
        }

        public virtual byte[] Serialize(Token token) {
            using (var memory = new MemoryStream()) {
                using (var writer = new BinaryWriter(memory)) {
                    Write(writer, token);
                }
                return memory.ToArray();
            }
        }

        public virtual Token Deserialize(byte[] data) {
            using (var memory = new MemoryStream(data)) {
                using (var reader = new BinaryReader(memory)) {
                    return Read(reader);
                }
            }
        }

        public virtual void Write(BinaryWriter writer, Token token) {
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

        public virtual Token Read(BinaryReader reader) {
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

        protected virtual Claim ReadClaim(BinaryReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }
            var type = ReadWithDefault(reader, ClaimsIdentity.DefaultNameClaimType);
            var value = reader.ReadString();
            var valueType = ReadWithDefault(reader, ClaimValueTypes.String);
            var issuer = ReadWithDefault(reader, ClaimsIdentity.DefaultIssuer);
            var originalIssuer = ReadWithDefault(reader, issuer);

            var claim = new Claim(type, value, valueType, issuer, originalIssuer);

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
