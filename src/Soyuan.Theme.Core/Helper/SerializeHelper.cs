using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Soyuan.Theme.Core.Helper
{
    public class SerializeHelper
    {
        /// <summary>
        /// 对数据进行序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string serializeToString(object obj, bool isCamelCase = false, bool ignore = false)
        {
            var jSetting = new JsonSerializerSettings();
            if (isCamelCase)
                jSetting.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            if (ignore)
                jSetting.NullValueHandling = NullValueHandling.Ignore;

            jSetting.DateFormatString = "yyyy-MM-dd HH:mm:ss";

            return JsonConvert.SerializeObject(obj, jSetting);

        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T deserializeToObject<T>(string str) where T : new()
        {
            if (string.IsNullOrWhiteSpace(str)) return new T();
            return JsonConvert.DeserializeObject<T>(str);
        }
        /// <summary>
        /// 在Json字符串中寻找指定字符集合
        /// </summary>
        /// <param name="jsonStr">Json字符串</param>
        /// <param name="searchStr">指定Key值</param>
        /// <returns></returns>
        public static List<string> getValueListFromJson(string jsonStr, string searchStr)
        {
            List<string> fieldList = new List<string>();
            string[] tempStrs = jsonStr.Split(new string[] { searchStr + "\":\"" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < tempStrs.Length; i++)
            {
                fieldList.Add(tempStrs[i].Substring(0, tempStrs[i].IndexOf('"')));
            }
            return fieldList;
        }


        public static string getValueByObj(string jsonStr, string key)
        {
            if (!string.IsNullOrWhiteSpace(jsonStr))
            {
                var obj = JsonConvert.DeserializeObject(jsonStr);
                JObject _jsonObj = obj as JObject;
                if (_jsonObj == null) return "";
                return (string)_jsonObj[key];
            }
            else
            {
                return "";
            }
        }

        public static string GetJsonByKey(JObject obj, string key)
        {
            if (obj == null) return null;
            if (string.IsNullOrWhiteSpace(key)) return null;
            JToken value;
            if (obj.TryGetValue(key, StringComparison.InvariantCultureIgnoreCase, out value))
            {
                return value.Value<string>();
            }

            return null;
        }
    }
}
