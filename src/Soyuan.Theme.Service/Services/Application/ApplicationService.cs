using Soyuan.Theme.Contracts;
using Soyuan.Theme.Contracts.DB;
using Soyuan.Theme.Domain;
using Soyuan.Theme.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Soyuan.Theme.Service.Services.Application
{
    public class ApplicationService : IApplicationService
    {
        private readonly ThemeDBContext _dbContext;

        public ApplicationService(ThemeDBContext db)
        {
            this._dbContext = db;
        }

        /// <summary>
        /// 根据查询参数获取应用列表
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public List<ApplicationContract> GetApplication(ApplicationContract contract)
        {
            var query = (from a in this._dbContext.Application
                         select new ApplicationContract
                         {
                             AppName = a.AppName,
                             AppSecret = a.AppSecret,
                             Id = a.Id,
                             CreateTime = a.CreateTime,
                             isDelete = a.isDelete,
                             OrganizationId = a.OrganizationId
                         });
            if (contract.Id != null)
                query = query.Where(t => t.Id == contract.Id);
            if (contract.AppName != null)
                query = query.Where(t => t.AppName == contract.AppName);
            return query.ToList();
        }

        /// <summary>
        /// 根据id获取应用
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public ApplicationEntity GetApplicationByID(Guid? applicationId)
        {
            return this._dbContext.Application.FirstOrDefault(e => e.Id == applicationId);
        }

        /// <summary>
        /// 注册应用
        /// </summary>
        /// <param name="registerApplicationData"></param>
        /// <returns></returns>
        public ResultContract<Guid?> RegisterApplication(RegisterApplicationData registerApplicationData)
        {
            var result = new ResultContract<Guid?>();
            var applicationEntity = new ApplicationEntity()
            {
                Id = Guid.NewGuid(),
                AppName = registerApplicationData.AppName,
                AppSecret = Guid.NewGuid().ToString(),
                CreateTime = DateTime.Now,
            };
            //添加应用
            this._dbContext.Application.Add(applicationEntity);

            if (registerApplicationData.Tags != null && registerApplicationData.Tags.Count > 0)
            {
                List<TagEntity> tags = new List<TagEntity>();
                foreach (var item in registerApplicationData.Tags)
                {
                    tags.Add(new TagEntity()
                    {
                        TagId = Guid.NewGuid(),
                        TagName = item.TagName,
                        TagDescription = item.TagDescription,
                        OrganizationId = item.OrganizationId,
                        CreateTime = DateTime.Now
                    });
                }
                //添加标签
                this._dbContext.AddRange(tags);
            }

            this._dbContext.SaveChanges();
            result.Data = applicationEntity.Id;
            return result;
        }
    }
}
