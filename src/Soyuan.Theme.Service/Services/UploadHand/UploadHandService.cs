using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Soyuan.Theme.Contracts;
using Soyuan.Theme.Domain;
using Soyuan.Theme.Contracts.DB;
using Soyuan.Theme.Core.Helper;

namespace Soyuan.Theme.Service.Services.UploadHand
{
    public class UploadHandService : IUploadHandService
    {
        private readonly ThemeDBContext _dbContext;

        public UploadHandService(ThemeDBContext db)
        {
            this._dbContext = db;
        }

        /// <summary>
        /// 验证appid
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public bool CheckAppId(string appId)
        {
            var id = Guid.Parse(appId);
            var application = this._dbContext.Application.SingleOrDefault(d => d.Id == id);
            return application != null;
        }

        /// <summary>
        /// 验证组织机构id
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public OrganizationContract CheckOrgInfo(Guid orgId)
        {
            return (from a in this._dbContext.Organization
                    where a.OrganizationId == orgId
                    select new OrganizationContract
                    {
                        CreateTime = a.CreateTime,
                        OrganizationId = a.OrganizationId,
                        OrganizationName = a.OrganizationName,
                        ParentId = a.ParentId
                    }
                    ).SingleOrDefault();
        }



        /// <summary>
        /// 报错文件到本地
        /// </summary>
        /// <param name="data"></param>
        public void SavaTagsFile(UploadContract data)
        {
            try
            {
                FileDbContract file = new FileDbContract();
                file.Id = data.FileId.ToString();
                file.data = SerializeHelper.serializeToString(data);
                this._dbContext.FileDB.Save<FileDbContract>(file.Id, file);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
