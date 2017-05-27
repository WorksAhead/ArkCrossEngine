using System.Collections.Generic;

namespace ArkCrossEngine
{
    public enum XSoulPart
    {
        kWeapon = 1,
        kSwing,
        kMax
    }

    public class XSoulInfo<T>
    {
        public XSoulInfo()
        {
        }

        public T GetXSoulPartData(XSoulPart part)
        {
            T result = default(T);
            m_XSoulPartDict.TryGetValue(part, out result);
            return result;
        }

        public void SetXSoulPartData(XSoulPart part, T item)
        {
            m_XSoulPartDict[part] = item;
        }

        public Dictionary<XSoulPart, T> GetAllXSoulPartData()
        {
            return m_XSoulPartDict;
        }

        private Dictionary<XSoulPart, T> m_XSoulPartDict = new Dictionary<XSoulPart, T>();
    }
}
