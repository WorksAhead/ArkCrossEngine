using System.Collections.Generic;

namespace ArkCrossEngine
{
    public class SceneLogic_UserEnterArea : AbstractSceneLogic
    {
        public override void Execute(SceneLogicInfo info, long deltaTime)
        {
            if (null == info || info.IsLogicFinished || info.IsLogicPaused) return;
            info.Time += deltaTime;
            if (info.Time > 100)
            {
                UserEnterAreaLogicInfo data = info.LogicDatas.GetData<UserEnterAreaLogicInfo>();
                if (null == data)
                {
                    data = new UserEnterAreaLogicInfo();
                    info.LogicDatas.AddData<UserEnterAreaLogicInfo>(data);

                    SceneLogicConfig sc = info.SceneLogicConfig;
                    if (null != sc)
                    {
                        List<float> pts = Converter.ConvertNumericList<float>(sc.m_Params[0]);
                        data.m_Area = new Vector3[pts.Count / 2];
                        for (int ix = 0; ix < pts.Count - 1; ix += 2)
                        {
                            data.m_Area[ix / 2].X = pts[ix];
                            data.m_Area[ix / 2].Z = pts[ix + 1];
                        }
                        data.m_TriggerType = (UserEnterAreaLogicInfo.TiggerTypeEnum)int.Parse(sc.m_Params[1]);
                    }
                }
                info.Time = 0;
                //执行逻辑
                if (null != data && !data.m_IsTriggered)
                {
                    if (data.m_TriggerType == UserEnterAreaLogicInfo.TiggerTypeEnum.All)
                    {
                        int ct = 0;
                        info.SpatialSystem.VisitObjectInPolygon(data.m_Area, (float distSqr, ArkCrossEngineSpatial.ISpaceObject obj) =>
                        {
                            if (obj.GetObjType() == ArkCrossEngineSpatial.SpatialObjType.kUser)
                            {
                                UserInfo user = obj.RealObject as UserInfo;
                                if (HasNoPartnerOrPartnerInArea(user, data.m_Area))
                                {
                                    ++ct;
                                }
                            }
                        });
                        if (ct == info.UserManager.Users.Count)
                        {
                            SceneLogicSendStoryMessage(info, "alluserenterarea:" + info.ConfigId, ct);
                            data.m_IsTriggered = true;
                        }
                    }
                    else
                    {
                        int id = 0;
                        info.SpatialSystem.VisitObjectInPolygon(data.m_Area, (float distSqr, ArkCrossEngineSpatial.ISpaceObject obj) =>
                        {
                            if (obj.GetObjType() == ArkCrossEngineSpatial.SpatialObjType.kUser)
                            {
                                UserInfo user = obj.RealObject as UserInfo;
                                if (HasNoPartnerOrPartnerInArea(user, data.m_Area))
                                {
                                    id = (int)obj.GetID();
                                    return false;
                                }
                            }
                            return true;
                        });
                        if (id > 0)
                        {
                            SceneLogicSendStoryMessage(info, "anyuserenterarea:" + info.ConfigId, id);
                            data.m_IsTriggered = true;
                        }
                    }
                }
            }
        }
        private bool HasNoPartnerOrPartnerInArea(UserInfo user, IList<Vector3> pts)
        {
            CharacterInfo partner = user.SceneContext.GetCharacterInfoById(user.PartnerId);
            if (null == partner)
            {
                return true;
            }
            else
            {
                NpcInfo npc = partner.CastNpcInfo();
                if (null == npc || npc.NpcType != (int)NpcTypeEnum.Partner)
                {
                    return true;
                }
                Vector3 point = partner.GetMovementStateInfo().GetPosition3D();
                if (Geometry.PointInPolygon(point, pts, 0, pts.Count) == 1)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
