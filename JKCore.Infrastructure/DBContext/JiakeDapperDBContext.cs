using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace JKCore.Infrastructure.DBContext
{
    public class JiakeDapperDBContext : DapperDBContext
    {
        private readonly string _configuration;
        public JiakeDapperDBContext(string configuration) : base(configuration)
        {
            _configuration = configuration;
        }

        protected override IDbConnection CreateConnection(string connectionString)
        {
            IDbConnection conn = new MySqlConnection(connectionString);
            return conn;
        }
    }
}
