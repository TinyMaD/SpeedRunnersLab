using SpeedRunners.Model;
using System;
using System.Reflection;
using System.Text;

namespace SpeedRunners
{
    public static class CommonUtils
    {
        /// <summary>
        /// 深拷贝
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="newobj"></param>
        public static void SetValue<T>(this T obj, T newobj) where T : class
        {
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                prop.SetValue(obj, prop.GetValue(newobj));
            }
        }

        /// <summary>
        /// 生成新AccessToken
        /// </summary>
        /// <returns></returns>
        public static string CreateToken() => Guid.NewGuid().ToString("N") + "&" + DateTime.Now.ToString("s");

        public static StringBuilder WhereIf(this StringBuilder builder, bool condition, string where)
        {
            return condition ? builder.Append(where) : builder;
        }
    }
}
