using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using ExternalSQL.Net.common;

namespace ExternalSQL.Net
{
    public class LogWriter
    {
        /// <summary>
        /// 写入日志
        /// </summary>
        public static void WriteLog(LogModel lm)
        {
            foreach (LogType type in Enum.GetValues(typeof(LogType)))
            {
                if (!Directory.Exists(System.Threading.Thread.GetDomain().BaseDirectory + "logs\\" + type))
                {
                    //不存在时创建路径和文件
                    Directory.CreateDirectory(System.Threading.Thread.GetDomain().BaseDirectory + "logs\\" + type);
                }
            }
            string dir = System.Threading.Thread.GetDomain().BaseDirectory + "logs\\" + lm.LogType;
            //删除日志
            DeletePassLogs();
            //写入日志
            string filePath = dir + "\\" + GetLogName(dir);
            var fm = !File.Exists(filePath) ? FileMode.Create : FileMode.Append;
            FileStream fs = new FileStream(filePath, fm);
            var wr = new StreamWriter(fs);
            string logStr = "[" + lm.WriteTime.ToString("yyyy-MM-dd HH:mm:ss:ms") + "]:";

            wr.WriteLine((logStr + lm.LogText).Replace("\n", "\n         "));
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
            WriteLog(new LogModel { LogType = LogType.Error, LogText = logTxt, WriteTime = DateTime.Now });
        }
        /// <summary>
        /// 写入常规日志
        /// </summary>
        /// <param name="logTxt">日志内容</param>
        public static void InfoLog(string logTxt)
        {
            WriteLog(new LogModel { LogType = LogType.Info, LogText = logTxt, WriteTime = DateTime.Now });
        }
        /// <summary>
        /// 写入警告日志
        /// </summary>
        /// <param name="logTxt">日志内容</param>
        public static void WarningLog(string logTxt)
        {
            WriteLog(new LogModel { LogType = LogType.Warning, LogText = logTxt, WriteTime = DateTime.Now });
        }
        /// <summary>
        /// 删除超期日志
        /// </summary>
        /// <param name="days">表示几天之前的日志需要删除</param>
        public static void DeletePassLogs(int days = 7)
        {
            string path = System.Threading.Thread.GetDomain().BaseDirectory + "logs\\";
            //获取文件夹中的文件集合
            int toDelete = int.Parse(DateTime.Now.AddDays(7 * -1).ToString("yyyyMMdd"));
            foreach (LogType type in Enum.GetValues(typeof(LogType)))
            {
                FileInfo[] files = new DirectoryInfo(path + type).GetFiles();
                foreach (var file in files)
                {
                    DoDelete(file, toDelete);
                }
            }
        }
        /// <summary>
        /// 执行删除方法，有异常时释放
        /// </summary>
        /// <param name="file">文件对象</param>
        /// <param name="toDelete">可删除时间线</param>
        public static void DoDelete(FileInfo file, int toDelete)
        {
            //全字匹配20190101-0.txt格式的文件名
            MatchCollection matches = Regex.Matches(file.Name, @"^(\d{8})-(\d{1,10})\.txt$");
            if (matches.Count > 0)
            {
                int logDate = int.Parse(file.Name.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                if (logDate <= toDelete)
                {
                    file.Delete();
                }
            }
        }
    }
}
