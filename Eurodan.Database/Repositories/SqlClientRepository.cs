using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Eurodan.Data.Repositories
{
    public class SqlClientRepository(string connectionString) : IRepository<Client>
    {
        private readonly string _connectionString = connectionString;

        public void Add(Client client)
        {
            string query = @"INSERT INTO dbo.CLIENT (ContactName, CompanyName, Phone, Email)
                             OUTPUT INSERTED.ClientId
                             VALUES (@ContactName, @CompanyName, @Phone, @Email)";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.Add("@ContactName", SqlDbType.NVarChar, 100)
                .Value = (object?)client.ContactName ?? DBNull.Value;
            cmd.Parameters.Add("@CompanyName", SqlDbType.NVarChar, 150)
                .Value = client.CompanyName;
            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 30)
                .Value = (object?)client.Phone ?? DBNull.Value;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 254)
                .Value = (object?)client.Email ?? DBNull.Value;

            connection.Open();

            object? result = cmd.ExecuteScalar();

            if (result is not int clientId)
            {
                throw new InvalidOperationException(
                    "Failed to insert client and retrieve the new ClientId.");
            }

            client.ClientId = clientId;
        }

        public void Delete(int id)
        {
            string query = @"DELETE FROM dbo.CLIENT
                            WHERE ClientId = @ClientId";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.Add("@ClientId", SqlDbType.Int).Value = id;
            connection.Open();

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException(
                    "No client found with the specified ClientId. Failed to delete client.");
            }
        }

        public IEnumerable<Client> GetAll()
        {
            var clients = new List<Client>();
            string query = @"SELECT ClientId, ContactName, CompanyName, Phone, Email 
                             FROM dbo.CLIENT";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    clients.Add(new Client
                    {
                        ClientId = reader.GetInt32(0),
                        ContactName = reader.IsDBNull(1) ? null : reader.GetString(1),
                        CompanyName = reader.GetString(2),
                        Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Email = reader.IsDBNull(4) ? null : reader.GetString(4)
                    });
                }
            }

            return clients;
        }

        public Client? GetById(int id)
        {
            Client? client = null;
            string query = @"SELECT ClientId, ContactName, CompanyName, Phone, Email 
                             FROM dbo.CLIENT
                             WHERE ClientId = @ClientId";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@ClientId", id);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    client = new Client
                    {
                        ClientId = reader.GetInt32(0),
                        ContactName = reader.IsDBNull(1) ? null : reader.GetString(1),
                        CompanyName = reader.GetString(2),
                        Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Email = reader.IsDBNull(4) ? null : reader.GetString(4)
                    };
                }
            }

            return client;
        }

        public void Update(Client client)
        {
            string query = @"UPDATE dbo.CLIENT
                            SET ContactName = @ContactName,
                                CompanyName = @CompanyName,
                                Phone = @Phone,
                                Email = @Email
                            WHERE ClientId = @ClientId";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, connection);

            cmd.Parameters.Add("@ClientId", SqlDbType.Int).Value = client.ClientId;
            cmd.Parameters.Add("@ContactName", SqlDbType.NVarChar, 100)
                .Value = (object?)client.ContactName ?? DBNull.Value;
            cmd.Parameters.Add("@CompanyName", SqlDbType.NVarChar, 150)
                .Value = client.CompanyName;
            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 30)
                .Value = (object?)client.Phone ?? DBNull.Value;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 254)
                .Value = (object?)client.Email ?? DBNull.Value;

            connection.Open();

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException(
                    "No client found with the specified ClientId. Failed to update client.");
            }
        }
    }
}
