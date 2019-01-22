using Soyuan.Theme.Contracts;
using Soyuan.Theme.Contracts.DB;
using Soyuan.Theme.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soyuan.Theme.Service.Services.Organization
{
    public interface IOrganizationService
    {
        /// <summary>
        /// 获取机构列表
        /// </summary>
        /// <returns></returns>
        ResultContract<List<OrganizationContract>> GetOrganizations();

        /// <summary>
        /// 根据主键获取后代
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        ResultContract<List<OrganizationContract>> GetDescendantsById(Guid? organizationId);

        /// <summary>
        /// 根据主键获取机构
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        ResultContract<OrganizationContract> GetOrganizationById(Guid? organizationId);

        /// <summary>
        /// 添加机构
        /// </summary>
        /// <param name="organizationEntity"></param>
        /// <returns></returns>
        ResultContract<bool> AddOrganization(OrganizationContract organizationEntity);

        /// <summary>
        /// 更新机构
        /// </summary>
        /// <param name="organizationContract"></param>
        /// <returns></returns>
        ResultContract<bool> UpdateOrganization(OrganizationContract organizationContract);

    }
}
