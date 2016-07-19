namespace RigoFunc.IdentityServer.DistributedStore {
    public interface ISecureDataFormat<TData> {
        string Protect(TData data);
        string Protect(TData data, string purpose);
        TData Unprotect(string protectedText);
        TData Unprotect(string protectedText, string purpose);
    }
}
