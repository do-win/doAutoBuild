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
using System.Text;
using System.Threading;

namespace doAutoBuild
{
    class Program
    {

        private static BuildConfigBean _configBean;
        private static Hashtable _sourceCodeRootDirs = new Hashtable();
       

        static void Main(string[] args)
        {

            _sourceCodeRootDirs.Add("77f74579412c40a68164b7809c4026fe", "E:\\AutoBuildHome\\SourceFile\\myProjct\\liuliang\\master");
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
                _configBean.SecurityKey = _buildConfigObj.GetValue("securitykey").ToString();

            }
            catch (Exception ex)
            {
                LogUtils.Error(null, new Exception("请在 " + Constants.BuildConfig + " 配置msbuildpath值"));
                LogUtils.Error(null, ex);
                throw;
            }

            //Thread buildThread = new Thread(BuildThreadChild);
            //buildThread.Start();

            Thread deployThread = new Thread(DeployThreadChild);
            deployThread.Start();

            Console.ReadKey();
        }


        static void BuildThreadChild()
        {
            //while (true) {

            //请求http获取打包任务
            long _cDate = DateTime.Now.ToFileTime();
            Dictionary<string, object> _dictData = new Dictionary<string, object>();
            _dictData.Add("TimeStamp", _cDate);
            var _sign = SignData.Md5SignDict(_configBean.SecurityKey, _dictData);
            string _postDataStr = "TimeStamp=" + _cDate + "&Sign=" + _sign;
            string _result = HttpUtils.HttpGet(Constants.GET_BUILD_TASK, _postDataStr);

            //如果取到任务
            if (_result != null && _result.Length > 0)
            {
                BuildTaskBean _buildBean = new BuildTaskBean();
                try
                {
                    JObject _buildConfigObj = JObject.Parse(_result);
                    _buildBean.TaskId = _buildConfigObj.GetValue("Id").ToString();
                    _buildBean.ProjectId = _buildConfigObj.GetValue("ProjectId").ToString();
                    _buildBean.ProjectName = _buildConfigObj.GetValue("ProjectName").ToString();
                    _buildBean.BranchName = _buildConfigObj.GetValue("BranchName").ToString();
                }
                catch (Exception ex)
                {
                    LogUtils.Error(null, new Exception("解析打包任务数据请求失败 " + _result));
                    LogUtils.Error(null, ex);
                    throw;
                }

                LogEngin _logEngin = new LogEngin(_buildBean.ProjectName + " 项目打包" , _buildBean.TaskId);

                //根据TaskID创建一个临时目录
                string _projectTempDir = Path.Combine(Constants.Temp, _buildBean.TaskId, _buildBean.ProjectName);
                FileUtils.CreateDir(_projectTempDir);
                _logEngin.Info("创建 " + _projectTempDir + " 临时目录");

                //////////////////下载源代码
                try
                {
                    SourceCodeBean _sourceCodeBean = DownloadSourceCode(_logEngin, _buildBean);          
                    ////////////////build源代码
                    BuildSource(_logEngin, _buildBean, _sourceCodeBean, _projectTempDir);               
                }
                catch (Exception ex)
                {
                    ////////build失败
                    _logEngin.Error(ex);
                    _logEngin.IsSuccess = false;
                }

                //上传日志文件
                string _log = _logEngin.ToHtml();
                string _logPath = Path.Combine(Constants.Temp, _buildBean.TaskId + ".html");
                IOUtils.WriteUTF8String(_logPath, _log);
                string _logUrl = UploadLogFile(_logEngin, _logPath);


                //请求http获取打包任务
                _cDate = DateTime.Now.ToFileTime();
                _dictData.Clear();

                int _state = (_logEngin.IsSuccess ? 2 : 1);

                _dictData.Add("TimeStamp", _cDate);
                _dictData.Add("Id", _buildBean.TaskId);
                _dictData.Add("State", _state);
                _dictData.Add("LogUrl", _logUrl);

                _sign = SignData.Md5SignDict(_configBean.SecurityKey, _dictData);

                _postDataStr = "Id=" + _buildBean.TaskId+ "&State=" + _state + "&LogUrl=" + _logUrl + "&TimeStamp=" + _cDate + "&Sign=" + _sign;
                HttpUtils.HttpPut(Constants.GET_BUILD_TASK, _postDataStr);

                Console.WriteLine("Build完成");
            }
            else //没有取到任务，隔段时间再去取
            {
                Thread.Sleep(_configBean.GetBuildTaskInterval * 1000);
            }
        }

        static void DeployThreadChild()
        {
            //while (true) {

            //请求http获取部署任务
            long _cDate = DateTime.Now.ToFileTime();
            Dictionary<string, object> _dictData = new Dictionary<string, object>();
            _dictData.Add("TimeStamp", _cDate);
            var _sign = SignData.Md5SignDict(_configBean.SecurityKey, _dictData);
            string _postDataStr = "TimeStamp=" + _cDate + "&Sign=" + _sign;
            string _result = HttpUtils.HttpGet(Constants.GET_DEPLOY_TASK, _postDataStr);

            //{
            //    "Id": "d617f77f182f46379b8201de01a7dc9f",
            //    "ProjectName": "crm",
            //    "BranchName": "crm_master",
            //    "Environment": "UAT",
            //    "UpgradeType": null,
            //    "AutoUpgrade": "false",
            //}


            //如果取到任务
            if (_result != null && _result.Length > 0)
            {

                //如果取到任务
                DeployTaskBean _deployBean = new DeployTaskBean();
                try
                {
                    JObject _buildConfigObj = JObject.Parse(_result);
                    _deployBean.Id = _buildConfigObj.GetValue("Id").ToString();
                    _deployBean.TaskId = _buildConfigObj.GetValue("BuildId").ToString();
                    _deployBean.ProjectName = _buildConfigObj.GetValue("ProjectName").ToString();
                    _deployBean.BranchName = _buildConfigObj.GetValue("BranchName").ToString();
                    _deployBean.Environment = _buildConfigObj.GetValue("Environment").ToString();
                    _deployBean.UpgradeType = _buildConfigObj.GetValue("UpgradeType").ToString(); ;
                    _deployBean.AutoUpgrade = Boolean.Parse(_buildConfigObj.GetValue("AutoUpgrade").ToString());
                }
                catch (Exception ex)
                {
                    LogUtils.Error(null, new Exception("解析部署任务数据请求失败 " + _result));
                    LogUtils.Error(null, ex);
                    throw;
                }
                LogEngin _logEngin = new LogEngin("部署", _deployBean.TaskId);

                try
                {
                    string _taskTempDir = Path.Combine(Constants.Temp, _deployBean.TaskId);
                    string _projectTempDir = Path.Combine(_taskTempDir, _deployBean.ProjectName);

                    //E:\AutoBuildHome\SourceFile\myProjct\project1\master
                    ////////////////////根据UnitConfig Copy 文件
                    if (_sourceCodeRootDirs.ContainsKey(_deployBean.TaskId))
                    {
                        CopyFileByUnitConfig(_logEngin, _deployBean, _sourceCodeRootDirs[_deployBean.TaskId].ToString(), _projectTempDir);
                        string _zipSourceDir = _projectTempDir;

                        ArrayList _modifyFiles = new ArrayList();
                        ///////////////////判断是否增量升级
                        if (_deployBean.AutoUpgrade)
                        {
                            //MD5比较文件是否修改
                            string _sourcePath = Path.Combine(Constants.Temp, _deployBean.TaskId, _deployBean.ProjectName);
                            string _targetPath = Path.Combine(Constants.CurrentVersion, _deployBean.ProjectName);

                            ArrayList _files = new ArrayList();
                            FileUtils.GetFiles(new DirectoryInfo(_sourcePath), _files);
                            string _outTempDir = Path.Combine(_taskTempDir, "upgrade");
                            FileUtils.CreateDir(_outTempDir);

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
                                        string _outPath = _file.Replace(_taskTempDir, _outTempDir);
                                        FileUtils.CopyDirOrFile(_file, _outPath);
                                    }
                                }
                                else
                                {
                                    _logEngin.Info("新增文件：" + _file);
                                    _modifyFiles.Add(_file);
                                    string _outPath = _file.Replace(_taskTempDir, _outTempDir);
                                    FileUtils.CopyDirOrFile(_file, _outPath);
                                }
                            }

                            if (_modifyFiles.Count > 0)
                            {
                                _zipSourceDir = Path.Combine(_outTempDir, _deployBean.ProjectName);
                            }
                            else
                            {
                                _logEngin.Error(new Exception("选择增量升级但无文件改动，部署失败"));
                            }

                        }

                        if (!_deployBean.AutoUpgrade || _modifyFiles.Count > 0)
                        {
                            //压缩文件
                            string _buildZip = _deployBean.TaskId + ".zip";
                            _logEngin.Info("压缩文件 " + _buildZip);
                            string _zipPath = Path.Combine(Constants.Temp, _deployBean.TaskId, _buildZip);
                            ZipFile.CreateFromDirectory(_zipSourceDir, _zipPath, CompressionLevel.Fastest, true);
                            _logEngin.Info("     压缩 " + _projectTempDir + " 目录，生成" + _buildZip + " 文件");
                            _logEngin.Info("     上传 " + _buildZip + " 文件到七牛");

                            ////////////////////压缩build包，并上传到七牛云
                            _deployBean.DeployQiniuUrl = UploadZip(_logEngin, _deployBean.TaskId, _deployBean.ProjectName, _zipPath, _buildZip);
                        }

                        //删除临时目录
                        FileUtils.DeleteDir(Path.Combine(Constants.Temp, _deployBean.TaskId));
                    }
                    else
                    {
                        throw new Exception("deployed 失败： 不存在 " + _deployBean.TaskId + " build 任务！");
                    }
        
                }
                catch (Exception _ex)
                {
                    ////////build失败
                    _logEngin.Error(_ex);
                    _logEngin.IsSuccess = false;
                }

                ////////////////文件上传成功，把文件上传路径传给服务器
                _logEngin.Info("本地Deployed完成通知服务器");

                //上传日志文件                             
                string _log = _logEngin.ToHtml();
                string _logPath = Path.Combine(Constants.Temp, _deployBean.TaskId + "_deploy.html");
                IOUtils.WriteUTF8String(_logPath, _log);
                string _logUrl = UploadLogFile(_logEngin, _logPath);


                //请求http获取打包任务
                _cDate = DateTime.Now.ToFileTime();
                _dictData.Clear();

                int _state = (_logEngin.IsSuccess ? 2 : 1);

                _dictData.Add("TimeStamp", _cDate);
                _dictData.Add("Id", _deployBean.Id);
                _dictData.Add("State", _state);
                _dictData.Add("Url", _deployBean.DeployQiniuUrl);

                _sign = SignData.Md5SignDict(_configBean.SecurityKey, _dictData);

                _postDataStr = "Id=" + _deployBean.Id + "&State=" + _state + "&Url=" + _deployBean.DeployQiniuUrl + "&TimeStamp=" + _cDate + "&Sign=" + _sign;
                HttpUtils.HttpPut(Constants.GET_DEPLOY_TASK, _postDataStr);
                /////
                _logEngin.Info("组装资源文件完成，通知部署服务器去Qiniu下载资源文件");
                Console.WriteLine("组装资源文件完成，通知部署服务器去Qiniu下载资源文件");

                if (_logEngin.IsSuccess && _deployBean.DeployQiniuUrl !=null && _deployBean.DeployQiniuUrl.Length > 0) {               
                    Dispatcher(_logEngin, _deployBean.ProjectName, _deployBean.Environment, _deployBean.DeployQiniuUrl);
                }
            }
            else //没有取到任务，隔段时间再去取
            {
                Thread.Sleep(_configBean.GetBuildTaskInterval * 1000);
            }
        }

        /// <summary>
        /// 下载源代码
        /// </summary>
        /// <param name="_buildBean"></param>
        /// <returns></returns>
        private static SourceCodeBean DownloadSourceCode(LogEngin _logEngin, BuildTaskBean _buildBean)
        {
            IDownloadSourceCode _dsc = null;
            //根据projectName 可以读取project目录下面的Source.config 文件
            SourceCodeBean _sourceCodeBean = null;
            string _projectPath = Path.Combine(Constants.CurrentConfigProjects, _buildBean.ProjectName);
            if (!IOUtils.DirExists(_projectPath))
            { //表示文件目录不存在 配置有问题
                throw new Exception("项目" + _buildBean.ProjectName + "不存在，配置有问题");
            }

            string _sourceConfigFile = Path.Combine(_projectPath, "Source.config");
            if (!IOUtils.FileExists(_sourceConfigFile))
            { //表示文件目录不存在 配置有问题
                throw new Exception("项目" + _buildBean.ProjectName + " Source.config 不存在，配置有问题");
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
            catch(Exception _ex)
            {
                throw new Exception("Source.config 配置内容有误！ \n" + _ex);
            }

            if ("git".Equals(_sourceType))
            {
                _dsc = new GitDownloadSourceCode();
            }

            _sourceCodeBean.DestPath = Path.Combine(Constants.SourceFile, _sourceCodeBean.SourceId, _buildBean.ProjectName, _buildBean.BranchName);
            FileUtils.CreateDir(_sourceCodeBean.DestPath);
            _logEngin.Info("创建 " + _sourceCodeBean.DestPath + "目录");

            _logEngin.Info("去远程仓库下载代码,下载代码到指定目录： " + _sourceCodeBean.DestPath);
            //根据sourceConfig里面的配置去远程仓库下载代码  
            int _code = _dsc.DownloadSourceCode(_logEngin, _sourceCodeBean, _buildBean);
            if (_code != 0) {
                throw new Exception("源代码下载失败！请检查 "+_buildBean.ProjectName+" 项目下的 Source.config文件");
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
            //TODO 可能有点耗时，排除.git 目录
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
                        _logEngin.Debug("build " + _slnFile + " Success");
                    }
                    else
                    {
                        _logEngin.Debug("build " + _slnFile + " Fail");                       
                    }
                }
            }

            //build 完成
            if (_slnFiles != null && _slnFiles.Count > 0)
            {
                _sourceCodeRootDirs.Add(_buildBean.TaskId, _sourceBean.DestPath);
            }
            else {
                _logEngin.IsSuccess = false;
            }
        }

        /// <summary>
        /// 根据Unit.config Copy文件
        /// </summary>
        private static void CopyFileByUnitConfig(LogEngin _logEngin, DeployTaskBean _deployBean, string _sourceCodeRootDir, string _projectTempDir)
        {

            _logEngin.Info("根据Unit.config Copy File");
            string _projectPath = Path.Combine(Constants.CurrentConfigProjects, _deployBean.ProjectName);
            string _environmentPath = Path.Combine(_projectPath, _deployBean.Environment);
            if (!IOUtils.DirExists(_environmentPath))
            { //表示文件目录不存在 配置有问题
               throw new Exception(_deployBean.Environment + " 环境不存在，配置有问题");
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
                catch (Exception _ex)
                {
                    throw new Exception("Unit.config 配置内容有误！ \n" + _ex);
                }
            }
        }

        private static string UploadZip(LogEngin _logEngin, string _taskId, string _projectName, string _zipPath, string _buildZip)
        {
            string _zipUrl = "";
            try
            {
                string _path = QiniuManager.Instance().writeFile(_buildZip, File.ReadAllBytes(_zipPath));
                _zipUrl = QiniuManager.Instance().getAccessUrl(_path);
            }
            catch (Exception _ex)
            {
                throw new Exception(_buildZip + " 文件上传失败！ \n" + _ex);
            }
            _logEngin.Info(_buildZip + " 文件上传成功");

            /////Copy到CurrentVersion目录下面，删除 TaskID 目录
            string _sourcePath = Path.Combine(Constants.Temp, _taskId, _projectName);
            string _targetPath = Constants.CurrentVersion;

            //删除CurrentVersion 里面有projectName 的目录
            string _currentVersionProjectDir = Path.Combine(Constants.CurrentVersion, _projectName);
            if (IOUtils.DirExists(_currentVersionProjectDir))
            {
                FileUtils.DeleteDir(_currentVersionProjectDir);
                _logEngin.Info("删除 " + _currentVersionProjectDir + " 目录");
            }

            FileUtils.CopyDirOrFile(_sourcePath, _targetPath);
            _logEngin.Info("从 " + _sourcePath + " Copy 到" + _targetPath + " 目录");
            return _zipUrl;
        }

        public static string UploadLogFile(LogEngin _logEngin, string _logPath) {
            string _logUrl = "";
            try
            {
                string _path = QiniuManager.Instance().writeFile(Path.GetFileName(_logPath), File.ReadAllBytes(_logPath));
                _logUrl = QiniuManager.Instance().getAccessUrl(_path);
            }
            catch (Exception _ex)
            {
                throw new Exception(_logPath + " 日志文件上传失败！ \n" + _ex);
            }

            return _logUrl;
        }

        //把build文件在Qiniu上面的地址分发给每台服务器
        public static void Dispatcher(LogEngin _logEngin, string _projectName, string _environment, string _zipUrl) {

            //读取当前环境下面的Environment.config文件，获取需要部署的服务器列表，然后循环调用各自服务器的接口
            string _environmentConfig = Path.Combine(Constants.CurrentConfigProjects, _projectName, _environment, "Environment.config");

            if (!IOUtils.FileExists(_environmentConfig))
            {
                LogUtils.Error(null, new Exception(_environmentConfig + "文件不存在，配置有问题"));
                return;
            }

            string _environmentConfigContent = IOUtils.GetUTF8String(_environmentConfig);
            try
            {
                JObject _environmentConfigObj = JObject.Parse(_environmentConfigContent);

                foreach (var _item in _environmentConfigObj)
                {
                    string _unit = _item.Key;  //哪个模块
                    JArray _serviceIds = JArray.Parse(_item.Value.ToString()); //发布到哪些服务器

                    //循环调用接口发送数据
                    foreach (string _serviceId in _serviceIds)
                    {
                        string _serviceCofnigPath = Path.Combine(Constants.TargetServices, _serviceId, "TargetService.config");

                        if (!IOUtils.FileExists(_serviceCofnigPath))
                        { //表示文件目录不存在 配置有问题
                            _logEngin.Error(new Exception(_serviceId + " 对应的配置不存在，配置有问题"));
                            continue;
                        }

                        string _serviceCofnigContent = IOUtils.GetUTF8String(_serviceCofnigPath);

                        try
                        {
                            JObject _serviceCofnigObj = JObject.Parse(_serviceCofnigContent);

                            string _ipAddress = _serviceCofnigObj.GetValue("IpAddress").ToString();
                            string _port = _serviceCofnigObj.GetValue("Port").ToString();
                            //string _sign = _serviceCofnigObj.GetValue("Sign").ToString();
                            //调用接口，参数 七牛地址，Unit名称

                        }
                        catch (Exception _ex)
                        {
                            throw new Exception(_serviceCofnigPath + " 配置文件内容有误！ \n" + _ex);
                        }
                    }
                }
            }
            catch (Exception _ex)
            {
                throw new Exception(_environmentConfig + " 配置文件内容有误！ \n" + _ex);
            }
        }
    }
}
