using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Soyuan.Theme.Core.Helper
{
    public static class AutoMapperHelper
    {
        /// <summary>
        ///  类型映射
        /// </summary>
        public static T MapTo<T>(this object obj)
        {
            try
            {
                if (obj == null)
                    return default(T);

                MapperConfiguration configuration = new MapperConfiguration(cfg => cfg.CreateMap(obj.GetType(), typeof(T)));

                var mapper = configuration.CreateMapper();
                return mapper.Map<T>(obj);
            }
            finally
            {
                Mapper.Reset();
            }

        }

        public static T MapTo<T>(this object src, T target)
        {

            Type srcType = src.GetType();//获得该类的Type

            Type targetType = target.GetType();

            //再用Type.GetProperties获得PropertyInfo[],然后就可以用foreach 遍历了
            foreach (PropertyInfo srcPI in srcType.GetProperties())
            {
                string srcName = srcPI.Name;//获得属性的名字,后面就可以根据名字判断来进行些自己想要的操作
                object srcValue = srcPI.GetValue(src, null);//用pi.GetValue获得值


                foreach (PropertyInfo targetPI in targetType.GetProperties())
                {
                    string targetName = targetPI.Name;
                    if (srcName == targetName)
                    {
                        targetPI.SetValue(target, srcValue, null);
                    }
                }
            }

            return target;
        }

        /// <summary>
        /// 集合列表类型映射
        /// </summary>
        public static List<TDestination> MapToList<TDestination>(this IEnumerable<object> source)
        {
            try
            {
                var enumerable = source as object[] ?? source.ToArray();
                if (!enumerable.Any())
                {
                    return new List<TDestination>();
                }

                IMapper mapper = null;
                foreach (var first in enumerable)
                {
                    var type = first.GetType();
                    //  Mapper.Initialize(cfg => cfg.CreateMap(type, typeof(TDestination)));
                    MapperConfiguration configuration = new MapperConfiguration(cfg => cfg.CreateMap(type, typeof(TDestination)));
                    mapper = configuration.CreateMapper();
                    break;
                }
                //  return Mapper.Map<List<TDestination>>(source);
                return mapper.Map<List<TDestination>>(source);
            }
            finally
            {
                Mapper.Reset();
            }
        }

        /// <summary>
        /// 集合列表类型映射
        /// </summary>
        public static List<TDestination> MapToList<TSource, TDestination>(this IEnumerable<TSource> source)
        {
            try
            {
                Mapper.Initialize(cfg => cfg.CreateMap<TSource, TDestination>());
                return Mapper.Map<List<TDestination>>(source);
            }
            finally
            {
                Mapper.Reset();
            }
        }

        /// <summary>
        /// 类型映射
        /// </summary>
        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
            where TSource : class
            where TDestination : class
        {
            try
            {
                if (source == null) return destination;
                Mapper.Initialize(cfg => cfg.CreateMap<TSource, TDestination>());
                return Mapper.Map(source, destination);
            }
            finally
            {
                Mapper.Reset();
            }
        }
    }
}
