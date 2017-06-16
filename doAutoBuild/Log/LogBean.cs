using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.Log
{
    class LogBean
    {
        private long _time;
        private string _level = "info"; // info,error,debug,h
        private string _message;

        public long Time { get => _time; set => _time = value; }
        public string Level { get => _level; set => _level = value; }
        public string Message { get => _message; set => _message = value; }


        public LogBean(Exception ex) : this(ex.ToString())
        {
            this._level = "error";
        }

        public LogBean(string message, string level) : this(message)
        {
            this._level = level;
        }

        public LogBean(string message)
        {
            this._time = DateTime.Now.ToFileTime();
            this._message = message;
            this._level = "info";
        }


        public string ToHtml()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<tr>");
            string color = "#000000";
            if (_level.Equals("error"))
            {
                color = "#FF0000";
            }
            else if (_level.Equals("debug"))
            {
                color = "#2E8B57";
            }
            else if (_level.Equals("h"))
            {
                color = "#0000FF";
            }

            sb.Append("<td title=\"时间\"><font color=\"" + color + "\">" + DateTime.FromFileTime(_time).ToLocalTime() + "</font></td>");
            sb.Append("<td title=\"级别\"><font color=\"" + color + "\">" + _level + "</td>");
            sb.Append("<td title=\"内容\"><font color=\"" + color + "\"><pre>" + _message + "</pre></font></td>");
            sb.Append("</tr>");
            return sb.ToString();
        }
    }
}
