using System;
using DashFire;
using ArkCrossEngine;

namespace Lobby
{
    internal class SceneInfo
    {
        internal SceneInfo()
        {
            this.SceneID = 0;
            this.Type = SceneTypeEnum.TYPE_PVE;
            this.SubType = SceneSubTypeEnum.TYPE_STORY;
            this.IconID = 0;
            this.Title = "Scene Title";
            this.Decription = "GameLevel !";
        }
        internal int SceneID
        {
            get;
            set;
        }
        internal ArkCrossEngine.SceneTypeEnum Type
        {
            get;
            set;
        }
        internal ArkCrossEngine.SceneSubTypeEnum SubType
        {
            get;
            set;
        }
        internal int IconID
        {
            get;
            set;
        }
        internal string Title
        {
            get;
            set;
        }
        internal string Decription
        {
            get;
            set;
        }
    }
}
