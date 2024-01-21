using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SpeedRunners.Utils
{
    public class DbHelper : IDisposable
    {
        readonly IDbConnection connection = null;
        IDbTransaction transaction = null;

        /// <summary>
        /// 使用IDbConnection构造一个Dbhelper
        /// </summary>
        /// <param name="connection">数据库连接</param>
        public DbHelper(IDbConnection connection)
        {
            this.connection = connection;
        }

        public void Dispose()
        {
            connection?.Close();
            transaction?.Dispose();
            connection?.Dispose();
        }

        #region 事务操作方法

        public void BeginTrans()
        {
            connection.Open();
            transaction?.Dispose();
            transaction = null;
            transaction = connection.BeginTransaction();
        }

        public void CommitTrans()
        {
            transaction?.Commit();
            transaction?.Dispose();
            transaction = null;
        }

        public void RollbackTrans()
        {
            transaction?.Rollback();
            transaction?.Dispose();
            transaction = null;
        }

        #endregion

        #region 暴露Dapper方法,参考Dapper的方法,不够自己加(复制Dapper方法删除transaction参数)

        /// <summary>
        /// 插入单行数据，返回自增ID
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="model"></param>
        /// <param name="removeFields"></param>
        /// <returns></returns>
        public int Insert<T>(string tableName, T model, string[] removeFields = null)
            where T : new()
        {
            DynamicParameters dyParam = new DynamicParameters();
            return ExecuteScalar<int>(AddParamAndGetInsertSql(tableName, model, removeFields, dyParam, true), dyParam);
        }

        public string AddParamAndGetInsertSql<T>(string tableName, T model, string[] removeFields, DynamicParameters dyParam, bool isNeedKey = false) where T : new()
        {
            StringBuilder colStr = new StringBuilder();
            StringBuilder valueStr = new StringBuilder();
            PropertyInfo[] colList = typeof(T).GetProperties();

            foreach (PropertyInfo col in colList)
            {
                if (removeFields != null && removeFields.Contains(col.Name))
                {
                    continue;
                }
                colStr.AppendFormat("[{0}] ,", col.Name);
                valueStr.AppendFormat("?{0} ,", col.Name);
                dyParam.Add(string.Format("?{0}", col.Name), col.GetValue(model, null));
            }
            string sql = string.Format("INSERT INTO {0}({1}) VALUES({2}); {3}", tableName, colStr.ToString().TrimEnd(','), valueStr.ToString().TrimEnd(','), isNeedKey ? "select @@identity;" : string.Empty);
            return sql;
        }

        /// <summary>
        /// 执行参数化SQL。
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="param">参数</param>
        /// <param name="commandTimeout">命令执行超时前的秒数。</param>
        /// <param name="commandType">这是一个存储的过程还是一个脚本</param>
        /// <returns>受影响的行数。</returns>
        public int Execute(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.Execute(sql, param, this.transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行参数化SQL，选择单个值。(首行首列)
        /// </summary>
        /// <typeparam name="T">要返回的类型。</typeparam>
        /// <param name="sql">sql</param>
        /// <param name="param">参数</param>
        /// <param name="commandTimeout">命令执行超时前的秒数。</param>
        /// <param name="commandType">这是一个存储的过程还是一个脚本</param>
        /// <returns>选择为System.Object的第一个单元格。</returns>
        public T ExecuteScalar<T>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.ExecuteScalar<T>(sql, param, this.transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行参数化SQL，选择单个值。(首行首列)
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="param">参数</param>
        /// <param name="commandTimeout">命令执行超时前的秒数。</param>
        /// <param name="commandType">这是一个存储的过程还是一个脚本</param>
        /// <returns>选择为System.Object的第一个单元格。</returns>
        public object ExecuteScalar(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.ExecuteScalar(sql, param, this.transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 返回具有与列匹配的属性的动态对象序列。
        /// </summary>
        /// <param name="sql">要为查询执行的SQL。</param>
        /// <param name="param">参数</param>
        /// <param name="buffered">是否在内存中缓冲结果。</param>
        /// <param name="commandTimeout">命令执行超时前的秒数。</param>
        /// <param name="commandType">这是一个存储的过程还是一个脚本</param>
        /// <remarks>注意：每一行都可以通过“动态”访问，或者通过转换到IDictionary<String，Object>来访问。</remarks>
        /// <returns>动态对象序列。</returns>
        public IEnumerable<dynamic> Query(string sql, object param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.Query(sql, param, this.transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        /// 使用两个输入类型执行多映射查询.这将返回一个类型，通过map从原始类型组合而成。
        /// </summary>
        /// <typeparam name="TFirst">记录集中的第一个类型。</typeparam>
        /// <typeparam name="TSecond">记录集中的第二个类型。</typeparam>
        /// <typeparam name="TReturn">返回的组合类型。</typeparam>
        /// <param name="sql">要为查询执行的SQL。</param>
        /// <param name="map">将行类型映射到返回类型的函数。</param>
        /// <param name="param">参数</param>
        /// <param name="buffered">是否在内存中缓冲结果。</param>
        /// <param name="splitOn">我们应该拆分和读取第二个对象的字段(默认值：“ID”)。</param>
        /// <param name="commandTimeout">命令执行超时前的秒数。</param>
        /// <param name="commandType">这是一个存储的过程还是一个脚本</param>
        /// <returns>TReturn的序列</returns>
        public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.Query(sql, map, param, this.transaction, buffered, splitOn, commandTimeout, commandType);
        }

        /// <summary>
        /// 使用两个输入类型执行多映射查询.这将返回一个类型，通过map从原始类型组合而成。
        /// </summary>
        /// <typeparam name="TFirst">记录集中的第一个类型。</typeparam>
        /// <typeparam name="TSecond">记录集中的第二个类型。</typeparam>
        /// <typeparam name="TThird">记录集中的第三个类型。</typeparam>
        /// <typeparam name="TReturn">返回的组合类型。</typeparam>
        /// <param name="sql">要为查询执行的SQL。</param>
        /// <param name="map">将行类型映射到返回类型的函数。</param>
        /// <param name="param">参数</param>
        /// <param name="buffered">是否在内存中缓冲结果。</param>
        /// <param name="splitOn">我们应该拆分和读取第二个对象的字段(默认值：“ID”)。</param>
        /// <param name="commandTimeout">命令执行超时前的秒数。</param>
        /// <param name="commandType">这是一个存储的过程还是一个脚本</param>
        /// <returns>TReturn的序列</returns>
        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.Query(sql, map, param, this.transaction, buffered, splitOn, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行查询，返回输入为T的对象序列。
        /// </summary>
        /// <typeparam name="T">要返回的结果类型。</typeparam>
        /// <param name="sql">要为查询执行的SQL。</param>
        /// <param name="param">参数</param>
        /// <param name="buffered">是否在内存中缓冲结果。</param>
        /// <param name="commandTimeout">命令执行超时前的秒数。</param>
        /// <param name="commandType">这是一个存储的过程还是一个脚本</param>
        /// <returns>T的序列</returns>
        public IEnumerable<T> Query<T>(string sql, object param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.Query<T>(sql, param, this.transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行单行查询，返回类型为type的数据。
        /// </summary>
        /// <param name="type">返回类型</param>
        /// <param name="sql">要为查询执行的SQL。</param>
        /// <param name="param">参数</param>
        /// <param name="buffered">是否在内存中缓冲结果。</param>
        /// <param name="commandTimeout">命令执行超时前的秒数。</param>
        /// <param name="commandType">这是一个存储的过程还是一个脚本</param>
        /// <returns>提供的类型的数据序列；如果查询基本类型(int、String等)，则从假设的第一列中查询数据，否则每一行创建一个实例，并假定直接列名称=成员名映射(大小写不敏感)。</returns>
        public IEnumerable<object> Query(Type type, string sql, object param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.Query(type, sql, param, this.transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        /// 使用任意数量的输入类型执行多映射查询。这将返回一个类型，通过map从原始类型组合而成。
        /// </summary>
        /// <typeparam name="TReturn">要返回的结果类型。</typeparam>
        /// <param name="sql">要为查询执行的SQL。</param>
        /// <param name="types">记录集中的类型数组。</param>
        /// <param name="map">将行类型映射到返回类型的函数。</param>
        /// <param name="param">参数</param>
        /// <param name="buffered">是否在内存中缓冲结果。</param>
        /// <param name="splitOn">我们应该拆分和读取第二个对象的字段(默认值：“ID”)。</param>
        /// <param name="commandTimeout">命令执行超时前的秒数。</param>
        /// <param name="commandType">这是一个存储的过程还是一个脚本</param>
        /// <returns>TReturn的序列</returns>
        public IEnumerable<TReturn> Query<TReturn>(string sql, Type[] types, Func<object[], TReturn> map, object param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.Query<TReturn>(sql, types, map, param, this.transaction, buffered, splitOn, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行单行查询，返回输入为T的数据。
        /// </summary>
        /// <typeparam name="T">要返回的结果类型。</typeparam>
        /// <param name="sql">要为查询执行的SQL。</param>
        /// <param name="param">参数</param>
        /// <param name="commandTimeout">命令执行超时前的秒数。</param>
        /// <param name="commandType">这是一个存储的过程还是一个脚本</param>
        /// <returns>T的序列</returns>
        public T QueryFirst<T>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.QueryFirst<T>(sql, param, this.transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行单行查询，返回输入为T的数据。
        /// </summary>
        /// <typeparam name="T">要返回的结果类型。</typeparam>
        /// <param name="sql">要为查询执行的SQL。</param>
        /// <param name="param">参数</param>
        /// <param name="commandTimeout">命令执行超时前的秒数。</param>
        /// <param name="commandType">这是一个存储的过程还是一个脚本</param>
        /// <returns>T的序列</returns>
        public T QueryFirstOrDefault<T>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.QueryFirstOrDefault<T>(sql, param, this.transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 查询多个结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="buffered"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public SqlMapper.GridReader QueryMultiple(string sql, object param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.QueryMultiple(sql, param, this.transaction, commandTimeout, commandType);
        }

        #endregion
    }
}
