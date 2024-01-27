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
            string ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
            var conn = new MySqlConnection(ConnectionString);
            return conn;
        }

       
    }
}
