using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.Log
{
    interface ILog
    {
        void Info(string message);

        void Debug(string message);

        void Error(Exception ex);

        void H(string message);

    }
}
