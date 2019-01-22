using System;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Contracts
{
    /// <summary>
    /// 对接ES数据库封装实体
    /// </summary>
    public class EsContract
    {
        public Guid? fileId { get; set; }
        public Guid? userId { get; set; }
        public Guid? orgId { get; set; }
        public string appId { get; set; }
        public string orgName { get; set; }
        public string fileName { get; set; }
        public string newFileName { get; set; }
        public bool upload { get; set; }
        public string url { get; set; }
        public string thumbnailUrl { get; set; }
        public string tags { get; set; }
        public DateTime? uploadTime { get; set; }
        public DateTime? insertTime { get; set; }

        public string groupId { get; set; }

        public string displayName { get; set; }

        public string fileType { get; set; }

        public string remark { get; set; }


        public int order { get; set; }

        public int themeType { get; set; }


    }
}
