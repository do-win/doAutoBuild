using doAutoBuild.Build;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.DownloadSourceCode
{
    interface IDownloadSourceCode
    {
        void DownloadSourceCode(SourceCodeBean _sourceCodeBean, BuildTaskBean _buildTaskBean);
    }
}
