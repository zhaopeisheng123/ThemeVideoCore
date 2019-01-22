using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Core.Helper
{
    public class JsonDatabase : FileDatabase
    {
        /// <summary>
        /// JSON文件数据库
        /// </summary>
        /// <param name="directory">数据库文件所在目录</param>
        public JsonDatabase(string directory)
          : base(directory)
        {
            FileExtension = @"json";
        }

        protected override TDocument Deserialize<TDocument>(string data)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException("data");

            return JsonConvert.DeserializeObject<TDocument>(data);
        }

        protected override string Serialize(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return JsonConvert.SerializeObject(value);
        }
    }
}
