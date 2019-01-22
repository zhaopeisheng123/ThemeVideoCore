using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Soyuan.Theme.Contracts;
using Soyuan.Theme.Contracts.DB;

namespace Soyuan.Theme.Service.Services.UploadHand
{
    public interface IUploadHandService
    {
        /// <summary>
        /// 报错文件到本地
        /// </summary>
        /// <param name="file"></param>
        void SavaTagsFile(UploadContract file);

        /// <summary>
        /// 验证appid
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        bool CheckAppId(string appId);

        /// <summary>
        /// 请输入组织机构id
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        OrganizationContract CheckOrgInfo(Guid orgId);
    }
}
