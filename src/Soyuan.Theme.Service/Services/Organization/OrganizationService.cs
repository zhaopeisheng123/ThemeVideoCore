using Soyuan.Theme.Contracts;
using Soyuan.Theme.Contracts.DB;
using Soyuan.Theme.Core.Helper;
using Soyuan.Theme.Domain;
using Soyuan.Theme.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soyuan.Theme.Service.Services.Organization
{
    public class OrganizationService : IOrganizationService
    {
        private readonly ThemeDBContext _dbContext;

        public OrganizationService(ThemeDBContext db)
        {
            this._dbContext = db;
        }

        /// <summary>
        /// 获取机构列表
        /// </summary>
        /// <returns></returns>
        public ResultContract<List<OrganizationContract>> GetOrganizations()
        {
            var result = new ResultContract<List<OrganizationContract>>();
            result.Data = this._dbContext.Organization.ToList().MapToList<OrganizationContract>();
            return result;
        }

        /// <summary>
        /// 根据主键获取后代
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public ResultContract<List<OrganizationContract>> GetDescendantsById(Guid? organizationId)
        {
            var result = new ResultContract<List<OrganizationContract>>();

            var top = this._dbContext.Organization.FirstOrDefault(e => e.OrganizationId == organizationId);
            var descendants = GetDescendantes(top.OrganizationId, this._dbContext.Organization.ToList(), new List<OrganizationEntity>());
            result.Data = descendants.MapToList<OrganizationContract>();
            return result;

        }

        /// <summary>
        /// 获取后代机构
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="source"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private List<OrganizationEntity> GetDescendantes(Guid? parentId, List<OrganizationEntity> source, List<OrganizationEntity> result)
        {
            var child = source.Where(e => e.ParentId == parentId).ToList();
            if (child != null && child.Count > 0)
            {
                result.AddRange(child);
                foreach (var item in child)
                {
                    GetDescendantes(item.OrganizationId, source, result);
                }
            }
            return result;
        }

        /// <summary>
        /// 根据主键获取机构
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public ResultContract<OrganizationContract> GetOrganizationById(Guid? organizationId)
        {
            var result = new ResultContract<OrganizationContract>() { Code = 0, Msg = "" };
            var data = this._dbContext.Organization.Find(organizationId).MapTo<OrganizationContract>();
            result.Data = data;
            return result;
        }

        /// <summary>
        /// 增加机构
        /// </summary>
        /// <param name="organizationContract"></param>
        /// <returns></returns>
        public ResultContract<bool> AddOrganization(OrganizationContract organizationContract)
        {
            var result = new ResultContract<bool>() { Code = 0, Msg = "", Data = true };
            var organizationEntity = organizationContract.MapTo<OrganizationEntity>();

            organizationEntity.OrganizationId = Guid.NewGuid();
            organizationEntity.CreateTime = DateTime.Now;
            this._dbContext.Organization.Add(organizationEntity);
            this._dbContext.SaveChanges();

            return result;
        }

        /// <summary>
        /// 更改机构
        /// </summary>
        /// <param name="organizationContract"></param>
        /// <returns></returns>
        public ResultContract<bool> UpdateOrganization(OrganizationContract organizationContract)
        {
            var result = new ResultContract<bool>() { Code = 0, Msg = "", Data = true };
            var organizationEntity = organizationContract.MapTo<OrganizationEntity>();
            this._dbContext.Organization.Update(organizationEntity);
            this._dbContext.SaveChanges();

            return result;
        }
 

    }
}
