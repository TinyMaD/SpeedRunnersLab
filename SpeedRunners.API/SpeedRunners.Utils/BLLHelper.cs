using System;
using System.Data;
using System.Data.SqlClient;

namespace SpeedRunners.Utils
{
    public abstract class BLLHelper<TDAL> : BaseBLL where TDAL : DALBase
    {
        /// <summary>
        /// 数据库访问委托(无返回值)
        /// </summary>
        /// <param name="conn">数据库连接</param>
        protected delegate void DbDelegate(TDAL dal);

        /// <summary>
        /// 数据库访问委托(有返回值)
        /// </summary>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <returns>委托执行结果</returns>
        protected delegate TReturn DbDelegate<TReturn>(TDAL dal);

        private IDbConnection GetConnection => new SqlConnection(AppSettings.GetConfig("ConnectionString"));

        /// <summary>
        /// 没有返回值的 访问数据库方法
        /// </summary>
        /// <param name="delegate">数据库方法</param>
        /// <param name="beginTrans">是否开启事务,默认不开启</param>
        /// <param name="connConfigName">配置连接字符串的Name</param>
        protected void BeginDb(DbDelegate @delegate)
        {
            using IDbConnection conn = GetConnection;
            DbHelper dbhelper = new DbHelper(conn);
            TDAL dal = Activator.CreateInstance(typeof(TDAL), dbhelper) as TDAL;
            try
            {
                @delegate?.Invoke(dal);
            }
            catch (Exception ex)
            {
                dbhelper.RollbackTrans();
                dbhelper.Dispose();
                throw;
            }
        }

        /// <summary>
        /// 有返回值的 访问数据库方法
        /// </summary>
        /// <typeparam name="TReturn">返回类型</typeparam>
        /// <param name="delegate">数据库方法</param>    
        /// <param name="beginTrans">是否开启事务,默认不开启</param>
        /// <param name="connConfigName">配置连接字符串的Name</param>
        /// <returns>方法返回值</returns>
        protected TReturn BeginDb<TReturn>(DbDelegate<TReturn> @delegate)
        {
            using IDbConnection conn = GetConnection;
            DbHelper dbhelper = new DbHelper(conn);
            TDAL dal = Activator.CreateInstance(typeof(TDAL), dbhelper) as TDAL;
            try
            {
                return @delegate.Invoke(dal);
            }
            catch (Exception)
            {
                dbhelper.RollbackTrans();
                dbhelper.Dispose();
                throw;
            }
        }
    }
}
