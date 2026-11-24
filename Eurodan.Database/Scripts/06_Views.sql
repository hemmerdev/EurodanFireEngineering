USE EurodanDB;
GO

CREATE OR ALTER VIEW dbo.vwProjectBuildings
AS
SELECT pb.ProjectId, b.BuildingId, b.Name, b.Address 
    FROM dbo.BUILDING b
    INNER JOIN dbo.PROJECT_BUILDING pb
       ON b.BuildingId = pb.BuildingId;
GO

CREATE OR ALTER VIEW dbo.vwProjectBuildingClasses
AS
SELECT pbc.ProjectId, bc.BuildingClassId, bc.ClassCode, bc.Description 
    FROM dbo.BUILDINGCLASS bc
    INNER JOIN dbo.PROJECT_BUILDINGCLASS pbc
       ON bc.BuildingClassId = pbc.BuildingClassId;
GO

CREATE OR ALTER VIEW dbo.vwProjectTechnicalIssues
AS
SELECT pti.ProjectId, ti.TechnicalIssueId, ti.Name, ti.Description 
    FROM dbo.TECHNICALISSUE ti
    INNER JOIN dbo.PROJECT_TECHNICALISSUE pti
       ON ti.TechnicalIssueId = pti.TechnicalIssueId;
GO

CREATE OR ALTER VIEW dbo.vwProjects
AS
SELECT ProjectId, ProjectNumber, ProjectName, 
       FeeProposalStatus, ProjectManager, 
       FeeProposal, PbdbStatus, FserStatus,
       PaymentDueDate, OutstandingAmount, InvoicedAmount,
       BuildingSurveyor, Comments, ClientId, IsArchived
FROM dbo.PROJECT;
GO
