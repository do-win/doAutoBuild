using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.DownloadSourceCode
{
    class SourceCodeBean
    {
        //"SourceId":"myProjct",
        //"SourceType": "git",
        //"Url": "https://github.com/maoruiily/project1.git",
        //"Port": "",
        //"Account": "",
        //"Password": ""
        private string _sourceId; //表示源码从哪个Git服务器取
      //  private String _sourceType;
        private string _url;
        private string _port;
        private string _account;
        private string _password;
        private string _destPath; //输出目录

        public string SourceId { get => _sourceId; set => _sourceId = value; }
        //public string SourceType { get => _sourceType; set => _sourceType = value; }
        public string Url { get => _url; set => _url = value; }
        public string Port { get => _port; set => _port = value; }
        public string Account { get => _account; set => _account = value; }
        public string Password { get => _password; set => _password = value; }
        public string DestPath { get => _destPath; set => _destPath = value; }
    }
}
