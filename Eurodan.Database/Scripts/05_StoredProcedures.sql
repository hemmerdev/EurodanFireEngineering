USE EurodanDB;
GO

CREATE OR ALTER PROCEDURE dbo.uspArchiveProject
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE dbo.PROJECT
		SET IsArchived = 1
		WHERE ProjectId = @ProjectId;

	IF @@ROWCOUNT = 0
		THROW 50001, 'No project found with the specified ProjectId.', 1;
END;
GO

CREATE OR ALTER PROCEDURE dbo.uspUnarchiveProject
	@ProjectId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE dbo.PROJECT
		SET IsArchived = 0
		WHERE ProjectId = @ProjectId;

	IF @@ROWCOUNT = 0
		THROW 50001, 'No project found with the specified ProjectId.', 1;
END;
GO

CREATE OR ALTER PROCEDURE dbo.uspInsertProject
	@ProjectNumber NVARCHAR(30),
    @ProjectName NVARCHAR(200),
    @FeeProposalStatus VARCHAR(8),
    @ProjectManager NVARCHAR(100),
    @FeeProposal DECIMAL(10,2),
    @PbdbStatus VARCHAR(13),
    @FserStatus VARCHAR(13),
    @PaymentDueDate DATE,
    @OutstandingAmount DECIMAL(10,2),
    @InvoicedAmount DECIMAL(10,2),
    @BuildingSurveyor NVARCHAR(100),
    @Comments NVARCHAR(MAX),
    @ClientId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

    INSERT INTO dbo.PROJECT (ProjectNumber, ProjectName, FeeProposalStatus, 
                             ProjectManager, FeeProposal, PbdbStatus, FserStatus,
                             PaymentDueDate, OutstandingAmount, InvoicedAmount, 
                             BuildingSurveyor, Comments, ClientId)
                     OUTPUT INSERTED.ProjectId
                     Values (@ProjectNumber, @ProjectName, @FeeProposalStatus, 
                             @ProjectManager, @FeeProposal, @PbdbStatus, @FserStatus,
                             @PaymentDueDate, @OutstandingAmount, @InvoicedAmount,
                             @BuildingSurveyor, @Comments, @ClientId);

END;
GO

CREATE OR ALTER PROCEDURE dbo.uspInsertProjectBuilding
    @ProjectId INT,
    @BuildingId INT
AS 
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    INSERT INTO dbo.PROJECT_BUILDING (ProjectId, BuildingId)
    VALUES (@ProjectId, @BuildingId);
END;
GO

CREATE OR ALTER PROCEDURE dbo.uspInsertProjectBuildingClass
    @ProjectId INT,
    @BuildingClassId INT
AS 
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    INSERT INTO dbo.PROJECT_BUILDINGCLASS (ProjectId, BuildingClassId)
    VALUES (@ProjectId, @BuildingClassId);
END;
GO

CREATE OR ALTER PROCEDURE dbo.uspInsertProjectTechnicalIssue
    @ProjectId INT,
    @TechnicalIssueId INT
AS 
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    INSERT INTO dbo.PROJECT_TECHNICALISSUE (ProjectId, TechnicalIssueId)
    VALUES (@ProjectId, @TechnicalIssueId);
END;
GO


CREATE OR ALTER PROCEDURE dbo.uspClearProjectAssociations
    @ProjectId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DELETE FROM dbo.PROJECT_BUILDING WHERE ProjectId = @ProjectId;
    DELETE FROM dbo.PROJECT_BUILDINGCLASS WHERE ProjectId = @ProjectId;
    DELETE FROM dbo.PROJECT_TECHNICALISSUE WHERE ProjectId = @ProjectId;
END;
GO


CREATE OR ALTER PROCEDURE dbo.uspUpdateProject
    @ProjectId INT,
    @ProjectNumber NVARCHAR(30),
    @ProjectName NVARCHAR(200),
    @FeeProposalStatus VARCHAR(8),
    @ProjectManager NVARCHAR(100),
    @FeeProposal DECIMAL(10,2),
    @PbdbStatus VARCHAR(13),
    @FserStatus VARCHAR(13),
    @PaymentDueDate DATE,
    @OutstandingAmount DECIMAL(10,2),
    @InvoicedAmount DECIMAL(10,2),
    @BuildingSurveyor NVARCHAR(100),
    @Comments NVARCHAR(MAX),
    @ClientId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE dbo.PROJECT
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
       WHERE ProjectId = @ProjectId;

       IF @@ROWCOUNT = 0
           THROW 50001, 'No project found with the specified ProjectId.', 1;
END;
GO

