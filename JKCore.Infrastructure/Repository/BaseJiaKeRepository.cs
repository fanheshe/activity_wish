using JKCore.Domain.IRepository;
using JKCore.Infrastructure.DBContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace JKCore.Infrastructure.Repository
{
    public class BaseJiakeRepository<T> : BaseRepository<T>, IJiakeRepository<T> where T : class
    {
        private readonly JiakeDapperDBContext _context;
        public BaseJiakeRepository(JiakeDapperDBContext context) : base(context)
        {
            _context = context;
        }
    }
}
