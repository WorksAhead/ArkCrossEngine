using System;

namespace ArkCrossEngine
{
    internal sealed class AiViewManager
    {
        internal void Init()
        {
            AbstractUserStateLogic.OnUserStartAttack += this.OnUserStartAttack;
            AbstractUserStateLogic.OnUserSkill += this.OnUserSkill;
            AbstractUserStateLogic.OnUserStopSkill += this.OnUserStopSkill;
            AbstractUserStateLogic.OnUserSendStoryMessage += this.OnUserSendStoryMessage;
            AbstractNpcStateLogic.OnNpcSkill += this.NpcSkill;
            AbstractNpcStateLogic.OnNpcStopSkill += this.NpcStopSkill;
            AbstractNpcStateLogic.OnNpcAddImpact += this.OnNpcAddImpact;
            AbstractNpcStateLogic.OnNpcFaceClient += this.OnNpcFace;
            AbstractNpcStateLogic.OnNpcMeetEnemy += this.OnNpcMeetEnemy;
            AbstractNpcStateLogic.OnNpcSendStoryMessage += this.OnNpcSendStoryMessage;
            AiLogic_DropOut_AutoPick.OnDropoutPlayEffect += this.OnDropoutPlayEffect;
        }

        private void OnUserStartAttack(UserInfo user, float x, float y, float z)
        {
            GfxSystem.PublishGfxEvent("Ai_InputAttackCmd", "Input", user.GetId(), x, y, z);
        }

        private void OnUserSkill(UserInfo user, int skillId)
        {
            //user.SkillController.PushSkill(SkillCategory.kAttack, ScriptRuntime.Vector3.Zero);
            GfxSystem.PublishGfxEvent("Ai_InputSkillCmd", "Input", user.GetId());
        }

        private void OnUserStopSkill(UserInfo user)
        {
        }

        private void OnUserSendStoryMessage(UserInfo user, string msgId, object[] args)
        {
            if (WorldSystem.Instance.IsPveScene() || WorldSystem.Instance.IsPureClientScene())
            {
                ClientStorySystem.Instance.SendMessage(msgId, args);
            }
        }

        private void OnNpcAddImpact(NpcInfo npc, int impactId)
        {
            ImpactSystem.Instance.SendImpactToCharacter(npc, impactId, npc.GetId(), -1, -1, npc.GetMovementStateInfo().GetPosition3D(), npc.GetMovementStateInfo().GetFaceDir());
        }
        private void OnNpcFace(NpcInfo npc, float faceDirection)
        {
            npc.GetMovementStateInfo().SetWantFaceDir(faceDirection);
            ControlSystemOperation.AdjustCharacterFaceDir(npc.GetId(), faceDirection);
        }
        private void NpcSkill(NpcInfo npc, int skillId)
        {
            if (null != npc)
            {
                if (npc.SkillController != null)
                {
                    SkillInfo skillInfo = npc.GetSkillStateInfo().GetSkillInfoById(skillId);
                    if (null != skillInfo)
                    {
                        long curTime = TimeUtility.GetServerMilliseconds();
                        if (!skillInfo.IsInCd(curTime / 1000.0f))
                        {
                            npc.SkillController.ForceStartSkill(skillId);
                            skillInfo.BeginCD();
                        }
                    }
                }
            }
        }
        private void NpcStopSkill(NpcInfo npc)
        {
            if (null != npc)
            {
                if (npc.SkillController != null)
                {
                    npc.SkillController.ForceInterruptCurSkill();
                }
            }
        }

        private void OnNpcMeetEnemy(NpcInfo npc, Animation_Type animType)
        {
            CharacterView view = EntityManager.Instance.GetCharacterViewById(npc.GetId());
            if (null != view)
            {
                GfxSystem.SendMessage(view.Actor, "OnEventMeetEnemy", null);
            }
            ImpactSystem.Instance.SendImpactToCharacter(npc, npc.GetMeetEnemyImpact(), npc.GetId(), -1, -1, npc.GetMovementStateInfo().GetPosition3D(), npc.GetMovementStateInfo().GetFaceDir());
        }

        private void OnNpcSendStoryMessage(NpcInfo npc, string msgId, object[] args)
        {
            if (WorldSystem.Instance.IsPveScene() || WorldSystem.Instance.IsPureClientScene())
            {
                ClientStorySystem.Instance.SendMessage(msgId, args);
            }
        }

        private void OnDropoutPlayEffect(UserInfo user, NpcInfo npc, string effect, string path, int dropType, int dropNum)
        {
            CharacterView view = EntityManager.Instance.GetCharacterViewById(user.GetId());
            if (null != view)
            {
                if (!String.IsNullOrEmpty(effect))
                {
                    GfxSystem.CreateAndAttachGameObject(effect, view.Actor, path, 1.0f);
                }
                GfxSystem.PlaySound(view.Actor, "Sound/Scene/UI_UP_PickupMoney", user.GetMovementStateInfo().GetFaceDir());
            }
            Network.NetworkSystem.Instance.SyncPickUpNpc(npc);
        }

        private AiViewManager() { }

        internal static AiViewManager Instance
        {
            get
            {
                return s_Instance;
            }
        }
        private static AiViewManager s_Instance = new AiViewManager();
    }
}
