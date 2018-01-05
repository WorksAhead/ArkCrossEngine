using ArkCrossEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ScriptableData
{
    public class ScriptManager
    {
#region Singleton
        private static ScriptManager s_instance_ = new ScriptManager();
        public static ScriptManager Instance
        {
            get { return s_instance_; }
        }
#endregion

        public delegate byte[] CustomLoaderDelegate ( ref string file );

        public object GetScriptObjectByThread(bool bGfxThread)
        {
            return bGfxThread ? LuaImpl[(int)LuaThread.GfxThread].L : LuaImpl[(int)LuaThread.LogicThread].L;
        }

        public void Init( object scriptImpl, CustomLoaderDelegate func, bool bCallFromGfxThread, string luaDebuggerPath = "" )
        {
            if (bCallFromGfxThread)
            {
                LuaImpl[(int)LuaThread.GfxThread] = scriptImpl as LuaInterface;
                LuaImpl[(int)LuaThread.GfxThread].Init(func, luaDebuggerPath);
            }
            else
            {
                LuaImpl[(int)LuaThread.LogicThread] = scriptImpl as LuaInterface;
                LuaImpl[(int)LuaThread.LogicThread].Init(func, luaDebuggerPath);
            }
        }

        public void Tick( bool bCallFromGfxThread )
        {
            if ( bCallFromGfxThread )
            {
                LuaImpl[(int)LuaThread.GfxThread].Tick();
            }
            else
            {
                LuaImpl[(int)LuaThread.LogicThread].Tick();
            }
        }

        public void Destroy ( bool bCallFromGfxThread )
        {
            if ( bCallFromGfxThread )
            {
                LuaImpl[(int)LuaThread.GfxThread].Destroy();
            }
            else
            {
                LuaImpl[(int)LuaThread.LogicThread].Destroy();
            }
        }

        public void ExecuteBuffer( string scriptData, bool bCallFromGfxThread, object extra = null, string chunk = "default" )
        {
            if ( bCallFromGfxThread )
            {
                LuaImpl[(int)LuaThread.GfxThread].ExecuteBuffer(scriptData, extra, chunk);
            }
            else
            {
                LuaImpl[(int)LuaThread.LogicThread].ExecuteBuffer(scriptData, extra, chunk);
            }
        }

        public void ExecuteFile( string file, bool bCallFromGfxThread, object extra = null, string chunk = "default" )
        {
            if ( bCallFromGfxThread )
            {
                LuaImpl[(int)LuaThread.GfxThread].ExecuteFile(file, extra, chunk);
            }
            else
            {
                LuaImpl[(int)LuaThread.LogicThread].ExecuteFile(file, extra, chunk);
            }
        }

        public object SetupNewEnv ( string scriptEnv, bool bCallFromGfxThread )
        {
            if ( bCallFromGfxThread )
            {
                return LuaImpl[(int)LuaThread.GfxThread].SetupNewEnv(scriptEnv);
            }
            else
            {
                return LuaImpl[(int)LuaThread.LogicThread].SetupNewEnv(scriptEnv);
            }
        }

        public Action QueryAction ( string action, object env, bool bCallFromGfxThread )
        {
            if ( bCallFromGfxThread )
            {
                return LuaImpl[(int)LuaThread.GfxThread].QueryAction(action, env);
            }
            else
            {
                return LuaImpl[(int)LuaThread.LogicThread].QueryAction(action, env);
            }
        }

        public Action<object> QueryAction_1 ( string action, object env, bool bCallFromGfxThread )
        {
            if ( bCallFromGfxThread )
            {
                return LuaImpl[(int)LuaThread.GfxThread].QueryAction_1(action, env);
            }
            else
            {
                return LuaImpl[(int)LuaThread.LogicThread].QueryAction_1(action, env);
            }
        }
        public Action[] QueryActions ( string[] actions, object env, bool bCallFromGfxThread )
        {
            if ( bCallFromGfxThread )
            {
                return LuaImpl[(int)LuaThread.GfxThread].QueryActions(actions, env);
            }
            else
            {
                return LuaImpl[(int)LuaThread.LogicThread].QueryActions(actions, env);
            }
        }
        public Action<object>[] QueryActions_1 ( string[] action, object env, bool bCallFromGfxThread )
        {
            if ( bCallFromGfxThread )
            {
                return LuaImpl[(int)LuaThread.GfxThread].QueryActions_1(action, env);
            }
            else
            {
                return LuaImpl[(int)LuaThread.LogicThread].QueryActions_1(action, env);
            }
        }

        public string GetLuaScriptPathForDebugger ( string script, bool bCallFromGfxThread )
        {
            if ( bCallFromGfxThread )
            {
                return LuaImpl[(int)LuaThread.GfxThread].GetLuaScriptPathForDebugger(script);
            }
            else
            {
                return LuaImpl[(int)LuaThread.LogicThread].GetLuaScriptPathForDebugger(script);
            }
        }

        enum LuaThread : int
        {
            GfxThread,
            LogicThread,
            Max
        }

        private LuaInterface[] LuaImpl = new LuaInterface[(int)LuaThread.Max];
    }
}