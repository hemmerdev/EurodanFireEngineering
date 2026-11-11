using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;
using Microsoft.Data.SqlClient;

namespace Eurodan.Data.Repositories
{
    public class SqlBuildingClassRepository(string connectionString) : IReadOnlyRepository<BuildingClass>
    {
        private readonly string _connectionString = connectionString;

        public IEnumerable<BuildingClass> GetAll()
        {
            var buildingClasses = new List<BuildingClass>();
            string query = @"SELECT BuildingClassId, ClassCode, Description
                             FROM dbo.BUILDINGCLASS";

            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand cmd = new(query, connection);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    buildingClasses.Add(new BuildingClass
                    {
                        BuildingClassId = reader.GetInt32(0),
                        ClassCode = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                    });
                }
            }

            return buildingClasses;
        }

        public BuildingClass? GetById(int id)
        {
            BuildingClass? buildingClass = null;
            string query = @"SELECT BuildingClassId, ClassCode, Description
                             FROM dbo.BUILDINGCLASS
                             WHERE BuildingClassId = @BuildingClassId";

            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand cmd = new(query, connection);
                cmd.Parameters.AddWithValue("@BuildingClassId", id);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    buildingClass = new BuildingClass
                    {
                        BuildingClassId = reader.GetInt32(0),
                        ClassCode = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                    };
                }
            }

            return buildingClass;
        }
    }
}
