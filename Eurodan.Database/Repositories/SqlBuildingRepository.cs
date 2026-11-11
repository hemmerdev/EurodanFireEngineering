using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Eurodan.Data.Repositories
{
    public class SqlBuildingRepository(string connectionString) : IRepository<Building>
    {
        private readonly string _connectionString = connectionString;
        public void Add(Building building)
        {
            string query = @"INSERT INTO dbo.BUILDING (Name, Address)
                             OUTPUT INSERTED.BuildingId
                             VALUES (@Name, @Address)";

            using SqlConnection connection = new(_connectionString);
            using SqlCommand cmd = new(query, connection);

            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 150)
                .Value = building.Name;
            cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 250)
                .Value = building.Address;

            connection.Open();

            object? result = cmd.ExecuteScalar();

            if (result is not int buildingId)
            {
                throw new InvalidOperationException(
                    "Failed to insert building and retrieve the new BuildingId.");
            }

            building.BuildingId = buildingId;
        }

        public void Delete(int id)
        {
            string query = @"DELETE FROM dbo.BUILDING
                            WHERE BuildingId = @BuildingId";

            using SqlConnection connection = new(_connectionString);
            using SqlCommand cmd = new(query, connection);

            cmd.Parameters.Add("@BuildingId", SqlDbType.Int)
                .Value = id;

            connection.Open();
            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException(
                    "No Building found with the specified ID. Failed to delete building.");
            }
        }

        public IEnumerable<Building> GetAll()
        {
            var buildings = new List<Building>();
            string query = @"SELECT BuildingId, Name, Address
                             FROM dbo.BUILDING";

            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand cmd = new(query, connection);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    buildings.Add(new Building
                    {
                        BuildingId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Address = reader.GetString(2)
                    });
                }
            }

            return buildings;
        }
        public Building? GetById(int id)
        {
            Building? building = null;
            string query = @"SELECT BuildingId, Name, Address
                             FROM dbo.BUILDING
                             WHERE BuildingId = @BuildingId";

            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand cmd = new(query, connection);
                cmd.Parameters.AddWithValue("@BuildingId", id);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    building = new Building
                    {
                        BuildingId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Address = reader.GetString(2)
                    };
                }
            }

            return building;
        }

        public void Update(Building building)
        {
            string query = @"UPDATE dbo.BUILDING
                             SET Name = @Name, 
                                 Address = @Address
                             WHERE BuildingId = @BuildingId";

            using SqlConnection connection = new(_connectionString);
            using SqlCommand cmd = new(query, connection);
            cmd.Parameters.Add("@BuildingId", SqlDbType.Int)
                .Value = building.BuildingId;
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 150)
                .Value = (object?)building.Name ?? DBNull.Value;
            cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 250)
                .Value = (object?)building.Address ?? DBNull.Value;

            connection.Open();
            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException(
                    "No building found with the specified BuildingId. Failed to update building");
            }
        }
    }
}

