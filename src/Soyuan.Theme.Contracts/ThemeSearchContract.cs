using System;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Contracts
{
    public class ThemeSearchContract
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// 从第几条开始
        /// </summary>
        public int? From { get; set; }

        /// <summary>
        /// 每次查询多少条
        /// </summary>
        public int? Size { get; set; }

        /// <summary>
        /// 组织机构ID
        /// </summary>
        public string OrgId { get; set; }

        /// <summary>
        /// 系统ID
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 标签里数据查询
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string FileType { get; set; }
        
        /// <summary>
        /// 不查的文件类型
        /// </summary>
        public string NotFileType { get; set; }

        /// <summary>
        /// 全文检索关键字
        /// </summary>
        public string RetrievalText { get; set; }

        public string ThemeType { get; set; }

        /// <summary>
        /// 分组ID
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// 是否上云
        /// </summary>
        public string IsUpload { get; set; }

    }
}
