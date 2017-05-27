namespace ArkCrossEngine
{
    public class LegacyInfo
    {
        public ItemDataInfo[] Legacy
        {
            get { return m_Legacy; }
        }
        public ItemDataInfo[] ComplexAttr
        {
            get { return m_ComplexAttr; }
        }
        public void Reset()
        {
            for (int ix = 0; ix < LegacyStateInfo.c_LegacyCapacity; ++ix)
            {
                m_Legacy[ix] = null;
            }
            for (int ix = 0; ix < LegacyStateInfo.c_AttrCapacity; ++ix)
            {
                m_ComplexAttr[ix] = null;
            }
        }
        private ItemDataInfo[] m_Legacy = new ItemDataInfo[LegacyStateInfo.c_LegacyCapacity];
        private ItemDataInfo[] m_ComplexAttr = new ItemDataInfo[LegacyStateInfo.c_AttrCapacity];
    }
    public class LegacyStateInfo
    {
        public const int c_AttrCarrier = 101080;
        public const int c_LegacyCapacity = 4;
        public const int c_AttrCapacity = 7;
        public bool LegacyChanged
        {
            get { return m_LegacyChanged; }
            set { m_LegacyChanged = value; }
        }
        public LegacyInfo LegacyInfo
        {
            get { return m_LegacyInfo; }
        }
        public void SetLegacyData(int index, ItemDataInfo info)
        {
            if (index >= 0 && index < c_LegacyCapacity && info != null)
            {
                m_LegacyInfo.Legacy[index] = info;
                m_LegacyChanged = true;
            }
        }
        public void UpdateLegacyComplexAttr()
        {
            for (int i = 0; i < c_AttrCapacity; i++)
            {
                if (null == m_LegacyInfo.ComplexAttr[i])
                {
                    int assit_index = i + 1;
                    LegacyComplexAttrConifg lcac_data = LegacyComplexAttrConifgProvider.Instance.GetDataById(assit_index);
                    if (null != lcac_data && lcac_data.Property > 0
                      && IsUnlock(lcac_data.m_PeerA)
                      && IsUnlock(lcac_data.m_PeerB))
                    {
                        ItemDataInfo attr_carrier = new ItemDataInfo();
                        attr_carrier.Level = 1;
                        attr_carrier.ItemNum = 1;
                        attr_carrier.RandomProperty = lcac_data.Property;
                        attr_carrier.ItemConfig = ItemConfigProvider.Instance.GetDataById(c_AttrCarrier);
                        if (null != attr_carrier.ItemConfig)
                        {
                            SetLegacyComplexAttr(i, attr_carrier);
                        }
                    }
                }
            }
        }
        public ItemDataInfo GetLegacyData(int index)
        {
            ItemDataInfo info = null;
            if (index >= 0 && index < c_LegacyCapacity)
            {
                info = m_LegacyInfo.Legacy[index];
            }
            return info;
        }
        public void ResetLegacyData(int index)
        {
            if (index >= 0 && index < c_LegacyCapacity)
            {
                m_LegacyInfo.Legacy[index] = null;
                m_LegacyChanged = true;
            }
        }
        public void ResetLegacyComplexAttr(int index)
        {
            if (index >= 0 && index < c_AttrCapacity)
            {
                m_LegacyInfo.ComplexAttr[index] = null;
                m_LegacyChanged = true;
            }
        }
        public void Reset()
        {
            m_LegacyInfo.Reset();
            m_LegacyChanged = true;
        }
        public bool IsUnlock(int LegacyId)
        {
            bool ret = false;
            if (null == m_LegacyInfo.Legacy)
                return ret;
            for (int index = 0; index < m_LegacyInfo.Legacy.Length; ++index)
            {
                if (m_LegacyInfo.Legacy[index] != null && m_LegacyInfo.Legacy[index].ItemId == LegacyId && m_LegacyInfo.Legacy[index].IsUnlock)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }
        public ItemDataInfo GetLegacyComplexAttr(int index)
        {
            ItemDataInfo info = null;
            for (int i = 0; i < m_LegacyInfo.ComplexAttr.Length; i++)
            {
                if (i == index)
                {
                    info = m_LegacyInfo.ComplexAttr[i];
                    break;
                }
            }
            return info;
        }
        public void SetLegacyComplexAttr(int pos, ItemDataInfo info)
        {
            if (null != m_LegacyInfo.ComplexAttr && m_LegacyInfo.ComplexAttr.Length > 0)
            {
                for (int i = 0; i < m_LegacyInfo.ComplexAttr.Length; i++)
                {
                    if (i == pos)
                    {
                        m_LegacyInfo.ComplexAttr[i] = info;
                        m_LegacyChanged = true;
                        break;
                    }
                }
            }
        }
        private bool m_LegacyChanged = false;
        private LegacyInfo m_LegacyInfo = new LegacyInfo();
    }
}
