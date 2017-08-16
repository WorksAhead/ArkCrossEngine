namespace ArkCrossEngine
{
    public static class DelayManager
    {
        private static bool IsDelayEnabled_ = false;
        private static long LastDelayMoveTime = 0;
        private static int DelayMoveState = -1;
        private static int delayFrameCount = 0;

        /// constnts
        static int c_PVPMissingRate = 70;
        static int c_CannotMoveTime0 = 2;
        static int c_CannotMoveTime1 = 4;
        static int c_CanMoveTime0 = 1;
        static int c_CanMoveTime1 = 3;

        static int c_NoDelayPing0 = 80;
        static int c_NoDelayPing1 = 130;
        static int c_DelayPing0 = 200;
        static int c_DelayPing1 = 600;

        public static int c_TimeScaleDelayRate = 30;
        public static float c_TimeScaleTime = 0.1f;

        public static bool IsDelayEnabled
        {
            get { return IsDelayEnabled_; }
            set
            {
                if (IsDelayEnabled_ != value)
                {
                    IsDelayEnabled_ = value;
                    OnDelayStateChanged();
                }

            }
        }

        private static void OnDelayStateChanged()
        {

        }

        public static long GetFakePingValue()
        {
            int ping = 0;
            if (IsDelayEnabled_)
            {
                ping = CrossEngineHelper.Random.Next(c_DelayPing0, c_DelayPing1);
            }
            else
            {
                ping = CrossEngineHelper.Random.Next(c_NoDelayPing0, c_NoDelayPing1);
            }

            return ping;
        }

        public static bool FilterDamage()
        {
            if (IsDelayEnabled)
            {
                int number = CrossEngineHelper.Random.Next(0, 100);
                if (number < c_PVPMissingRate)
                    return false;
            }
            
            return true;
        }

        public static bool FilterMove()
        {
            if (IsDelayEnabled)
            {
                long curTime = TimeUtility.GetLocalMilliseconds();

                
                if (DelayMoveState == -1)
                {
                    delayFrameCount = CrossEngineHelper.Random.Next(c_CannotMoveTime0, c_CannotMoveTime0);
                    DelayMoveState = 0;
                }

                if (DelayMoveState == 0)
                {
                    if (curTime > LastDelayMoveTime + delayFrameCount*1000)
                    {
                        delayFrameCount = CrossEngineHelper.Random.Next(c_CanMoveTime0, c_CanMoveTime1);
                        LastDelayMoveTime = curTime;
                        DelayMoveState = 1;
                        return true;
                    }
                    return false;
                }
                else if (DelayMoveState == 1)
                {
                    if (curTime > LastDelayMoveTime + delayFrameCount*1000)
                    {
                        LastDelayMoveTime = curTime;
                        DelayMoveState = -1;
                        return true;
                    }
                    return true;
                }
                else
                {
                    DelayMoveState = -1;
                    return true;
                }
                
                
            }
            return true;  
        }
    }
}
