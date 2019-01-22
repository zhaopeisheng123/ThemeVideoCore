using Soyuan.Theme.Contracts.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Contracts
{
    public class RegisterApplicationData
    {

        public Guid? UserId { get; set; }

        public Guid? OrganizationId { get; set; }

        public string AppName { get; set; }

        public List<TagContract> Tags { get; set; }

    }
}
