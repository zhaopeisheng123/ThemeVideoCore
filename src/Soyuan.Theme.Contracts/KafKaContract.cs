using System;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Contracts
{
    public class KafKaContract
    {
        /// <summary>
        /// 消息id
        /// </summary>
        public string MsgId { get; set; }

        /// <summary>
        /// 消息(UpladContract)
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public KafkaMsgCodeEnum MsgCode { get; set; }
    }

    public enum KafkaMsgCodeEnum
    {
        AddList = 5,
        Add = 0,
        Update = 1,
        InsertError = 2,
        InsertSuccess = 3,
        InsertListSuccess = 6
    }

    public static class KafkaMsgCodeEnumExtension
    {
        /// <summary>
        /// 获取枚举的对应的字符串值
        /// </summary>
        /// <returns></returns>
        public static int GetIntValue(this KafkaMsgCodeEnum DataViewType)
        {
            return ((int)DataViewType);
        }
    }
}
