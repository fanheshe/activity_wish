using JKCore.Domain.IData;
using JKCore.Infrastructure.DBContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace JKCore.Infrastructure.Data
{
    public class JiakeDapperUnitOfWorkFactory : DapperUnitOfWorkFactory, IJiakeUnitOfWorkFactory
    {
        private readonly JiakeDapperDBContext _context;
        public JiakeDapperUnitOfWorkFactory(JiakeDapperDBContext context) : base(context)
        {
            _context = context;
        }
    }
}
