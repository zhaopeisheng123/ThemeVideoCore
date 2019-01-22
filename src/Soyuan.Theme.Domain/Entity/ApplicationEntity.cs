using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Soyuan.Theme.Domain.Entity
{
    public class ApplicationEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        [Required]
        [Display(Name = "Id")]
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
        /// 是否删除
        /// </summary>
        public int? isDelete { get; set; }

        /// <summary>
        /// 机构主键
        /// </summary>
        public Guid? OrganizationId { get; set; }
    }
}
