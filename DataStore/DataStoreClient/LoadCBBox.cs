using DashFire.DataStore;
using Google.ProtocolBuffers;

internal interface LoadCBBox
{
    void Invoke ( DSLoadResult ret, string error, IMessage data );
}

internal class LoadCBBoxI<T> : LoadCBBox
  where T : IMessage
{
    public LoadCBBoxI ( DataStoreClient.LoadCallback<T> cb )
    {
        cb_ = cb;
    }

    public void Invoke ( DSLoadResult ret, string error, IMessage data )
    {
        cb_(ret, error, (T)data);
    }

    private DataStoreClient.LoadCallback<T> cb_;
}