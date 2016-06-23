namespace RigoFunc.IdentityServer.Api {
    /// <summary>
    /// Represents all the options can be used to configure account Api.
    /// </summary>
    public class AccountApiOptions {
        /// <summary>
        /// Gets or sets a value indicating whether allows any origin.
        /// </summary>
        /// <value>
        ///   <c>true</c> if allows any origin; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAnyOrigin { get; set; }
        /// <summary>
        /// Gets or sets the allowed origins.
        /// </summary>
        /// <value>The allowed origins.</value>
        public string[] AllowedOrigins { get; set; }
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
        /// <summary>
        /// Gets or sets the send code Sms template.
        /// </summary>
        /// <value>The send code Sms template.</value>
        public string SendCodeTemplate { get; set; }
        /// <summary>
        /// Gets or sets the send password Sms template.
        /// </summary>
        /// <value>The send password Sms template.</value>
        public string SendPasswordTemplate { get; set; }
    }
}
