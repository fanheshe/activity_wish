using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JKCore.Domain.IData
{
    public interface IUnitOfWork : IDisposable
    {
        void SaveChanges();
    }
}
