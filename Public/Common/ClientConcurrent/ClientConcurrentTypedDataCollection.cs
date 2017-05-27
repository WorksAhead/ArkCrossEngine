using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace ArkCrossEngine
{
    /// <remarks>
    /// ���ַ�ʽ��Ҫ�������ݲ��治�ó���ĳ��ϣ������ҵ��Ƕ��ܹ������ݶ�����г��󣬾;�����Ҫ�����ֶ�̬�ľۺ����ݵķ�ʽ������ʹ��
    /// ��̬���͵Ĺ�����
    /// ���������ڹ������ɲ�ͬ���͵����ݣ���֮ǰC++����union����ͬ���ݵ����Σ�ʹ�����ƣ�
    /// 1�����������ÿ�����͵�����ֻ����һ��ʵ����
    /// 2������ʹ������ʱ������Ϣ����ǿ���ͼ��ϣ�����Ч�ʽϵ͡�
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
