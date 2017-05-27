using System;

namespace ArkCrossEngine
{
    public class CodeNotImplException : Exception
    {
        public CodeNotImplException()
            : base("Code not impl.")
        {

        }

        public CodeNotImplException(string message)
            : base(message)
        {

        }

        public CodeNotImplException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}