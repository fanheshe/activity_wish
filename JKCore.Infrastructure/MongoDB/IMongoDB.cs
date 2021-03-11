using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace JKCore.Infrastructure.MongoDB
{
    public interface IMongoDb
    {
        /// <summary>
        /// 创建集合
        /// </summary>
        /// <param name="collectionName"></param>
        void CreateCollection(string collectionName);
        /// <summary>
        /// 获得Collection
        /// </summary>
        /// <param name="collectionName"></param>
        IMongoCollection<T> GetCollection<T>(string collectionName);
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="dictIndex"></param>
        void CreateCollectionIndex(string collectionName, Dictionary<string, int> dictIndex);
        /// <summary>
        /// 插入多条数据，数据用list表示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        void InsertMany<T>(List<T> list);
        /// <summary>
        /// 插入一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        void InsertOne<T>(T model);

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="collectionName">表名称</param>
        /// <param name="filter">条件数据</param>
        /// <param name="update">更新数据</param>
        /// <returns>更新结果</returns>
        UpdateResult UpdateOne<T>(string collectionName, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions option = null);

        UpdateResult UpdateManyData<T>(string collectionName, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions option = null);
        
        List<T> Find<T>(string collectionName, FilterDefinition<T> filter);

        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <param name="collectionName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> FindAll<T>(string collectionName);

        T FindSortLast<T>(string collectionName, Expression<Func<T, bool>> filter);
        /// <summary>
        ///  查询数据
        /// </summary>
        /// <typeparam name="T">类型格式</typeparam>
        /// <param name="collectionName">表名称</param>
        /// <param name="filter">查询条件</param>
        /// <param name="sort">排序条件</param>
        /// <param name="skip">开始索引</param>
        /// <param name="limit">获取数据条数</param>
        /// <returns>List</returns>
        List<T> Find<T>(string collectionName, FilterDefinition<T> filter, SortDefinition<T> sort, int skip = 0, int limit = 50);

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="requests"></param>
        /// <returns></returns>
        BulkWriteResult<BsonDocument> BulkWrite(string collectionName, IEnumerable<WriteModel<BsonDocument>> requests);

        /// <summary>
        /// 根据条件删除数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        DeleteResult RemoveMany<T>(string collectionName, FilterDefinition<T> filter);

        #region 事务相关  用到事务的地方，一定要先创建集合
        /// <summary>
        /// 开始客户端会话
        /// </summary>
        /// <returns></returns>
        IClientSessionHandle GetSession();

        /// <summary>
        /// 带事务的，批量删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="collectionName"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        DeleteResult RemoveManySession<T>(IClientSessionHandle session,string collectionName, FilterDefinition<T> filter);

        /// <summary>
        /// 带事务的，批量插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="list"></param>
        void InsertManySession<T>(IClientSessionHandle session, List<T> list);

        #endregion

    }
}
