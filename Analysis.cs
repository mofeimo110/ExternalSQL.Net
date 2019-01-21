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
        public static XmlDocument GetXmLdoc(string pathName, string docName)
        {
            string path = "";
            NameValueCollection config = (NameValueCollection)ConfigurationSettings.GetConfig("sqlPaths");
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
        public static XmlNodeList GetNodesByXml(XmlDocument xml)
        {
            XmlNode root = xml.SelectSingleNode("/root");
            XmlNodeList sqllist = null;
            if (root != null && root.HasChildNodes)
            {
                sqllist = root.SelectNodes("sql");
                List<string> ids = new List<string>();
                foreach (XmlNode item in sqllist)
                {
                    ids.Add(item.Attributes["id"].Value);
                }
                //如果一个文件中出现重复id则返回空
                if (ids.Count != ids.Distinct().Count())
                {
                    sqllist = null;
                }
            }
            return sqllist;
        }

        public static SqlCommand GetCommandByNode(string id, XmlNodeList nodelist, object paras, string pathName, string docName, SqlConnection con)
        {
            string sql = GetSqlByNode(id, nodelist, paras, pathName, docName, con);
            XmlDocument xml = new XmlDocument();
            xml.LoadXml("<sql>" + sql + "</sql>");
            XmlNodeList parasBySql = xml.SelectSingleNode("sql").SelectNodes("parameter");
            List<string> paraTemp = new List<string>();
            foreach (XmlNode item in parasBySql)
            {
                paraTemp.Add(item.InnerText);
                sql = sql.Replace("<parameter>" + item.InnerText + "</parameter>", "");
            }
            logWriter.WriteLog(sql,paras);
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

        public static object GetAttrValue(this object paras, string attrName)
        {
            if (paras.GetType().GetProperty(attrName) != null)
            {
                return paras.GetType().GetProperty(attrName).GetValue(paras);
            }
            else
            {
                return null;
            }
        }
        public static string GetSqlByNode(string id, XmlNodeList nodelist, object paras, string pathName, string docName, SqlConnection con)
        {
            string retSql = "";
            foreach (XmlNode item in nodelist)
            {
                if (item.Attributes["id"].Value == id)
                {
                    retSql = item.InnerXml.AddSqlPara(item, paras, con).AddSqlInclude(item, paras, pathName, docName, con);
                }
            }
            return retSql;
        }

        public static string AddSqlPara(this string sql, XmlNode node, object paras, SqlConnection con)
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
                                    sql = sql.Replace("$" + para + "$", "'" + p.GetValue(paras) + "'");
                                }
                            }
                        }
                    }
                }
                foreach (var para in sqlParas)
                {
                    sql = sql.Replace("$" + para + "$", "''");
                }
            }
            return sql;
        }

        public static string AddSqlInclude(this string sql, XmlNode node, object paras, string pathName, string docName, SqlConnection con)
        {
            if (node.HasChildNodes)
            {
                foreach (XmlNode item in node.ChildNodes)
                {
                    if (item.Name == "include")
                    {
                        XmlDocument xml = new XmlDocument();
                        if (item.AttributesIsNull("pathName") && item.AttributesIsNull("docName"))
                        {
                            //默认寻找路径为当前文件
                        }
                        else if (item.AttributesIsNull("pathName") && !item.AttributesIsNull("docName"))
                        {
                            //路径默认为当前路径，文件指向文件更换
                            docName = item.Attributes["docName"].Value;
                        }
                        else
                        {
                            //完全根据配置的path和docName查找文件
                            pathName = item.Attributes["pathName"].Value;
                            docName = item.Attributes["docName"].Value;
                        }
                        xml = GetXmLdoc(pathName, docName);
                        string includeSql = GetSqlByNode(item.Attributes["target"].Value, GetNodesByXml(xml), paras, pathName, docName, con);
                        sql = sql.Replace(item.OuterXml, includeSql);
                    }
                }
            }

            return sql;
        }

        public static bool AttributesIsNull(this XmlNode node, string attrName)
        {
            return node.Attributes[attrName] == null || node.Attributes[attrName].Value == "";
        }

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
                        sql = sql.Replace("$" + item + "$", "'" + p.GetValue(paras) + "'");
                    }
                }
            }

            return sql.Replace("<parameter>" + node.InnerText + "</parameter>", newpara);
        }
    }
}
