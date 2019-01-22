using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Domain.Entity
{
    public class UserEntity
    {
        /// <summary>
        /// 用户主键
        /// </summary>
        [Key]
        [Required]
        [Display(Name = "UserId")]
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

        /// <summary>
        /// 用户私钥
        /// </summary>
        public string Secretkey { get; set; }

        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool IsLogin { get; set; }
    }
}
