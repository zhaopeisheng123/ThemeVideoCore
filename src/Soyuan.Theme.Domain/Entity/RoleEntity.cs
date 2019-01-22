using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Soyuan.Theme.Domain.Entity
{
    public class RoleEntity
    {
        /// <summary>
        /// 角色主键
        /// </summary>
        /// <summary>
        /// 机构主键
        /// </summary>
        [Key]
        [Required]
        [Display(Name = "RoleId")]
        public Guid? RoleId { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; set; }
    }
}
