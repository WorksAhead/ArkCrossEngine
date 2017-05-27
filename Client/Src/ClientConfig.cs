using System;

namespace ArkCrossEngine
{
    internal class ClientConfig
    {
        internal static float s_PositionRefix = 0.1f;

        internal static float s_PredictMoveDurationRefix = 0.5f;

        /**
         * @brief 相机配置参数
         */
        // Base
        internal static float s_PitchDefault = (-(float)Math.PI / 4);
        internal static float s_YawDefault = 0;
        internal static float s_RollDefault = 0;
        internal static float s_DistanceDefault = 25;

        // Zoom
        internal static float s_ZoomVelocity = 1.0f;
        internal static float s_ClipNearLimit = 10.0f;
        internal static float s_ClipFarLimit = 2000.0f;

        // Turn
        internal static float s_TurnUpVelocity = 0.1f;
        internal static float s_TurnUpLimit = -(float)Math.PI / 2 + 0.1f;

        internal static float s_TurnDownVelocity = 0.1f;
        internal static float s_TurnDownLimit = 0.1f;

        internal static float s_TurnLeftVelocity = 0.1f;
        internal static float s_TurnLeftLimit = (float)Math.PI / 2 - 0.1f;

        internal static float s_TurnRightVelocity = 0.1f;
        internal static float s_TurnRightLimit = -(float)Math.PI / 2 + 0.1f;

        internal static float s_TurnClockwiseVelocity = 0.1f;
        internal static float s_TurnClockwiseLimit = (float)Math.PI / 2 - 0.1f;

        internal static float s_TurnAntiClockwiseVelocity = 0.1f;
        internal static float s_TurnAntiClockwiseLimit = -(float)Math.PI / 2 + 0.1f;

        // Move
        internal static float s_MoveVelocity = 1.0f;

        // Follow
        internal static float s_MoveTweaker = 5;

        // auto aim
        internal static bool s_IsAutoAimTurnOn = false;
        internal static float s_AutoAimAngleDegree = 30;
        internal static float s_AutoAimRangeRadius = 1;
        internal static string s_AutoAimMarkAsset = "";

        /**
        * @brief 客户端资源配置
        */
        internal static string s_SpartAsset = "";
        internal static string s_BulletholeAsset = "";
        internal static string s_BeijiAsset = "";
        internal static string s_GroundAsset = "";
        internal static string s_BloodAsset = "";
        internal static string s_HitAsset = "";
        internal static string s_SourAsset = "";

        internal static float s_ShineTime = 0.05f;


        /**
         * @brief 载入配置
         *
         */
        internal static bool LoadClientConfig(string file)
        {
            /*
            // Read Camera data
            s_PitchDefault = ini.ExtractNumeric<float>("Camera", "PitchDefault", (-Math.PI / 4));
            s_YawDefault = ini.ExtractNumeric<float>("Camera", "YawDefault", 0);
            s_RollDefault = ini.ExtractNumeric<float>("Camera", "RollDefault", 0);
            s_DistanceDefault = ini.ExtractNumeric<float>("Camera", "DistanceDefault", 25);
            s_ZoomVelocity = ini.ExtractNumeric<float>("Camera", "ZoomVelocity", 1.0f);
            s_ClipNearLimit = ini.ExtractNumeric<float>("Camera", "ClipNearLimit", 10.0f);
            s_ClipFarLimit = ini.ExtractNumeric<float>("Camera", "ClipFarLimit", 2000.0f);
            s_TurnUpVelocity = ini.ExtractNumeric<float>("Camera", "TurnUpVelocity", 0.1f);
            s_TurnUpLimit = ini.ExtractNumeric<float>("Camera", "TurnUpLimit", (Math.PI / 2 + 0.1));
            s_TurnLeftVelocity = ini.ExtractNumeric<float>("Camera", "TurnLeftVelocity", 0.1f);
            s_TurnLeftLimit = ini.ExtractNumeric<float>("Camera", "TurnLeftLimit", (Math.PI / 2 - 0.1));
            s_TurnRightVelocity = ini.ExtractNumeric<float>("Camera", "TurnRightVelocity", 0.1f);
            s_TurnRightLimit = ini.ExtractNumeric<float>("Camera", "TurnRightLimit", (Math.PI / 2 + 0.1));
            s_TurnClockwiseVelocity = ini.ExtractNumeric<float>("Camera", "TurnClockwiseVelocity", 0.1f);
            s_TurnClockwiseLimit = ini.ExtractNumeric<float>("Camera", "TurnClockwiseLimit", (Math.PI / 2 - 0.1));
            s_TurnAntiClockwiseVelocity = ini.ExtractNumeric<float>("Camera", "TurnAntiClockwiseVelocity", 0.1f);
            s_TurnAntiClockwiseLimit = ini.ExtractNumeric<float>("Camera", "TurnAntiClockwiseLimit", (Math.PI / 2 + 0.1));
            s_MoveVelocity = ini.ExtractNumeric<float>("Camera", "MoveVelocity", 1.0f);
            s_MoveTweaker = ini.ExtractNumeric<float>("Camera", "MoveTweaker", 5);

            s_IsAutoAimTurnOn = ini.ExtractBool("AutoAim", "IsTurnOn", false);
            s_AutoAimAngleDegree = ini.ExtractNumeric<float>("AutoAim", "AngleDegree", 30);
            s_AutoAimRangeRadius = ini.ExtractNumeric<float>("AutoAim", "RangeRadius", 1);
            s_AutoAimMarkAsset = ini.ExtractString("AutoAim", "MarkAsset", "");

            // Read asset data
            s_SpartAsset = ini.ExtractString("Asset", "SpartAsset", "");
            s_BulletholeAsset = ini.ExtractString("Asset", "BulletholeAsset", "");
            s_BeijiAsset = ini.ExtractString("Asset", "BeijiAsset", "");
            s_GroundAsset = ini.ExtractString("Asset", "GroundAsset", "");
            s_BloodAsset = ini.ExtractString("Asset", "BloodAsset", "");
            s_HitAsset = ini.ExtractString("Asset", "HitAsset", "");
            s_SourAsset = ini.ExtractString("Asset", "SourAsset", "");

            s_ShineTime = ini.ExtractNumeric<float>("Asset", "ShineTime", 0.05f);
            */
            return true;
        }
    }
}
