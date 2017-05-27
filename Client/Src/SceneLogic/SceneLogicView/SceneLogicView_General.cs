namespace ArkCrossEngine
{
    internal class SceneLogicView_General
    {
        internal SceneLogicView_General()
        {
            AbstractSceneLogic.OnSceneLogicSendStoryMessage += this.OnSceneLogicSendStoryMessage;
        }

        internal void OnSceneLogicSendStoryMessage(SceneLogicInfo info, string msgId, object[] args)
        {
            if (WorldSystem.Instance.IsPveScene() || WorldSystem.Instance.IsPureClientScene())
            {
                ClientStorySystem.Instance.SendMessage(msgId, args);
            }
        }
    }
}
