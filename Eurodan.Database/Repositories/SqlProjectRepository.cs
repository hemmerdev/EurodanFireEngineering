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
            string projectQuery = @"SELECT ProjectId, ProjectNumber, ProjectName, FeeProposalStatus,
                                           ProjectManager, FeeProposal, PbdbStatus, FserStatus,
                                           PaymentDueDate, OutstandingAmount, InvoicedAmount,
                                           BuildingSurveyor, Comments, ClientId
                                    FROM dbo.PROJECT
                                    ORDER BY ProjectId DESC";

            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand cmd = new(projectQuery, connection);
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        projects.Add(new Project
                        {
                            ProjectId = reader.GetInt32(0),
                            ProjectNumber = reader.GetString(1),
                            ProjectName = reader.GetString(2),
                            FeeProposalStatus = Enum.Parse<FeeProposalStatus>(reader.GetString(3)),
                            ProjectManager = reader.IsDBNull(4) ? null : reader.GetString(4),
                            FeeProposal = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                            PbdbStatus = reader.IsDBNull(6) ? null : Enum.Parse<CompletionStatus>(reader.GetString(6)),
                            FserStatus = reader.IsDBNull(7) ? null : Enum.Parse<CompletionStatus>(reader.GetString(7)),
                            PaymentDueDate = reader.IsDBNull(8) ? null : DateOnly.FromDateTime(reader.GetDateTime(8)),
                            OutstandingAmount = reader.IsDBNull(9) ? null : reader.GetDecimal(9),
                            InvoicedAmount = reader.IsDBNull(10) ? null : reader.GetDecimal(10),
                            BuildingSurveyor = reader.IsDBNull(11) ? null : reader.GetString(11),
                            Comments = reader.IsDBNull(12) ? null : reader.GetString(12),
                            ClientId = reader.GetInt32(13)
                        });
                    }
                }

                // Fetch associated buildings for each project
                string buildingQuery = @"SELECT b.BuildingId, b.Name, b.Address
                                 FROM dbo.BUILDING b
                                 INNER JOIN dbo.PROJECT_BUILDING pb
                                    ON b.BuildingId = pb.BuildingId
                                 WHERE pb.ProjectId = @ProjectId";

                foreach (var project in projects)
                {
                    using SqlCommand buildingCmd = new(buildingQuery, connection);
                    buildingCmd.Parameters.AddWithValue("@ProjectId", project.ProjectId);
                    using SqlDataReader buildingReader = buildingCmd.ExecuteReader();

                    while (buildingReader.Read())
                    {
                        project.Buildings.Add(new Building
                        {
                            BuildingId = buildingReader.GetInt32(0),
                            Name = buildingReader.GetString(1),
                            Address = buildingReader.GetString(2)
                        });
                    }
                }

                // Fetch associated building classes for each project
                string buildingClassQuery = @"SELECT bc.BuildingClassId, bc.ClassCode, bc.Description
                                              FROM dbo.BUILDINGCLASS bc
                                              INNER JOIN dbo.PROJECT_BUILDINGCLASS pbc
                                                  ON bc.BuildingClassId = pbc.BuildingClassId
                                              WHERE pbc.ProjectId = @ProjectId";

                foreach (var project in projects)
                {
                    using SqlCommand buildingClassCmd = new(buildingClassQuery, connection);
                    buildingClassCmd.Parameters.AddWithValue("@ProjectId", project.ProjectId);
                    using SqlDataReader buildingClassReader = buildingClassCmd.ExecuteReader();
                    while (buildingClassReader.Read())
                    {
                        project.BuildingClasses.Add(new BuildingClass
                        {
                            BuildingClassId = buildingClassReader.GetInt32(0),
                            ClassCode = buildingClassReader.GetString(1),
                            Description = buildingClassReader.IsDBNull(2) ? null : buildingClassReader.GetString(2)
                        });
                    }
                }

                // Fetch associated technical issues for each project
                string technicalIssueQuery = @"SELECT ti.TechnicalIssueId, ti.Name, ti.Description
                                             FROM dbo.TECHNICALISSUE ti
                                             INNER JOIN dbo.PROJECT_TECHNICALISSUE pti
                                                 ON ti.TechnicalIssueId = pti.TechnicalIssueId
                                             WHERE pti.ProjectId = @ProjectId";

                foreach (var project in projects)
                {
                    using SqlCommand technicalIssueCmd = new(technicalIssueQuery, connection);
                    technicalIssueCmd.Parameters.AddWithValue("@ProjectId", project.ProjectId);
                    using SqlDataReader technicalIssueReader = technicalIssueCmd.ExecuteReader();
                    while (technicalIssueReader.Read())
                    {
                        project.TechnicalIssues.Add(new TechnicalIssue
                        {
                            TechnicalIssueId = technicalIssueReader.GetInt32(0),
                            Name = technicalIssueReader.GetString(1),
                            Description = technicalIssueReader.IsDBNull(2) ? null : technicalIssueReader.GetString(2)
                        });
                    }
                }
            }

            return projects;
        }

        public Project? GetById(int id)
        {
            Project? project = null;
            string projectQuery = @"SELECT ProjectId, ProjectNumber, ProjectName, FeeProposalStatus,
                                           ProjectManager, FeeProposal, PbdbStatus, FserStatus,
                                           PaymentDueDate, OutstandingAmount, InvoicedAmount,
                                           BuildingSurveyor, Comments, ClientId
                                    FROM dbo.PROJECT
                                    WHERE ProjectId = @ProjectId";

            using (SqlConnection connection = new(_connectionString))
            {
                using SqlCommand cmd = new(projectQuery, connection);
                cmd.Parameters.AddWithValue("@ProjectId", id);
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    if (reader.Read())
                    {
                        project = new Project
                        {
                            ProjectId = reader.GetInt32(0),
                            ProjectNumber = reader.GetString(1),
                            ProjectName = reader.GetString(2),
                            FeeProposalStatus = Enum.Parse<FeeProposalStatus>(reader.GetString(3)),
                            ProjectManager = reader.IsDBNull(4) ? null : reader.GetString(4),
                            FeeProposal = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                            PbdbStatus = reader.IsDBNull(6) ? null : Enum.Parse<CompletionStatus>(reader.GetString(6)),
                            FserStatus = reader.IsDBNull(7) ? null : Enum.Parse<CompletionStatus>(reader.GetString(7)),
                            PaymentDueDate = reader.IsDBNull(8) ? null : DateOnly.FromDateTime(reader.GetDateTime(8)),
                            OutstandingAmount = reader.IsDBNull(9) ? null : reader.GetDecimal(9),
                            InvoicedAmount = reader.IsDBNull(10) ? null : reader.GetDecimal(10),
                            BuildingSurveyor = reader.IsDBNull(11) ? null : reader.GetString(11),
                            Comments = reader.IsDBNull(12) ? null : reader.GetString(12),
                            ClientId = reader.GetInt32(13)
                        };
                    }
                }

                if (project is null)
                {
                    return null;
                }

                // Fetch associated buildings for the project
                string buildingQuery = @"SELECT b.BuildingId, b.Name, b.Address
                                         FROM dbo.BUILDING b
                                         INNER JOIN dbo.PROJECT_BUILDING pb ON b.BuildingId = pb.BuildingId
                                         WHERE pb.ProjectId = @ProjectId";

                using SqlCommand buildingCmd = new(buildingQuery, connection);
                buildingCmd.Parameters.AddWithValue("@ProjectId", id);

                using (SqlDataReader buildingReader = buildingCmd.ExecuteReader())
                {
                    while (buildingReader.Read())
                    {
                        project.Buildings.Add(new Building
                        {
                            BuildingId = buildingReader.GetInt32(0),
                            Name = buildingReader.GetString(1),
                            Address = buildingReader.GetString(2)
                        });
                    }
                }

                // Fetch associated building classes for the project
                string buildingClassQuery = @"SELECT bc.BuildingClassId, bc.ClassCode, bc.Description
                                              FROM dbo.BUILDINGCLASS bc
                                              INNER JOIN dbo.PROJECT_BUILDINGCLASS pbc ON bc.BuildingClassId = pbc.BuildingClassId
                                              WHERE pbc.ProjectId = @ProjectId";

                using SqlCommand buildingClassCmd = new(buildingClassQuery, connection);
                buildingClassCmd.Parameters.AddWithValue("@ProjectId", id);

                using (SqlDataReader buildingClassReader = buildingClassCmd.ExecuteReader())
                {
                    while (buildingClassReader.Read())
                    {
                        project.BuildingClasses.Add(new BuildingClass
                        {
                            BuildingClassId = buildingClassReader.GetInt32(0),
                            ClassCode = buildingClassReader.GetString(1),
                            Description = buildingClassReader.IsDBNull(2) ? null : buildingClassReader.GetString(2)
                        });
                    }
                }

                // Fetch associated technical issues for the project
                string technicalIssueQuery = @"SELECT ti.TechnicalIssueId, ti.Name, ti.Description
                                               FROM dbo.TECHNICALISSUE ti
                                               INNER JOIN dbo.PROJECT_TECHNICALISSUE pti ON ti.TechnicalIssueId = pti.TechnicalIssueId
                                               WHERE pti.ProjectId = @ProjectId";

                using SqlCommand technicalIssueCmd = new(technicalIssueQuery, connection);
                technicalIssueCmd.Parameters.AddWithValue("@ProjectId", id);

                using SqlDataReader technicalIssueReader = technicalIssueCmd.ExecuteReader();
                while (technicalIssueReader.Read())
                {
                    project.TechnicalIssues.Add(new TechnicalIssue
                    {
                        TechnicalIssueId = technicalIssueReader.GetInt32(0),
                        Name = technicalIssueReader.GetString(1),
                        Description = technicalIssueReader.IsDBNull(2) ? null : technicalIssueReader.GetString(2)
                    });
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
