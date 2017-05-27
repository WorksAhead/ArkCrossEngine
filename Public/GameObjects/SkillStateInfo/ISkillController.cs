using System.Collections.Generic;

namespace ArkCrossEngine
{
    public enum SkillCannotCastType
    {
        kUnknow,
        kNotFindSkill,
        kOwnerDead,
        kCannotCtrol,
        kInCD,
        kCostNotEnough,
    }

    public class SkillInputData
    {
        public float castRange;
        public float targetChooseRange;
    }

    public class SkillNode
    {
        public int SkillId;
        public SkillCategory Category;
        public SkillNode SkillQ = null;
        public SkillNode SkillE = null;
        public SkillNode NextSkillNode = null;
        public float StartTime;
        public Vector3 TargetPos;
        public bool IsLocked = false;
        public bool IsCDChecked = false;
        public bool IsAlreadyPressUp = false;
    }

    public delegate void SkillQECanInputHandler(float remaintime, List<SkillNode> skills);
    public delegate void SkillStartHandler();
    public interface ISkillController
    {
        void Init();
        void OnTick();
        void LearnSkill(int skillid);
        SkillNode GetHead(SkillCategory category);
        SkillNode InitCategorySkillNode(List<SkillInfo> skills, SkillInfo ss);
        SkillNode ChangeCategorySkill(SkillCategory category, SkillNode new_head);
        void SetSkillControlMode(SkillControlMode mode);
        SkillControlMode GetSkillControlMode();
        void AddBreakSkillTask();
        void CancelBreakSkillTask();
        void StartAttack(Vector3 targetpos);
        void StopAttack();
        void PushSkill(SkillCategory category, Vector3 targetpos);
        void BreakSkill(SkillCategory category);
        List<SkillNode> QuerySkillQE(SkillCategory category, int times);
        void AddBreakSection(int skillid, int breaktype, int starttime, int endtime, bool isinterrupt, string skillmessage);
        bool ForceStartSkill(int skillid);
        bool BreakCurSkill(int breaktype, SkillCategory category);
        void ForceInterruptCurSkill();
        void RemoveBreakSectionByType(int skillid, int breaktype);
    }
}