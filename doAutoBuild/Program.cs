using doAutoBuild.Build;
using doAutoBuild.Deploy;
using doAutoBuild.DownloadSourceCode;
using doAutoBuild.Log;
using doAutoBuild.Storage;
using doAutoBuild.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace doAutoBuild
{
    class Program
    {

        private static BuildConfigBean _configBean;
        private static Hashtable _sourceCodeRootDirs = new Hashtable();
        private static string _taskId = "task" + DateTime.Now.ToFileTime();

        static void Main(string[] args)
        {

            //////////////////////读取配置文件////////////////////////////
            _configBean = new BuildConfigBean();

            if (!IOUtils.FileExists(Constants.BuildConfig))
            {
                LogUtils.Error(null, new Exception(Constants.BuildConfig + "文件不存在，配置有问题"));
                return;
            }

            string _buildConfigContent = IOUtils.GetUTF8String(Constants.BuildConfig);
            try
            {
                JObject _buildConfigObj = JObject.Parse(_buildConfigContent);
                _configBean.Msbuildpath = _buildConfigObj.GetValue("msbuildpath").ToString();
                _configBean.GetBuildTaskInterval = int.Parse(_buildConfigObj.GetValue("buildtaskinterval").ToString());
            }
            catch (Exception ex)
            {
                LogUtils.Error(null, new Exception("请在 " + Constants.BuildConfig + " 配置msbuildpath值"));
                LogUtils.Error(null, ex);
                throw;
            }

            Thread buildThread = new Thread(BuildThreadChild);
            buildThread.Start();

            Thread deployThread = new Thread(DeployThreadChild);
            deployThread.Start();

            Console.ReadKey();
        }


        static void BuildThreadChild()
        {
            //while (true) {

            //请求http获取打包任务

            //如果取到任务
            LogEngin _logEngin = new LogEngin("取到打包任务，TaskID " + _taskId);
            BuildTaskBean _buildBean = new BuildTaskBean();

            _buildBean.TaskId = _taskId;
            _buildBean.ProjectId = "project1";
            _buildBean.BranchName = "master";

            //根据TaskID创建一个临时目录
            string _projectTempDir = Path.Combine(Constants.Temp, _buildBean.TaskId, _buildBean.ProjectId);
            FileUtils.CreateDir(_projectTempDir);
            _logEngin.Info("创建 " + _projectTempDir + " 临时目录");

            //////////////////下载源代码
            SourceCodeBean _sourceCodeBean = DownloadSourceCode(_logEngin, _buildBean);

            ////////////////build源代码
            BuildSource(_logEngin, _buildBean, _sourceCodeBean, _projectTempDir);

            //上传日志文件
            string _log = _logEngin.ToHtml();

            //没有取到任务，隔段时间再去取
            //    Thread.Sleep(_configBean.GetBuildTaskInterval * 1000);
            //}
        }

        static void DeployThreadChild()
        {
            //while (true) {

            //请求http获取部署任务
            LogEngin _logEngin = new LogEngin("取到部署任务，TaskID " + _taskId);
            //如果取到任务
            DeployTaskBean _deployBean = new DeployTaskBean();
            _deployBean.Environment = "Release";
            _deployBean.UpgradeType = "all";
            _deployBean.AutoUpgrade = true;
            _deployBean.TaskId = _taskId;

            string _projectTempDir = Path.Combine(Constants.Temp, _deployBean.TaskId, _deployBean.ProjectId);

            //E:\AutoBuildHome\SourceFile\myProjct\project1\master
            ////////////////////根据UnitConfig Copy 文件
            if (!_sourceCodeRootDirs.ContainsKey(_deployBean.TaskId))
            {
                _logEngin.Error(new Exception("deployed 失败： 不存在 " + _deployBean.TaskId + " build 任务！"));
                return;
            }
            CopyFileByUnitConfig(_logEngin, _deployBean, _sourceCodeRootDirs[_deployBean.TaskId].ToString(), _projectTempDir);

            ///////////////////判断是否增量升级
            if (_deployBean.AutoUpgrade)
            {
                //MD5比较文件是否修改
                string _sourcePath = Path.Combine(Constants.Temp, _deployBean.TaskId, _deployBean.ProjectId);
                string _targetPath = Path.Combine(Constants.CurrentVersion, _deployBean.ProjectId);

                ArrayList _files = new ArrayList();
                FileUtils.GetFiles(new DirectoryInfo(_sourcePath), _files);
                ArrayList _modifyFiles = new ArrayList();

                foreach (string _file in _files)
                {
                    string _oldFile = _file.Replace(_sourcePath, _targetPath);
                    //文件存在就MD5比较
                    if (IOUtils.FileExists(_oldFile))
                    {
                        string _newMD5 = MD5Utils.MD5File(_file);
                        string _oldMD5 = MD5Utils.MD5File(_oldFile);
                        if (!_newMD5.Equals(_oldMD5))
                        {
                            _logEngin.Info("不一样的文件：" + _file);
                            _modifyFiles.Add(_file);
                        }
                    }
                    else
                    {
                        _logEngin.Info("新增文件：" + _file);
                        _modifyFiles.Add(_file);
                    }
                }
            }

            ////////////////////压缩build包，并上传到七牛云
            UploadZip(_logEngin, _deployBean.TaskId, _deployBean.ProjectId, _projectTempDir);

            //上传日志文件
            string _log = _logEngin.ToHtml();

            //没有取到任务，隔段时间再去取
            //    Thread.Sleep(_configBean.GetBuildTaskInterval * 1000);
            //}
        }

        /// <summary>
        /// 下载源代码
        /// </summary>
        /// <param name="_buildBean"></param>
        /// <returns></returns>
        private static SourceCodeBean DownloadSourceCode(LogEngin _logEngin, BuildTaskBean _buildBean)
        {
            IDownloadSourceCode _dsc = null;
            //根据projectid 可以读取project目录下面的Source.config 文件
            SourceCodeBean _sourceCodeBean = null;
            string _projectPath = Path.Combine(Constants.CurrentConfigProjects, _buildBean.ProjectId);
            if (!IOUtils.DirExists(_projectPath))
            { //表示文件目录不存在 配置有问题
                _logEngin.Error(new Exception("项目" + _buildBean.ProjectId + "不存在，配置有问题"));
                return _sourceCodeBean;
            }

            string _sourceConfigFile = Path.Combine(_projectPath, "Source.config");
            if (!IOUtils.FileExists(_sourceConfigFile))
            { //表示文件目录不存在 配置有问题
                _logEngin.Error(new Exception("项目" + _buildBean.ProjectId + " Source.config 不存在，配置有问题"));
                return _sourceCodeBean;
            }

            string _sourceConfigContent = IOUtils.GetUTF8String(_sourceConfigFile);
            string _sourceType = "git";
            try
            {
                JObject _sourceConfigObj = JObject.Parse(_sourceConfigContent);
                _sourceCodeBean = new SourceCodeBean();
                _sourceCodeBean.SourceId = _sourceConfigObj.GetValue("SourceId").ToString();
                _sourceCodeBean.Url = _sourceConfigObj.GetValue("Url").ToString();
                _sourceCodeBean.Port = _sourceConfigObj.GetValue("Port").ToString();
                _sourceCodeBean.Account = _sourceConfigObj.GetValue("Account").ToString();
                _sourceCodeBean.Password = _sourceConfigObj.GetValue("Password").ToString();
                _sourceType = _sourceConfigObj.GetValue("SourceType").ToString();
            }
            catch (Exception ex)
            {
                _logEngin.Error(new Exception("Source.config 配置内容有误！"));
                _logEngin.Error(ex);
                throw;
            }

            if ("git".Equals(_sourceType))
            {
                _dsc = new GitDownloadSourceCode();
            }

            _sourceCodeBean.DestPath = Path.Combine(Constants.SourceFile, _sourceCodeBean.SourceId, _buildBean.ProjectId, _buildBean.BranchName);
            FileUtils.CreateDir(_sourceCodeBean.DestPath);
            _logEngin.Info("创建 " + _sourceCodeBean.DestPath + "目录");

            _logEngin.Info("去远程仓库下载代码,下载代码到指定目录： " + _sourceCodeBean.DestPath);
            //根据sourceConfig里面的配置去远程仓库下载代码
            if (_dsc != null)
            {
                _dsc.DownloadSourceCode(_sourceCodeBean, _buildBean);
            }
            return _sourceCodeBean;
        }

        /// <summary>
        /// build 源代码
        /// </summary>
        /// <param name="_sourceCodeRootDir"></param>
        /// <param name="_projectTempDir"></param>
        private static void BuildSource(LogEngin _logEngin, BuildTaskBean _buildBean, SourceCodeBean _sourceBean, string _projectTempDir)
        {
            _logEngin.Info("开始build代码");
            //找到该目录下面的所有".sln"后缀的文件
            ArrayList _slnFiles = FileUtils.GetFiles(_sourceBean.DestPath, "*.sln");
            for (int i = 0; i < _slnFiles.Count; i++)
            {
                string _slnFile = Path.GetDirectoryName((string)_slnFiles[i]);
                string msbulidBatPath = _projectTempDir + Path.DirectorySeparatorChar + "msbuild.bat";
                StringBuilder _sb = new StringBuilder();
                string changeDir = "cd /d " + _slnFile;
                //string changeDir = "cd /d " + _destPath + Path.DirectorySeparatorChar + "UnitA";
                _sb.Append(changeDir + "\n");
                _sb.Append("\"" + _configBean.Msbuildpath + "\"");
                IOUtils.WriteString(msbulidBatPath, _sb.ToString());

                if (IOUtils.FileExists(msbulidBatPath))
                {
                    _logEngin.Info(msbulidBatPath + " 文件创建成功！");
                    int _code = CMDUtils.Execute(_logEngin, msbulidBatPath);
                    if (_code == 0)
                    {
                        //删除bat 文件
                        FileUtils.DeleteFile(msbulidBatPath);
                        _logEngin.Info("build " + _slnFile + "success");
                    }
                    else
                    {
                        _logEngin.Info("build " + _slnFile + "fail");
                    }
                }
            }

            //build 完成
            if (_slnFiles != null && _slnFiles.Count > 0)
            {
                _sourceCodeRootDirs.Add(_buildBean.TaskId, _sourceBean.DestPath);
            }
        }

        /// <summary>
        /// 根据Unit.config Copy文件
        /// </summary>
        private static void CopyFileByUnitConfig(LogEngin _logEngin, DeployTaskBean _deployBean, string _sourceCodeRootDir, string _projectTempDir)
        {

            _logEngin.Info("根据Unit.config Copy File");
            string _projectPath = Path.Combine(Constants.CurrentConfigProjects, _deployBean.ProjectId);
            string _environmentPath = Path.Combine(_projectPath, _deployBean.Environment);
            if (!IOUtils.DirExists(_environmentPath))
            { //表示文件目录不存在 配置有问题
                _logEngin.Error(new Exception(_deployBean.Environment + " 环境不存在，配置有问题"));
                return;
            }
            //获取当前环境下面的所以单元项目路径
            ArrayList _unitDirs = FileUtils.GetDirs(_environmentPath);

            foreach (DirectoryInfo _unitDir in _unitDirs)
            {
                string _unitTempDir = _projectTempDir + Path.DirectorySeparatorChar + _unitDir.Name;
                FileUtils.CreateDir(_unitTempDir);

                string _unitConfigFile = _unitDir.FullName + Path.DirectorySeparatorChar + "Unit.config";
                string _unitConfigContent = IOUtils.GetUTF8String(_unitConfigFile);

                try
                {
                    JObject _unitConfigObj = JObject.Parse(_unitConfigContent);
                    var _appFiles = _unitConfigObj.GetValue("AppFiles");
                    string _upgradeType = _deployBean.UpgradeType;
                    if (_appFiles != null && ("all".Equals(_upgradeType) || "app".Equals(_upgradeType)))
                    {
                        JArray _copyFiles = _appFiles as JArray;
                        foreach (JObject _copyFile in _copyFiles)
                        {
                            string _sourcePathStr = _copyFile.GetValue("sourcePath").ToString();
                            string _targetPathStr = _copyFile.GetValue("targetPath").ToString();

                            //string _ignore = _appFileObj.GetValue("ignore").ToString();
                            string _sourcePath = Path.Combine(_sourceCodeRootDir, _unitDir.Name, _sourcePathStr);
                            string _targetPath = _unitTempDir;
                            if (_targetPathStr != null && !"".Equals(_targetPathStr))
                            {
                                _targetPath = _unitTempDir + Path.DirectorySeparatorChar + _targetPathStr;
                            }
                            //copy到temp/projectid/unit/目录下面
                            FileUtils.CopyDirOrFile(_sourcePath, _targetPath);
                            _logEngin.Info("     Unit = " + _unitDir.Name + " 从  " + _sourcePath + " Copy 到" + _targetPath);
                        }
                    }

                    var _configFiles = _unitConfigObj.GetValue("ConfigFiles");
                    if (_configFiles != null && ("all".Equals(_upgradeType) || "config".Equals(_upgradeType)))
                    {
                        JArray _copyFiles = _configFiles as JArray;
                        foreach (JObject _copyFile in _copyFiles)
                        {
                            string _sourcePathStr = _copyFile.GetValue("sourcePath").ToString();
                            string _targetPathStr = _copyFile.GetValue("targetPath").ToString();

                            //string _ignore = _appFileObj.GetValue("ignore").ToString();
                            string _sourcePath = Path.Combine(_unitDir.FullName, _sourcePathStr);
                            string _targetPath = _unitTempDir;
                            if (_targetPathStr != null && !"".Equals(_targetPathStr))
                            {
                                _targetPath = Path.Combine(_unitTempDir, _targetPathStr);
                            }
                            //copy到temp/projectid/unit/目录下面
                            FileUtils.CopyDirOrFile(_sourcePath, _targetPath);
                            _logEngin.Info("     Unit = " + _unitDir.Name + " 从  " + _sourcePath + " Copy 到" + _targetPath);
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logEngin.Error(new Exception("Unit.config 配置内容有误！"));
                    _logEngin.Error(ex);
                    throw;
                }
            }
        }

        private static void UploadZip(LogEngin _logEngin, string _taskId, string _projectId, string _projectTempDir)
        {
            string _buildZip = _taskId + ".zip";
            _logEngin.Info("压缩文件 " + _buildZip);
            string _zipPath = Path.Combine(Constants.Temp, _taskId, _buildZip);
            ZipFile.CreateFromDirectory(_projectTempDir, _zipPath, CompressionLevel.Fastest, true);
            _logEngin.Info("     压缩 " + _projectTempDir + " 目录，生成" + _buildZip + " 文件");

            _logEngin.Info("     上传 " + _buildZip + " 文件到七牛");
            string _zipUrl = "";
            try
            {
                string _path = QiniuManager.Instance().writeFile(_buildZip, File.ReadAllBytes(_zipPath));
                _zipUrl = QiniuManager.Instance().getAccessUrl(_path);
            }
            catch (Exception ex)
            {
                _logEngin.Error(new Exception(_buildZip + " 文件上传失败"));
                _logEngin.Error(ex);
                throw;
            }
            _logEngin.Info(_buildZip + " 文件上传成功");

            /////Copy到CurrentVersion目录下面，删除 TaskID 目录
            string _sourcePath = Path.Combine(Constants.Temp, _taskId, _projectId);
            string _targetPath = Constants.CurrentVersion;

            //删除CurrentVersion 里面有projectId 的目录
            string _currentVersionProjectDir = Path.Combine(Constants.CurrentVersion, _projectId);
            if (IOUtils.DirExists(_currentVersionProjectDir))
            {
                FileUtils.DeleteDir(_currentVersionProjectDir);
                _logEngin.Info("删除 " + _currentVersionProjectDir + " 目录");
            }

            FileUtils.CopyDirOrFile(_sourcePath, _targetPath);
            _logEngin.Info("从 " + _sourcePath + " Copy 到" + _targetPath + " 目录");
            FileUtils.DeleteDir(Path.Combine(Constants.Temp, _taskId));
            ////////////////文件上传成功，把文件上传路径传给服务器
            _logEngin.Info("本地Deployed Success 通知服务器");

        }


    }
}
