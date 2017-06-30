using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.Build
{
    class BuildConfigBean
    {
        private string _msbuildpath;
        private int _getBuildTaskInterval =30; //获取打包任务时间间隔
        private string _securityKey;

        public string Msbuildpath { get => _msbuildpath; set => _msbuildpath = value; }
        public int GetBuildTaskInterval { get => _getBuildTaskInterval; set => _getBuildTaskInterval = value; }
        public string SecurityKey { get => _securityKey; set => _securityKey = value; }
    }
}
