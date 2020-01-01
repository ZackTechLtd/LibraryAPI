

namespace DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Dapper;

    public static class DapperExtensions
    {
        public static IEnumerable<T> Query<T>(this IDbConnection connection, Func<T> typeBuilder, string sql, DynamicParameters parameters)
        {
            return connection.Query<T>(sql, param: parameters, commandType: CommandType.Text);
        }

        public static IEnumerable<T> Query<T>(this IDbConnection connection, Func<T> typeBuilder, string sql, DynamicParameters parameters, IDbTransaction transaction)
        {
            return connection.Query<T>(sql, param: parameters, commandType: CommandType.Text, transaction: transaction);
        }
    }
}
