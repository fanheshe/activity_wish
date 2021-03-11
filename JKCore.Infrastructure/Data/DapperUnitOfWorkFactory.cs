using JKCore.Domain.IData;
using JKCore.Infrastructure.DBContext;
using System.Data;

namespace JKCore.Infrastructure.Data
{
    public class DapperUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly DapperDBContext _context;

        public DapperUnitOfWorkFactory(DapperDBContext context)
        {
            _context = context;
        }

        public IUnitOfWork Create()
        {
            return new UnitOfWork(_context);
        }
        public IUnitOfWork Create(IsolationLevel level)
        {
            return new UnitOfWork(_context,level);
        }
    }
}
