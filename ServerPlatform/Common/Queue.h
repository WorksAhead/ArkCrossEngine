#ifndef __QUEUE_H__
#define __QUEUE_H__

#include <memory.h>
#include "BaseType.h"

namespace CollectionMemory
{
	template<typename T,int MaxSizeV>
	class StaticT
	{
	public:
		inline T* Create(int& size)
		{
			if(size>MaxSizeV)
			{
				size = 1;
				return NULL;
			}
			return m_Data;
		}
		inline void Clear(void)
		{
			memset(m_Data,0,sizeof(m_Data));
		}
		size_t GetMemoryInUsed(void) const
		{
			return sizeof(T) * MaxSizeV;
		}
	private:
		T	m_Data[MaxSizeV];
	};

	template<typename T>
	class DynamicT
	{
	public:
		inline T* Create(int& size)
		{
			Cleanup();
			m_Data = new T[size];
			if(NULL == m_Data)
			{
				size = 1;
				return NULL;
			}
			m_MaxSize = size;
			return m_Data;
		}
		inline void Clear(void)
		{
			if(m_MaxSize>0 && NULL!=m_Data)
			{
				memset(m_Data,0,sizeof(T)*m_MaxSize);
			}
		}
		size_t GetMemoryInUsed(void) const
		{
			return (m_Data == NULL ? 0 : (sizeof(T) * m_MaxSize));	
		}
	public:
		DynamicT(void):m_Data(NULL),m_MaxSize(0)
		{}
		virtual ~DynamicT(void)
		{
			Cleanup();
		}
	private:
		inline void Cleanup(void)
		{
			if(NULL!=m_Data)
			{
				delete[] m_Data;
				m_Data=NULL;
			}
			m_MaxSize = 0;
		}
	private:
		T*	m_Data;
		int	m_MaxSize;
	};
	template<typename T,int MaxSizeV>
	class SelectorT
	{
	public:
		typedef StaticT<T,MaxSizeV> Type;
	};
	template<typename T>
	class SelectorT<T,0>
	{
	public:
		typedef DynamicT<T> Type;
	};
}

//
//T�Ƕ���Ԫ�����ͣ�SizeV���������Ķ��е����Ԫ����Ŀ(Ϊ0ʱ���ö�̬�ڴ���䷽��)
template<typename T,int SizeV=0>
class DequeT
{
	typedef typename CollectionMemory::SelectorT<T,(SizeV == 0 ? 0 : SizeV+1)>::Type MemoryType;
public://��׼˫����з��ʷ���
	//�����Ƿ��
	inline int Empty(void) const
	{
		return m_Head==m_Tail;
	}
	//�����Ƿ���
	inline int Full(void) const
	{
		return m_Head==(m_Tail+1)%m_MaxSize;
	}
	//��ն���
	inline void Clear(void)
	{
		m_Size=0;
		m_Head=m_Tail=0;
		m_Memory.Clear();
	}
	//��ǰ�����е�Ԫ�ظ���
	inline int Size(void) const
	{
		return m_Size;
	}
	//����β��һ��Ԫ��
	inline int PushBack(const T& t)
	{
		DebugAssert(m_Data);
		DebugAssert(!Full());

		int id=m_Tail;
		m_Data[id]=t;
		m_Tail=(m_Tail+1) % m_MaxSize;

		UpdateSize();
		return id;
	}
	//����ͷ��һ��Ԫ��
	inline int PushFront(const T& t)
	{
		DebugAssert(m_Data);
		DebugAssert(!Full());

		m_Head=(m_MaxSize+m_Head-1) % m_MaxSize;
		m_Data[m_Head]=t;

		UpdateSize();
		return m_Head;
	}
	//ɾ������βһ��Ԫ��
	inline T PopBack(void)
	{
		DebugAssert(m_Data);
		DebugAssert(!Empty());

		int id=BackID();
		m_Tail=id;

		UpdateSize();
		return m_Data[id];
	}
	//ɾ������ͷһ��Ԫ��
	inline T PopFront(void)
	{
		DebugAssert(m_Data);
		DebugAssert(!Empty());

		int id=m_Head;
		m_Head = (m_Head+1) % m_MaxSize;

		UpdateSize();
		return m_Data[id];
	}
	//������βԪ��
	inline const T& Back(void) const
	{
		return m_Data[BackID()];
	}
	//������βԪ�ؿ�д���ã������޸Ķ���βԪ�أ�
	inline T& Back(void)
	{
		return m_Data[BackID()];
	}
	//������ͷԪ��
	inline const T& Front(void) const
	{
		return m_Data[FrontID()];
	}
	//������ͷԪ�ؿ�д���ã������޸Ķ���ͷԪ�أ�
	inline T& Front(void)
	{
		return m_Data[FrontID()];
	}
public://��չ˫����з��ʷ������������д������
	//FrontID�Ƕ���ͷԪ�ص�ID
	inline int FrontID(void) const
	{
		return m_Head;
	}
	//BackID�Ƕ���βԪ�ص�ID
	inline int BackID(void) const
	{
		if(Empty())
		{			
			return m_Head;
		}
		int newId = (m_MaxSize+m_Tail-1) % m_MaxSize;
		return newId;
	}
	//ȡ��ǰID��ǰһ��ID������Ѿ���ͷԪ��ID���򷵻�INVALID_ID
	inline int PrevID(int id) const
	{
		if(id==m_Head)
			return INVALID_ID;
		int newId = (m_MaxSize+id-1) % m_MaxSize;
		return newId;
	}
	//ȡ��ǰID�ĺ�һ��ID������Ѿ���βԪ��ID���򷵻�INVALID_ID
	inline int NextID(int id) const
	{
		if(id==BackID())
			return INVALID_ID;
		int newId = (id+1) % m_MaxSize;
		return newId;
	}
	//�ж��Ƿ�����Ч��ID���Կն��У�ͷID��βID������ЧID
	inline int IsValidID(int id) const
	{
		if(Empty())
		{
			return FALSE;
		}
		if(id<0 || id>=m_MaxSize)
		{
			return FALSE;
		}
		int idVal=CalcIndex(id);
		int tailVal=CalcIndex(m_Tail);
		if(idVal>=tailVal)
			return FALSE;
		return TRUE;
	}
	//ȡָ��ID��Ԫ��
	inline const T& operator [] (int id) const
	{
		if(id<0 || id>=m_MaxSize)
		{
			return GetInvalidValueRef();
		}
		else
		{
			return m_Data[id];
		}
	}
	//ȡָ��ID��Ԫ�صĿ�д���ã������޸�Ԫ�أ�
	inline T& operator [] (int id)
	{
		if(id<0 || id>=m_MaxSize)
		{
			return GetInvalidValueRef();
		}
		else
		{
			return m_Data[id];
		}
	}
	//��2��ID�ľ��루���Ԫ�ظ���+1��
	inline int Distance(int id1,int id2) const
	{
		int val=Difference(id1,id2);
		if(val<0)
			return -val;
		else
			return val;
	}
	//��2��ID֮�����ͷ��β��˳����Ԫ�أ����֮�
	inline int Difference(int id1,int id2) const
	{
		int id1Val=CalcIndex(id1);
		int id2Val=CalcIndex(id2);
		DebugAssert(id1Val>=0 && id2Val>=0);
		return id2Val-id1Val;
	}
public:
	DequeT(void):m_Size(0),m_MaxSize(1),m_Head(0),m_Tail(0),m_Data(NULL)
	{
		if(SizeV>0)
		{
			Init(SizeV);
		}
	}
	DequeT(int size):m_Size(0),m_MaxSize(1),m_Head(0),m_Tail(0),m_Data(NULL)
	{
		Init(size);
	}
	virtual ~DequeT(void)
	{
		m_Size = 0;
		m_MaxSize = 1;
		m_Head = 0;
		m_Tail = 0;
		m_Data = NULL;	
	}
public:
	DequeT(const DequeT& other)
	{
		Init(other.m_MaxSize-1);
		CopyFrom(other);
	}
	DequeT& operator=(const DequeT& other)
	{
		if(this==&other)
			return *this;
		Clear();
		Init(other.m_MaxSize-1);
		CopyFrom(other);
		return *this;
	}
public:
	inline void	Init(int size)
	{
		Create(size+1);
	}
protected:
	inline void Create(int size)
	{
		m_MaxSize = size;
		m_Data = m_Memory.Create(m_MaxSize);
		DebugAssert(m_Data);
		Clear();
	}
private:
	//����Ԫ�ص�������ͷԪ������Ϊ0��
	inline int CalcIndex(int id) const
	{
		if(id<m_Head)
			return id+m_MaxSize-m_Head;
		else
			return id-m_Head;
	}
	//���¶��гߴ�
	inline void UpdateSize(void)
	{
		m_Size=(m_MaxSize+m_Tail-m_Head)%m_MaxSize;
	}
private:
	void CopyFrom(const DequeT& other)
	{
		Clear();
		for(int id=other.FrontID();TRUE==other.IsValidID(id);id=other.NextID(id))
		{
			PushBack(other[id]);
		}
	}
private:
	MemoryType m_Memory;
	T* m_Data;
	int m_Size;
	int m_MaxSize;
	//ͷԪ�ص�ID
	int m_Head;
	//βԪ�غ���һ��λ�õ�ID������������β��λ�ã�����ֵ����һ����Ч��ID
	int m_Tail;
public:
	static T& GetInvalidValueRef(void)
	{
		static T s_Temp;
		return s_Temp;
	}
};

#endif //__QUEUE_H__