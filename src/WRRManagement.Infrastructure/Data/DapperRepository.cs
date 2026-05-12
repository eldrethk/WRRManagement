using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Infrastructure.Data
{
    /// Base repository with common Dapper operations for stored procedures
    public abstract class DapperRepository
    {
        protected readonly IDbConnectionFactory _connectionFactory;

        protected DapperRepository(IDbConnectionFactory connectionFactory) 
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        //Execute a stored procedure and returns the first result or default
        protected async Task<T> QueryFirstOrDefaultAsync<T>(string storedProcedure, object? parameters)
        {
            using var connection = _connectionFactory.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        }

        //Execute a stored procedure and return all results
        protected async Task<IEnumerable<T>> QueryAsync<T>(string storedProcedure, object? parameters)
        {
            using var connection = _connectionFactory.CreateConnection();

            return await connection.QueryAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        }

        //Execute a stored procedure that returns a scalar value
        protected async Task<int> ExecuteScalarIntAsync(string storedProcedure, object? parameters)
        {
            using var connection = _connectionFactory.CreateConnection();

            return await connection.ExecuteScalarAsync<int>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        }
        protected async Task<char> ExecuteScalarCharAsync(string storedProcedure, object? parameters)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<char>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        }

        protected async Task<string?> ExecuteScalarStringAsync(string storedProcedure, object? parameters)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<string>(storedProcedure, parameters);            
        }
        protected async Task<bool> ExecuteScalarBoolAsync(string storedProcedure, object? parameters)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<bool>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        }

        //execute a stored procedure that doesn't return results (Insert, update, delete)
        protected async Task ExecuteAsync(string storedProcedure, object? parameter)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(storedProcedure, parameter, commandType: CommandType.StoredProcedure);
        }

        //execute a stored procedure and return number of rows affected
        // Useful for checking if operation actually changed anything
        protected async Task<int> ExecuteWithRowCountAsync(string storedProcedure, object? parameters)
        {
            using var connection = _connectionFactory.CreateConnection();

            return await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        }

        protected async Task ExecuteWithTransactionAsync(Func<IDbConnection, IDbTransaction, Task> operation)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                await operation(connection, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        protected async Task<T> ExecuteWithTransactionAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> operation)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var result = await operation(connection, transaction);
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
