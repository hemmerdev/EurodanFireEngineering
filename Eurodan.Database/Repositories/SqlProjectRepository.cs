using Eurodan.Core.Enums;
using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Eurodan.Data.Repositories
{
    public class SqlProjectRepository(string connectionString) : IProjectRepository
    {
        private readonly string _connectionString = connectionString;
        public void Add(Project project)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                using SqlCommand cmd = new("dbo.uspInsertProject", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;

                AddProjectParameters(cmd, project);

                object? result = cmd.ExecuteScalar();

                if (result is not int projectId)
                {
                    throw new InvalidOperationException(
                        "Failed to insert project and retrieve the new ProjectId.");
                }

                InsertProjectAssociations(project, projectId, connection, transaction);

                transaction.Commit();
                project.ProjectId = projectId;
            }
            catch
            {
                try
                {
                    transaction.Rollback();
                }
                catch (InvalidOperationException)
                {
                    // Rollback failed because the transaction has already been completed or rolled back.
                    // Preserve the original save error.
                }
                catch (SqlException)
                {
                    // Rollback failed with a SQL error. Preserve the original save error.
                }
                throw;
            }
        }

        public void Archive(int projectId)
        {
            using SqlConnection connection = new(_connectionString);

            using SqlCommand cmd = new("dbo.uspArchiveProject", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ProjectId", SqlDbType.Int).Value = projectId;

            connection.Open();

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex) when (ex.Number == 50001) // Custom error number for project not found
            {
                // Handle specific SQL exceptions if needed
                throw new InvalidOperationException(
                    ex.Message, ex);
            }
        }

        public IEnumerable<Project> GetAll()
        {
            var projects = new List<Project>();
            string projectQuery = @"SELECT * FROM dbo.vwProjects
                                    ORDER BY ProjectId DESC";

            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand cmd = new(projectQuery, connection);
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        projects.Add(MapProject(reader));
                    }
                }

                // Fetch associated buildings for each project
                string buildingQuery = @"SELECT * FROM dbo.vwProjectBuildings pb
                                         WHERE pb.ProjectId = @ProjectId";

                foreach (var project in projects)
                {
                    using SqlCommand buildingCmd = new(buildingQuery, connection);
                    buildingCmd.Parameters.Add("@ProjectId", SqlDbType.Int).Value = project.ProjectId;
                    using SqlDataReader buildingReader = buildingCmd.ExecuteReader();

                    while (buildingReader.Read())
                    {
                        project.Buildings.Add(MapBuilding(buildingReader));
                    }
                }

                // Fetch associated building classes for each project
                string buildingClassQuery = @"SELECT * FROM dbo.vwProjectBuildingClasses pbc
                                              WHERE pbc.ProjectId = @ProjectId";

                foreach (var project in projects)
                {
                    using SqlCommand buildingClassCmd = new(buildingClassQuery, connection);
                    buildingClassCmd.Parameters.Add("@ProjectId", SqlDbType.Int).Value = project.ProjectId;
                    using SqlDataReader buildingClassReader = buildingClassCmd.ExecuteReader();
                    while (buildingClassReader.Read())
                    {
                        project.BuildingClasses.Add(MapBuildingClass(buildingClassReader));
                    }
                }

                // Fetch associated technical issues for each project
                string technicalIssueQuery = @"SELECT * FROM dbo.vwProjectTechnicalIssues pti
                                             WHERE pti.ProjectId = @ProjectId";

                foreach (var project in projects)
                {
                    using SqlCommand technicalIssueCmd = new(technicalIssueQuery, connection);
                    technicalIssueCmd.Parameters.Add("@ProjectId", SqlDbType.Int).Value = project.ProjectId;
                    using SqlDataReader technicalIssueReader = technicalIssueCmd.ExecuteReader();
                    while (technicalIssueReader.Read())
                    {
                        project.TechnicalIssues.Add(MapTechnicalIssue(technicalIssueReader));
                    }
                }
            }

            return projects;
        }

        public Project? GetById(int id)
        {
            Project? project = null;
            string projectQuery = @"SELECT * FROM dbo.vwProjects 
                                    WHERE ProjectId = @ProjectId";

            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand cmd = new(projectQuery, connection);
                cmd.Parameters.Add("@ProjectId", SqlDbType.Int).Value = id;
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    if (reader.Read())
                    {
                        project = MapProject(reader);
                    }
                }

                if (project is null)
                {
                    return null;
                }

                // Fetch associated buildings for the project
                string buildingQuery = @"SELECT * FROM dbo.vwProjectBuildings pb
                                         WHERE pb.ProjectId = @ProjectId";

                using SqlCommand buildingCmd = new(buildingQuery, connection);
                buildingCmd.Parameters.Add("@ProjectId", SqlDbType.Int).Value = id;

                using (SqlDataReader buildingReader = buildingCmd.ExecuteReader())
                {
                    while (buildingReader.Read())
                    {
                        project.Buildings.Add(MapBuilding(buildingReader));
                    }
                }

                // Fetch associated building classes for the project
                string buildingClassQuery = @"SELECT * FROM dbo.vwProjectBuildingClasses pbc
                                              WHERE pbc.ProjectId = @ProjectId";

                using SqlCommand buildingClassCmd = new(buildingClassQuery, connection);
                buildingClassCmd.Parameters.Add("@ProjectId", SqlDbType.Int).Value = id;

                using (SqlDataReader buildingClassReader = buildingClassCmd.ExecuteReader())
                {
                    while (buildingClassReader.Read())
                    {
                        project.BuildingClasses.Add(MapBuildingClass(buildingClassReader));
                    }
                }

                // Fetch associated technical issues for the project
                string technicalIssueQuery = @"SELECT * FROM dbo.vwProjectTechnicalIssues pti
                                               WHERE pti.ProjectId = @ProjectId";

                using SqlCommand technicalIssueCmd = new(technicalIssueQuery, connection);
                technicalIssueCmd.Parameters.Add("@ProjectId", SqlDbType.Int).Value = id;

                using SqlDataReader technicalIssueReader = technicalIssueCmd.ExecuteReader();
                while (technicalIssueReader.Read())
                {
                    project.TechnicalIssues.Add(MapTechnicalIssue(technicalIssueReader));
                }
            }

            return project;
        }

        public void Update(Project project)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                using SqlCommand cmd = new("dbo.uspUpdateProject", connection, transaction);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@ProjectId", SqlDbType.Int)
                    .Value = project.ProjectId;
                AddProjectParameters(cmd, project);

                cmd.ExecuteNonQuery();

                // Delete existing associations in PROJECT_BUILDING, PROJECT_BUILDINGCLASS, and PROJECT_TECHNICALISSUE
                using SqlCommand deleteCmd = new("dbo.uspClearProjectAssociations", connection, transaction);
                deleteCmd.CommandType = CommandType.StoredProcedure;

                deleteCmd.Parameters.Add("@ProjectId", SqlDbType.Int).Value = project.ProjectId;
                deleteCmd.ExecuteNonQuery();

                InsertProjectAssociations(project, project.ProjectId, connection, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (InvalidOperationException)
                {
                    // Rollback failed because the transaction has already been completed or rolled back.
                    // Preserve the original save error.
                }
                catch (SqlException)
                {
                    // Rollback failed with a SQL error. Preserve the original save error.
                }

                if (ex is SqlException sqlEx && sqlEx.Number == 50001) // Custom error number for project not found
                {
                    throw new InvalidOperationException(
                        "The project to update could not be found.", ex);
                }
                throw;
            }
        }

        // Helper method to add parameters for Project properties to avoid duplicate code in Add and Update methods
        private static void AddProjectParameters(SqlCommand cmd, Project project)
        {
            cmd.Parameters.Add("@ProjectNumber", SqlDbType.NVarChar, 30)
                    .Value = project.ProjectNumber;
            cmd.Parameters.Add("@ProjectName", SqlDbType.NVarChar, 200)
                .Value = project.ProjectName;
            cmd.Parameters.Add("@FeeProposalStatus", SqlDbType.VarChar, 8)
                .Value = project.FeeProposalStatus.ToString();
            cmd.Parameters.Add("@ProjectManager", SqlDbType.NVarChar, 100)
                .Value = (object?)project.ProjectManager ?? DBNull.Value;

            var feeProposalParam =
                cmd.Parameters.Add("@FeeProposal", SqlDbType.Decimal);
            feeProposalParam.Precision = 10;
            feeProposalParam.Scale = 2;
            feeProposalParam.Value = (object?)project.FeeProposal ?? DBNull.Value;

            cmd.Parameters.Add("@PbdbStatus", SqlDbType.VarChar, 13)
                .Value = (object?)project.PbdbStatus?.ToString() ?? DBNull.Value;
            cmd.Parameters.Add("@FserStatus", SqlDbType.VarChar, 13)
                .Value = (object?)project.FserStatus?.ToString() ?? DBNull.Value;
            cmd.Parameters.Add("@PaymentDueDate", SqlDbType.Date).
                Value = (object?)project.PaymentDueDate ?? DBNull.Value;

            var outstandingAmountParam =
                cmd.Parameters.Add("@OutstandingAmount", SqlDbType.Decimal);
            outstandingAmountParam.Precision = 10;
            outstandingAmountParam.Scale = 2;
            outstandingAmountParam.Value = (object?)project.OutstandingAmount ?? DBNull.Value;

            var invoicedAmountParam =
                cmd.Parameters.Add("@InvoicedAmount", SqlDbType.Decimal);
            invoicedAmountParam.Precision = 10;
            invoicedAmountParam.Scale = 2;
            invoicedAmountParam.Value = (object?)project.InvoicedAmount ?? DBNull.Value;

            cmd.Parameters.Add("@BuildingSurveyor", SqlDbType.NVarChar, 100)
                .Value = (object?)project.BuildingSurveyor ?? DBNull.Value;
            cmd.Parameters.Add("@Comments", SqlDbType.NVarChar, -1)
                .Value = (object?)project.Comments ?? DBNull.Value;
            cmd.Parameters.Add("@ClientId", SqlDbType.Int)
                .Value = project.ClientId;
        }

        // Helper methods to map SqlDataReader rows to entity objects
        private static Project MapProject(SqlDataReader reader)
        {
            return new Project
            {
                ProjectId = reader.GetInt32(reader.GetOrdinal("ProjectId")),
                ProjectNumber = reader.GetString(reader.GetOrdinal("ProjectNumber")),
                ProjectName = reader.GetString(reader.GetOrdinal("ProjectName")),
                FeeProposalStatus = Enum.Parse<FeeProposalStatus>(reader.GetString(reader.GetOrdinal("FeeProposalStatus"))),
                ProjectManager = reader.IsDBNull(reader.GetOrdinal("ProjectManager")) ? null : reader.GetString(reader.GetOrdinal("ProjectManager")),
                FeeProposal = reader.IsDBNull(reader.GetOrdinal("FeeProposal")) ? null : reader.GetDecimal(reader.GetOrdinal("FeeProposal")),
                PbdbStatus = reader.IsDBNull(reader.GetOrdinal("PbdbStatus")) ? null : Enum.Parse<CompletionStatus>(reader.GetString(reader.GetOrdinal("PbdbStatus"))),
                FserStatus = reader.IsDBNull(reader.GetOrdinal("FserStatus")) ? null : Enum.Parse<CompletionStatus>(reader.GetString(reader.GetOrdinal("FserStatus"))),
                PaymentDueDate = reader.IsDBNull(reader.GetOrdinal("PaymentDueDate")) ? null : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("PaymentDueDate"))),
                OutstandingAmount = reader.IsDBNull(reader.GetOrdinal("OutstandingAmount")) ? null : reader.GetDecimal(reader.GetOrdinal("OutstandingAmount")),
                InvoicedAmount = reader.IsDBNull(reader.GetOrdinal("InvoicedAmount")) ? null : reader.GetDecimal(reader.GetOrdinal("InvoicedAmount")),
                BuildingSurveyor = reader.IsDBNull(reader.GetOrdinal("BuildingSurveyor")) ? null : reader.GetString(reader.GetOrdinal("BuildingSurveyor")),
                Comments = reader.IsDBNull(reader.GetOrdinal("Comments")) ? null : reader.GetString(reader.GetOrdinal("Comments")),
                ClientId = reader.GetInt32(reader.GetOrdinal("ClientId")),
                IsArchived = reader.GetBoolean(reader.GetOrdinal("IsArchived"))
            };
        }

        private static Building MapBuilding(SqlDataReader reader)
        {
            return new Building
            {
                BuildingId = reader.GetInt32(reader.GetOrdinal("BuildingId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Address = reader.GetString(reader.GetOrdinal("Address"))
            };
        }

        private static BuildingClass MapBuildingClass(SqlDataReader reader)
        {
            return new BuildingClass
            {
                BuildingClassId = reader.GetInt32(reader.GetOrdinal("BuildingClassId")),
                ClassCode = reader.GetString(reader.GetOrdinal("ClassCode")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
            };
        }

        private static TechnicalIssue MapTechnicalIssue(SqlDataReader reader)
        {
            return new TechnicalIssue
            {
                TechnicalIssueId = reader.GetInt32(reader.GetOrdinal("TechnicalIssueId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
            };
        }

        // Helper method to insert associated buildings, building classes, and technical issues for a project
        private static void InsertProjectAssociations(
            Project project,
            int projectId,
            SqlConnection connection,
            SqlTransaction transaction)
        {
            // Insert associated buildings into the PROJECT_BUILDING table
            foreach (var building in project.Buildings)
            {
                using SqlCommand buildingsCmd = new("dbo.uspInsertProjectBuilding",
                                                    connection,
                                                    transaction);
                buildingsCmd.CommandType = CommandType.StoredProcedure;
                buildingsCmd.Parameters.Add("@ProjectId", SqlDbType.Int).Value = projectId;
                buildingsCmd.Parameters.Add("@BuildingId", SqlDbType.Int).Value = building.BuildingId;
                buildingsCmd.ExecuteNonQuery();
            }
            // Insert associated building classes into the PROJECT_BUILDINGCLASS table
            foreach (var buildingClass in project.BuildingClasses)
            {
                using SqlCommand buildingClassesCmd = new("dbo.uspInsertProjectBuildingClass",
                                                          connection,
                                                          transaction);
                buildingClassesCmd.CommandType = CommandType.StoredProcedure;
                buildingClassesCmd.Parameters.Add("@ProjectId", SqlDbType.Int).Value = projectId;
                buildingClassesCmd.Parameters.Add("@BuildingClassId", SqlDbType.Int).Value = buildingClass.BuildingClassId;
                buildingClassesCmd.ExecuteNonQuery();
            }
            // Insert associated technical issues into the PROJECT_TECHNICALISSUE table
            foreach (var technicalIssue in project.TechnicalIssues)
            {
                using SqlCommand technicalIssuesCmd = new("dbo.uspInsertProjectTechnicalIssue",
                                                          connection,
                                                          transaction);
                technicalIssuesCmd.CommandType = CommandType.StoredProcedure;
                technicalIssuesCmd.Parameters.Add("@ProjectId", SqlDbType.Int).Value = projectId;
                technicalIssuesCmd.Parameters.Add("@TechnicalIssueId", SqlDbType.Int).Value = technicalIssue.TechnicalIssueId;
                technicalIssuesCmd.ExecuteNonQuery();
            }
        }
    }
}
