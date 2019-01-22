using System;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Contracts.DB
{
    public class TagContract
    {
        /// <summary>
        /// 标签主键
        /// </summary>
        public Guid? TagId { get; set; }

        /// <summary>
        /// 标签名称
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// 标签说明
        /// </summary>
        public string TagDescription { get; set; }

        /// <summary>
        /// 机构主键
        /// </summary>
        public Guid? OrganizationId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; } 
    }
}
