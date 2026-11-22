USE EurodanDB;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '1a')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('1a', N'Detached houses, townhouses, terrace houses');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '1b')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('1b', N'Small boarding houses, guest houses and certain holiday accommodation');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '2')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('2', N'Apartment buildings');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '3')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('3', N'Hotels, motels, hostels and other residential accommodation');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '4')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('4', N'A single dwelling within a Class 5-9 building');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '5')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('5', N'Offices');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '6')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('6', N'Shops, restaurants, cafés and retail premises');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '7a')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('7a', N'Carparks');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '7b')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('7b', N'Warehouses and storage buildings');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '8')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('8', N'Factories, workshops and laboratories');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '9a')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('9a', N'Health-care buildings');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '9b')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('9b', N'Assembly buildings, schools and public halls');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '9c')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('9c', N'Residential care buildings');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '10a')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('10a', N'Sheds, private garages and carports');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '10b')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('10b', N'Fences, retaining walls, swimming pools and similar structures');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BUILDINGCLASS WHERE ClassCode = '10c')
BEGIN
    INSERT INTO dbo.BUILDINGCLASS (ClassCode, Description)
    VALUES ('10c', N'Private bushfire shelters');
END;
GO

-- Display the records to verify the seed data.
SELECT BuildingClassId, ClassCode, Description
FROM dbo.BUILDINGCLASS
ORDER BY BuildingClassId;