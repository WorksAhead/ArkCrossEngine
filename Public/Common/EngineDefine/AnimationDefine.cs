namespace ArkCrossEngine
{
    public enum PlayMode
    {
        StopSameLayer = 0,
        StopAll = 4
    }

    public enum QueueMode
    {
        CompleteOthers = 0,
        PlayNow = 2
    }

    public enum AnimationBlendMode
    {
        Blend = 0,
        Additive = 1
    }

    public enum WrapMode
    {
        Default = 0,
        Once = 1,
        Clamp = 1,
        Loop = 2,
        PingPong = 4,
        ClampForever = 8
    }
}
