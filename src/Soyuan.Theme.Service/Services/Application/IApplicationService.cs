using Soyuan.Theme.Contracts;
using Soyuan.Theme.Contracts.DB;
using Soyuan.Theme.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soyuan.Theme.Service.Services.Application
{
    public interface IApplicationService
    {
        /// <summary>
        /// 注册应用
        /// </summary>
        /// <param name="registerApplicationData"></param>
        /// <returns></returns>
        ResultContract<Guid?> RegisterApplication(RegisterApplicationData registerApplicationData);

        /// <summary>
        /// 根据id获取应用
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        ApplicationEntity GetApplicationByID(Guid? appId);

        /// <summary>
        /// 根据查询条件获取应用列表
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        List<ApplicationContract> GetApplication(ApplicationContract contract);


    }
}
