using System;

namespace SpeedRunners
{
    /// <summary>
    /// 未登录用户返回公共数据，已登录用户额外返回定制数据
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class PersonaAttribute : Attribute
    {
    }
}
