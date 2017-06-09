using doAutoBuild.Build;
using doAutoBuild.DownloadSourceCode;
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
using System.Threading.Tasks;

namespace doAutoBuild
{
    class Program
    {
        static void Main(string[] args)
        {

            BuildConfigBean _configBean = new BuildConfigBean();

            if (!IOUtils.FileExists(Constants.BuildConfig)) {
                Console.WriteLine(Constants.BuildConfig + "文件不存在，配置有问题");
                return;
            }

            string _buildConfigContent = IOUtils.GetUTF8String(Constants.BuildConfig);

            JObject _buildConfigObj = JObject.Parse(_buildConfigContent);

            try
            {
                _configBean.Msbuildpath = _buildConfigObj.GetValue("msbuildpath").ToString();
            }
            catch (Exception)
            {
                Console.WriteLine("请在 "+Constants.BuildConfig + " 配置msbuildpath值");
                throw;
            }


            IDownloadSourceCode _dsc = null;

            BuildTaskBean _buildBean = new BuildTaskBean();

            _buildBean.TaskId = "task" + DateTime.Now.ToFileTime();
            _buildBean.ProjectId = "project1";
            _buildBean.Environment = "Release";

            //根据projectid 可以读取project目录下面的Source.config 文件

            SourceCodeBean _sourceCodeBean = new SourceCodeBean();

            string _projectPath = Constants.Projects + Path.DirectorySeparatorChar + _buildBean.ProjectId;

            if (!IOUtils.DirExists(_projectPath))
            { //表示文件目录不存在 配置有问题
                Console.WriteLine("项目"+_buildBean.ProjectId + "不存在，配置有问题");
                return;
            }

            string _sourceConfigFile = _projectPath + Path.DirectorySeparatorChar + "Source.config";

            if (!IOUtils.FileExists(_sourceConfigFile))
            { //表示文件目录不存在 配置有问题
                Console.WriteLine("Source.config 不存在，配置有问题");
                return;
            }

            string _sourceConfigContent =  IOUtils.GetUTF8String(_sourceConfigFile);
            string _sourceType = "git";
            try
            {
                JObject _sourceConfigObj = JObject.Parse(_sourceConfigContent);
                _sourceCodeBean.SourceId = _sourceConfigObj.GetValue("SourceId").ToString();
                _sourceCodeBean.Url = _sourceConfigObj.GetValue("Url").ToString();
                _sourceCodeBean.Port = _sourceConfigObj.GetValue("Port").ToString();
                _sourceCodeBean.Account = _sourceConfigObj.GetValue("Account").ToString();
                _sourceCodeBean.Password = _sourceConfigObj.GetValue("Password").ToString();
                _sourceType = _sourceConfigObj.GetValue("SourceType").ToString();
            }
            catch (Exception)
            {
                Console.WriteLine("Source.config 配置内容有误！");
                throw;
            }


            if ("git".Equals(_sourceType)) {
                _dsc = new GitDownloadSourceCode();
            }

            string _destPath = Constants.SourceFile + Path.DirectorySeparatorChar + _sourceCodeBean.SourceId + Path.DirectorySeparatorChar + _buildBean.ProjectId + Path.DirectorySeparatorChar + _buildBean.BranchName;

            FileUtils.CreateDir(_destPath);
       
            _sourceCodeBean.DestPath = _destPath;

            Console.WriteLine("========去远程仓库下载代码==============");

            Console.WriteLine("     下载代码到指定目录： "+_destPath);

            //根据sourceConfig里面的配置去远程仓库下载代码
            if (_dsc != null) {
                _dsc.DownloadSourceCode(_sourceCodeBean,_buildBean);
            }


            Console.WriteLine("========开始build代码==============");
            //根据TaskID创建一个临时目录
            string _tempDir = Constants.Temp + Path.DirectorySeparatorChar + _buildBean.TaskId;
            FileUtils.CreateDir(_tempDir);
            //string msbulidPath = "\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Professional\\MSBuild\\15.0\\Bin\\MSBuild.exe\"";

            string _projectTempDir = _tempDir + Path.DirectorySeparatorChar + _buildBean.ProjectId;
            FileUtils.CreateDir(_projectTempDir);
 
            //找到该目录下面的所有".sln"后缀的文件
            ArrayList _slnFiles = FileUtils.GetFiles(_destPath, "*.sln");

            for (int i = 0; i< _slnFiles.Count; i++) {
                string _slnFile = Path.GetDirectoryName((string)_slnFiles[i]);

                string msbulidBatPath = _projectTempDir + Path.DirectorySeparatorChar + "msbuild.bat";
                StringBuilder _sb = new StringBuilder();
                string changeDir = "cd /d " + _slnFile;
                
                //string changeDir = "cd /d " + _destPath + Path.DirectorySeparatorChar + "UnitA";
                _sb.Append(changeDir + "\n");
                _sb.Append("\""+ _configBean.Msbuildpath + "\"");
                IOUtils.WriteString(msbulidBatPath, _sb.ToString());

                if (IOUtils.FileExists(msbulidBatPath))
                {
                    Console.WriteLine(msbulidBatPath + " 文件创建成功！");
                    Console.WriteLine(CMDUtils.Execute(msbulidBatPath));
                }
              
            }

            Console.WriteLine("========根据Unit.config Copy File==============");

            string _environmentPath = _projectPath + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar + _buildBean.Environment;
            if (!IOUtils.DirExists(_environmentPath))
            { //表示文件目录不存在 配置有问题
                Console.WriteLine(_buildBean.Environment + " 环境不存在，配置有问题");
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

                Console.WriteLine("根据Unit.config Copy File");
                try
                {
                    JObject _unitConfigObj = JObject.Parse(_unitConfigContent);
                    var _appFiles = _unitConfigObj.GetValue("AppFiles");
                    if (_appFiles != null)
                    {
                        JArray _copyFiles = _appFiles as JArray;                 
                        foreach (JObject _copyFile in _copyFiles)
                        {
                            string _sourcePathStr = _copyFile.GetValue("sourcePath").ToString();
                            string _targetPathStr = _copyFile.GetValue("targetPath").ToString();

                            //string _ignore = _appFileObj.GetValue("ignore").ToString();
                            string _sourcePath = Path.Combine(_destPath, _unitDir.Name , _sourcePathStr);         
                            //string _sourcePath = _unitDir.FullName + Path.DirectorySeparatorChar + _sourcePathStr;
                            string _targetPath = _unitTempDir;
                            if (_targetPathStr != null && !"".Equals(_targetPathStr))
                            {
                                _targetPath = _unitTempDir + Path.DirectorySeparatorChar + _targetPathStr;
                            }
                            //copy到temp/projectid/unit/目录下面
                            FileUtils.CopyDirOrFile(_sourcePath, _targetPath);
                            Console.WriteLine("     Unit = " + _unitDir.Name + " 从  " + _sourcePath + " Copy 到" + _targetPath);
                        }
                    }

                    var _configFiles = _unitConfigObj.GetValue("ConfigFiles");
                    if (_configFiles != null)
                    {
                        JArray _copyFiles = _configFiles as JArray;
                        foreach (JObject _copyFile in _copyFiles)
                        {
                            string _sourcePathStr = _copyFile.GetValue("sourcePath").ToString();
                            string _targetPathStr = _copyFile.GetValue("targetPath").ToString();

                            //string _ignore = _appFileObj.GetValue("ignore").ToString();
                            string _sourcePath = _unitDir.FullName + Path.DirectorySeparatorChar + _sourcePathStr;
                            string _targetPath = _unitTempDir;
                            if (_targetPathStr != null && !"".Equals(_targetPathStr))
                            {
                                _targetPath = _unitTempDir + Path.DirectorySeparatorChar + _targetPathStr;
                            }
                            //copy到temp/projectid/unit/目录下面
                            FileUtils.CopyDirOrFile(_sourcePath, _targetPath);
                            Console.WriteLine("     Unit = " + _unitDir.Name + " 从  " + _sourcePath + " Copy 到" + _targetPath);
                        }
                    }

                }
                catch (Exception)
                {
                    Console.WriteLine("Unit.config 配置内容有误！");
                    throw;
                }

            }

            //Console.WriteLine("========Copy bin目录==============");
            ////build完成需要把bin目录 Copy 到
            //foreach (DirectoryInfo _unitDir in _unitDirs)
            //{
            //    ArrayList _binDirs = FileUtils.FindDirFullPath(Path.Combine(_destPath, _unitDir.Name), "bin");
            //    foreach (string _binDirPath in _binDirs)
            //    {
            //        if (_binDirPath != null) { //找到当前Unit 下面的bin 目录
            //            string _targetPath = Path.Combine(_projectTempDir, _unitDir.Name);
            //            FileUtils.CopyDirOrFile(_binDirPath, _targetPath);
            //            Console.WriteLine("从 " + _binDirPath + " Copy 到 "+ _targetPath);
            //        }
            //    }
            //}

            Console.WriteLine("========压缩build包==============");

            string _buildZip = _buildBean.TaskId + ".zip";

            string _zipPath = Path.Combine(_tempDir, _buildZip);
            ZipFile.CreateFromDirectory(_projectTempDir, _zipPath , CompressionLevel.Fastest, true);
            Console.WriteLine("     压缩 " + _projectTempDir + " 目录，生成" + _buildZip + " 文件");

            Console.WriteLine("========上传build包到七牛==============");

            Console.WriteLine("     上传 " + _buildZip + " 文件到七牛");
            QiniuManager.Instance().writeFile(_buildZip, File.ReadAllBytes(_zipPath));
            Console.WriteLine( _buildZip + " 文件上传成功");

            Console.ReadKey();
        }
    }
}
