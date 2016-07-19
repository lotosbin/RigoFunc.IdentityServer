using System.Collections.Generic;
using IdentityServer4.Models;
using Newtonsoft.Json;

namespace RigoFunc.IdentityServer.Services.EntityFrameworkCore {
    public class ScopeEntity : Scope {
        private ScopeEntity() {
        }

        public ScopeEntity(string name) {
            this.Name = name;
        }
        public string ClaimsValue {
            get { return JsonConvert.SerializeObject(this.Claims); }
            set { this.Claims = string.IsNullOrEmpty(value) ? new List<ScopeClaim>() : JsonConvert.DeserializeObject<List<ScopeClaim>>(value); }
        }

        public string ScopeSecretsValue {
            get { return JsonConvert.SerializeObject(this.ScopeSecrets); }
            set { this.ScopeSecrets = string.IsNullOrEmpty(value) ? new List<Secret>() : JsonConvert.DeserializeObject<List<Secret>>(value); }
        }
    }
}