using DigitalStore.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DigitalStore.Core.Services
{
    public interface IService<T> where T : BaseEntity
    {
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    }
}
