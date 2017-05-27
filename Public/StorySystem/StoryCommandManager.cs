using System.Collections.Generic;
using ArkCrossEngine;

namespace StorySystem
{
    /// <summary>
    /// 这个类不加锁，约束条件：所有命令注册必须在程序启动时完成。
    /// </summary>
    public sealed class StoryCommandManager
    {
        public void RegisterCommandFactory(string type, IStoryCommandFactory factory)
        {
            if (!m_StoryCommandFactories.ContainsKey(type))
            {
                m_StoryCommandFactories.Add(type, factory);
            }
            else
            {
                //error
            }
        }
        public IStoryCommand CreateCommand(ScriptableData.ISyntaxComponent commandConfig)
        {
            IStoryCommand command = null;
            string type = commandConfig.GetId();
            IStoryCommandFactory factory = GetFactory(type);
            if (null != factory)
            {
                command = factory.Create(commandConfig);
            }
            else
            {
                LogSystem.Debug("CreateCommand failed, unkown type:{0}", type);
            }
            if (null != command)
            {
                //DashFire.LogSystem.Debug("CreateCommand, type:{0} command:{1}", type, command.GetType().Name);
            }
            return command;
        }

        private IStoryCommandFactory GetFactory(string type)
        {
            IStoryCommandFactory factory;
            m_StoryCommandFactories.TryGetValue(type, out factory);
            return factory;
        }

        private StoryCommandManager()
        {
            //注册通用命令
            RegisterCommandFactory("=", new StoryCommandFactoryHelper<CommonCommands.AssignCommand>());
            RegisterCommandFactory("assign", new StoryCommandFactoryHelper<CommonCommands.AssignCommand>());
            RegisterCommandFactory("inc", new StoryCommandFactoryHelper<CommonCommands.IncCommand>());
            RegisterCommandFactory("dec", new StoryCommandFactoryHelper<CommonCommands.DecCommand>());
            RegisterCommandFactory("propset", new StoryCommandFactoryHelper<CommonCommands.PropSetCommand>());
            RegisterCommandFactory("foreach", new StoryCommandFactoryHelper<CommonCommands.ForeachCommand>());
            RegisterCommandFactory("looplist", new StoryCommandFactoryHelper<CommonCommands.LoopListCommand>());
            RegisterCommandFactory("loop", new StoryCommandFactoryHelper<CommonCommands.LoopCommand>());
            RegisterCommandFactory("wait", new StoryCommandFactoryHelper<CommonCommands.SleepCommand>());
            RegisterCommandFactory("sleep", new StoryCommandFactoryHelper<CommonCommands.SleepCommand>());
            RegisterCommandFactory("terminate", new StoryCommandFactoryHelper<CommonCommands.TerminateCommand>());
            RegisterCommandFactory("localmessage", new StoryCommandFactoryHelper<CommonCommands.LocalMessageCommand>());
            RegisterCommandFactory("clearmessage", new StoryCommandFactoryHelper<CommonCommands.ClearMessageCommand>());
            RegisterCommandFactory("while", new StoryCommandFactoryHelper<CommonCommands.WhileCommand>());
            RegisterCommandFactory("if", new StoryCommandFactoryHelper<CommonCommands.IfElseCommand>());
            RegisterCommandFactory("log", new StoryCommandFactoryHelper<CommonCommands.LogCommand>());
            RegisterCommandFactory("listset", new StoryCommandFactoryHelper<CommonCommands.ListSetCommand>());

            //注册通用值与内部函数
            //object
            StoryValueManager.Instance.RegisterValueHandler("propget", new StoryValueFactoryHelper<CommonValues.PropGetValue>());
            StoryValueManager.Instance.RegisterValueHandler("rndint", new StoryValueFactoryHelper<CommonValues.RandomIntValue>());
            StoryValueManager.Instance.RegisterValueHandler("rndfloat", new StoryValueFactoryHelper<CommonValues.RandomFloatValue>());
            StoryValueManager.Instance.RegisterValueHandler("vector2", new StoryValueFactoryHelper<CommonValues.Vector2Value>());
            StoryValueManager.Instance.RegisterValueHandler("vector3", new StoryValueFactoryHelper<CommonValues.Vector3Value>());
            StoryValueManager.Instance.RegisterValueHandler("vector4", new StoryValueFactoryHelper<CommonValues.Vector4Value>());
            StoryValueManager.Instance.RegisterValueHandler("quaternion", new StoryValueFactoryHelper<CommonValues.QuaternionValue>());
            StoryValueManager.Instance.RegisterValueHandler("eular", new StoryValueFactoryHelper<CommonValues.EularValue>());
            StoryValueManager.Instance.RegisterValueHandler("stringlist", new StoryValueFactoryHelper<CommonValues.StringListValue>());
            StoryValueManager.Instance.RegisterValueHandler("intlist", new StoryValueFactoryHelper<CommonValues.IntListValue>());
            StoryValueManager.Instance.RegisterValueHandler("floatlist", new StoryValueFactoryHelper<CommonValues.FloatListValue>());
            StoryValueManager.Instance.RegisterValueHandler("vector2list", new StoryValueFactoryHelper<CommonValues.Vector2ListValue>());
            StoryValueManager.Instance.RegisterValueHandler("vector3list", new StoryValueFactoryHelper<CommonValues.Vector3ListValue>());
            StoryValueManager.Instance.RegisterValueHandler("list", new StoryValueFactoryHelper<CommonValues.ListValue>());
            StoryValueManager.Instance.RegisterValueHandler("rndfromlist", new StoryValueFactoryHelper<CommonValues.RandomFromListValue>());
            StoryValueManager.Instance.RegisterValueHandler("listget", new StoryValueFactoryHelper<CommonValues.ListGetValue>());
            StoryValueManager.Instance.RegisterValueHandler("listsize", new StoryValueFactoryHelper<CommonValues.ListSizeValue>());
            StoryValueManager.Instance.RegisterValueHandler("vector2dist", new StoryValueFactoryHelper<CommonValues.Vector2DistanceValue>());
            StoryValueManager.Instance.RegisterValueHandler("vector3dist", new StoryValueFactoryHelper<CommonValues.Vector3DistanceValue>());
            StoryValueManager.Instance.RegisterValueHandler("vector2to3", new StoryValueFactoryHelper<CommonValues.Vector2To3Value>());
            StoryValueManager.Instance.RegisterValueHandler("vector3to2", new StoryValueFactoryHelper<CommonValues.Vector3To2Value>());
            StoryValueManager.Instance.RegisterValueHandler("+", new StoryValueFactoryHelper<CommonValues.AddOperator>());
            StoryValueManager.Instance.RegisterValueHandler("-", new StoryValueFactoryHelper<CommonValues.SubOperator>());
            StoryValueManager.Instance.RegisterValueHandler("*", new StoryValueFactoryHelper<CommonValues.MulOperator>());
            StoryValueManager.Instance.RegisterValueHandler("/", new StoryValueFactoryHelper<CommonValues.DivOperator>());
            StoryValueManager.Instance.RegisterValueHandler("%", new StoryValueFactoryHelper<CommonValues.ModOperator>());
            StoryValueManager.Instance.RegisterValueHandler("abs", new StoryValueFactoryHelper<CommonValues.AbsOperator>());
            StoryValueManager.Instance.RegisterValueHandler("floor", new StoryValueFactoryHelper<CommonValues.FloorOperator>());
            StoryValueManager.Instance.RegisterValueHandler("ceiling", new StoryValueFactoryHelper<CommonValues.CeilingOperator>());
            StoryValueManager.Instance.RegisterValueHandler("round", new StoryValueFactoryHelper<CommonValues.RoundOperator>());
            StoryValueManager.Instance.RegisterValueHandler("pow", new StoryValueFactoryHelper<CommonValues.PowOperator>());
            StoryValueManager.Instance.RegisterValueHandler("log", new StoryValueFactoryHelper<CommonValues.LogOperator>());
            StoryValueManager.Instance.RegisterValueHandler("sqrt", new StoryValueFactoryHelper<CommonValues.SqrtOperator>());
            StoryValueManager.Instance.RegisterValueHandler("sin", new StoryValueFactoryHelper<CommonValues.SinOperator>());
            StoryValueManager.Instance.RegisterValueHandler("cos", new StoryValueFactoryHelper<CommonValues.CosOperator>());
            StoryValueManager.Instance.RegisterValueHandler("sinh", new StoryValueFactoryHelper<CommonValues.SinhOperator>());
            StoryValueManager.Instance.RegisterValueHandler("cosh", new StoryValueFactoryHelper<CommonValues.CoshOperator>());
            StoryValueManager.Instance.RegisterValueHandler("min", new StoryValueFactoryHelper<CommonValues.MinOperator>());
            StoryValueManager.Instance.RegisterValueHandler("max", new StoryValueFactoryHelper<CommonValues.MaxOperator>());
            StoryValueManager.Instance.RegisterValueHandler(">", new StoryValueFactoryHelper<CommonValues.GreaterThanOperator>());
            StoryValueManager.Instance.RegisterValueHandler(">=", new StoryValueFactoryHelper<CommonValues.GreaterEqualThanOperator>());
            StoryValueManager.Instance.RegisterValueHandler("==", new StoryValueFactoryHelper<CommonValues.EqualOperator>());
            StoryValueManager.Instance.RegisterValueHandler("!=", new StoryValueFactoryHelper<CommonValues.NotEqualOperator>());
            StoryValueManager.Instance.RegisterValueHandler("<", new StoryValueFactoryHelper<CommonValues.LessThanOperator>());
            StoryValueManager.Instance.RegisterValueHandler("<=", new StoryValueFactoryHelper<CommonValues.LessEqualThanOperator>());
            StoryValueManager.Instance.RegisterValueHandler("&&", new StoryValueFactoryHelper<CommonValues.AndOperator>());
            StoryValueManager.Instance.RegisterValueHandler("||", new StoryValueFactoryHelper<CommonValues.OrOperator>());
            StoryValueManager.Instance.RegisterValueHandler("!", new StoryValueFactoryHelper<CommonValues.NotOperator>());
            StoryValueManager.Instance.RegisterValueHandler("format", new StoryValueFactoryHelper<CommonValues.FormatValue>());
            StoryValueManager.Instance.RegisterValueHandler("substring", new StoryValueFactoryHelper<CommonValues.SubstringValue>());
            StoryValueManager.Instance.RegisterValueHandler("time", new StoryValueFactoryHelper<CommonValues.TimeValue>());
            StoryValueManager.Instance.RegisterValueHandler("isnull", new StoryValueFactoryHelper<CommonValues.IsNullOperator>());

        }

        private Dictionary<string, IStoryCommandFactory> m_StoryCommandFactories = new Dictionary<string, IStoryCommandFactory>();

        public static StoryCommandManager Instance
        {
            get { return s_Instance; }
        }
        private static StoryCommandManager s_Instance = new StoryCommandManager();
    }
}
