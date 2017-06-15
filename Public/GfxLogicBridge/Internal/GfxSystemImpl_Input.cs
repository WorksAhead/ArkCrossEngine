using System.Collections.Generic;
using UnityEngine;

namespace ArkCrossEngine
{
    public sealed partial class GfxSystem
    {
        public bool IsLastHitUi
        {
            get { return m_IsLastHitUi; }
            internal set { m_IsLastHitUi = value; }
        }
        private float GetMouseXImpl()
        {
            return m_CurMousePos.x;
        }
        private float GetMouseYImpl()
        {
            return m_CurMousePos.y;
        }
        private float GetMouseZImpl()
        {
            return m_CurMousePos.z;
        }
        private float GetMouseRayPointXImpl()
        {
            return m_MouseRayPoint.x;
        }
        private float GetMouseRayPointYImpl()
        {
            return m_MouseRayPoint.y;
        }
        private float GetMouseRayPointZImpl()
        {
            return m_MouseRayPoint.z;
        }
        private bool IsButtonPressedImpl(Mouse.Code c)
        {
            return m_ButtonPressed[(int)c];
        }
        private bool IsKeyPressedImpl(Keyboard.Code c)
        {
            return m_KeyPressed[(int)c];
        }
        private void ListenKeyPressStateImpl(Keyboard.Code[] codes)
        {
            for (int i = 0; i < codes.Length; i++)
            {
                if (!m_KeysForListen.Contains((int)codes[i]))
                {
                    m_KeysForListen.Add((int)codes[i]);
                }
            }
            /*
            foreach (Keyboard.Code c in codes) {
              if (!m_KeysForListen.Contains((int)c)) {
                m_KeysForListen.Add((int)c);
              }
            }*/
        }
        private void ListenKeyboardEventImpl(Keyboard.Code c, MyAction<int, int> handler)
        {
            if (m_KeyboardHandlers.ContainsKey((int)c))
            {
                m_KeyboardHandlers[(int)c] = handler;
            }
            else
            {
                m_KeyboardHandlers.Add((int)c, handler);
            }
        }
        private void ListenMouseEventImpl(Mouse.Code c, MyAction<int, int> handler)
        {
            if (m_MouseHandlers.ContainsKey((int)c))
            {
                m_MouseHandlers[(int)c] = handler;
            }
            else
            {
                m_MouseHandlers.Add((int)c, handler);
            }
        }

        private void ResetInputStateImpl()
        {
            for (int i = 0; i < (int)Keyboard.Code.MaxNum; ++i)
            {
                m_KeyPressed[i] = false;
            }
            for (int i = 0; i < (int)Mouse.Code.MaxNum; ++i)
            {
                m_ButtonPressed[i] = false;
            }
        }

        private void HandleInput()
        {
            m_LastMousePos = m_CurMousePos;
            m_CurMousePos = Input.mousePosition;

            if ((m_CurMousePos - m_LastMousePos).sqrMagnitude >= 1 && null != Camera.main)
            {
                UnityEngine.Ray ray = Camera.main.ScreenPointToRay(m_CurMousePos);
                UnityEngine.RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    m_MouseRayPoint = hitInfo.point;
                }
            }

            foreach (int c in m_KeysForListen)
            {
                if (Input.GetKeyDown((UnityEngine.KeyCode)c))
                {
                    m_KeyPressed[c] = true;
                    FireKeyboard(c, (int)Keyboard.Event.Down);
                }
                else if (Input.GetKeyUp((UnityEngine.KeyCode)c))
                {
                    m_KeyPressed[c] = false;
                    FireKeyboard(c, (int)Keyboard.Event.Up);
                }
                /*
                if (Input.GetKey((KeyCode)c)) {
                  FireKeyboard(c, (int)Keyboard.Event.LongPressed);
                }*/
            }
            /*
          for(int i=0;i<3;++i){
            if (Input.GetMouseButtonDown(i)) {
              m_ButtonPressed[i] = true;
              FireMouse(i, (int)Mouse.Event.Down);
            } else if (Input.GetMouseButtonUp(i)) {
              m_ButtonPressed[i] = false;
              FireMouse(i, (int)Mouse.Event.Up);
            }
            if (Input.GetMouseButton(i)) {
              FireMouse(i, (int)Mouse.Event.LongPressed);
            }
          }*/
        }
        private void FireKeyboard(int c, int e)
        {
            MyAction<int, int> handler;
            if (null != m_LogicInvoker && m_KeyboardHandlers.TryGetValue(c, out handler))
            {
                QueueLogicActionWithDelegation(handler, c, e);
            }
        }
        private void FireMouse(int c, int e)
        {
            MyAction<int, int> handler;
            if (null != m_LogicInvoker && m_MouseHandlers.TryGetValue(c, out handler))
            {
                QueueLogicActionWithDelegation(handler, c, e);
            }
        }

        private bool m_IsLastHitUi;
        private UnityEngine.Vector3 m_LastMousePos;
        private UnityEngine.Vector3 m_CurMousePos;
        private UnityEngine.Vector3 m_MouseRayPoint;

        private bool[] m_KeyPressed = new bool[(int)Keyboard.Code.MaxNum];
        private bool[] m_ButtonPressed = new bool[(int)Mouse.Code.MaxNum];
        private HashSet<int> m_KeysForListen = new HashSet<int>();

        private MyDictionary<int, MyAction<int, int>> m_KeyboardHandlers = new MyDictionary<int, MyAction<int, int>>();
        private MyDictionary<int, MyAction<int, int>> m_MouseHandlers = new MyDictionary<int, MyAction<int, int>>();
    }
}
