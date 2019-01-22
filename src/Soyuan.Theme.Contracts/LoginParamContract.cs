using System;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Contracts
{
    public class LoginParamContract
    {
        public string Account { get; set; }

        public string Password { get; set; }

        public Guid? AppId { get; set; }
    }
}
