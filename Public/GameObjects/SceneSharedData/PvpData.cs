﻿namespace ArkCrossEngine
{
    public class CampTowerState
    {
        public bool m_IsUpTowersExist = true;
        public bool m_IsMiddleTowersExist = true;
        public bool m_IsBottomTowersExist = true;
    }
    public class PvpData_TowerState
    {
        public CampTowerState m_BlueTowerState = new CampTowerState();
        public CampTowerState m_RedTowerState = new CampTowerState();
    }
}
