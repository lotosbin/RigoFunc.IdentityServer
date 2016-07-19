using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RigoFunc.IdentityServer.IntergrationTests.Common {
    public static class JsonExtension {
        public static string ToJson(this object obj) {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
