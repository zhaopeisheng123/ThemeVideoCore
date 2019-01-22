using System;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Contracts
{
    public class ResultContract<T>
    {
        /// <summary>
        /// code 0 成功 -1失败
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 返回消息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        public T Data { get; set; }
    }
}
