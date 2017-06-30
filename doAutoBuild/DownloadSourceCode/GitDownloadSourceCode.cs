using doAutoBuild.Build;
using doAutoBuild.Log;
using doAutoBuild.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.DownloadSourceCode
{
    class GitDownloadSourceCode : IDownloadSourceCode
    {
        public int DownloadSourceCode(LogEngin _logEngin, SourceCodeBean _sourceCodeBean, BuildTaskBean _buildTaskBean)
        {

            string branch = _buildTaskBean.BranchName;
            string url = _sourceCodeBean.Url;
            string dest = _sourceCodeBean.DestPath;

            string command = "git clone ";

            if (branch != null && branch.Length > 0)
                command = command + "-b " + branch + " ";
            command = command + "--progress -v " + url + " " + dest;

            int _code = CMDUtils.Execute(_logEngin, command);

            return _code;

        }
    }
}
