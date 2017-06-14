using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.Build
{
    class BuildTaskBean
    {
        private string _taskId;
        private string _projectId;   //项目名称
        private string _branchName = "master"; //代码分支名称， 默认为master
        private string _environment;
        private bool _autoUpgrade = true;  //是否只更新变化的文件(检查文件大小和内容)  0：否 1：是  ; 默认为1
        private string _upgradeType = "app";  //all 全部升级，app 只升级应用程序，  config 只升级配置文件 ；默认为 app

        public string TaskId { get => _taskId; set => _taskId = value; }
        public string ProjectId { get => _projectId; set => _projectId = value; }
        public string BranchName { get => _branchName; set => _branchName = value; }
        public string Environment { get => _environment; set => _environment = value; }
        public bool AutoUpgrade { get => _autoUpgrade; set => _autoUpgrade = value; }
        public string UpgradeType { get => _upgradeType; set => _upgradeType = value; }
    }
}
