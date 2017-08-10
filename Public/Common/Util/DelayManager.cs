namespace ArkCrossEngine
{
    public static class DelayManager
    {
        private static bool IsDelayEnabled_ = false;
        private static long LastDelayMoveTime = 0;
        private static int DelayMoveState = -1;
        private static int delayFrameCount = 0;
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

        public static bool FilterDamage()
        {
            if (IsDelayEnabled)
            {
                int number = CrossEngineHelper.Random.Next(0, 10);
                if (number < 5)
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
                    delayFrameCount = CrossEngineHelper.Random.Next(500, 1000);
                    DelayMoveState = 0;
                }

                if (DelayMoveState == 0)
                {
                    if (curTime > LastDelayMoveTime + delayFrameCount)
                    {
                        delayFrameCount = CrossEngineHelper.Random.Next(500, 3000);
                        LastDelayMoveTime = curTime;
                        DelayMoveState = 1;
                        return true;
                    }
                    return false;
                }
                else if (DelayMoveState == 1)
                {
                    if (curTime > LastDelayMoveTime + delayFrameCount)
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
