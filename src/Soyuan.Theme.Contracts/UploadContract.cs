using System;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Contracts
{
    public class UploadContract : BaseUploadContract
    {
        /// <summary>
        /// fileId 手动生成
        /// </summary>
        public Guid? FileId { get; set; }

        /// <summary>
        /// 组织机构名称
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 上传原来的文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 重命名文件（fileId + 后缀名）
        /// </summary>

        public string NewFileName { get; set; }

        /// <summary>
        /// 是否上云
        /// </summary>
        public bool IsUpload { get; set; }

        /// <summary>
        /// 文件地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime? UploadTime { get; set; }

        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime? InsertTime { get; set; }

        /// <summary>
        /// 是否是上传失败的文件
        /// </summary>
        public bool IsFailure { get; set; }

        /// <summary>
        /// 分组id（批量文件上传 组id）
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// 缩略图
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// 资源排序
        /// </summary>
        public int Order { get; set; }
    }
}
