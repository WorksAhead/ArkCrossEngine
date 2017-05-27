namespace ArkCrossEngine
{
    public class ArenaItemInfo
    {
        public int ItemId;
        public int Level = 1;
        public int AppendProperty = 0;
        public bool IsUnlocked = true;
    }

    public class ArenaXSoulInfo
    {
        public int ItemId;
        public int Level = 1;
        public int Experience = 0;
        public int ModelLevel = -1;
    }

    public class ArenaPartnerInfo
    {
        public int id;
        public int AdditionLevel = 1;
        public int SkillStage = 1;
    }

    public class ArenaSkillInfo
    {
        public int Id;
        public int Level;
        public int EquipPos;
    }
}
