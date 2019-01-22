using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Soyuan.Theme.Domain.Entity
{
    public class UserRelateRoleEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        [Required]
        public Guid? Id { get; set; }

        /// <summary>
        /// 用户主键
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 角色主键
        /// </summary>
        public Guid? RoleId { get; set; }
    }
}
