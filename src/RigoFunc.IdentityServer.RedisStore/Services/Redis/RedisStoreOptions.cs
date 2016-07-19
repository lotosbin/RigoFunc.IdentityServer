namespace RigoFunc.IdentityServer.Services.Redis {
    public class RedisStoreOptions {
        public string Configuration { get; set; }
        public int Db { get; set; }

        public RedisStoreOptions(string config = "localhost", int db = 0) {
            Configuration = config;
            Db = db;
        }
    }
}