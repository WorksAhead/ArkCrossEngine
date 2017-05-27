using System;

namespace ArkCrossEngine
{

    public class DropOutInfo
    {
        public DropOutType DropType;
        public string Model;
        public string Particle;
        public int Value;
        public bool IsBornOver = false;
        public Vector3 BornTarget = Vector3.Zero;
        public float BornAngle = 0;
    }
    public enum DropOutType
    {
        GOLD = 0,
        HP = 1,
        MP = 2,
        MULT_GOLD = 3,
    }

    public delegate void DropOutPlayEffectDelegation(UserInfo user, NpcInfo npc, string effect, string path, int dropType, int dropNum);
    public class AiLogic_DropOut_AutoPick : AbstractNpcStateLogic
    {

        public static DropOutPlayEffectDelegation OnDropoutPlayEffect;
        protected override void OnInitStateHandlers()
        {
            SetStateHandler((int)AiStateId.Idle, this.IdleHandler);
        }

        protected override void OnStateLogicInit(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
        {
            NpcAiStateInfo info = npc.GetAiStateInfo();
            info.Time = 0;
            npc.GetMovementStateInfo().IsMoving = false;
            info.HomePos = npc.GetMovementStateInfo().GetPosition3D();
            info.Target = 0;
        }

        private void IdleHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
        {
            NpcAiStateInfo info = npc.GetAiStateInfo();
            info.Time += deltaTime;
            if (info.Time > m_IntervalTime)
            {
                info.Time = 0;
                DropOutInfo dropInfo = npc.GetAiStateInfo().AiDatas.GetData<DropOutInfo>();
                if (null != dropInfo)
                {
                    if (npc.IsBorning)
                    {
                        Vector3 curPos = npc.GetMovementStateInfo().GetPosition3D();
                        if (dropInfo.BornTarget == Vector3.Zero)
                        {
                            double tempAngle = CrossEngineHelper.Random.NextFloat() * Math.PI;
                            float length = CrossEngineHelper.Random.NextFloat() * 3.0f;
                            dropInfo.BornTarget = curPos + new Vector3((float)Math.Cos(tempAngle), 0, (float)Math.Sin(tempAngle)) * length;
                            dropInfo.BornAngle = (float)tempAngle;
                        }
                        //npc.GetMovementStateInfo().SetMoveDir(dropInfo.BornAngle);
                        //npc.GetMovementStateInfo().IsMoving = true;
                        //NotifyNpcWalk(npc);
                        return;
                    }
                    else
                    {
                        if (!dropInfo.IsBornOver)
                        {
                            npc.GetMovementStateInfo().IsMoving = false;
                            NotifyNpcMove(npc);
                        }
                    }
                }
                UserInfo target = null;
                target = AiLogicUtility.GetNearstTargetHelper(npc, CharacterRelation.RELATION_FRIEND, AiTargetType.USER) as UserInfo;
                if (null != target && !npc.NeedDelete)
                {
                    Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
                    Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
                    float dis = Geometry.Distance(srcPos, targetPos);
                    float angle = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
                    if (dis <= m_AttractRange && dis > m_PickUpRange)
                    {
                        // 吸取
                        float move = npc.GetActualProperty().MoveSpeed * m_IntervalTime / 1000;
                        npc.GetMovementStateInfo().SetMoveDir(angle);
                        //npc.GetMovementStateInfo().PositionY = targetPos.Y + (1 - dis / m_AttractRange) * m_Height;
                        //npc.GetMovementStateInfo().SetPosition2D(srcPos.X + move * (float)Math.Cos(angle), srcPos.Z + move * (float)Math.Sin(angle));
                        npc.GetMovementStateInfo().IsMoving = true;
                        NotifyNpcRun(npc);
                    }
                    else if (dis <= m_PickUpRange)
                    {
                        npc.GetMovementStateInfo().IsMoving = false;
                        NotifyNpcMove(npc);
                        string path = "";
                        if (null != dropInfo)
                        {
                            switch (dropInfo.DropType)
                            {
                                case DropOutType.GOLD:
                                    target.Money += dropInfo.Value;
                                    target.UserManager.FireGainMoneyEvent(target.GetId(), dropInfo.Value);
                                    path = "ef_head";
                                    break;
                                case DropOutType.HP:
                                    int addHp = (int)(target.GetActualProperty().HpMax * dropInfo.Value / 100.0f);
                                    if (target.GetActualProperty().HpMax > addHp + target.Hp)
                                    {
                                        target.SetHp(Operate_Type.OT_Relative, addHp);
                                    }
                                    else
                                    {
                                        target.SetHp(Operate_Type.OT_Absolute, target.GetActualProperty().HpMax);
                                    }
                                    target.UserManager.FireDamageEvent(target.GetId(), -1, false, false, addHp, 0);
                                    path = "Bone_Root";
                                    break;
                                case DropOutType.MP:
                                    int addEnergy = (int)(target.GetActualProperty().EnergyMax * dropInfo.Value / 100.0f);
                                    if (target.GetActualProperty().EnergyMax > addEnergy + target.Energy)
                                    {
                                        target.SetEnergy(Operate_Type.OT_Relative, addEnergy);
                                    }
                                    else
                                    {
                                        target.SetEnergy(Operate_Type.OT_Absolute, target.GetActualProperty().EnergyMax);
                                    }
                                    target.UserManager.FireDamageEvent(target.GetId(), -1, false, false, 0, addEnergy);
                                    path = "Bone_Root";
                                    break;
                                case DropOutType.MULT_GOLD:
                                    target.Money += dropInfo.Value;
                                    target.UserManager.FireGainMoneyEvent(target.GetId(), dropInfo.Value);
                                    path = "ef_head";
                                    break;
                            }
                            if (null != OnDropoutPlayEffect)
                            {
                                OnDropoutPlayEffect(target, npc, dropInfo.Particle, path, (int)dropInfo.DropType, dropInfo.Value);
                            }
                        }
                        npc.NeedDelete = true;
                    }
                }
            }
        }

        private const long m_IntervalTime = 100; //检测间隔
        private const float m_PickUpRange = 1.0f; // 拾取半径
        private const float m_AttractRange = 6.0f; // 吸取半径
        private const float m_Height = 1.0f;       // 高度
    }
}
