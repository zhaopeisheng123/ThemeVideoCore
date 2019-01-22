using System;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Contracts.DB
{
    public class ApplicationContract
    {
        /// <summary>
        /// 主键
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// 密钥
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// 系统名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public int? isDelete { get; set; }

        /// <summary>
        /// 组织机构id
        /// </summary>
        public Guid? OrganizationId { get; set; }
    }
}
