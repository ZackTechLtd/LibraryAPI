using System.Configuration;
using System.Data;
using System.Text;
using Common.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace LibraryApp.Infrastructure.Repository;

public abstract class RepositoryBase
{
    /// <summary>
    /// The _connection string settings.
    /// </summary>
    protected readonly ConnectionStringSettings connectionStringSettings;
    private readonly IOptions<ApiConfiguration> _apiConfiguration;

    protected RepositoryBase(IConfiguration config, IOptions<ApiConfiguration> apiConfiguration)
    {
        _apiConfiguration = apiConfiguration;
        this.connectionStringSettings = new ConnectionStringSettings()
        {
            Name = "DefaultConnection",
            ConnectionString = config.GetConnectionString("DefaultConnection")
        };
    }

    /// <summary>
    /// The open database connection.
    /// </summary>
    /// <returns>
    /// The <see cref="IDbConnection"/>.
    /// </returns>
    protected IDbConnection OpenConnection()
    {


        //if (string.Equals(_apiConfiguration.Value.RDBMS, "Postgres", StringComparison.OrdinalIgnoreCase))
        //{
        //    var postgresconnection = new NpgsqlConnection(this.connectionStringSettings.ConnectionString);

        //    postgresconnection.Open();

        //    return postgresconnection;
        //}

        if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase))
        {
            MySqlConnection mysqlconnection = new MySqlConnection(this.connectionStringSettings.ConnectionString);
            mysqlconnection.Open();
            return mysqlconnection;
        }

        var connection = new SqlConnection(this.connectionStringSettings.ConnectionString);
        connection.Open();

        return connection;
    }

    private string GetValueAfterEquals(string item)
    {
        if (!string.IsNullOrEmpty(item))
        {
            string[] parts = item.Split('=');
            if (parts.Length > 1)
                return parts[1];
        }

        return string.Empty;

    }

    /// <summary>
    /// The begin SQL transaction.
    /// </summary>
    /// <param name="connection">
    /// The connection.
    /// </param>
    /// <returns>
    /// The <see cref="IDbTransaction"/>.
    /// </returns>
    protected IDbTransaction BeginSqlTransaction(IDbConnection connection)
    {
        return connection.BeginTransaction();
    }

    /// <summary>
    /// The rollback SQL transaction.
    /// </summary>
    /// <param name="sqlTrans">
    /// The SQL trans.
    /// </param>
    /// <param name="methodName">
    /// The method name.
    /// </param>
    /// <param name="ex">
    /// The ex.
    /// </param>
    /// <returns>
    /// The <see cref="Exception"/>.
    /// </returns>
    protected Exception RollbackSqlTransaction(IDbTransaction sqlTrans, string methodName, Exception ex)
    {
        try
        {
            if (sqlTrans != null)
            {
                sqlTrans.Rollback();
            }
        }
        catch (Exception)
        {
            // TODO: logging
            return new Exception($"RollingBack {methodName} SQLRolledBack", ex);
        }

        // TODO: logging
        return new Exception($"ErrorCallingMethod {methodName} SQLRolledBack", ex);
    }

    /// <summary>
    /// The commit SQL transaction.
    /// </summary>
    /// <param name="sqlTrans">
    /// The SQL trans.
    /// </param>
    protected void CommitSqlTransaction(IDbTransaction sqlTrans)
    {
        sqlTrans.Commit();
    }

    /// <summary>
    /// The execute save procedure.
    /// </summary>
    /// <param name="storedProcedureName">
    /// The stored procedure name.
    /// </param>
    /// <param name="idParameterName">
    /// The id parameter name.
    /// </param>
    /// <param name="databaseType">
    /// The database type.
    /// </param>
    /// <param name="idGetter">
    /// The id getter.
    /// </param>
    /// <param name="idSetter">
    /// The id setter.
    /// </param>
    /// <param name="parameters">
    /// The parameters.
    /// </param>
    /// <param name="connection">
    /// The connection.
    /// </param>
    /// <param name="transaction">
    /// The transaction.
    /// </param>
    /// <typeparam name="TId">
    /// The type of the Id (e.g. null-able integer)
    /// </typeparam>
    protected void ExecuteSaveProcedure<TId>(
        string storedProcedureName,
        string idParameterName,
        DbType databaseType,
        Func<TId> idGetter,
        Action<TId> idSetter,
        object parameters,
        IDbConnection connection,
        IDbTransaction transaction = default!)
    {
        var dynamicParameters = new DynamicParameters(parameters);

        dynamicParameters.Add(idParameterName, idGetter(), direction: ParameterDirection.InputOutput, dbType: databaseType);

        connection.Execute(
            storedProcedureName,
            dynamicParameters,
            commandType: CommandType.StoredProcedure,
            transaction: transaction);

        idSetter(dynamicParameters.Get<TId>(idParameterName));
    }

#pragma warning disable CS8602

    /// <summary>
    /// The execute save procedure.
    /// </summary>
    /// <param name="storedProcedureName">The stored procedure name.</param>
    /// <param name="parameters">An anonymous type containing input parameter values plus output values/placeholders</param>
    /// <param name="connection">An open connection instance.</param>
    /// <param name="outputParameters">An optional list of OutputParam object instructing how to assign return parameters back to the object</param>
    /// <param name="transaction">The transaction.</param>
    protected void ExecuteSaveProcedure(
        string storedProcedureName,
        object parameters,
        IDbConnection connection,
        List<OutputParam> outputParameters = default!,
        IDbTransaction transaction = default!)
    {
        var dynamicParameters = new DynamicParameters(parameters);

        if (outputParameters != null)
        {
            outputParameters.ForEach(p =>
            {
                dynamicParameters.Add(p.Name, p.Getter.Invoke(), direction: p.Direction, dbType: p.DbType);
            });
        }

        connection.Execute(
            storedProcedureName,
            dynamicParameters,
            commandType: CommandType.StoredProcedure,
            transaction: transaction);

        if (outputParameters != null)
        {
            outputParameters.ForEach(p =>
            {
                var method = typeof(DynamicParameters).GetMethod("Get").MakeGenericMethod(p.Type);
                var result = method.Invoke(dynamicParameters, new[] { p.Name });
                if (result != null)
                {
                    p.Setter.Invoke(result);
                }
                
            });
        }
    }
#pragma warning restore CS8602
    /// <summary>
    /// Stores how return parameter values from a stored procedure should update an object
    /// </summary>
    public class OutputParam
    {
        /// <summary>
        /// Gets or sets the Parameter name (must have the @ prefix)
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Gets or sets the CLR type the parameter return value should be casted to
        /// </summary>
        public Type Type { get; set; } = default!;

        /// <summary>
        /// Gets or sets the TSQL type of the output parameter
        /// </summary>
        public DbType DbType { get; set; }

        /// <summary>
        /// Gets or sets the direction of the parameter
        /// </summary>
        public ParameterDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets a delegate that retrieves the value of the parameter.  Needed for Input/Output parameters
        /// </summary>
        public Func<object> Getter { get; set; } = default!;

        /// <summary>
        /// Gets or sets a delegate that sets an object property based on the stored procedure return value 
        /// </summary>
        public Action<object> Setter { get; set; } = default!;
    }

    protected int? GetId(string sql, string code)
    {
        int? id = null;
        using (var connection = this.OpenConnection())
        {
            id = connection.Query<int?>(sql, new { code }).SingleOrDefault();
        }

        return id;
    }

    protected (string stringpart, int intpart) SeperateStringAndInt(string source)
    {
        string stringPart = String.Empty;
        var buffer = new StringBuilder();
        foreach (char c in source)
        {
            if (Char.IsDigit(c))
            {
                if (stringPart == String.Empty)
                {
                    stringPart = buffer.ToString();
                    buffer.Remove(0, buffer.Length);
                }
            }

            buffer.Append(c);
        }

        if (!int.TryParse(buffer.ToString(), out int intPart))
        {
            return (stringpart: buffer.ToString(), intpart: 0);
        }

        return (stringpart: stringPart, intpart: intPart);
    }
}

