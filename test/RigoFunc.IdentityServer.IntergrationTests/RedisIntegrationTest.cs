using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services.InMemory;
using Microsoft.VisualBasic;
using RigoFunc.IdentityServer.IntergrationTests.Common;
using RigoFunc.IdentityServer.IntergrationTests.Comparers;
using RigoFunc.IdentityServer.Services.Redis;
using Xunit;
using ClaimComparer = IdentityModel.ClaimComparer;

namespace RigoFunc.IdentityServer.IntergrationTests {
    /// <summary>
    /// Integration test for redis stores
    /// </summary>
    /// <remarks>You will need to have a redis server to test with.</remarks>
    public class RedisIntegrationTest {
        private const string RedisServer = "localhost";

        [Fact]
        public void AuthorizationCodePersists() {
            var subClaim = new Claim("sub", "kyle@tester.com");
            var emailClaim = new Claim("email", "kyle@tester.com");
            var code = new AuthorizationCode {
                Client = new Client {
                    ClientId = "cid"
                },
                RequestedScopes = new List<Scope> { new Scope { Description = "this is description", Enabled = true, Name = "sname", DisplayName = "This is Name!" } },
                Subject = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { subClaim, emailClaim }))
            };

            var clients = new List<Client>
            {
                new Client
                {
                    ClientId = "cid",
                    ClientName = "cname",
                    Enabled = true,
                    SlidingRefreshTokenLifetime = 100,
                    AccessTokenType = AccessTokenType.Jwt,
                    //todo 
                    //Flow = Flows.Implicit
                }
            };
            var clientStore = new InMemoryClientStore(clients);

            var scopes = new List<Scope>
            {
                new Scope
                {
                    Description = "sdescription",
                    Name = "sname",
                    Enabled = true,
                    Emphasize = false,
                    IncludeAllClaimsForUser = true,
                    Required = false,
                    Type = ScopeType.Identity
                }
            };
            var scopeStore = new InMemoryScopeStore(scopes);

            var store = new RedisAuthorizationCodeStore(clientStore, scopeStore, RedisServer);
            store.StoreAsync("key1", code).Wait();

            var result = store.GetAsync("key1").Result;
            Assert.Equal(code.SubjectId, result.SubjectId);
            Assert.Equal(code.ClientId, result.ClientId);
        }

        [Fact]
        public void RefreshTokenPersists() {
            var subClaim = new Claim("sub", "kyle@tester.com");
            var emailClaim = new Claim("email", "kyle@tester.com");

            var token = new RefreshToken {
                AccessToken = new Token {

                    CreationTime = DateTimeOffset.Now,
                    Audience = "aud",
                    Claims = new List<Claim> { subClaim, emailClaim },
                    Client = new Client {
                        ClientId = "cid",
                        ClientName = "cname",
                        Enabled = true,
                        SlidingRefreshTokenLifetime = 100,
                        AccessTokenType = AccessTokenType.Jwt,
                        //todo 
                        //Flow = Flows.Implicit
                    },
                    Issuer = "iss",
                    Lifetime = 1234567,
                    Type = OidcConstants.TokenTypes.RefreshToken,
                    Version = 1,
                },

                CreationTime = DateTimeOffset.Now,
                Version = 1,
                LifeTime = 1234567,
                Subject = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { subClaim, emailClaim }))
            };

            var clients = new List<Client>
            {
                new Client
                {
                    ClientId = "cid",
                    ClientName = "cname",
                    Enabled = true,
                    SlidingRefreshTokenLifetime = 100,
                    AccessTokenType = AccessTokenType.Jwt,
                    //todo
                    //Flow = Flows.Implicit
                }
            };
            var clientStore = new InMemoryClientStore(clients);

            var scopes = new List<Scope>
            {
                new Scope
                {
                    Description = "sdescription",
                    Name = "sname",
                    Enabled = true,
                    Emphasize = false,
                    IncludeAllClaimsForUser = true,
                    Required = false,
                    Type = ScopeType.Identity
                }
            };
            var scopeStore = new InMemoryScopeStore(scopes);

            var store = new RedisRefreshTokenStore(clientStore, scopeStore, RedisServer);
            store.StoreAsync("key2", token).Wait();

            var result = store.GetAsync("key2").Result;
            Assert.Equal(token.SubjectId, result.SubjectId);
            Assert.Equal(token.ClientId, result.ClientId);
        }

        [Fact]
        public void ConsentPersists() {
            var subClaim = new Claim("sub", "kyle@tester.com");
            var emailClaim = new Claim("email", "kyle@tester.com");

            var token = new RefreshToken {
                AccessToken = new Token {

                    CreationTime = DateTimeOffset.Now,
                    Audience = "aud",
                    Claims = new List<Claim> { subClaim, emailClaim },
                    Client = new Client {
                        ClientId = "cid",
                        ClientName = "cname",
                        Enabled = true,
                        SlidingRefreshTokenLifetime = 100,
                        AccessTokenType = AccessTokenType.Jwt,
                        //todo 
                        //Flow = Flows.Implicit
                    },
                    Issuer = "iss",
                    Lifetime = 1234567,
                    Type = OidcConstants.TokenTypes.RefreshToken,
                    Version = 1,
                },

                CreationTime = DateTimeOffset.Now,
                Version = 1,
                LifeTime = 1234567,
                Subject = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { subClaim, emailClaim }))
            };

            var clients = new List<Client>
            {
                new Client
                {
                    ClientId = "cid",
                    ClientName = "cname",
                    Enabled = true,
                    SlidingRefreshTokenLifetime = 100,
                    AccessTokenType = AccessTokenType.Jwt,
                    //todo
                    //Flow = Flows.Implicit
                }
            };
            var clientStore = new InMemoryClientStore(clients);

            var scopes = new List<Scope>
            {
                new Scope
                {
                    Description = "sdescription",
                    Name = "sname",
                    Enabled = true,
                    Emphasize = false,
                    IncludeAllClaimsForUser = true,
                    Required = false,
                    Type = ScopeType.Identity
                }
            };
            var scopeStore = new InMemoryScopeStore(scopes);
            var consent = new Consent() {
                Subject = "test",
                ClientId = "sldfkjs",
                Scopes = new List<string>()
                {
                    "test1",
                    "scope2",
                }
            };
            var store = new RedisConsentStore(clientStore, scopeStore, RedisServer);
            store.UpdateAsync(consent).Wait();

            var result = store.LoadAsync(consent.Subject, consent.ClientId).Result;
            Assert.Equal(consent.Subject, result.Subject);
            Assert.Equal(consent.ClientId, result.ClientId);
            Assert.Equal(consent.Scopes, result.Scopes);

        }

        [Fact]
        public void TokenPersists() {
            var subClaim = new Claim("sub", "kyle@tester.com");
            var emailClaim = new Claim("email", "kyle@tester.com");

            var token = new RefreshToken {
                AccessToken = new Token {

                    CreationTime = DateTimeOffset.Now,
                    Audience = "aud",
                    Claims = new List<Claim> { subClaim, emailClaim },
                    Client = new Client {
                        ClientId = "cid",
                        ClientName = "cname",
                        Enabled = true,
                        SlidingRefreshTokenLifetime = 100,
                        AccessTokenType = AccessTokenType.Jwt,
                        //todo 
                        //Flow = Flows.Implicit
                    },
                    Issuer = "iss",
                    Lifetime = 1234567,
                    Type = OidcConstants.TokenTypes.RefreshToken,
                    Version = 1,
                },

                CreationTime = DateTimeOffset.Now,
                Version = 1,
                LifeTime = 1234567,
                Subject = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { subClaim, emailClaim }))
            };

            var clients = new List<Client>
            {
                new Client
                {
                    ClientId = "cid",
                    ClientName = "cname",
                    Enabled = true,
                    SlidingRefreshTokenLifetime = 100,
                    AccessTokenType = AccessTokenType.Jwt,
                    //todo
                    //Flow = Flows.Implicit
                }
            };
            var clientStore = new InMemoryClientStore(clients);

            var scopes = new List<Scope>
            {
                new Scope
                {
                    Description = "sdescription",
                    Name = "sname",
                    Enabled = true,
                    Emphasize = false,
                    IncludeAllClaimsForUser = true,
                    Required = false,
                    Type = ScopeType.Identity
                }
            };
            var scopeStore = new InMemoryScopeStore(scopes);
            var store = new RedisTokenHandleStore(clientStore, scopeStore, RedisServer);
            store.StoreAsync("key2", token.AccessToken).Wait();

            var result = store.GetAsync("key2").Result;
            Assert.Equal(token.AccessToken.ClientId, result.ClientId);
            Assert.Equal(token.AccessToken.Audience, result.Audience);
            Assert.Equal(token.AccessToken.Claims.ToJson(), result.Claims.ToJson());
            Assert.Equal(token.AccessToken.Client.ToJson(), result.Client.ToJson());
            Assert.Equal(token.AccessToken.CreationTime, result.CreationTime);
            Assert.Equal(token.AccessToken.Issuer, result.Issuer);
            Assert.Equal(token.AccessToken.Lifetime, result.Lifetime);
            Assert.Equal(token.AccessToken.Scopes, result.Scopes);
            Assert.Equal(token.AccessToken.SubjectId, result.SubjectId);
            Assert.Equal(token.AccessToken.Type, result.Type);
            Assert.Equal(token.AccessToken.Version, result.Version);
        }
    }

}
