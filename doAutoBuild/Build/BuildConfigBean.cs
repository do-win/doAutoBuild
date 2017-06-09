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
    
        public string Msbuildpath { get => _msbuildpath; set => _msbuildpath = value; }
    }
}
