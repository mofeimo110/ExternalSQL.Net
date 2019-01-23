using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Xml;

namespace ExternalSQL.Net
{
    public static class Analysis
    {
        /// <summary>
        /// 通过基本配置获取XML对象
        /// </summary>
        /// <param name="pathName">准备使用的路径在config中配置的key</param>
        /// <param name="docName">加载的xml文件名，无后缀</param>
        /// <returns></returns>
        public static XmlDocument GetXmLdoc(string pathName, string docName)
        {
            string path = "";
            NameValueCollection config = (NameValueCollection)ConfigurationSettings.GetConfig("sqlPaths");
            if (config == null)
            {
                throw new Exception("未在config中找到sqlPaths");
            }
            foreach (var key in config.AllKeys)
            {
                if (pathName == key)
                {
                    path = config[key] + "/" + docName + ".xml";
                    break;
                }
            }
            XmlDocument xml = new XmlDocument();
            xml.Load(System.Threading.Thread.GetDomain().BaseDirectory + path);
            return xml;
        }
        /// <summary>
        /// 传入XmlDocument对象获取文件中所有XmlNode
        /// </summary>
        /// <param name="xml">xml对象</param>
        /// <returns></returns>
        public static XmlNodeList GetNodesByXml(XmlDocument xml)
        {
            XmlNode root = xml.SelectSingleNode("/root");
            XmlNodeList sqllist = null;
            if (root != null && root.HasChildNodes)
            {
                sqllist = root.SelectNodes("sql");
                List<string> ids = new List<string>();
                if (sqllist != null)
                {
                    foreach (XmlNode item in sqllist)
                    {
                        if (item.Attributes != null)
                        {
                            ids.Add(item.Attributes["id"].Value);
                        }
                    }

                    //如果一个文件中出现重复id则返回空
                    if (ids.Count != ids.Distinct().Count())
                    {
                        throw new Exception("xml文件中sql节点存在重复id");
                    }
                }
                else
                {
                    throw new Exception("xml文件中未找到sql节点");
                }
            }
            return sqllist;
        }

        /// <summary>
        /// 根据返回参数化的SQLcommand
        /// </summary>
        /// <param name="id">sql语句的ID</param>
        /// <param name="nodelist">xml文件中所有的XmlNode</param>
        /// <param name="paras">调用时传入的参数键值对</param>
        /// <param name="pathName">准备使用的路径在config中配置的key</param>
        /// <param name="docName">加载的xml文件名，无后缀</param>
        /// <param name="paraTemp">需要添加进command的数据对象</param>
        /// <returns></returns>
        public static string GetSqlByNode(string id, XmlNodeList nodelist, object paras, string pathName, string docName, out List<string> paraTemp)
        {
            string sql = GetSqlByNode(id, nodelist, paras, pathName, docName);
            XmlDocument xml = new XmlDocument();
            xml.LoadXml("<sql>" + sql + "</sql>");
            XmlNodeList parasBySql = xml.SelectSingleNode("sql").SelectNodes("parameter");
            paraTemp = new List<string>();
            foreach (XmlNode item in parasBySql)
            {
                paraTemp.Add(item.InnerText);
                sql = sql.Replace("<parameter>" + item.InnerText + "</parameter>", "");
            }
            return sql;
        }

        public static SqlCommand GetCommand(string sqlId, XmlNodeList nodelist, object paras, string pathName, string docName, SqlConnection con)
        {

            List<string> paraTemp;
            string sql = GetSqlByNode(sqlId, nodelist, paras, pathName, docName, out paraTemp);
            SqlCommand cmd = new SqlCommand(sql, con);
            foreach (var item in paraTemp)
            {
                if (sql.IndexOf("@" + item, StringComparison.Ordinal) > -1)
                {
                    cmd.Parameters.AddWithValue("@" + item, paras.GetAttrValue(item));
                }
            }
            return cmd;
        }
        /// <summary>
        /// 获取匿名对象的某个属性的值
        /// </summary>
        /// <param name="paras"></param>
        /// <param name="attrName">属性Name</param>
        /// <returns></returns>
        public static object GetAttrValue(this object paras, string attrName)
        {
            if (paras.GetType().GetProperty(attrName) != null)
            {
                // ReSharper disable once PossibleNullReferenceException
                return paras.GetType().GetProperty(attrName).GetValue(paras);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 返回指定的sql节点生成的sql语句
        /// </summary>
        /// <param name="id">节点id</param>
        /// <param name="nodelist">包含指定XmlNode的集合</param>
        /// <param name="paras">传入的参数键值对</param>
        /// <param name="pathName">准备使用的路径在config中配置的key</param>
        /// <param name="docName">加载的xml文件名，无后缀</param>
        /// <returns></returns>
        public static string GetSqlByNode(string id, XmlNodeList nodelist, object paras, string pathName, string docName)
        {
            string retSql = "";
            foreach (XmlNode item in nodelist)
            {
                if (item.Attributes != null && item.Attributes["id"].Value == id)
                {
                    retSql = item.InnerXml.AddSqlPara(item, paras).AddSqlInclude(item, paras, pathName, docName);
                }
            }
            return retSql;
        }

        /// <summary>
        /// 根据sql节点中的parameter整理语句
        /// </summary>
        /// <param name="sql">已生成的sql</param>
        /// <param name="node">指定的sql节点</param>
        /// <param name="paras">传入的参数键值对</param>
        /// <returns></returns>
        public static string AddSqlPara(this string sql, XmlNode node, object paras)
        {
            if (node.HasChildNodes)
            {
                List<string> sqlParas = new List<string>();
                foreach (XmlNode item in node.ChildNodes)
                {
                    if (item.Name == "parameter")
                    {
                        string para = item.InnerText;
                        if (para.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length > 1)
                        {
                            //说明一个parameter中包含了多个para参数
                            sql = item.MultipleParas(sql, sqlParas, paras);
                        }
                        else
                        {
                            sqlParas.Add(para);
                            foreach (System.Reflection.PropertyInfo p in paras.GetType().GetProperties())
                            {
                                if (para == p.Name)
                                {
                                    sqlParas.Remove(p.Name);
                                    sql = sql.Replace("$" + para + "$", p.GetValue(paras) + "");
                                    sql = sql.Replace("#" + para + "#", "'" + p.GetValue(paras) + "'");
                                }
                            }
                        }
                    }
                }
                foreach (var para in sqlParas)
                {
                    sql = sql.Replace("$" + para + "$", "");
                    sql = sql.Replace("#" + para + "#", "''");
                }
            }
            return sql;
        }

        /// <summary>
        /// 根据sql节点中的include整理语句
        /// </summary>
        /// <param name="sql">已生成的sql</param>
        /// <param name="node">指定的sql节点</param>
        /// <param name="paras">传入的参数键值对</param>
        /// <param name="pathName">准备使用的路径在config中配置的key</param>
        /// <param name="docName">加载的xml文件名，无后缀</param>
        /// <returns></returns>
        public static string AddSqlInclude(this string sql, XmlNode node, object paras, string pathName, string docName)
        {
            if (node.HasChildNodes)
            {
                foreach (XmlNode item in node.ChildNodes)
                {
                    if (item.Name == "include")
                    {
                        if (item.AttributesIsNull("pathName") && item.AttributesIsNull("docName"))
                        {
                            //默认寻找路径为当前文件
                        }
                        else if (item.AttributesIsNull("pathName") && !item.AttributesIsNull("docName"))
                        {
                            //路径默认为当前路径，文件指向文件更换
                            if (item.Attributes != null) docName = item.Attributes["docName"].Value;
                        }
                        else
                        {
                            //完全根据配置的path和docName查找文件
                            if (item.Attributes != null)
                            {
                                pathName = item.Attributes["pathName"].Value;
                                docName = item.Attributes["docName"].Value;
                            }
                        }
                        var xml = GetXmLdoc(pathName, docName);
                        if (item.Attributes != null)
                        {
                            string includeSql = GetSqlByNode(item.Attributes["target"].Value, GetNodesByXml(xml), paras, pathName, docName);
                            sql = sql.Replace(item.OuterXml, includeSql);
                        }
                    }
                }
            }

            return sql;
        }
        /// <summary>
        /// XmlNode对象的某个属性的值 是否存在或者是否不为空
        /// </summary>
        /// <param name="node">XmlNode</param>
        /// <param name="attrName">属性Name</param>
        /// <returns></returns>
        public static bool AttributesIsNull(this XmlNode node, string attrName)
        {
            return node.Attributes != null && (node.Attributes[attrName] == null || node.Attributes[attrName].Value == "");
        }
        /// <summary>
        /// parameter节点中包含了多了parameter时，分别解析
        /// </summary>
        /// <param name="node">XmlNode</param>
        /// <param name="sql">sql语句</param>
        /// <param name="sqlParas">当前语句所需要的parameter集合</param>
        /// <param name="paras">传入的参数键值对</param>
        /// <returns></returns>
        public static string MultipleParas(this XmlNode node, string sql, List<string> sqlParas, object paras)
        {
            string[] parameters = node.InnerText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string newpara = "";
            foreach (var item in parameters)
            {
                sqlParas.Add(item);
                newpara += "<parameter>" + item + "</parameter>\n";
                foreach (System.Reflection.PropertyInfo p in paras.GetType().GetProperties())
                {
                    if (item == p.Name)
                    {
                        sqlParas.Remove(p.Name);
                        sql = sql.Replace("$" + item + "$", p.GetValue(paras) + "");
                        sql = sql.Replace("#" + item + "#", "'" + p.GetValue(paras) + "'");
                    }
                }
            }

            return sql.Replace("<parameter>" + node.InnerText + "</parameter>", newpara);
        }
    }
}
