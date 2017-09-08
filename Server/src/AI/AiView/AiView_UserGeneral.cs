using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArkCrossEngineMessage;
using ArkCrossEngine;

namespace DashFire
{
    internal class AiView_UserGeneral
    {
        internal AiView_UserGeneral()
        {
            AbstractUserStateLogic.OnUserMove += this.OnUserMove;
            AbstractUserStateLogic.OnUserFace += this.OnUserFace;
            AbstractUserStateLogic.OnUserSkill += this.OnUserSkill;
            AbstractUserStateLogic.OnUserStopSkill += this.OnUserStopSkill;
            AbstractUserStateLogic.OnUserPropertyChanged += this.OnUserPropertyChanged;
            AbstractUserStateLogic.OnUserSendStoryMessage += this.OnUserSendStoryMessage;
        }
        private void OnUserMove(UserInfo user)
        {
            Scene scene = user.SceneContext.CustomData as Scene;
            if (null != scene)
            {
                Msg_RC_UserMove userMoveBuilder = DataSyncUtility.BuildUserMoveMessage(user);
                if (null != userMoveBuilder)
                    scene.NotifyAreaUser(user, userMoveBuilder, false);
            }
        }
        private void OnUserFace(UserInfo user)
        {
            Scene scene = user.SceneContext.CustomData as Scene;
            if (null != scene)
            {
                Msg_RC_UserFace userFaceBuilder = DataSyncUtility.BuildUserFaceMessage(user);
                if (null != userFaceBuilder)
                    scene.NotifyAreaUser(user, userFaceBuilder, false);
            }
        }
        private void OnUserSkill(UserInfo user, int skillId)
        {
            Scene scene = user.SceneContext.CustomData as Scene;
            if (null != scene)
            {
                SkillInfo skillInfo = user.GetSkillStateInfo().GetCurSkillInfo();
                if (null == skillInfo || !skillInfo.IsSkillActivated)
                {
                    scene.SkillSystem.StartSkill(user.GetId(), skillId);

                    Msg_CRC_Skill skillBuilder = new Msg_CRC_Skill();
                    skillBuilder.role_id = user.GetId();
                    skillBuilder.skill_id = skillId;
                    ArkCrossEngineMessage.Position posBuilder1 = new ArkCrossEngineMessage.Position();
                    posBuilder1.x = user.GetMovementStateInfo().GetPosition3D().X;
                    posBuilder1.z = user.GetMovementStateInfo().GetPosition3D().Z;
                    skillBuilder.stand_pos = posBuilder1;
                    skillBuilder.face_direction = (float)user.GetMovementStateInfo().GetFaceDir();
                    skillBuilder.want_face_dir = (float)user.GetMovementStateInfo().GetFaceDir();

                    LogSystem.Debug("Send Msg_CRC_Skill, EntityId={0}, SkillId={1}",
                      user.GetId(), skillId);
                    scene.NotifyAreaUser(user, skillBuilder, false);
                }
            }
        }
        private void OnUserStopSkill(UserInfo user)
        {
            Scene scene = user.SceneContext.CustomData as Scene;
            if (null != scene)
            {
                SkillInfo skillInfo = user.GetSkillStateInfo().GetCurSkillInfo();
                if (null == skillInfo || skillInfo.IsSkillActivated)
                {
                    scene.SkillSystem.StopSkill(user.GetId());
                }

                Msg_CRC_StopSkill skillBuilder = new Msg_CRC_StopSkill();
                skillBuilder.role_id = user.GetId();

                LogSystem.Debug("Send Msg_CRC_StopSkill, EntityId={0}",
                  user.GetId());
                scene.NotifyAreaUser(user, skillBuilder, false);
            }
        }
        private void OnUserPropertyChanged(UserInfo user)
        {
            Msg_RC_SyncProperty propBuilder = DataSyncUtility.BuildSyncPropertyMessage(user);
            Scene scene = user.SceneContext.CustomData as Scene;
            if (null != scene)
            {
                scene.NotifyAllUser(propBuilder);
            }
        }
        private void OnUserSendStoryMessage(UserInfo user, string msgId, object[] args)
        {
            Scene scene = user.SceneContext.CustomData as Scene;
            if (null != scene)
            {
                scene.StorySystem.SendMessage(msgId, args);
            }
        }
    }
}
