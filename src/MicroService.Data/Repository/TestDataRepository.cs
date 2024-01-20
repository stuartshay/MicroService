using Dapper;
using MicroService.Data.Models;
using Npgsql;
using System.Data;

namespace MicroService.Data.Repository
{
    public class TestDataRepository : ITestDataRepository
    {
        private readonly string _connectionString;

        public TestDataRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        internal IDbConnection Connection => new NpgsqlConnection(_connectionString);

        public Task Add(TestData item)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TestData>> FindAll()
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return await dbConnection.QueryAsync<TestData>("select id, data from test_data;");
            }
        }

        public async Task<TestData> FindById(int id)
        {
            using IDbConnection dbConnection = Connection;
            dbConnection.Open();
            var result = await dbConnection.QueryAsync<TestData>("SELECT id, data FROM test_data WHERE id = @Id", new { Id = id });
            return result.FirstOrDefault();
        }

        public Task Remove(int id)
        {
            throw new NotImplementedException();
        }

        public Task Update(TestData item)
        {
            throw new NotImplementedException();
        }
    }
}
