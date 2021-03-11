using System;
using System.Collections.Generic;
using System.Text;

namespace JKCore.Domain.IRepository
{
    public interface IDemoRepository<T> : IRepository<T> where T : class
    {
    }
}
