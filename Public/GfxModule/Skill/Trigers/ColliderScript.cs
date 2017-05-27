using ArkCrossEngine;

/// TODO: remove dep on unity
public class ColliderScript : UnityEngine.MonoBehaviour
{
    public ColliderScript()
    {

    }
    public void SetOnTriggerEnter(MyAction<ArkCrossEngine.Collider> onEnter)
    {
        m_OnTrigerEnter += onEnter;
    }
    public void SetOnTriggerExit(MyAction<ArkCrossEngine.Collider> onExit)
    {
        m_OnTrigerExit += onExit;
    }

    public void SetOnDestroy(MyAction onDestroy)
    {
        m_OnDestroy += onDestroy;
    }

    public void OnDestroy()
    {
        if (m_OnDestroy != null)
        {
            m_OnDestroy();
        }
    }

    void OnTriggerEnter(UnityEngine.Collider collider)
    {
        if (null != m_OnTrigerEnter)
        {
            Collider nativeCollider = ObjectFactory.Create<Collider>(collider);
            m_OnTrigerEnter(nativeCollider);
        }
    }
    void OnTriggerExit(UnityEngine.Collider collider)
    {
        if (null != m_OnTrigerExit)
        {
            Collider nativeCollider = ObjectFactory.Create<Collider>(collider);
            m_OnTrigerExit(nativeCollider);
        }
    }

    private MyAction<ArkCrossEngine.Collider> m_OnTrigerEnter;
    private MyAction<ArkCrossEngine.Collider> m_OnTrigerExit;
    private MyAction m_OnDestroy;
}
