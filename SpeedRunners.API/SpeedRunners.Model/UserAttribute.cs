using System;

namespace SpeedRunners
{
    /// <summary>
    /// 登录后才能访问
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class UserAttribute : Attribute
    {
    }
}
