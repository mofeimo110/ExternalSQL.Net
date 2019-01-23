using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Linq;

namespace ExternalSQL.Net
{
    public class SqlServerHelper
    {
        /// <summary>
        /// 检查select查询，xml节点中只存在select逻辑时调用
        /// </summary>
        /// <param name="pathName">准备使用的路径在config中配置的key</param>
        /// <param name="docName">加载的xml文件名，无后缀</param>
        /// <param name="sqlId">节点id</param>
        /// <param name="paras">传入的参数键值对</param>
        /// <param name="conStr">数据库连接字符串</param>
        /// <returns></returns>
        public static DataTable SimpleQuery(string pathName, string docName, string sqlId, object paras, string conStr)
        {
            //todo 建库，整合语句，测试语句，日志写入
            SqlConnection sqlCnt = new SqlConnection(conStr);
            sqlCnt.Open();
            try
            {
                XmlDocument xml = Analysis.GetXmLdoc("sqlserver", "sql1");
                XmlNodeList nodelist = Analysis.GetNodesByXml(xml);
                SqlCommand cmd = Analysis.GetCommand(sqlId, nodelist, paras, pathName, docName, sqlCnt);
                SqlDataReader dr = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dr);
                sqlCnt.Close();
                return dt;
            }
            catch (Exception ex)
            {
                sqlCnt.Close();
                return null;
            }

        }
        /// <summary>
        /// 根据name获取数据库连接字符串
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public static string GetConnectionString(string connectionName)
        {
            return ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
        }
    }
}
