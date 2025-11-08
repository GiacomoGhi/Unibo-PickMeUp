using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace PickMeUp.Core.Common.Extensions;

public static class IQueryableExtensions
{
    public static IQueryable<TEntity> AsNoTracking<TEntity>(this IQueryable<TEntity> source, bool useNoTracking) where TEntity : class
    {
        if (useNoTracking)
        {
            return source.AsNoTracking();
        }
        return source;
    }
}
