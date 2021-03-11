using Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JKCore.Domain
{

    public interface IRepository<T> where T : class
    {
        #region  Simple.CRUD
        /// <summary>
        /// 新增
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns>插入数据的主键</returns>
        Task<TKey> InsertAsync<TKey>(T entity);

        /// <summary>
        /// 通用Host不能自动调用IDispose，需要手动调用
        /// </summary>
        void Dispose();
        Task<int> UpdateAsync(T entity);

        Task<int> DeleteAsync(T entity);

        /// <summary>
        /// 删除-通过条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereConditions"> whereConditions="Where age > 20"</param>
        /// <returns></returns>
        Task<int> DeleteListAsync(object whereConditions);

        /// <summary>
        ///  删除-通过条件 示例：DeleteListAsync<User>("Where age > @Age", new {Age = 20});
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<int> DeleteListAsync(string conditions, object parameters = null);

        /// <summary>
        /// 获取详情通过主键
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T> GetAsync(object id);

        // <summary>
        /// 获取列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereConditions">whereConditions="Where age > 20"</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetListAsync(object whereConditions);

        /// <summary>
        /// 获取列表 示例：GetListAsync<User>("Where age > @Age", new {Age = 20});
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetListAsync(string conditions, object parameters = null);

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
        Task<IEnumerable<T>> GetListPagedAsync(int pageNumber, int rowsPerPage, string conditions, string orderby, object parameters = null);
        #endregion

        #region 特殊查询

        /// <summary>
        /// 列表查询-sql通用查询
        /// </summary>
        /// <typeparam name="TEnity"> 根据sql定义Model</typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="isCommandTypeText"></param>
        /// <returns></returns>
        Task<IEnumerable<TEnity>> SQLQueryAsync<TEnity>(string sql, object param = null, bool isCommandTypeText = true);


        /// <summary>
        /// 获取详情-sql通用查询
        /// </summary>
        /// <typeparam name="TEnity">根据sql定义Model</typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="isCommandTypeText"></param>
        /// <param name="isInvoke">是否是委托方法里的执行sql</param>
        /// <returns></returns>
        Task<TEnity> SQLQueryFirstOrDefaultAsync<TEnity>(string sql, object param = null, bool isCommandTypeText = true, bool isInvoke = false);


        /// <summary>
        /// 列表查询分页-sql通用查询
        /// </summary>
        /// <typeparam name="TEnity"> 根据sql定义Model</typeparam>
        /// <param name="sql"></param>
        /// <param name="param">@totalCoun 返回行数</param>
        /// <param name="isCommandTypeText"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<TEnity>, int>> SQLQueryWithReturnAsync<TEnity>(string sql, object param = null, bool isCommandTypeText = true);

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="isCommandTypeText"></param>
        /// <returns></returns>
        Task<int> SQLExecute(string sql, object param = null, bool isCommandTypeText = true);

        #endregion
    }

}