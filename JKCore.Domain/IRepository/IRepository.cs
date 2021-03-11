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
        /// ����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns>�������ݵ�����</returns>
        Task<TKey> InsertAsync<TKey>(T entity);

        /// <summary>
        /// ͨ��Host�����Զ�����IDispose����Ҫ�ֶ�����
        /// </summary>
        void Dispose();
        Task<int> UpdateAsync(T entity);

        Task<int> DeleteAsync(T entity);

        /// <summary>
        /// ɾ��-ͨ������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereConditions"> whereConditions="Where age > 20"</param>
        /// <returns></returns>
        Task<int> DeleteListAsync(object whereConditions);

        /// <summary>
        ///  ɾ��-ͨ������ ʾ����DeleteListAsync<User>("Where age > @Age", new {Age = 20});
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<int> DeleteListAsync(string conditions, object parameters = null);

        /// <summary>
        /// ��ȡ����ͨ������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T> GetAsync(object id);

        // <summary>
        /// ��ȡ�б�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereConditions">whereConditions="Where age > 20"</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetListAsync(object whereConditions);

        /// <summary>
        /// ��ȡ�б� ʾ����GetListAsync<User>("Where age > @Age", new {Age = 20});
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetListAsync(string conditions, object parameters = null);

        /// <summary>
        /// ��ҳ��ȡ����
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

        #region �����ѯ

        /// <summary>
        /// �б��ѯ-sqlͨ�ò�ѯ
        /// </summary>
        /// <typeparam name="TEnity"> ����sql����Model</typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="isCommandTypeText"></param>
        /// <returns></returns>
        Task<IEnumerable<TEnity>> SQLQueryAsync<TEnity>(string sql, object param = null, bool isCommandTypeText = true);


        /// <summary>
        /// ��ȡ����-sqlͨ�ò�ѯ
        /// </summary>
        /// <typeparam name="TEnity">����sql����Model</typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="isCommandTypeText"></param>
        /// <param name="isInvoke">�Ƿ���ί�з������ִ��sql</param>
        /// <returns></returns>
        Task<TEnity> SQLQueryFirstOrDefaultAsync<TEnity>(string sql, object param = null, bool isCommandTypeText = true, bool isInvoke = false);


        /// <summary>
        /// �б��ѯ��ҳ-sqlͨ�ò�ѯ
        /// </summary>
        /// <typeparam name="TEnity"> ����sql����Model</typeparam>
        /// <param name="sql"></param>
        /// <param name="param">@totalCoun ��������</param>
        /// <param name="isCommandTypeText"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<TEnity>, int>> SQLQueryWithReturnAsync<TEnity>(string sql, object param = null, bool isCommandTypeText = true);

        /// <summary>
        /// ִ��SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="isCommandTypeText"></param>
        /// <returns></returns>
        Task<int> SQLExecute(string sql, object param = null, bool isCommandTypeText = true);

        #endregion
    }

}