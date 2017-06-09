using doAutoBuild.Build;
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
        public void DownloadSourceCode(SourceCodeBean _sourceCodeBean, BuildTaskBean _buildTaskBean)
        {

            string branch = _buildTaskBean.BranchName;
            string url = _sourceCodeBean.Url;
            string dest = _sourceCodeBean.DestPath;

            string command = "git clone ";

            if (branch != null && branch.Length > 0)
                command = command + "-b " + branch + " ";
            command = command + "--progress -v " + url + " " + dest;

            CMDUtils.Execute(command);
            //Console.WriteLine("out: \n"+CMDUtils.Execute(command));

        }
    }
}
