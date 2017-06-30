using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.Log
{
    class LogEngin : ILog
    {
        private ArrayList _logs = new ArrayList();
        private string _taskName;
        private string _taskId;
        
        private bool _isSuccess = true; //是否成功

        public LogEngin(string _taskName , string _taskId)
        {
            this._taskName = _taskName;
            this._taskId = _taskId;
        }

        public bool IsSuccess { get => _isSuccess; set => _isSuccess = value; }

        public void Debug(string message)
        {
            _logs.Add(new LogBean(message, "debug"));
        }

        public void Error(Exception ex)
        {
            _logs.Add(new LogBean(ex));
        }

        public void H(string message)
        {
            _logs.Add(new LogBean(message, "h"));
        }

        public void Info(string message)
        {
            _logs.Add(new LogBean(message));
        }


        public string ToHtml()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.Append("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
            sb.Append("<head>"+ this._taskName + "报告");
            sb.Append("<table cellspacing=\"0\" cellpadding=\"4\" border=\"1\" bordercolor=\"#224466\" width=\"100%\" style=\"font-family: arial,sans-serif; font-size: x-small;\">");
            sb.Append("<tr>");
            sb.Append("<th style=\"background: #808000; color: #FFFFFF; text-align: left;\">任务ID</th>");
            //sb.Append("<th style=\"background: #808000; color: #FFFFFF; text-align: left;\">操作系统</th>");
            //sb.Append("<th style=\"background: #808000; color: #FFFFFF; text-align: left;\">打包类型</th>");
            //sb.Append("<th style=\"background: #808000; color: #FFFFFF; text-align: left;\">开始时间</th>");
            //sb.Append("<th style=\"background: #808000; color: #FFFFFF; text-align: left;\">结束时间</th>");
            //sb.Append("<th style=\"background: #808000; color: #FFFFFF; text-align: left;\">打包时间</th>");
            sb.Append("<th style=\"background: #808000; color: #FFFFFF; text-align: left;\">打包结果</th>");
            //sb.Append("<th style=\"background: #808000; color: #FFFFFF; text-align: left;\">失败原因</th>");
            //sb.Append("<th style=\"background: #808000; color: #FFFFFF; text-align: left;\">建议</th>");
            sb.Append("</tr>");

            string color = "#000000";
            sb.Append("<tr>");
            sb.Append("<td title=\"任务ID\"><font color=\"" + color + "\">" + _taskId + "</td>");
            //sb.Append("<td title=\"操作系统\"><font color=\"" + color + "\">" + Factory.Instance().getEnv().getOSName()+ "</td>");
            //sb.Append("<td title=\"打包类型\"><font color=\"" + color + "\">" + Factory.Instance().getBuildType() + "</td>");
            //sb.Append("<td title=\"开始时间\"><font color=\"" + color + "\">" + TimeHelper.getSTime(startTime) + "</td>");
            //sb.Append("<td title=\"结束时间\"><font color=\"" + color + "\">" + TimeHelper.getSTime(endTime) + "</td>");
            //sb.Append("<td title=\"打包时间\"><font color=\"" + color + "\">" + cost() + "</td>");
            String result = "成功";
            if (!IsSuccess)
            {
                color = "#FF0000";
                result = "失败";
            }
            sb.Append("<td title=\"打包结果\"><font color=\"" + color + "\">" + result + "</td>");

            //         color = "#FF0000";
            //         String suggestion = "";
            //         if (fail instanceof ISuggestion)
            //suggestion = ((ISuggestion)fail).getSuggestion();
            //         sb.Append("<td title=\"失败原因\"><font color=\"" + color + "\"><pre>" + suggestion + "</pre></td>");

            //         sb.Append("<td title=\"建议\"><font color=\"" + color + "\"><pre>" + MiscHelper.getExceptionDetail(fail)
            //                 + "</pre></td>");

            sb.Append("</tr>");
            sb.Append("</table>");
            sb.Append("</head>");
            sb.Append("<body>");
            sb.Append("<br>");
            sb.Append("<table cellspacing=\"0\" cellpadding=\"4\" border=\"1\" bordercolor=\"#224466\" width=\"100%\" style=\"font-family: arial,sans-serif; font-size: x-small;\">");
            sb.Append("<tr>");
            sb.Append("<th style=\"background: #808000; color: #FFFFFF; text-align: left;\">时间</th>");
            sb.Append("<th style=\"background: #808000; color: #FFFFFF; text-align: left;\">级别</th>");
            sb.Append("<th style=\"background: #808000; color: #FFFFFF; text-align: left;\">内容</th>");
            sb.Append("</tr>");
            foreach (LogBean _log in _logs)
            {
                sb.Append(_log.ToHtml());
            }
            sb.Append("</table>");

            sb.Append("</body>");
            sb.Append("</html>");

            return sb.ToString();
        }
    }
}
