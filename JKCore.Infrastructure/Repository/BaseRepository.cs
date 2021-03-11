using Dapper;
using JKCore.Domain;
using JKCore.Infrastructure.DBContext;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JKCore.Infrastructure.Repository
{
    public class BaseRepository<T> : IRepository<T> where T : class
    {
        private readonly DapperDBContext _context;
        public BaseRepository(DapperDBContext context)
        {
            _context = context;
        }

        public void Dispose() {
            _context.Dispose();
        }

        public Task<int> DeleteAsync(T entity)
        {
            return _context.DeleteAsync(entity);
        }

        public Task<int> DeleteListAsync(object whereConditions)
        {
            return _context.DeleteListAsync<T>(whereConditions);
        }

        public Task<int> DeleteListAsync(string conditions, object parameters = null)
        {
            return _context.DeleteListAsync<T>(conditions, parameters);
        }

        public Task<T> GetAsync(object id)
        {
            return _context.GetAsync<T>(id);
        }

        public Task<IEnumerable<T>> GetListAsync(object whereConditions)
        {
            return _context.GetListAsync<T>(whereConditions);
        }

        public Task<IEnumerable<T>> GetListAsync(string conditions, object parameters = null)
        {
            return _context.GetListAsync<T>(conditions, parameters);
        }

        public Task<IEnumerable<T>> GetListPagedAsync(int pageNumber, int rowsPerPage, string conditions, string orderby, object parameters = null)
        {
            return _context.GetListPagedAsync<T>(pageNumber, rowsPerPage, conditions, orderby, parameters);
        }

        public Task<TKey> InsertAsync<TKey>(T entity)
        {
            return _context.InsertAsync<TKey, T>(entity);
        }

        public Task<int> UpdateAsync(T entity)
        {
            return _context.UpdateAsync<T>(entity);
        }

        #region �����ѯ

        /// <summary>
        /// �б��ѯ-sqlͨ�ò�ѯ
        /// </summary>
        /// <typeparam name="TEnity"> ����sql����Model</typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="isCommandTypeText"></param>
        /// <returns></returns>
        public Task<IEnumerable<TEnity>> SQLQueryAsync<TEnity>(string sql, object param = null,bool isCommandTypeText = true)
        {
            return _context.QueryAsync<TEnity>(sql, param, isCommandTypeText ? CommandType.Text: CommandType.StoredProcedure);
        }

        /// <summary>
        /// �б��ѯ��ҳ-sqlͨ�ò�ѯ
        /// </summary>
        /// <typeparam name="TEnity"> ����sql����Model</typeparam>
        /// <param name="sql"></param>
        /// <param name="param">@totalCoun ��������</param>
        /// <param name="isCommandTypeText"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<TEnity>, int>> SQLQueryWithReturnAsync<TEnity>(string sql, object param = null, bool isCommandTypeText = true)
        {
         var gridReader= await  _context.QueryMultipleAsync(sql, param, isCommandTypeText ? CommandType.Text : CommandType.StoredProcedure);
            var list=await gridReader.ReadAsync<TEnity>();
            var count = await gridReader.ReadAsync<long>();
            return new Tuple<IEnumerable<TEnity>, int>(list, (int)count.FirstOrDefault());
        }


        /// <summary>
        /// ��ȡ����-sqlͨ�ò�ѯ
        /// </summary>
        /// <typeparam name="TEnity">����sql����Model</typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="isCommandTypeText"></param>
        /// <param name="isInvoke">�Ƿ���ί�з������ִ��sql</param>
        /// <returns></returns>
        public Task<TEnity> SQLQueryFirstOrDefaultAsync<TEnity>(string sql, object param = null, bool isCommandTypeText = true, bool isInvoke = false)
        {
            return _context.QueryFirstOrDefaultAsync<TEnity>(sql, param, isCommandTypeText ? CommandType.Text : CommandType.StoredProcedure,isInvoke);
        }

        public Task<int> SQLExecute(string sql, object param = null, bool isCommandTypeText = true)
        {
            return _context.ExecuteAsync(sql, param, isCommandTypeText ? CommandType.Text : CommandType.StoredProcedure);
        }
        #endregion
    }
}
