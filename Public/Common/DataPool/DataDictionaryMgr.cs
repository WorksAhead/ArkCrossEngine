﻿/**
 * @file DataDictionaryMgr.cs
 * @brief 数据模板管理器
 *              负责：
 *                      定义数据接口
 *                      使用MyDictionary存储数据
 */

using System.Collections.Generic;

namespace ArkCrossEngine
{
    /**
     * @brief 数据模板管理器
     */
    public class DataDictionaryMgr<TData> where TData : IData, new()
    {
        //---------------------------------------------------------
        // 属性
        //---------------------------------------------------------

        /**
         * @brief 数据容器，默认使用字典
         */
        MyDictionary<int, object> m_DataContainer;

        /**
         * @brief 构造函数
         *
         * @return 
         */
        public DataDictionaryMgr()
        {
            m_DataContainer = new MyDictionary<int, object>();
        }

        /**
         * @brief 提取数据
         *
         * @param file 文件路径
         *
         * @return 
         */
        public bool CollectDataFromDBC(string file, string rootLabel)
        {
            bool result = true;

            DBC document = new DBC();
            document.Load(HomePath.GetAbsolutePath(file));

            for (int index = 0; index < document.RowNum; index++)
            {
                DBC_Row node = document.GetRowByIndex(index);
                if (node != null)
                {
                    TData data = new TData();
                    bool ret = data.CollectDataFromDBC(node);
                    string info = string.Format("DataTempalteMgr.CollectDataFromDBC collectData Row:{0} failed!", index);
                    LogSystem.Assert(ret, info);
                    if (ret)
                    {
                        m_DataContainer.Add(data.GetId(), data);
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        /**
         * @brief 获取数据，根据ID
         *
         * @param id
         *
         * @return 
         */
        public TData GetDataById(int id)
        {
            object ret;
            if (m_DataContainer.TryGetValue(id, out ret))
                return (TData)ret;
            else
                return default(TData);
        }

        /**
         * @brief 获取数据数量
         *
         * @param id
         *
         * @return 
         */
        public int GetDataCount()
        {
            return m_DataContainer.Count;
        }

        /**
         * @brief 返回数据
         *
         * @param id
         *
         * @return 
         */
        public MyDictionary<int, object> GetData()
        {
            return m_DataContainer;
        }

        /**
         * @brief 清除所有数据
         *
         * @return 
         */
        public void Clear()
        {
            m_DataContainer.Clear();
        }
    }
}
