/**
 * @file XmlUtil.cs
 * @brief Xml辅助接口
 *
 * @author lixiaojiang
 * @version 0.0.1
 * @date 2012-12-12
 */

using System;
using System.Collections.Generic;
using System.Xml;
//using System.Diagnostics;

namespace DashFire
{

  /**
   * @brief Xml辅助接口
   */
  public class XmlUtil
  {
    /**
     * @brief 从Xml节点中读取字符串
     *
     * @param node xml节点
     * @param nodeName 节点名字
     * @param defualtVal 默认值
     * @param isMust 是否强制不能为空
     *
     * @return 
     */
    public static string ExtractString(XmlNode node, string nodeName, string defualtVal, bool isMust)
    {
      string result = defualtVal;

      if (node == null || !node.HasChildNodes || node.SelectSingleNode(nodeName) == null)
      {
        if (isMust)
        {
          string errorInfo = string.Format("ExtactString Error node:{0} nodeName:{1}", node.Name, nodeName);
          LogSystem.Assert(false, errorInfo);
        }

        return result;
      }

      XmlNode childNode = node.SelectSingleNode(nodeName);

      string nodeText = childNode.InnerText;
      if (Helper.StringIsNullOrEmpty(nodeText))
      {
        if (isMust)
        {
          string errorInfo = string.Format("ExtactString Error node:{0} nodeName:{1}", node.Name, nodeName);
          LogSystem.Assert(false, errorInfo);
        }
      }
      else
      {
        result = nodeText;
      }

      return result;
    }

    /**
     * @brief 从Xml节点中读取字符串数组
     *
     * @param node xml节点
     * @param nodeName 节点名字
     * @param defualtVal 默认值
     * @param isMust 是否强制不能为空
     *
     * @return 
     */
    public static List<string> ExtractStringList(XmlNode node, string nodeName, string defualtVal, bool isMust)
    {
      List<string> result = new List<string>();

      if (node == null || !node.HasChildNodes)
      {
        if (isMust)
        {
          string errorInfo = string.Format("ExtractStringList Error node:{0} nodeName:{1}", node.Name, nodeName);
          LogSystem.Assert(false, errorInfo);
        }

        return result;
      }

      XmlNode childNode = node.SelectSingleNode(nodeName);

      string nodeText = childNode.InnerText;
      if (Helper.StringIsNullOrEmpty(nodeText))
      {
        if (isMust)
        {
          string errorInfo = string.Format("ExtractStringList Error node:{0} nodeName:{1}", node.Name, nodeName);
          LogSystem.Assert(false, errorInfo);
        }
      }
      else
      {
        result = Converter.ConvertStringList(nodeText);
      }

      return result;
    }

    /**
     * @brief 从Xml节点中读取布尔值
     *
     * @param node xml节点
     * @param nodeName 节点名字
     * @param defualtVal 默认值
     * @param isMust 是否强制不能为空
     *
     * @return 
     */
    public static bool ExtractBool (XmlNode node, string nodeName, bool defualtVal, bool isMust)
    {
      bool result = defualtVal;

      if (node == null || !node.HasChildNodes || node.SelectSingleNode(nodeName) == null)
      {
        if (isMust)
        {
          string errorInfo = string.Format("ExtractBool Error node:{0} nodeName:{1}", node.Name, nodeName);
          LogSystem.Assert(false, errorInfo);
        }

        return result;
      }

      XmlNode childNode = node.SelectSingleNode(nodeName);

      string nodeText = childNode.InnerText;
      if (Helper.StringIsNullOrEmpty(nodeText))
      {
        if (isMust)
        {
          string errorInfo = string.Format("ExtractBool Error node:{0} nodeName:{1}", node.Name, nodeName);
          LogSystem.Assert(false, errorInfo);
        }
      }
      else
      {
        if (nodeText.Trim().ToLower() == "true" || nodeText.Trim().ToLower() == "1")
        {
          result = true;
        }

        if (nodeText.Trim().ToLower() == "false" || nodeText.Trim().ToLower() == "0")
        {
          result = false;
        }
      }

      return result;
    }

    /**
     * @brief 从Xml节点中读取数值类型，使用时，必须在函数中指明数值类型
     *          如: int id = ExtractNumeric<int>(xmlNode, "Id", -1, true);
     *
     * @param node xml节点
     * @param nodeName 节点名字
     * @param defualtVal 默认值
     * @param isMust 是否强制不能为空
     *
     * @return 
     */
    public static T ExtractNumeric<T>(XmlNode node, string nodeName, T defualtVal, bool isMust)
    {
      T result = defualtVal;

      if (node == null || !node.HasChildNodes || node.SelectSingleNode(nodeName) == null)
      {
        if (isMust)
        {
          string errorInfo = string.Format("ExtractNumeric Error node:{0} nodeName:{1}", node.Name, nodeName);
          LogSystem.Assert(false, errorInfo);
        }

        return result;
      }

      XmlNode childNode = node.SelectSingleNode(nodeName);

      string nodeText = childNode.InnerText;
      if (Helper.StringIsNullOrEmpty(nodeText))
      {
        if (isMust)
        {
          string errorInfo = string.Format("ExtractNumeric Error node:{0} nodeName:{1}", node.Name, nodeName);
          LogSystem.Assert(false, errorInfo);
        }
      }
      else
      {
        try
        {
          result = (T)Convert.ChangeType(nodeText, typeof(T));
        }
        catch (System.Exception ex)
        {
          string info = string.Format("ExtractNumeric Error node:{0} nodeName:{1} ex:{2} stacktrace:{3}", 
            node.Name, nodeName, ex.Message, ex.StackTrace);
          LogSystem.Debug(info);
        }
      }

      return result;
    }

    /**
     * @brief 从Xml节点中读取数值类型，使用时，必须在函数中指明数值类型
     *          如: int id = ExtractNumeric<int>(xmlNode, "Id", -1, true);
     *
     * @param node xml节点
     * @param nodeName 节点名字
     * @param defualtVal 默认值
     * @param isMust 是否强制不能为空
     *
     * @return 
     */
    public static List<T> ExtractNumericList<T>(XmlNode node, string nodeName, T defualtVal, bool isMust)
    {
      List<T> result = new List<T>();

      if (node == null || !node.HasChildNodes || node.SelectSingleNode(nodeName) == null)
      {
        if (isMust)
        {
          string errorInfo = string.Format("ExtractNumericList Error node:{0} nodeName:{1}", node.Name, nodeName);
          LogSystem.Assert(false, errorInfo);
        }

        return result;
      }

      XmlNode childNode = node.SelectSingleNode(nodeName);

      string nodeText = childNode.InnerText;
      if (Helper.StringIsNullOrEmpty(nodeText))
      {
        if (isMust)
        {
          string errorInfo = string.Format("ExtractNumericList Error node:{0} nodeName:{1}", node.Name, nodeName);
          LogSystem.Assert(false, errorInfo);
        }
      }
      else
      {
        result = Converter.ConvertNumericList<T>(nodeText);
      }

      return result;
    }

    /**
     * @brief 从Xml节点中抽取所有以prefix为前缀的节点
     *
     * @param node xml节点
     * @param prefix 前缀字符串
     *
     * @return 
     */
    public static List<XmlNode> ExtractNodeByPrefix(XmlNode node, string prefix)
    {
      List<XmlNode> result = new List<XmlNode>();

      if (node == null || !node.HasChildNodes)
      {
        return result;
      }

      XmlNodeList childs = node.ChildNodes;
      foreach (XmlNode child in childs)
      {
        if (child.Name.StartsWith(prefix))
        {
          result.Add(child);
        }
      }

      return result;
    }
  }
}
