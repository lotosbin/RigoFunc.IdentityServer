namespace RigoFunc.IdentityServer.Api {
    internal static class ApiConstants {
        public static readonly string[] RoutePaths = new string[] {
            // can this by reflection rather than hard code?
            "api/account/register",
            "api/account/sendcode",
            "api/account/login",
            "api/account/verifycode",
            "api/account/changepassword",
            "api/account/resetpassword",
            "api/account/update",
        };

        public const string ClientId = "clientId";
        public const string ClientSecret = "clientSecret";
        public const string Scope = "scope";
    }
}
