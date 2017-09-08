using System;
using System.Xml;

namespace DashFire
{
    internal class XmlAssist
    {
        internal static bool GetString(XmlNode parent,
            string name,
            ref string out_value)
        {
            if (parent == null)
            {
                throw new XmlParseException("parent node is null when parse "
                    + name);
            }
            XmlNode node = parent.SelectSingleNode(name);
            if (node == null)
            {
                throw new XmlParseException("can't find node "
                    + name + " from parent node!",
                    parent, name);
            }
            out_value = node.InnerText;
            return true;
        }

        internal static bool GetUint(XmlNode parent,
            string name,
            ref uint out_value)
        {
            if (parent == null)
            {
                throw new XmlParseException("parent node is null when parse "
                    + name);
            }
            XmlNode node = parent.SelectSingleNode(name);
            if (node == null)
            {
                throw new XmlParseException("can't find node "
                    + name + " from parent node!",
                    parent, name);
            }
            try
            {
                out_value = Convert.ToUInt32(node.InnerText);
            }
            catch (Exception ex)
            {
                throw new XmlParseException(ex.Message, node, name);
            }
            return true;
        }

        //没有配置时默认设置为false
        internal static bool GetBool(XmlNode parent,
            string name,
            ref bool out_value)
        {
            if (parent == null)
            {
                throw new XmlParseException("parent node is null when parse "
                    + name);
            }
            XmlNode node = parent.SelectSingleNode(name);
            if (node == null)
            {
                throw new XmlParseException("can't find node "
                    + name + " from parent node!",
                    parent, name);
            }
            string value = node.InnerText;
            if (value == "true")
            {
                out_value = true;
            }
            else
            {
                out_value = false;
            }
            return true;
        }

        internal static bool GetDouble(XmlNode parent,
            string name,
            ref float out_value)
        {
            if (parent == null)
            {
                throw new XmlParseException("parent node is null when parse "
                    + name);
            }
            XmlNode node = parent.SelectSingleNode(name);
            if (node == null)
            {
                throw new XmlParseException("can't find node "
                    + name + " from parent node!",
                    parent, name);
            }
            try
            {
                out_value = (float)Convert.ToDouble(node.InnerText);
            }
            catch (Exception ex)
            {
                throw new XmlParseException(ex.Message,
                    node, name);
            }
            return true;
        }

        internal static bool GetPosition(XmlNode parent,
            string name,
            ref float x,
            ref float y,
            ref float z)
        {
            if (parent == null)
            {
                throw new XmlParseException("parent node is null when parse "
                    + name);
            }
            XmlNode node = parent.SelectSingleNode(name);
            if (node == null)
            {
                throw new XmlParseException("can't find node "
                    + name + " from parent node!",
                    parent, name);
            }
            string[] values = node.InnerText.Split(',');
            if (values.Length > 0)
            {
                x = (float)Convert.ToDouble(values[0]);
            }
            if (values.Length > 1)
            {
                y = (float)Convert.ToDouble(values[1]);
            }
            if (values.Length > 2)
            {
                z = (float)Convert.ToDouble(values[2]);
            }
            return true;
        }

    }
}
