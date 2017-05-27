using System;

namespace ArkCrossEngine
{
    public class DeviceInfo
    {
        public string OperatingSystem
        {
            get { return m_OperatingSystem; }
            set { m_OperatingSystem = value; }
        }
        public string UniqueIdentifier
        {
            get { return m_UniqueIdentifier; }
            set { m_UniqueIdentifier = value; }
        }
        public string ClientLoginIp
        {
            get { return m_ClientLoginIp; }
            set { m_ClientLoginIp = value; }
        }

        private string m_ClientLoginIp = "127.0.0.1";           // 客户端Ip地址
        private string m_UniqueIdentifier;                      // 客户端设备标识
        private string m_OperatingSystem;                       // 客户端操作系统
    }
}
