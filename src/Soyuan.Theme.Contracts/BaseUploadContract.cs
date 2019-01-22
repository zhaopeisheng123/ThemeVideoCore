using System;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Contracts
{
    public class BaseUploadContract
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 组织机构id
        /// </summary>
        public Guid? OrgId { get; set; }


        /// <summary>
        /// 系统id
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 显示的名字
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 标签(json)
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// 资源类型
        /// </summary>
        public int ThemeType { get; set; }


        /// <summary>
        /// 断点续传 
        /// </summary>
        public string Name { get; set; }


    }
}
