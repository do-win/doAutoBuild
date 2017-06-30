﻿using doAutoBuild.Build;
using doAutoBuild.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.DownloadSourceCode
{
    interface IDownloadSourceCode
    {
        int DownloadSourceCode(LogEngin _logEngin, SourceCodeBean _sourceCodeBean, BuildTaskBean _buildTaskBean);
    }
}
