using Eurodan.Core.Enums;
using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Eurodan.Data.Repositories
{
    public class SqlProjectRepository(string connectionString) : IRepository<Project>
    {
        private readonly string _connectionString = connectionString;
        public void Add(Project project)
        {
            string query = @"INSERT INTO dbo.PROJECT (ProjectNumber, ProjectName, FeeProposalStatus, 
                                                      ProjectManager, FeeProposal, PbdbStatus, FserStatus,
                                                      PaymentDueDate, OutstandingAmount, InvoicedAmount, 
                                                      BuildingSurveyor, Comments, ClientId)
                             OUTPUT INSERTED.ProjectId
                             Values (@ProjectNumber, @ProjectName, @FeeProposalStatus, 
                                     @ProjectManager, @FeeProposal, @PbdbStatus, @FserStatus,
                                     @PaymentDueDate, @OutstandingAmount, @InvoicedAmount,
                                     @BuildingSurveyor, @Comments, @ClientId)";

            using SqlConnection connection = new(_connectionString);
            connection.Open();

            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                using SqlCommand cmd = new(query, connection, transaction);

                AddProjectParameters(cmd, project);

                object? result = cmd.ExecuteScalar();

                if (result is not int projectId)
                {
                    throw new InvalidOperationException(
                        "Failed to insert project and retrieve the new ProjectId.");
                }

                // Insert associated buildings into the PROJECT_BUILDING table
                string buildingInsertQuery
                    = @"INSERT INTO dbo.PROJECT_BUILDING (ProjectId, BuildingId)
                        VALUES (@ProjectId, @BuildingId)";

                foreach (var building in project.Buildings)
                {
                    using SqlCommand buildingsCmd = new(buildingInsertQuery,
                                                        connection,
                                                        transaction);

                    buildingsCmd.Parameters.AddWithValue("@ProjectId", projectId);
                    buildingsCmd.Parameters.AddWithValue("@BuildingId", building.BuildingId);
                    buildingsCmd.ExecuteNonQuery();
                }

                // Insert associated building classes into the PROJECT_BUILDINGCLASS table
                string buildingClassInsertQuery
                    = @"INSERT INTO dbo.PROJECT_BUILDINGCLASS (ProjectId, BuildingClassId)
                        VALUES (@ProjectId, @BuildingClassId)";

                foreach (var buildingClass in project.BuildingClasses)
                {
                    using SqlCommand buildingClassesCmd = new(buildingClassInsertQuery,
                                                              connection,
                                                              transaction);
                    buildingClassesCmd.Parameters.AddWithValue("@ProjectId", projectId);
                    buildingClassesCmd.Parameters.AddWithValue("@BuildingClassId", buildingClass.BuildingClassId);
                    buildingClassesCmd.ExecuteNonQuery();
                }

                // Insert associated technical issues into the PROJECT_TECHNICALISSUE table
                string technicalIssueInsertQuery
                    = @"INSERT INTO dbo.PROJECT_TECHNICALISSUE (ProjectId, TechnicalIssueId)
                        VALUES (@ProjectId, @TechnicalIssueId)";

                foreach (var technicalIssue in project.TechnicalIssues)
                {
                    using SqlCommand technicalIssuesCmd = new(technicalIssueInsertQuery,
                                                              connection,
                                                              transaction);
                    technicalIssuesCmd.Parameters.AddWithValue("@ProjectId", projectId);
                    technicalIssuesCmd.Parameters.AddWithValue("@TechnicalIssueId", technicalIssue.TechnicalIssueId);
                    technicalIssuesCmd.ExecuteNonQuery();
                }

                transaction.Commit();

                project.ProjectId = projectId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void Delete(int id)
        {

            using SqlConnection connection = new(_connectionString);

            using SqlCommand cmd = new("dbo.uspDeleteProject", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ProjectId", id);

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
                                    FROM dbo.PROJECT";

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
            string query = @"UPDATE dbo.PROJECT
                            SET ProjectNumber = @ProjectNumber,
                                ProjectName = @ProjectName,
                                FeeProposalStatus = @FeeProposalStatus,
                                ProjectManager = @ProjectManager,
                                FeeProposal = @FeeProposal,
                                PbdbStatus = @PbdbStatus,
                                FserStatus = @FserStatus,
                                PaymentDueDate = @PaymentDueDate,
                                OutstandingAmount = @OutstandingAmount,
                                InvoicedAmount = @InvoicedAmount,
                                BuildingSurveyor = @BuildingSurveyor,
                                Comments = @Comments,
                                ClientId = @ClientId
                            WHERE ProjectId = @ProjectId";

            using SqlConnection connection = new(_connectionString);
            connection.Open();

            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {

                using SqlCommand cmd = new(query, connection, transaction);

                cmd.Parameters.Add("@ProjectId", SqlDbType.Int)
                    .Value = project.ProjectId;
                AddProjectParameters(cmd, project);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        "No project found with the specified ProjectId.");
                }

                // Delete existing associations in PROJECT_BUILDING, PROJECT_BUILDINGCLASS, and PROJECT_TECHNICALISSUE
                string deleteProjectBuildingQuery = @"DELETE FROM dbo.PROJECT_BUILDING 
                                                      WHERE ProjectId = @ProjectId";

                string deleteProjectBuildingClassQuery = @"DELETE FROM dbo.PROJECT_BUILDINGCLASS 
                                                           WHERE ProjectId = @ProjectId";

                string deleteProjectTechnicalIssueQuery = @"DELETE FROM dbo.PROJECT_TECHNICALISSUE 
                                                            WHERE ProjectId = @ProjectId";

                using SqlCommand deleteProjectBuildingCmd
                    = new(deleteProjectBuildingQuery, connection, transaction);
                deleteProjectBuildingCmd.Parameters.AddWithValue("@ProjectId", project.ProjectId);
                deleteProjectBuildingCmd.ExecuteNonQuery();

                using SqlCommand deleteProjectBuildingClassCmd
                    = new(deleteProjectBuildingClassQuery, connection, transaction);
                deleteProjectBuildingClassCmd.Parameters.AddWithValue("@ProjectId", project.ProjectId);
                deleteProjectBuildingClassCmd.ExecuteNonQuery();

                using SqlCommand deleteProjectTechnicalIssueCmd
                    = new(deleteProjectTechnicalIssueQuery, connection, transaction);
                deleteProjectTechnicalIssueCmd.Parameters.AddWithValue("@ProjectId", project.ProjectId);
                deleteProjectTechnicalIssueCmd.ExecuteNonQuery();

                // Re-insert updated buildings into the PROJECT_BUILDING table
                string buildingInsertQuery
                    = @"INSERT INTO dbo.PROJECT_BUILDING (ProjectId, BuildingId)
                        VALUES (@ProjectId, @BuildingId)";

                foreach (var building in project.Buildings)
                {
                    using SqlCommand buildingsCmd = new(buildingInsertQuery,
                                                        connection,
                                                        transaction);

                    buildingsCmd.Parameters.AddWithValue("@ProjectId", project.ProjectId);
                    buildingsCmd.Parameters.AddWithValue("@BuildingId", building.BuildingId);
                    buildingsCmd.ExecuteNonQuery();
                }

                // Re-insert updated building classes into the PROJECT_BUILDINGCLASS table
                string buildingClassInsertQuery
                    = @"INSERT INTO dbo.PROJECT_BUILDINGCLASS (ProjectId, BuildingClassId)
                        VALUES (@ProjectId, @BuildingClassId)";

                foreach (var buildingClass in project.BuildingClasses)
                {
                    using SqlCommand buildingClassesCmd = new(buildingClassInsertQuery,
                                                              connection,
                                                              transaction);
                    buildingClassesCmd.Parameters.AddWithValue("@ProjectId", project.ProjectId);
                    buildingClassesCmd.Parameters.AddWithValue("@BuildingClassId", buildingClass.BuildingClassId);
                    buildingClassesCmd.ExecuteNonQuery();
                }

                // Re-insert updated technical issues into the PROJECT_TECHNICALISSUE table
                string technicalIssueInsertQuery
                    = @"INSERT INTO dbo.PROJECT_TECHNICALISSUE (ProjectId, TechnicalIssueId)
                        VALUES (@ProjectId, @TechnicalIssueId)";

                foreach (var technicalIssue in project.TechnicalIssues)
                {
                    using SqlCommand technicalIssuesCmd = new(technicalIssueInsertQuery,
                                                              connection,
                                                              transaction);
                    technicalIssuesCmd.Parameters.AddWithValue("@ProjectId", project.ProjectId);
                    technicalIssuesCmd.Parameters.AddWithValue("@TechnicalIssueId", technicalIssue.TechnicalIssueId);
                    technicalIssuesCmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
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
    }
}
