using System;
using System.IO;

namespace ExternalSQL.Net
{
    public class logWriter
    {
        public static void WriteLog(string log, object paras)
        {
            string dir = System.Threading.Thread.GetDomain().BaseDirectory + "log";
            if (!Directory.Exists(dir))
            {
                //不存在时创建路径和文件
                Directory.CreateDirectory(dir);
            }

            FileMode fm = new FileMode();
            if (!File.Exists(dir + "/log4net.txt"))
            {
                fm = FileMode.Create;
            }
            else
            {
                fm = FileMode.Append;
            }
            FileStream fs = new FileStream(dir + "/log4net.txt", fm);
            StreamWriter wr = null;
            wr = new StreamWriter(fs);
            string defult = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms") + "]:";

            wr.WriteLine(defult + log + "parameters=" + paras);
            wr.Close();
        }
    }
}
