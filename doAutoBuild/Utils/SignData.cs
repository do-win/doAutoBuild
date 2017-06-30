using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace doAutoBuild.Utils
{
    public class SignData
    {
        public static string Md5SignObject(string _securityKey, object _objData)
        {
            Dictionary<string, object> _dictData = new Dictionary<string, object>();
            Type t = _objData.GetType();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                object value1 = pi.GetValue(_objData, null);//用pi.GetValue获得值
                string name = pi.Name;
                _dictData[name] = value1;
            }
            return Md5SignDict(_securityKey, _dictData);
        }
        public static string Md5SignDict(string _securityKey, Dictionary<string, object> _dictData)
        {
            ArrayList listSortData = new ArrayList();
            foreach (string _key in _dictData.Keys)
            {
                if (_key == null || _key.Length <= 0) continue;
                if (_key.ToLower() == "sign") continue;
                if (_dictData[_key] is string && (_dictData[_key] as String).Length <= 0) continue;
                if (_dictData[_key] is IList && (_dictData[_key] as IList).Count <= 0) continue;
                bool _finded = false;
                for (int i = 0; i < listSortData.Count; i++)
                {
                    if (string.Compare(_key, ((SignValue)listSortData[i]).Key) > 0) continue;
                    _finded = true;
                    listSortData.Insert(i, new SignValue(_key, _dictData[_key]));
                    break;
                }
                if (!_finded) listSortData.Add(new SignValue(_key, _dictData[_key]));
            }
            StringBuilder _strB = new StringBuilder();
            _strB.Append(_securityKey);
            foreach (SignValue _signVal in listSortData)
            {
                string _strVal = "";
                if (_signVal.Value is IList)
                {
                    IList _list = _signVal.Value as IList;
                    StringBuilder _strBList = new StringBuilder();
                    foreach (var _val in _list)
                    {
                        _strBList.Append(_val.ToString());
                    }
                    _strVal = _strBList.ToString();
                }
                else
                {
                    _strVal = _signVal.Value.ToString();
                }
                _strB.Append(_signVal.Key);
                _strB.Append(_strVal);
            }
            _strB.Append(_securityKey);
            string md5str = TextHelper.GetMd5(_strB.ToString());
            return md5str;
        }
    }

    class SignValue
    {
        public SignValue(string _key, object _val)
        {
            this.Key = _key;
            this.Value = _val;
        }
        public string Key;
        public object Value;
    }
}