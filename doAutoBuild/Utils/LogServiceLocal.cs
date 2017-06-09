using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace doAutoBuild.Utils
{
    public class LogServiceLocal
    {
        private static LogServiceLocal _logService;
        public static LogServiceLocal Instance() {
            if (_logService == null) {
                _logService = new LogServiceLocal();
            }
            return _logService;
        }


        private string LogRootPath = null;
        private const string logHeader = "\r\n\r\n=======================================\r\n";
        private string buildFileFullName(string _rootPath, string _path)
        {
            if (this.LogRootPath == null)
            {
                if (!_rootPath.EndsWith("/")) _rootPath = _rootPath + "/";
                this.LogRootPath = this.LogRootPath.Replace("$", _rootPath);
                if (!this.LogRootPath.EndsWith("/")) this.LogRootPath = this.LogRootPath + "/";
            }
            string _resultPath = this.LogRootPath + DateTime.Now.ToString("yyyyMMdd") + "/" + _path;
            string _dir = Path.GetDirectoryName(_resultPath);
            if (!Directory.Exists(_dir)) Directory.CreateDirectory(_dir);
            return _resultPath;
        }

        private string getObjDesc(object obj)
        {
            if (obj == null) return "";
            if (obj is Exception)
            {
                Exception _err = obj as Exception;
                StringBuilder _strB = new StringBuilder();
                _strB.Append(_err.Message + "\r\n");
                _strB.Append(_err.Source + "\r\n");
                _strB.Append(_err.StackTrace + "\r\n\r\n");
                Exception _e = _err.InnerException;
                while (_e != null)
                {
                    _strB.Append(_e.Message + "\r\n");
                    _strB.Append(_e.Source + "\r\n");
                    _strB.Append(_e.StackTrace + "\r\n\r\n");
                    _e = _e.InnerException;
                }
                return _strB.ToString();
            }
            return JsonConvert.SerializeObject(obj);
        }

        private Dictionary<string, object> dictDebugLocks = new Dictionary<string, object>();
        private Dictionary<string, object> dictErrorLocks = new Dictionary<string, object>();
        private Dictionary<string, object> dictFatalLocks = new Dictionary<string, object>();
        private Dictionary<string, object> dictInfoLocks = new Dictionary<string, object>();
        private Dictionary<string, object> dictWarnLocks = new Dictionary<string, object>();
        private Dictionary<string, object> dictMyLogLocks = new Dictionary<string, object>();
        private object getLock(string _type, string _module)
        {
            Dictionary<string, object> locks = null;
            switch (_type)
            {
                case "Debug":
                    locks = this.dictDebugLocks;
                    break;
                case "Error":
                    locks = this.dictErrorLocks;
                    break;
                case "Fatal":
                    locks = this.dictFatalLocks;
                    break;
                case "Info":
                    locks = this.dictInfoLocks;
                    break;
                case "Warn":
                    locks = this.dictWarnLocks;
                    break;
                case "MyLog":
                    locks = this.dictMyLogLocks;
                    break;
                default:
                    throw new Exception("无效的日志类型:" + _type);
            }
            if (_module == null) return locks;
            if (!locks.ContainsKey(_module))
            {
                locks[_module] = new object();
            }
            return locks[_module];
        }

        private void WriteLog(object _lock, string _fileFullPath, string _content, object _obj)
        {
            if (_content == null && _obj == null) return;
            lock (_lock)
            {
                if (_content == null)
                {
                    File.AppendAllText(_fileFullPath, logHeader + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss \t") +
                         _obj == null ? "" : (this.getObjDesc(_obj) + "\r\n\r\n"), Encoding.UTF8);
                }
                else if (_obj == null)
                {
                    File.AppendAllText(_fileFullPath, logHeader + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss \t") + _content + "\r\n\r\n", Encoding.UTF8);
                }
                else
                {
                    File.AppendAllText(_fileFullPath, logHeader + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss \t") + _content + "\r\n\r\n" +
                         this.getObjDesc(_obj) + "\r\n\r\n", Encoding.UTF8);
                }
            }

        }

        public void Debug(string _module, string _content)
        {
            this.Debug(_module, _content, null);
        }

        public void Debug(string _module, string _content, object _obj)
        {
            // if (!DoEnvironment.Instance.IsDebug) return;
            string _fileFullPath = this.buildFileFullName(_module, "debug.txt");
            object _lock = this.getLock("Debug", _module);
            this.WriteLog(_lock, _fileFullPath, _content, _obj);
        }


        public void Error(string _module, string _content)
        {
            this.Error(_module, _content, null);
        }
        public void Error(string _module, string _content, object _obj)
        {
            string _fileFullPath = this.buildFileFullName(_module, "error.txt");
            object _lock = this.getLock("Error", _module);
            this.WriteLog(_lock, _fileFullPath, _content, _obj);
        }

        public void Fatal(string _module, string _content)
        {
            this.Fatal(_module, _content, null);
        }
        public void Fatal(string _module, string _content, object _obj)
        {
            string _fileFullPath = this.buildFileFullName(_module, "fatal.txt");
            object _lock = this.getLock("Fatal", _module);
            this.WriteLog(_lock, _fileFullPath, _content, _obj);
        }

        public void Info(string _module, string _content)
        {
            this.Info(_module, _content, null);
        }
        public void Info(string _module, string _content, object _obj)
        {
            string _fileFullPath = this.buildFileFullName(_module, "info.txt");
            object _lock = this.getLock("Fatal", _module);
            this.WriteLog(_lock, _fileFullPath, _content, _obj);
        }

        public void Warn(string _module, string _content)
        {
            this.Warn(_module, _content, null);
        }
        public void Warn(string _module, string _content, object _obj)
        {
            string _fileFullPath = this.buildFileFullName(_module, "warn.txt");
            object _lock = this.getLock("Warn", _module);
            this.WriteLog(_lock, _fileFullPath, _content, _obj);
        }

        public void MyLog(string _module, string typeId, string _content)
        {
            this.MyLog(_module, typeId, _content, null);
        }
        public void MyLog(string _module, string typeId, string _content, object _obj)
        {
            string _fileFullPath = this.buildFileFullName(_module, typeId);
            object _lock = this.getLock("MyLog", _module + ":" + typeId);
            this.WriteLog(_lock, _fileFullPath, _content, _obj);
        }

    }
}