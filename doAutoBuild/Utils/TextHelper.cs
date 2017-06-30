using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace doAutoBuild.Utils
{
    //文本处理工具
    public static class TextHelper
    {
        //--------------------------------------------------------------------
        //md5
        public static string GetMd5(string data)
        {
            return GetMd5(Encoding.UTF8.GetBytes(data));
        }
        public static string GetMd5(byte[] buffer)
        {
            MD5 md5 = MD5.Create();
            byte[] retVal = md5.ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        public static string GetMd5(Stream filestream)
        {
            MD5 md5 = MD5.Create();
            byte[] retVal = md5.ComputeHash(filestream);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }

        //--------------------------------------------------------------------
        //base64
        
        /// <summary>
        /// 将其它编码的字符串转换成Base64编码的字符串
        /// </summary>
        public static string EncodeBase64String(string source)
        {
            //如果字符串为空或者长度为0则抛出异常
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source", "不能为空。");
            }
            else
            {
                //将字符串转换成UTF-8编码的字节数组
                byte[] buffer = Encoding.UTF8.GetBytes(source);
                //将UTF-8编码的字节数组转换成Base64编码的字符串
                string result = Convert.ToBase64String(buffer);
                return result;
            }
        }
        /// <summary>
        /// 将Base64编码的字符串转换成其它编码的字符串
        /// </summary>
        public static string DecodeBase64String(string result)
        {
            //如果字符串为空或者长度为0则抛出异常
            if (string.IsNullOrEmpty(result))
            {
                throw new ArgumentNullException("result", "不能为空。");
            }
            else
            {
                //将字符串转换成Base64编码的字节数组
                byte[] buffer = Convert.FromBase64String(result);
                //将字节数组转换成UTF-8编码的字符串
                string source = Encoding.UTF8.GetString(buffer);
                return source;
            }
        }

        //--------------------------------------------------------------------
        //sha1
        /// <summary>
        /// sha1加密方法
        /// </summary>
        /// <param name="mystr"></param>
        /// <returns></returns>
        public static string GetSha1(string mystr)
        {
            //建立SHA1对象
            SHA1 sha = new SHA1CryptoServiceProvider();
            //将mystr转换成byte[]
            UTF8Encoding enc = new UTF8Encoding();
            byte[] dataToHash = enc.GetBytes(mystr);
            //Hash运算
            byte[] dataHashed = sha.ComputeHash(dataToHash);
            //将运算结果转换成string
            string hash = BitConverter.ToString(dataHashed).Replace("-", "").ToLower();

            return hash;
        }


        //--------------------------------------------------------------------
        //DES
        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="data">加密数据</param>
        /// <param name="key">8位字符的密钥字符串</param>
        /// <param name="iv">8位字符的初始化向量字符串</param>
        /// <returns></returns>
        public static string DESEncrypt(string data, string key, string iv)
        {
            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(key);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(iv);

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            int i = cryptoProvider.KeySize;
            MemoryStream ms = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);

            StreamWriter sw = new StreamWriter(cst);
            sw.Write(data);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();
            //16进制形式
            //  return BitConverter.ToString(ms.GetBuffer(),0,(int)ms.Length);

            return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="data">解密数据</param>
        /// <param name="key">8位字符的密钥字符串(需要和加密时相同)</param>
        /// <param name="iv">8位字符的初始化向量字符串(需要和加密时相同)</param>
        /// <returns></returns>
        public static string DESDecrypt(string data, string key, string iv)
        {
            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(key);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(iv);

            byte[] byEnc;
            try
            {
                byEnc = Convert.FromBase64String(data);
            }
            catch
            {
                return null;
            }

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream(byEnc);
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cst);
            return sr.ReadToEnd();
        }


        //--------------------------------------------------------------------
        //3DES
        #region 3DES 加密解密

        /// <summary>
        /// 把字符串转换为字节数组
        /// </summary>
        /// <param name="strKey">源串</param>
        public static byte[] covertStringToHex(string strKey)
        {
            int strLen = strKey.Length;
            //字节长度是字符串的一半
            int byLen = strKey.Length >> 1;

            byte[] key = new byte[byLen];
            for (int i = 0, j = 0; i < byLen && j < strLen; i++)
            {
                key[i] = (byte)(convertCharToHex(strKey[j++]) << 4);
                key[i] += (byte)(convertCharToHex(strKey[j++]) & 0xF);
            }
            return key;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static int convertCharToHex(char ch)
        {
            if ((ch >= '0') && (ch <= '9'))
                return ch - 0x30;
            else if ((ch >= 'A') && (ch <= 'F'))
                return ch - 'A' + 10;
            else if ((ch >= 'a') && (ch <= 'f'))
                return ch - 'a' + 10;
            else
                return -1;
        }

        public static string DES3Encrypt(string data, string key)
        {
            TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider();
            DES.Key = covertStringToHex(key);
            DES.Mode = CipherMode.ECB;
            DES.Padding = PaddingMode.PKCS7;
            ICryptoTransform DESEncrypt = DES.CreateEncryptor();
            byte[] Buffer = Encoding.UTF8.GetBytes(data);
            return getFormattedText(DESEncrypt.TransformFinalBlock(Buffer, 0, Buffer.Length));
        }
        private static String getFormattedText(byte[] bytes)
        {
            char[] HEX_DIGITS = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
            int len = bytes.Length;
            StringBuilder buf = new StringBuilder(len * 2);
            //把密文转换成十六进制的字符串形式		
            for (int j = 0; j < len; j++)
            {
                buf.Append(HEX_DIGITS[(bytes[j] >> 4) & 0x0f]);
                buf.Append(HEX_DIGITS[bytes[j] & 0x0f]);
            }
            return buf.ToString();
        }
        public static string DES3Decrypt(string data, string key)
        {
            TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider();

            DES.Key = ASCIIEncoding.ASCII.GetBytes(key);
            DES.Mode = CipherMode.CBC;
            DES.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

            ICryptoTransform DESDecrypt = DES.CreateDecryptor();

            string result = "";
            try
            {
                byte[] Buffer = Convert.FromBase64String(data);
                result = ASCIIEncoding.ASCII.GetString(DESDecrypt.TransformFinalBlock(Buffer, 0, Buffer.Length));
            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
        }

        #endregion

        //--------------------------------------------------------------------
        //随机数
        public static string GenerateRandomChars(int length)
        {
            var codeBuilder = new StringBuilder();
            Random Rnd = new Random();
            for (var i = 0; i < length; i++)
            {
                int _pos = Rnd.Next(0, CodeChars.Length);
                codeBuilder.Append(CodeChars[_pos]);
            }
            return codeBuilder.ToString();
        }
        private const string CodeChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string GenerateRandomNumbers(int length)
        {
            string result = "";
            System.Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                result += random.Next(10).ToString();
            }
            return result;
        }


        //---------------------------------------------------------------------------
        //类型转换
        /// <summary>
        /// 将字符串转换成整数
        /// </summary>
        public static int StrToInt(string str, int defaultVal)
        {
            if (str == null || str.Length <= 0) return defaultVal;
            int i;
            try
            {
                i = int.Parse(str.Trim());
            }
            catch
            {
                i = defaultVal;
            }
            return i;
        }

        /// <summary>
        /// 将字符串转换成长整数
        /// </summary>
        public static long StrToLong(string str, long defaultVal)
        {
            if (str == null || str.Length <= 0) return defaultVal;
            long i;
            try
            {
                i = long.Parse(str.Trim());
            }
            catch
            {
                i = defaultVal;
            }
            return i;
        }

        /// <summary>
        /// 将字符串转换成double
        /// </summary>
        public static double StrToDouble(string str, double defaultVal)
        {
            if (str == null || str.Length <= 0) return defaultVal;
            double i;
            try
            {
                i = double.Parse(str.Trim());
            }
            catch
            {
                i = defaultVal;
            }
            return i;
        }

        /// <summary>
        /// 将字符串转换成float
        /// </summary>
        public static float StrToFloat(string str, float defaultVal)
        {
            if (str == null || str.Length <= 0) return defaultVal;
            float i;
            try
            {
                i = float.Parse(str.Trim());
            }
            catch
            {
                i = defaultVal;
            }
            return i;
        }
        public static bool StrToBool(string str, bool defaultVal)
        {
            if (str == null || str.Length <= 0) return defaultVal;
            return str.Equals("true");
        }

    }
}