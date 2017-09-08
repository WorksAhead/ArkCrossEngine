using System;
using System.Xml;

namespace DashFire
{
    internal class XmlParseException : ApplicationException
    {
        private XmlNode node_ = null;
        private string field_;

        internal XmlParseException()
        {
        }

        internal XmlParseException(string message) : base(message)
        {
        }

        internal XmlParseException(string msg, XmlNode node, string field) : base(msg)
        {
            node_ = node;
            field_ = field;
        }

        public override string Message
        {
            get
            {
                string msg = "\nXmlParseException: " + base.Message;
                if (node_ != null)
                {
                    msg += "\nXmlParseException: \nparse " + field_ +
                      " from xml error xml:" + node_.OuterXml;
                }
                msg += "\n";
                return msg;
            }
        }
    }
}
