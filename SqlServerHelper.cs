using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace ExternalSQL.Net
{
    public class SqlServerHelper
    {
        public static SqlCommand GetSqlText(string pathName, string docName, string sqlId, object paras, SqlConnection con)
        {
            XmlDocument xml = Analysis.GetXmLdoc("sqlserver", "sql1");
            XmlNodeList sqllist = Analysis.GetNodesByXml(xml);
            //string sql = Analysis.GetSqlByNode(sqlId, sqllist,paras,pathName,docName);
            return Analysis.GetCommandByNode(sqlId, sqllist, paras, pathName, docName, con);
        }
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
                XmlNodeList sqllist = Analysis.GetNodesByXml(xml);
                SqlCommand cmd = Analysis.GetCommandByNode(sqlId, sqllist, paras, pathName, docName, sqlCnt);
                SqlDataReader dr = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dr);
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
