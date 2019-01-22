using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Soyuan.Theme.Contracts.DB;
using Soyuan.Theme.Service.Services.Organization;

namespace Soyuan.Theme.Service.Controllers
{

    [Authorize]
    [Produces("application/json")]
    [Route("api/Organization")]
    public class OrganizationController : Controller
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        /// <summary>
        /// 获取机构列表
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(GetOrganizations))]
        public async Task<Contracts.ResultContract<List<OrganizationContract>>> GetOrganizations()
        {
            try
            {
                return _organizationService.GetOrganizations();
            }
            catch (Exception e)
            {
                return new Contracts.ResultContract<List<OrganizationContract>>() { Code = -1, Msg = "服务异常" };
            }
        }

        /// <summary>
        /// 根据id获取机构
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        [HttpGet(nameof(GetOrganizationById))]
        public async Task<Contracts.ResultContract<OrganizationContract>> GetOrganizationById(Guid? organizationId)
        {
            try
            {
                return _organizationService.GetOrganizationById(organizationId);
            }
            catch (Exception)
            {
                return new Contracts.ResultContract<OrganizationContract>() { Code = -1, Msg = "服务异常" };
            }
        }

        /// <summary>
        /// 新增机构
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        [HttpPost(nameof(AddOrganization))]
        public async Task<Contracts.ResultContract<bool>> AddOrganization([FromBody]OrganizationContract organization)
        {
            try
            {
                return _organizationService.AddOrganization(organization);
            }
            catch (Exception)
            {
                return new Contracts.ResultContract<bool>() { Code = -1, Msg = "服务异常" };
            }
        }



    }
}