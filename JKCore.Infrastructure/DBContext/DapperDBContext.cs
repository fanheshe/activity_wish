using Dapper;
using JKCore.Domain.IData;
using JKCore.Infrastructure.Data;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JKCore.Infrastructure.DBContext
{
    public abstract class DapperDBContext : IContext
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private int? _commandTimeout = null;
        private readonly string _configuration;

        public bool IsTransactionStarted { get; private set; }
        /// <summary>
        /// 如果是委托调用的为true，不释放资源
        /// </summary>
        public bool IsInvoke { get; private set; }

        protected abstract IDbConnection CreateConnection(string connectionString);

        protected DapperDBContext(string configuration)
        {
            _configuration = configuration;

            _connection = CreateConnection(configuration);
            _connection.Open();

            DebugPrint("Connection started.");
        }

        #region Transaction

        public void BeginTransaction()
        {
            if (IsTransactionStarted)
                throw new InvalidOperationException("Transaction is already started.");
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
            _transaction = _connection.BeginTransaction();
            IsTransactionStarted = true;
            DebugPrint("Transaction started.");
        }
        public void BeginTransaction(IsolationLevel level)
        {
            if (IsTransactionStarted)
                throw new InvalidOperationException("Transaction is already started.");
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
            _transaction = _connection.BeginTransaction(level);
            IsTransactionStarted = true;
            DebugPrint("Transaction started.");
        }

        public void Commit()
        {
            if (!IsTransactionStarted)
                throw new InvalidOperationException("No transaction started.");

            _transaction.Commit();
            _transaction = null;

            IsTransactionStarted = false;

            DebugPrint("Transaction committed.");
        }

        public void Rollback()
        {
            if (!IsTransactionStarted)
                throw new InvalidOperationException("No transaction started.");

            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;

            IsTransactionStarted = false;

            DebugPrint("Transaction rollbacked and disposed.");
        }

        #endregion Transaction


        #region  Simple.CRUD
        public async Task<int> UpdateAsync<T>(T entity)
        {
            return await _connection.UpdateAsync(entity, _transaction, _commandTimeout);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns>插入数据的主键</returns>
        public async Task<TKey> InsertAsync<TKey, T>(T entity)
        {
            return await _connection.InsertAsync<TKey, T>(entity, _transaction, _commandTimeout);
        }

        public async Task<int> DeleteAsync<T>(T entity)
        {
            return await _connection.DeleteAsync(entity, _transaction, _commandTimeout);
        }

        /// <summary>
        /// 删除-通过主键
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<int> DeleteAsync(object id)
        {
            return await _connection.DeleteAsync(id, _transaction, _commandTimeout);
        }

        /// <summary>
        /// 删除-通过条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereConditions"> whereConditions="Where age > 20"</param>
        /// <returns></returns>
        public async Task<int> DeleteListAsync<T>(object whereConditions)
        {
            return await _connection.DeleteListAsync<T>(whereConditions, _transaction, _commandTimeout);
        }

        /// <summary>
        ///  删除-通过条件 示例：DeleteListAsync<User>("Where age > @Age", new {Age = 20});
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<int> DeleteListAsync<T>(string conditions, object parameters = null)
        {
            return await _connection.DeleteListAsync<T>(conditions, parameters, _transaction, _commandTimeout);
        }

        /// <summary>
        /// 获取详情通过主键
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(object id)
        {
            return await _connection.GetAsync<T>(id, _transaction, _commandTimeout);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereConditions">whereConditions="Where age > 20"</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetListAsync<T>(object whereConditions)
        {
            return await _connection.GetListAsync<T>(whereConditions, _transaction, _commandTimeout);
        }

        /// <summary>
        /// 获取列表 示例：GetListAsync<User>("Where age > @Age", new {Age = 20});
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetListAsync<T>(string conditions, object parameters = null)
        {
            return await _connection.GetListAsync<T>(conditions, parameters, _transaction, _commandTimeout);
        }

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageNumber"></param>
        /// <param name="rowsPerPage"></param>
        /// <param name="conditions"></param>
        /// <param name="orderby"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetListPagedAsync<T>(int pageNumber, int rowsPerPage, string conditions, string orderby, object parameters = null)
        {
            return await _connection.GetListPagedAsync<T>(pageNumber, rowsPerPage, conditions, orderby, parameters);
        }
        #endregion

        #region Dapper Execute & Query

        public async Task<int> ExecuteAsync(string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            return await _connection.ExecuteAsync(sql, param, _transaction, _commandTimeout, commandType);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            return await _connection.QueryAsync<T>(sql, param, _transaction, _commandTimeout, commandType);
        }
        public async Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            return await _connection.QueryMultipleAsync(sql, param, _transaction, _commandTimeout, commandType);

        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, CommandType commandType = CommandType.Text,bool isInvoke=false)
        {
            IsInvoke = isInvoke;
            return await _connection.QueryFirstOrDefaultAsync<T>(sql, param, _transaction, _commandTimeout, commandType);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id", CommandType commandType = CommandType.Text)
        {

            return await _connection.QueryAsync(sql, map, param, _transaction, true, splitOn, _commandTimeout, commandType);
        }

        #endregion Dapper Execute & Query

        public void Dispose()
        {
            if (IsTransactionStarted)
                return;
            if (IsInvoke)
                return;
            _connection.Close();
            _connection.Dispose();
            _connection = null;

            DebugPrint("Connection closed and disposed.");
        }

        private void DebugPrint(string message)
        {
#if DEBUG
            Debug.Print(">>> UnitOfWorkWithDapper - Thread {0}: {1}", Thread.CurrentThread.ManagedThreadId, message);
#endif
        }
    }
}
