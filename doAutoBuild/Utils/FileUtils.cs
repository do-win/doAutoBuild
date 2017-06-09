using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.Utils
{
    static class FileUtils
    {

        public static void CreateDir(string _dir) {
            if (!IOUtils.DirExists(_dir))
            {
                Boolean _result = IOUtils.CreateDir(_dir);
                if (!_result)
                {
                    Console.WriteLine(_dir + " 目录创建失败！");
                    throw new Exception(_dir + " 目录创建失败！");
                }
            }

        }


        public static void CopyDirOrFile(string sourcePath, string destinationPath)
        {
            if (IOUtils.DirExists(sourcePath))
            {
                string _tempDir = Path.Combine(destinationPath, Path.GetFileName(sourcePath));
                if (!IOUtils.DirExists(_tempDir))
                {
                    IOUtils.CreateDir(_tempDir);
                }
                CopyDir(sourcePath, _tempDir);
            }
            else if (IOUtils.FileExists(sourcePath))
            {
                //destinationPath 目标路径是一个目录，需要加上sourcePath 文件名
                if (IOUtils.DirExists(destinationPath))
                {
                    destinationPath = Path.Combine(destinationPath, Path.GetFileName(sourcePath));
                }

                //获取文件父目录路径，判断存在，不存在就创建
                string destinationParentPath = Path.GetDirectoryName(destinationPath);
                if (!IOUtils.DirExists(destinationParentPath)) {
                    IOUtils.CreateDir(destinationParentPath);
                }

                File.Copy(sourcePath, destinationPath, true);
            }


        }


        /// <summary>
        /// 复制文件夹（及文件夹下所有子文件夹和文件）
        /// </summary>
        /// <param name="sourcePath">待复制的文件夹路径</param>
        /// <param name="destinationPath">目标路径</param>
        public static void CopyDir(string sourcePath, string destinationPath)
        {
            DirectoryInfo info = new DirectoryInfo(sourcePath);
            
            foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
            {          
                string destName = Path.Combine(destinationPath, fsi.Name);
                if (fsi is System.IO.FileInfo)
                {         //如果是文件，复制文件
                    File.Copy(fsi.FullName, destName, true);
                }
                else       //如果是文件夹，新建文件夹，递归
                {
                    Directory.CreateDirectory(destName);
                    CopyDir(fsi.FullName, destName);
                }
            }
        }

        /// <summary>
        /// 删除文件夹（及文件夹下所有子文件夹和文件）
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void DeleteDir(string directoryPath)
        {
            foreach (string d in Directory.GetFileSystemEntries(directoryPath))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d);     //删除文件   
                }
                else
                    DeleteDir(d);    //删除文件夹
            }
            Directory.Delete(directoryPath);    //删除空文件夹
        }


   
        private static void GetFiles(DirectoryInfo directory, string pattern ,ArrayList _fileLists)
        {

            if (directory.Exists || pattern.Trim() != string.Empty)
            {

                foreach (FileInfo info in directory.GetFiles(pattern))
                {
                    _fileLists.Add(info.FullName.ToString());
                }

                foreach (DirectoryInfo info in directory.GetDirectories())
                {
                    GetFiles(info, pattern, _fileLists);
                }
            }
        }

        /// <summary>
        /// 得某文件夹下所有的文件
        /// </summary>
        /// <param name="directory">文件夹路径</param>
        /// <param name="pattern">搜寻指类型</param>
        /// <returns></returns>
        public static ArrayList GetFiles(string dir, string pattern) {
           ArrayList _fileLists = new ArrayList();
            DirectoryInfo info = new DirectoryInfo(dir);
            GetFiles(info, pattern , _fileLists);
            return _fileLists;
        }

        /// <summary>
        /// 获取当前目录下面的文件
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static ArrayList GetDirs(string dir)
        {
            ArrayList _dirs = new ArrayList();
            DirectoryInfo infos = new DirectoryInfo(dir);
            if (infos != null && infos.Exists) {
                foreach (DirectoryInfo info in infos.GetDirectories())
                {
                    _dirs.Add(info);
                }
            }
            return _dirs;
        }

        /// <summary>
        /// 在一个父目录找到指定子目录的全路径
        /// </summary>
        /// <param name="rootDir">父目录全路径</param>
        /// <param name="findDir">指定目录名</param>
        /// <returns></returns>
        public static ArrayList FindDirFullPath(string rootDir, string findDir) {
            ArrayList _dirLists = new ArrayList();
            if (findDir == null || findDir.Trim() == string.Empty) {
                return null;
            }
            FindDirFullPath(new DirectoryInfo(rootDir), findDir, _dirLists);
            return _dirLists;
        }

        private static void FindDirFullPath(DirectoryInfo sourceDirInfo, string findDir,ArrayList _dirLists)
        {
            if (sourceDirInfo != null && sourceDirInfo.Exists)
            {
                DirectoryInfo[] dInfos = sourceDirInfo.GetDirectories();
                foreach (DirectoryInfo info in dInfos)
                {
                    if (findDir.Equals(info.Name))
                    {
                        _dirLists.Add(info.FullName);
                    }

                    FindDirFullPath(info, findDir, _dirLists);
                }
            }
        }
    }
}
