using System;
using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class SceneLogicEventArgs : EventArgs
    {
        public int Index = 0;
        public int EntityId = -1;
        public int SkillId = -1;
        public List<int> CharacterList = new List<int>();
        public int TargetId = -1;
        public double DeltaTime;
        public Vector3 TargetPos;
    }
}
