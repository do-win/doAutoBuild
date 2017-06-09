using doAutoBuild.Utils;
using Newtonsoft.Json.Linq;
using Qiniu.Common;
using Qiniu.Http;
using Qiniu.IO;
using Qiniu.IO.Model;
using Qiniu.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.Storage
{
    public class QiniuManager
    {
        private string AK = null;
        private string SK = null;
        private string bucket = null;
        private string DNS = null;

        public QiniuManager()
        {

            string _qiniuConfigPath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Environment.CurrentDirectory)), Constants.ConfigFile);
            string _qiniuContent = IOUtils.GetUTF8String(_qiniuConfigPath);
            try
            {
                JObject _qiniuContentObj = JObject.Parse(_qiniuContent);
              
                this.AK = _qiniuContentObj.GetValue("AK").ToString();
                if (this.AK == null)
                {
                    throw new Exception("配置文件中未定义Ak");
                }
                this.SK = _qiniuContentObj.GetValue("SK").ToString();
                if (this.SK == null)
                {
                    throw new Exception("配置文件中未定义SK");
                }
                this.bucket = _qiniuContentObj.GetValue("bucket").ToString();
                if (this.bucket == null)
                {
                    throw new Exception("配置文件中未定义bucket");
                }

                this.DNS = _qiniuContentObj.GetValue("DNS").ToString();
                if (this.DNS == null)
                {
                    throw new Exception("配置文件中未定义DNS");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Qiniu.config 配置内容有误！");
                throw;
            }
        }

        private static QiniuManager _qiniuManager;
        public static QiniuManager Instance()
        {
            if (_qiniuManager == null)
            {
                _qiniuManager = new QiniuManager();
            }
            return _qiniuManager;
        }


        public string getAccessUrl(string _path)
        {
            return DNS + _path;
        }

        public byte[] readFile(string _path)
        {
            byte[] _data = null;
            string _fileFullPath = DNS + _path;
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                _data = wc.DownloadData(_fileFullPath);
            }
            return _data;
        }

        //写入七牛服务
        public string writeFile(string _path, byte[] _content)
        {
            //string _fileFullPath = _moduleId + "/" + _path;
            var result = uploadByte(_path, _content);
            if (result.Code != 200) throw new Exception("上传文件异常,code=" + result.Code + "test=" + result.Text);
            return _path;

        }

        public HttpResult uploadByte(string saveKeys, byte[] data)
        {
            Config.AutoZone(AK, bucket, false);
            Mac mac = new Mac(AK, SK);
            //string saveKey = saveKeys;
            PutPolicy putPolicy = new PutPolicy();
            // 如果需要设置为"覆盖"上传(如果云端已有同名文件则覆盖)，请使用 SCOPE = "BUCKET:KEY"
            putPolicy.Scope = bucket + ":" + saveKeys;
            //putPolicy.Scope = bucket;
            // 上传策略有效期(对应于生成的凭证的有效期)          
            putPolicy.SetExpires(100000);
            // 上传到云端多少天后自动删除该文件，如果不设置（即保持默认默认）则不删除
            //putPolicy.DeleteAfterDays = 100000;
            string jstr = putPolicy.ToJsonString();
            string token = Auth.CreateUploadToken(mac, jstr);
            FormUploader fu = new FormUploader();
            HttpResult result = fu.UploadData(data, saveKeys, token);
            return result;
        }


    }
}
