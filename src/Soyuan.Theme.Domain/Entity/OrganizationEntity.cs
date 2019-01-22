using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Domain.Entity
{
    public class OrganizationEntity
    {
        /// <summary>
        /// 机构主键
        /// </summary>
        [Key]
        [Required]
        [Display(Name = "OrganizationId")]
        public Guid? OrganizationId { get; set; }


        /// <summary>
        /// 机构名称
        /// </summary>
        public string OrganizationName { get; set; }


        /// <summary>
        /// 父机构主键
        /// </summary>
        public Guid? ParentId { get; set; }


        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }


        /// <summary>
        /// 机构编号(海关编号)
        /// </summary>
        public string OrganizationCode { get; set; }


    }
}
