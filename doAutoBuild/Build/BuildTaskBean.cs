using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.Build
{
    class BuildTaskBean
    {

        //TaskId
        //ProjectId
        //DelployType all 全部升级，app 只升级应用程序，  config 只升级配置文件 ；默认为 app
        //AutoUpgrade 是否只更新变化的文件(检查文件大小和内容)  0：否 1：是  ; 默认为1
        //NeedUpdateSource 是否需要更新相关源代码  0：否 1：是 ; 默认为1
        //Environment      部署服务器的地址
        //Reason        部署原因
        //branchName    代码分支名称， 默认为master

        private string _taskId;
        private string _projectId;   //项目名称
        private string _branchName = "master"; //代码分支名称， 默认为master
        private Boolean _needUpdateSource = true;  //是否需要更新相关源代码  0：否 1：是 ; 默认为1
        private string _environment;


        public string TaskId { get => _taskId; set => _taskId = value; }
        public string ProjectId { get => _projectId; set => _projectId = value; }
        public string BranchName { get => _branchName; set => _branchName = value; }
        public bool NeedUpdateSource { get => _needUpdateSource; set => _needUpdateSource = value; }
        public string Environment { get => _environment; set => _environment = value; }
    }
}
