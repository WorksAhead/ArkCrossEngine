using System;
using Google.ProtocolBuffers;

namespace Messenger
{
    internal interface IPBHandler
    {
        void Execute(IMessage msg, PBChannel channel, int src, uint seq);
    }

    public sealed class PBHandler<MsgType> : IPBHandler
        where MsgType : IMessage
    {
        public delegate void F(MsgType msg, PBChannel channel, int src, uint seq);

        public PBHandler(F f)
        {
            f_ = f;
        }

        public void Execute(IMessage msg, PBChannel channel, int src, uint seq)
        {
            if (null != f_)
                f_((MsgType)msg, channel, src, seq);
        }

        private F f_;
    }
}