namespace RigoFunc.IdentityServer.Api {
    /// <summary>
    /// Represents all the options can be used to configure account Api.
    /// </summary>
    public class AccountApiOptions {
        /// <summary>
        /// Gets or sets the default client id. This value will be used if the request doesn't contains clientId header.
        /// </summary>
        /// <value>The default client id.</value>
        public string DefaultClientId { get; set; }
        /// <summary>
        /// Gets or sets the default client secret. This value will be used if the request doesn't contains clientSecret header.
        /// </summary>
        /// <value>The default client secret.</value>
        public string DefaultClientSecret { get; set; }
        /// <summary>
        /// Gets or sets the default scope. This value will be used if the request doesn't contains the scope header.
        /// </summary>
        /// <value>The default scope.</value>
        public string DefaultScope { get; set; }
    }
}
