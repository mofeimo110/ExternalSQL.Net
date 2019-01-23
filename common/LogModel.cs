using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSQL.Net.common
{
    public class LogModel
    {
        public LogType LogType { get; set; }
        public string LogText { get; set; }
        public DateTime WriteTime { get; set; }
    }
}
