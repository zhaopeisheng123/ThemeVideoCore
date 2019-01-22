using System;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Core.JWT
{
    public class TokenDataModel
    {
        public Guid? UserId { get; set; }
        public string Account { get; set; }
        public string AppName { get; set; }
        public string FromSystem { get; set; }
    }
}
