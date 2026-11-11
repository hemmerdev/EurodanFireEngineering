using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Eurodan.Data.Repositories
{
    public class SqlTechnicalIssueRepository(string connectionString) : IRepository<TechnicalIssue>
    {
        private readonly string _connectionString = connectionString;

        public void Add(TechnicalIssue technicalIssue)
        {
            string query = @"INSERT INTO dbo.TECHNICALISSUE (Name, Description)
                             OUTPUT INSERTED.TechnicalIssueId
                             VALUES (@Name, @Description)";

            using SqlConnection connection = new(_connectionString);
            using SqlCommand cmd = new(query, connection);
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 150)
                .Value = technicalIssue.Name;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500)
                .Value = (object?)technicalIssue.Description ?? DBNull.Value;

            connection.Open();

            object? result = cmd.ExecuteScalar();

            if (result is not int technicalIssueId)
            {
                throw new InvalidOperationException(
                    "Failed to insert technical issue and retrieve the new TechnicalIssueId.");
            }

            technicalIssue.TechnicalIssueId = technicalIssueId;
        }

        public void Delete(int id)
        {
            string query = @"DELETE FROM dbo.TECHNICALISSUE
                            WHERE TechnicalIssueId = @TechnicalIssueId";

            using SqlConnection connection = new(_connectionString);
            using SqlCommand cmd = new(query, connection);

            cmd.Parameters.Add("@TechnicalIssueId", SqlDbType.Int)
                .Value = id;

            connection.Open();
            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException(
                    "No technical issue found with the specified ID. Failed to delete technical issue.");
            }
        }

        public IEnumerable<TechnicalIssue> GetAll()
        {
            var technicalIssues = new List<TechnicalIssue>();
            string query = @"SELECT TechnicalIssueId, Name, Description
                             FROM dbo.TECHNICALISSUE";

            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand cmd = new(query, connection);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    technicalIssues.Add(new TechnicalIssue
                    {
                        TechnicalIssueId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                    });
                }
            }

            return technicalIssues;
        }

        public TechnicalIssue? GetById(int id)
        {
            TechnicalIssue? technicalIssue = null;
            string query = @"SELECT TechnicalIssueId, Name, Description
                             FROM dbo.TECHNICALISSUE
                             WHERE TechnicalIssueId = @TechnicalIssueId";

            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand cmd = new(query, connection);
                cmd.Parameters.Add("@TechnicalIssueId", SqlDbType.Int)
                    .Value = id;
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    technicalIssue = new TechnicalIssue
                    {
                        TechnicalIssueId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                    };
                }
            }

            return technicalIssue;
        }

        public void Update(TechnicalIssue technicalIssue)
        {
            string query = @"UPDATE dbo.TECHNICALISSUE
                             SET Name = @Name,
                                 Description = @Description
                             WHERE TechnicalIssueId = @TechnicalIssueId";

            using SqlConnection connection = new(_connectionString);
            using SqlCommand cmd = new(query, connection);

            cmd.Parameters.Add("@TechnicalIssueId", SqlDbType.Int).
                Value = technicalIssue.TechnicalIssueId;
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 150)
                .Value = technicalIssue.Name;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500)
                .Value = (object?)technicalIssue.Description ?? DBNull.Value;

            connection.Open();
            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException(
                    "No technical issue found with the specified ID. Failed to update technical issue.");
            }
        }
    }
}
