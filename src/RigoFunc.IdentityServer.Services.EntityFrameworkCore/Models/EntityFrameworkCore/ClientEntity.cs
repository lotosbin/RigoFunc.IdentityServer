using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4.Models;
using Newtonsoft.Json;

namespace RigoFunc.IdentityServer.Services.EntityFrameworkCore {
    public class ClientEntity : Client {
        private ClientEntity() {

        }

        public ClientEntity(string clientId) {
            this.ClientId = clientId;
        }
        public string ClaimsValue {
            get { return JsonConvert.SerializeObject(this.Claims); }
            set { this.Claims = string.IsNullOrEmpty(value) ? new List<Claim>() : JsonConvert.DeserializeObject<List<Claim>>(value); }
        }

        public string AllowedCorsOriginsValue {
            get { return JsonConvert.SerializeObject(this.AllowedCorsOrigins ?? new List<string>()); }
            set { this.AllowedCorsOrigins = string.IsNullOrEmpty(value) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(value); }
        }
        public string AllowedGrantTypesValue {
            get { return JsonConvert.SerializeObject(this.AllowedGrantTypes); }
            set { this.AllowedGrantTypes = string.IsNullOrEmpty(value) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(value); }
        }
        public string AllowedScopesValue {
            get { return JsonConvert.SerializeObject(this.AllowedScopes); }
            set { this.AllowedScopes = string.IsNullOrEmpty(value) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(value); }
        }
        public string ClientSecretsValue {
            get { return JsonConvert.SerializeObject(this.ClientSecrets); }
            set { this.ClientSecrets = string.IsNullOrEmpty(value) ? new List<Secret>() : JsonConvert.DeserializeObject<List<Secret>>(value); }
        }
        public string IdentityProviderRestrictionsValue {
            get { return JsonConvert.SerializeObject(this.IdentityProviderRestrictions); }
            set { this.IdentityProviderRestrictions = string.IsNullOrEmpty(value) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(value); }
        }
        public string PostLogoutRedirectUrisValue {
            get { return JsonConvert.SerializeObject(this.PostLogoutRedirectUris); }
            set { this.PostLogoutRedirectUris = string.IsNullOrEmpty(value) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(value); }
        }
        public string RedirectUrisValue {
            get { return JsonConvert.SerializeObject(this.RedirectUris); }
            set { this.RedirectUris = string.IsNullOrEmpty(value) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(value); }
        }
    }
}