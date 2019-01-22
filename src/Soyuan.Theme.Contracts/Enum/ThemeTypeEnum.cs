using System;
using System.Collections.Generic;
using System.Text;

namespace Soyuan.Theme.Contracts.Enum
{
    public enum ThemeTypeEnum
    {
        /// <summary>
        /// 事件
        /// </summary>
        ThemeEvent = 1,

        /// <summary>
        /// 资源
        /// </summary>
        ThemeResource = 2,
    }

    public static class ThemeTypeEnumExtension
    {
        /// <summary>
        /// 获取枚举的对应的字符串值
        /// </summary>
        /// <returns></returns>
        public static int GetIntValue(this ThemeTypeEnum DataViewType)
        {
            return ((int)DataViewType);
        }
    }
}
