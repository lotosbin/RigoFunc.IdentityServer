using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RigoFunc.IdentityServer.DistributedStore {
    public class ClaimsPrincipalSerializer : IDataSerializer<ClaimsPrincipal> {
        public ClaimsPrincipal Deserialize(byte[] data) {
            throw new NotImplementedException();
        }

        public byte[] Serialize(ClaimsPrincipal model) {
            throw new NotImplementedException();
        }
    }
}
