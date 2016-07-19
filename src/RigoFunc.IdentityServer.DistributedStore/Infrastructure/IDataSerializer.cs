namespace RigoFunc.IdentityServer.DistributedStore {
    public interface IDataSerializer<TModel> {
        byte[] Serialize(TModel model);
        TModel Deserialize(byte[] data);
    }
}
