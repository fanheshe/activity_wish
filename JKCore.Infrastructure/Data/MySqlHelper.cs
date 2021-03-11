using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace JKCore.Infrastructure.Data
{
    public class JKSqlHelper
    {
        string connStr;

        public JKSqlHelper(string connectionString)
        {
            connStr = connectionString;
        }
        public async Task<IEnumerable<T>> QueryListAsync<T>(string sql, object param = null)
        {
            IEnumerable<T> resultIen;
            using (var conn = new MySqlConnection(connStr))
            {
                resultIen = await conn.QueryAsync<T>(sql, param);
            }
            return resultIen;
        }
        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<int> ExecuteAsync(string sql, object param = null)
        {
            var rows = 0;
            using (var conn = new MySqlConnection(connStr))
            {
                rows = await conn.ExecuteAsync(sql, param);
            }
            return rows;
        }
    }
}
