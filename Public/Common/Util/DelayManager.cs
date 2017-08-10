namespace ArkCrossEngine
{
    public static class DelayManager
    {
        private static bool IsDelayEnabled_ = false;
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
            int number = CrossEngineHelper.Random.Next(0, 10);
            if (number < 5)
                return false;

            return true;
        }
    }
}
