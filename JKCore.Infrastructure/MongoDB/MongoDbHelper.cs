using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace JKCore.Infrastructure.MongoDB
{
    public class MongoDbHelper : IMongoDb
    {
        private readonly IMongoDatabase _database = null;
        private readonly IMongoClient client = null;

        //public MongoDbHelper(string ConnectionString,string DataBase)
        //{
        //    var client = new MongoClient(ConnectionString);
        //    if (client != null)
        //    {
        //        _database = client.GetDatabase(DataBase);
        //    }
        //}
        public MongoDbHelper(IOptions<MongoDbConfig> _config)
        {
            client = new MongoClient(_config.Value.ConnectionString);
            if (client != null)
            {
                _database = client.GetDatabase(_config.Value.DataBase);
            }
        }
        /// <summary>
        /// 创建集合
        /// </summary>
        /// <param name="collectionName"></param>
        public void CreateCollection(string collectionName)
        {
            this._database.CreateCollection(collectionName);
        }
        
        /// <summary>
        /// 获得集合
        /// </summary>
        /// <param name="collectionName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
             return this._database.GetCollection<T>(collectionName);
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="dictIndex"></param>
        public void CreateCollectionIndex(string collectionName, Dictionary<string, int> dictIndex)
        {
            CreateIndexOptions indexOp = new CreateIndexOptions()
            {
                Background = true
            };
            BsonDocument indexDoc = new BsonDocument();
            indexDoc.AddRange(dictIndex);
            IMongoCollection<BsonDocument> collection = this._database.GetCollection<BsonDocument>(collectionName);
            collection.Indexes.CreateOne(indexDoc, indexOp);
        }

        #region SELECT
        public List<T> FindAll<T>(string collectionName)
        {
            List<T> result = new List<T>();
            ProjectionDefinition<T, T> prjection = new ProjectionDefinitionBuilder<T>().Exclude("_id");
            result = this._database.GetCollection<T>(collectionName).Find(Builders<T>.Filter.Empty).Project(prjection).ToList();
            return result;
        }
        /// <summary>
        ///  查询数据
        /// </summary>
        /// <typeparam name="T">类型格式</typeparam>
        /// <param name="collectionName">表名称</param>
        /// <param name="filter">查询条件</param>
        /// <param name="project">映射条件</param>
        /// <returns>List</returns>
        public List<T> Find<T>(string collectionName, FilterDefinition<T> filter)
        {
            List<T> result = new List<T>();

            ProjectionDefinition<T, T> project = new ProjectionDefinitionBuilder<T>().Exclude("_id");
            result = this._database.GetCollection<T>(collectionName).Find(filter).ToList();//.Project(project)

            return result;
        }
        public T FindSortLast<T>(string collectionName, Expression<Func<T, bool>> filter)
        {
            ProjectionDefinition<T, T> prjection = new ProjectionDefinitionBuilder<T>().Exclude("_id");
            return this._database.GetCollection<T>(collectionName).Find(filter).Project(prjection).Limit(1).FirstOrDefault();
        }
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
        public List<T> Find<T>(string collectionName, FilterDefinition<T> filter, SortDefinition<T> sort, int skip = 0, int limit = 50)
        {
            List<T> result = new List<T>();
            ProjectionDefinition<T, T> project = new ProjectionDefinitionBuilder<T>().Exclude("_id");
            result = this._database.GetCollection<T>(collectionName).Find(filter).Project(project).Skip(skip).Limit(limit).Sort(sort).ToList();
            return result;
        }
        #endregion

        #region INSERT
        /// <summary>
        /// 插入多条数据，数据用list表示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public void InsertMany<T>(List<T> list)
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            collection.InsertMany(list);
        }
        /// <summary>
        /// 插入一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public void InsertOne<T>(T model)
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            collection.InsertOne(model);
        }
        #endregion
        #region UPDATE

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="collectionName">表名称</param>
        /// <param name="filter">条件数据</param>
        /// <param name="update">更新数据</param>
        /// <returns>更新结果</returns>
        
        public UpdateResult UpdateOne<T>(string collectionName, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions option = null)
        {
            return this._database.GetCollection<T>(collectionName).UpdateOne(filter, update, option);
        }

        //public UpdateResult UpdateOneData(string collectionName, FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update, UpdateOptions option = null)
        //{
        //    return this._database.GetCollection<BsonDocument>(collectionName).UpdateOne(filter, update, option);
        //}
        /// <summary>
        /// 更新多条数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="collectionName">表名称</param>
        /// <param name="filter">条件数据</param>
        /// <param name="update">更新数据</param>
        public UpdateResult UpdateManyData<T>(string collectionName, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null)
        {
            return this._database.GetCollection<T>(collectionName).UpdateMany(filter, update, options);
        }

        public BulkWriteResult<BsonDocument> BulkWrite(string collectionName, IEnumerable<WriteModel<BsonDocument>> requests)
        {
            return this._database.GetCollection<BsonDocument>(collectionName).BulkWrite(requests);
        }
        //public UpdateResult UpdateManyData(string collectionName, FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update, UpdateOptions options = null)
        //{
        //    return this._database.GetCollection<BsonDocument>(collectionName).UpdateMany(filter, update, options);
        //}
        #endregion
            /// <summary>
            /// 根据条件删除数据
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="collectionName"></param>
            /// <param name="filter"></param>
            /// <returns></returns>
        public DeleteResult RemoveMany<T>(string collectionName, FilterDefinition<T> filter)
        {
            return this._database.GetCollection<T>(collectionName).DeleteMany(filter);
        }


        #region 事务相关  用到事务的地方，一定要先创建集合

        /// <summary>
        /// 开始客户端会话
        /// </summary>
        /// <returns></returns>
        public IClientSessionHandle GetSession()
        {
            return client.StartSession();
        }

        /// <summary>
        /// 带事务的，批量删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="collectionName"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public DeleteResult RemoveManySession<T>(IClientSessionHandle session, string collectionName, FilterDefinition<T> filter)
        {
            return this._database.GetCollection<T>(collectionName).DeleteMany(session,filter);
        }

        /// <summary>
        /// 带事务的，批量插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="list"></param>
        public void InsertManySession<T>(IClientSessionHandle session, List<T> list)
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            collection.InsertMany(session,list);
        }

        #endregion
    }
}
