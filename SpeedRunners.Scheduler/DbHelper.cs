using Dapper;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SpeedRunners.Scheduler
{
    /// <summary>
    /// 数据库配置等帮助类
    /// </summary>
    public static class DbHelper
    {
        /// <summary>
        /// 获取数据库链接
        /// </summary>
        /// <returns></returns>
        public static MySqlConnection GetConnection()
        {
            string connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            var builder = new MySqlConnectionStringBuilder(connectionString);

            if (!HasConnectionOption(connectionString, "SslMode", "Ssl Mode"))
            {
                builder.SslMode = MySqlSslMode.None;
            }

            if (!HasConnectionOption(connectionString, "Connection Timeout", "Connect Timeout"))
            {
                builder.ConnectionTimeout = 5;
            }

            if (!HasConnectionOption(connectionString, "Default Command Timeout", "Command Timeout"))
            {
                builder.DefaultCommandTimeout = 30;
            }

            var conn = new MySqlConnection(builder.ConnectionString);
            return conn;
        }

        private static bool HasConnectionOption(string connectionString, params string[] keys)
        {
            return connectionString
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split(new[] { '=' }, 2)[0].Trim())
                .Any(key => keys.Any(expected => string.Equals(key, expected, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
