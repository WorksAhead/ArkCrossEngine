using ArkCrossEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptableData
{
    public class LuaInterface
    {
        public object L { get { return NativeObject; } }

        public virtual void Init ( ScriptManager.CustomLoaderDelegate func, string luaDebuggerPath = "" ) { }
        public virtual void Tick () { }
        public virtual void Destroy() { }
        public virtual void ExecuteBuffer ( string scriptData, object extra = null, string chunk = "default" ) { }
        public virtual void ExecuteFile ( string file, object extra = null, string chunk = "default" ) { }

        public virtual object SetupNewEnv ( string scriptEnv ) { return null; }
        public virtual Action QueryAction( string action, object env ) { return null; }
        public virtual Action<object> QueryAction_1( string action, object env ) { return null; }
        public virtual Action[] QueryActions(string[] actions, object env ) { return null; }
        public virtual Action<object>[] QueryActions_1(string[] action, object env ) { return null; }
        
        public string GetLuaScriptPathForDebugger ( string script )
        {
            return LuaScriptPathForDebugger + script + ".lua.txt";
        }
        protected string LuaScriptPathForDebugger;
        protected object NativeObject;
    }

    [Serializable]
    public class Injection
    {
        public string Name;
        public object Object;
    }

    public interface ILuaBehaviour
    {
        void InitEnv ();
        void ExecuteLua ();
        void SetInjections ();
        Action QueryAction ();
        void InvokeAction (string action);
    }
}