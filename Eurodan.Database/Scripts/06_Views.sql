USE EurodanDB;
GO

CREATE OR ALTER VIEW dbo.vwProjectBuildings
AS
SELECT pb.ProjectId, b.BuildingId, b.Name, b.Address 
    FROM dbo.BUILDING b
    INNER JOIN dbo.PROJECT_BUILDING pb
       ON b.BuildingId = pb.BuildingId;
GO

CREATE OR ALTER VIEW dbo.vwProjectTechnicalIssues
AS
SELECT pti.ProjectId, ti.TechnicalIssueId, ti.Name, ti.Description 
    FROM dbo.TECHNICALISSUE ti
    INNER JOIN dbo.PROJECT_TECHNICALISSUE pti
       ON ti.TechnicalIssueId = pti.TechnicalIssueId;
GO
