using System.Data.SqlClient;
using System.Xml;

namespace ExternalSQL.Net
{
    public class SqlHelper
    {
        public static SqlCommand GetSqlText(string pathName,string docName,string sqlId,object paras,SqlConnection con)
        {
            XmlDocument xml = Analysis.GetXmLdoc("sqlserver", "sql1");
            XmlNodeList sqllist = Analysis.GetNodesByXml(xml);
            //string sql = Analysis.GetSqlByNode(sqlId, sqllist,paras,pathName,docName);
            return Analysis.GetCommandByNode(sqlId, sqllist, paras, pathName, docName, con);
        }
    }
}
