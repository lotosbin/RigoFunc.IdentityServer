dotnet restore
dotnet build ./src/Host
dotnet build ./src/RigoFunc.IdentityServer.RedisStore
dotnet test ./test/RigoFunc.IdentityServer.UnitTest
dotnet test ./test/RigoFunc.IdentityServer.IntergrationTests