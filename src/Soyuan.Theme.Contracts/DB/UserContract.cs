using System;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Contracts.DB
{
    public class UserContract
    {
        /// <summary>
        /// 用户主键
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户账号
        /// </summary>
        public string UserAccount { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

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
