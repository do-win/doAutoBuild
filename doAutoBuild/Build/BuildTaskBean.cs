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
        private string _projectId;   //项目ID
        private string _projectName; //项目名称
        private string _branchName = "master"; //代码分支名称， 默认为master
     
        public string TaskId { get => _taskId; set => _taskId = value; }
        public string ProjectId { get => _projectId; set => _projectId = value; }
        public string BranchName { get => _branchName; set => _branchName = value; }
        public string ProjectName { get => _projectName; set => _projectName = value; }
    }
}
