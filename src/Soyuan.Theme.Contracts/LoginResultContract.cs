using Soyuan.Theme.Contracts.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Contracts
{
    public class LoginResultContract
    {
        public string Token { get; set; }

        public UserContract User { get; set; }
    }
}
