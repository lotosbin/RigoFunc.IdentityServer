namespace RigoFunc.IdentityServer.Services.Redis.Serialization
{
    internal class ClaimsPrincipalLite
    {
        public string AuthenticationType { get; set; }
        public ClaimLite[] Claims { get; set; }
    }
}
