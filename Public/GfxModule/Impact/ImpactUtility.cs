using System;
using ArkCrossEngine;
using UnityEngine;

namespace GfxModule.Impact
{
    class ImpactUtility
    {
        public static float RadianToDegree(float dir)
        {
            return (float)(dir * 180 / Math.PI);
        }
        public static UnityEngine.Vector3 ConvertVector3D(string vec)
        {
            UnityEngine.Vector3 vector = UnityEngine.Vector3.zero;
            try
            {
                string strPos = vec;
                string[] resut = strPos.Split(s_ListSplitString, StringSplitOptions.None);
                vector = new UnityEngine.Vector3(Convert.ToSingle(resut[0]), Convert.ToSingle(resut[1]), Convert.ToSingle(resut[2]));
            }
            catch (System.Exception ex)
            {
                LogicSystem.LogicErrorLog("ImpactUtility.ConvertVector3D failed. ex:{0} st:{1}", ex.Message, ex.StackTrace);
            }

            return vector;
        }

        public static void MoveObject(GameObject obj, UnityEngine.Vector3 motion)
        {
            CharacterController ctrl = obj.GetComponent<CharacterController>();
            if (null != ctrl)
            {
                ctrl.Move(motion);
            }
            else
            {
                ctrl.transform.position += motion;
            }
        }

        public static bool IsLogicDead(GameObject obj)
        {
            if (null != obj)
            {
                SharedGameObjectInfo shareInfo = LogicSystem.GetSharedGameObjectInfo(obj);
                {
                    if (null != shareInfo && shareInfo.Blood > 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void PlaySound(GameObject obj, string sound)
        {
            if (null != obj)
            {
                AudioSource audioSource = obj.GetComponent<AudioSource>();
                if (null != audioSource)
                {
                    AudioClip clip = ResourceSystem.GetSharedResource(sound) as AudioClip;
                    if (null != clip)
                    {
                        audioSource.clip = clip;
                        audioSource.dopplerLevel = 0;
                        audioSource.Play();
                    }
                }
            }
        }
        public static void SetLayer(GameObject obj, int layer)
        {
            obj.layer = layer;
        }
        public static int PlayerIgnoreBlockLayer = LayerMask.NameToLayer("NoBlockPlayer");
        public static int MonsterIgnoreBlockLayer = LayerMask.NameToLayer("NoBlockMonster");
        public static int PlayerLayer = LayerMask.NameToLayer("Player");
        public static int MonsterLayer = LayerMask.NameToLayer("Monster");
        private static string[] s_ListSplitString = new string[] { ",", " ", ", ", "|" };
    }
}
