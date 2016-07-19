using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace RigoFunc.IdentityServer.DistributedStore {
    public class ConsentSerializer : IDataSerializer<Consent> {
        public static ConsentSerializer Default { get; } = new ConsentSerializer();

        public virtual byte[] Serialize(Consent consent) {
            using (var memory = new MemoryStream()) {
                using (var writer = new BinaryWriter(memory)) {
                    Write(writer, consent);
                }
                return memory.ToArray();
            }
        }

        public virtual Consent Deserialize(byte[] data) {
            using (var memory = new MemoryStream(data)) {
                using (var reader = new BinaryReader(memory)) {
                    return Read(reader);
                }
            }
        }

        public virtual void Write(BinaryWriter writer, Consent consent) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (consent == null) {
                throw new ArgumentNullException(nameof(consent));
            }

            writer.Write(consent.Subject);
            writer.Write(consent.ClientId);

            writer.Write(consent.Scopes.Count());
            foreach (var scope in consent.Scopes) {
                writer.Write(scope);
            }
        }

        public virtual Consent Read(BinaryReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            var consent = new Consent {
                Subject = reader.ReadString(),
                ClientId = reader.ReadString(),
            };

            // Read the number of scopes contained
            var count = reader.ReadInt32();
            if (count > 0) {
                var scopes = new List<string>(count);

                for (int index = 0; index != count; ++index) {
                    scopes.Add(reader.ReadString());
                }

                consent.Scopes = scopes;
            }

            return consent;
        }
    }
}
