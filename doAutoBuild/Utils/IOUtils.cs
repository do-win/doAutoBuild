using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.Utils
{
    static class IOUtils
    {

        public static string GetUTF8String(string _fileFullName)
        {
            StreamReader streamReader = new StreamReader(_fileFullName, Encoding.UTF8);
            String line;
            StringBuilder sb = new StringBuilder();
            while ((line = streamReader.ReadLine()) != null)
            {
                sb.Append(line.ToString());
            }
            streamReader.Close();
            return sb.ToString();
        }

        public static Boolean FileExists(string _fileFullName) {
            return File.Exists(_fileFullName);
        }

        public static Boolean DirExists(string _fileFullName)
        {
            return Directory.Exists(_fileFullName);
        }

        public static Boolean CreateDir(string _filePath) {
            Directory.CreateDirectory(_filePath);
            return DirExists(_filePath);

        }

        public static void WriteUTF8String(string _fileFullName,string _content) {
            File.WriteAllText(_fileFullName,_content, Encoding.UTF8);
        }

        public static void WriteString(string _fileFullName, string _content)
        {
            File.WriteAllText(_fileFullName, _content);
        }
    }
}
