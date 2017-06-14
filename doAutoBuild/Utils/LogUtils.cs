using doAutoBuild.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.Utils
{
    class LogUtils
    {
        static Hashtable _logs = new Hashtable();

        public static void Info(ILog log, string message)
        {
            if (log != null)
                log.Info(message);
            else
                Console.WriteLine(DateTime.Now.ToLocalTime() + " info : " + message);
        }

        public static void Debug(ILog log, string message)
        {
            if (log != null)
                log.Debug(message);
            else
                Console.WriteLine(DateTime.Now.ToLocalTime() + " debug : " + message);
        }

        public static void Error(ILog log, Exception ex)
        {
            if (log != null)
                log.Error(ex);
            else
                Console.WriteLine(DateTime.Now.ToLocalTime() + " error : \n" + ex.StackTrace);
        }

    }
}
