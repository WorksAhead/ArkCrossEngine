using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace ArkCrossEngine
{
    /// <remarks>
    /// 这种方式主要用于数据层面不好抽象的场合，如果从业务角度能够对数据定义进行抽象，就尽量不要用这种动态的聚合数据的方式而尽量使用
    /// 静态类型的关联。
    /// 这个类仅用于管理若干不同类型的数据，如之前C++里用union管理不同数据的情形，使用限制：
    /// 1、这个集合里每种类型的数据只能有一个实例。
    /// 2、由于使用运行时类型信息及非强类型集合，运行效率较低。
    /// </remarks>

    public sealed class ClientConcurrentTypedDataCollection
    {
        public void GetOrNewData<T>(out T t) where T : new()
        {
            t = GetData<T>();
            if (null == t)
            {
                t = new T();
                AddData(t);
            }
        }
        public void AddData<T>(T data)
        {
            Type t = typeof(T);
            if (null != data)
            {
                m_AiDatas.AddOrUpdate(t, data, data);
            }
        }
        public void RemoveData<T>(T t)
        {
            RemoveData<T>();
        }
        public void RemoveData<T>()
        {
            Type t = typeof(T);
            object o;
            m_AiDatas.TryRemove(t, out o);
        }
        public T GetData<T>()
        {
            Type t = typeof(T);
            object o;
            if (m_AiDatas.TryGetValue(t, out o))
            {
                return (T)o;
            }
            else
            {
                return default(T);
            }
        }
        public void Clear()
        {
            m_AiDatas.Clear();
        }
        public void Visit(MyAction<object, object> visitor)
        {
            foreach (KeyValuePair<Type, object> dict in m_AiDatas)
            {
                visitor(dict.Key, dict.Value);
            }
        }
        private ClientConcurrentDictionary<Type, object> m_AiDatas = new ClientConcurrentDictionary<Type, object>();
    }
}
