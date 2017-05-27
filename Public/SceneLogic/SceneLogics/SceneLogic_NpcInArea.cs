using System.Collections.Generic;

namespace ArkCrossEngine
{
    class SceneLogic_NpcInArea : AbstractSceneLogic
    {
        public override void Execute(SceneLogicInfo info, long deltaTime)
        {
            if (null == info || info.IsLogicFinished || info.IsLogicPaused) return;
            info.Time += deltaTime;
            if (info.Time > 100)
            {
                NpcInAreaLogicInfo data = info.LogicDatas.GetData<NpcInAreaLogicInfo>();
                if (null == data)
                {
                    data = new NpcInAreaLogicInfo();
                    info.LogicDatas.AddData<NpcInAreaLogicInfo>(data);
                    SceneLogicConfig sc = info.SceneLogicConfig;
                    if (null != sc)
                    {
                        if (null != sc)
                        {
                            List<float> pts = Converter.ConvertNumericList<float>(sc.m_Params[0]);
                            data.m_Area = new Vector3[pts.Count / 2];
                            for (int ix = 0; ix < pts.Count - 1; ix += 2)
                            {
                                data.m_Area[ix / 2].X = pts[ix];
                                data.m_Area[ix / 2].Z = pts[ix + 1];
                            }
                        }
                    }
                }
                info.Time = 0;
                if (null != data)
                {
                    info.SpatialSystem.VisitObjectOutPolygon(data.m_Area, (float distSqr, ArkCrossEngineSpatial.ISpaceObject obj) =>
                    {
                        if (obj.GetObjType() == ArkCrossEngineSpatial.SpatialObjType.kNPC)
                        {
                            NpcInfo npc = obj.RealObject as NpcInfo;
                            if (null != npc && !npc.IsDead())
                            {
                                npc.SetHp(Operate_Type.OT_Absolute, 0);
                                SceneLogicSendStoryMessage(info, "npcleavearea", npc.GetId());
                            }
                        }
                    });
                }
            }
        }
    }
}
