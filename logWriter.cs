using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ExternalSQL.Net.common;

namespace ExternalSQL.Net
{
    public class LogWriter
    {
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="logType">枚举：日志类型</param>
        /// <param name="logTxt">写入内容</param>
        public static void WriteLog(LogType logType, string logTxt)
        {
            string dir = System.Threading.Thread.GetDomain().BaseDirectory + "logs\\" + logType;
            if (!Directory.Exists(dir))
            {
                //不存在时创建路径和文件
                Directory.CreateDirectory(dir);
            }
            //todo 删除一周以上的日志

            //写入日志
            string filePath = dir + "\\" + GetLogName(dir);
            var fm = !File.Exists(filePath) ? FileMode.Create : FileMode.Append;
            FileStream fs = new FileStream(filePath, fm);
            StreamWriter wr = null;
            wr = new StreamWriter(fs);
            string logStr = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms") + "]:";
            wr.WriteLine(logStr + logTxt);
            wr.Close();
        }
        /// <summary>
        /// 创建一个合适的日志名称
        /// </summary>
        public static string GetLogName(string path)
        {
            string today = DateTime.Now.ToString("yyyyMMdd");
            DirectoryInfo dir = new DirectoryInfo(path);
            int maxIndex = 0;
            //遍历文件夹下所有文件
            foreach (FileInfo fi in dir.GetFiles())
            {
                if (fi.Name.IndexOf(today, StringComparison.Ordinal) > -1)
                {
                    string fiIndex = fi.Name.Replace(today + "-", "").Replace(".txt", "");
                    if (int.Parse(fiIndex) > maxIndex)
                    {
                        //寻找当前日期最大的编号
                        maxIndex = int.Parse(fiIndex);
                    }
                }
            }
            if (File.Exists(path + "\\" + today + "-" + maxIndex + ".txt"))
            {
                FileInfo fi = new FileInfo(path + "\\" + today + "-" + maxIndex + ".txt");
                //如果文件已经大于10M，需要创建新的文件
                if (fi.Length > 10 * 1024 * 1024)
                {
                    maxIndex++;
                }
            }
            return today + "-" + maxIndex + ".txt";
        }
        /// <summary>
        /// 写入错误日志
        /// </summary>
        /// <param name="logTxt">日志内容</param>
        public static void ErrorLog(string logTxt)
        {
            WriteLog(LogType.Error, logTxt);
        }
    }
}
