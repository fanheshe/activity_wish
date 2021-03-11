using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JKCore.Domain.IData
{
    public interface IContext : IDisposable
    {
        bool IsTransactionStarted { get; }

        void BeginTransaction();
        /// <summary>
        /// 带锁的事务
        /// </summary>
        /// <param name="level"></param>
        void BeginTransaction(IsolationLevel level);

        void Commit();

        void Rollback();
    }
}
