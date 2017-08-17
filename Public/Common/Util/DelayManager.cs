using System.Collections.Generic;

namespace ArkCrossEngine
{
    public static class DelayManager
    {
        public struct Position
        {
            public Position(float _x, float _y, float _z)
            {
                x = _x;
                y = _y;
                z = _z;
            }

            public float x;
            public float y;
            public float z;
        }

        private static bool IsDelayEnabled_ = true;
        private static long LastDelayMoveTime = 0;
        private static int DelayMoveState = -1;
        private static int delayFrameCount = 0;

        static long s_LastAddPositionTime = 0;

        /// constnts
        static int c_PVPMissingRate = 70;
        static int c_CannotMoveTime0 = 2;
        static int c_CannotMoveTime1 = 4;
        static int c_CanMoveTime0 = 1;
        static int c_CanMoveTime1 = 2;

        static int c_NoDelayPing0 = 80;
        static int c_NoDelayPing1 = 130;
        static int c_DelayPing0 = 900;
        static int c_DelayPing1 = 2300;

        //static List<Position> Positions = new List<Position>();
        //static int PositionIndex = 0;

        public static int c_TimeScaleDelayRate = 20;
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

        

        private static List<Position> Breadcrumbs = new List<Position>();
        private static long LastBreadcrumbTime = 0;
        private static long NextDragTime = 0;

        public static void intervene(ref float x, ref float y, ref float z)
        {
            long now = TimeUtility.GetLocalMilliseconds();

            // 以 0.1s 为间隔, 保存最近 3s 之内的面包屑
            if (now - LastBreadcrumbTime >= 100)
            {
                Breadcrumbs.Add(new Position(x, y, z));

                if (Breadcrumbs.Count > 30) {
                    Breadcrumbs.RemoveAt(0);
                }

                LastBreadcrumbTime = now;
            }

            // 触发不定期拖拽
            if (now - NextDragTime >= 0)
            {
                // 时间点在 0.1s 之内才触发拖拽, 超时不强行拖拽
                if (now - NextDragTime < 100)
                {
                    // 拖拽到最近 0.5s - 0.7s 的面包屑上
                    if (Breadcrumbs.Count >= 7)
                    {
                        int index = Breadcrumbs.Count - CrossEngineHelper.Random.Next(5, 7);

                        x = Breadcrumbs[index].x;
                        y = Breadcrumbs[index].y;
                        z = Breadcrumbs[index].z;
                    }
                }

                // 随机产生下一次拖拽的时间点(2.5s - 7.5s)
                NextDragTime = now + CrossEngineHelper.Random.Next(1500, 4500);
            }
        }

        //public static void AddPosition(float x, float y, float z)
        //{
        //    long time = TimeUtility.GetLocalMilliseconds();
        //    if (time > s_LastAddPositionTime + 1000)
        //    {
        //        int count = Positions.Count;
        //        if (count <= PositionIndex)
        //        {
        //            Positions.Add(new Position(x, y, z));
        //        }
        //        else
        //        {
        //            Positions[PositionIndex] = new Position(x, y, z);
        //        }

        //        PositionIndex = (PositionIndex + 1) % 3;

        //        s_LastAddPositionTime = time;
        //    }
        //}

        //public static Position GetFakePosition()
        //{
        //    if (Positions.Count < 3)
        //        return new Position();

        //    int index = PositionIndex - 1;
        //    if (index < 0)
        //    {
        //        index = Positions.Count - 1;
        //    }

        //    return Positions[index];
        //}

        //public static bool FilterMove()
        //{
        //    if (IsDelayEnabled)
        //    {
        //        long curTime = TimeUtility.GetLocalMilliseconds();

        //        if (Positions.Count < 3)
        //            return true;
                
        //        if (DelayMoveState == -1)
        //        {
        //            delayFrameCount = CrossEngineHelper.Random.Next(c_CannotMoveTime0, c_CannotMoveTime0);
        //            DelayMoveState = 0;
        //        }

        //        if (DelayMoveState == 0)
        //        {
        //            if (curTime > LastDelayMoveTime + delayFrameCount*1000)
        //            {
        //                delayFrameCount = CrossEngineHelper.Random.Next(c_CanMoveTime0, c_CanMoveTime1);
        //                LastDelayMoveTime = curTime;
        //                DelayMoveState = 1;
        //                return true;
        //            }
        //            return false;
        //        }
        //        else if (DelayMoveState == 1)
        //        {
        //            if (curTime > LastDelayMoveTime + delayFrameCount*1000)
        //            {
        //                LastDelayMoveTime = curTime;
        //                DelayMoveState = -1;
        //                return true;
        //            }
        //            return true;
        //        }
        //        else
        //        {
        //            DelayMoveState = -1;
        //            return true;
        //        }
                
                
        //    }
        //    return true;  
        //}
    }
}
