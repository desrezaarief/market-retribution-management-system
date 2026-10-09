/*
  SQL Server fallback schema for Sistem Retribusi Pasar - staging/demo.
  Normally the application creates these tables automatically using EF Core EnsureCreated().
  Run this script manually only if the hosting account does not allow CREATE TABLE from the application.
*/

IF OBJECT_ID('dbo.MstMarket','U') IS NULL
BEGIN
    CREATE TABLE dbo.MstMarket(
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MstMarket PRIMARY KEY,
        Code nvarchar(20) NOT NULL,
        Name nvarchar(150) NOT NULL,
        Type nvarchar(30) NOT NULL,
        Address nvarchar(300) NULL,
        IsActive bit NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CreatedBy nvarchar(max) NOT NULL,
        UpdatedAt datetime2 NULL,
        UpdatedBy nvarchar(max) NULL
    );
    CREATE UNIQUE INDEX UX_MstMarket_Code ON dbo.MstMarket(Code);
END;
GO

IF OBJECT_ID('dbo.MstRetributionType','U') IS NULL
BEGIN
    CREATE TABLE dbo.MstRetributionType(
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MstRetributionType PRIMARY KEY,
        Code nvarchar(20) NOT NULL,
        Name nvarchar(100) NOT NULL,
        MediaType nvarchar(20) NOT NULL,
        BillingCycle nvarchar(30) NOT NULL,
        IsActive bit NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CreatedBy nvarchar(max) NOT NULL,
        UpdatedAt datetime2 NULL,
        UpdatedBy nvarchar(max) NULL
    );
    CREATE UNIQUE INDEX UX_MstRetributionType_Code ON dbo.MstRetributionType(Code);
END;
GO

IF OBJECT_ID('dbo.MstTariff','U') IS NULL
BEGIN
    CREATE TABLE dbo.MstTariff(
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MstTariff PRIMARY KEY,
        MarketId int NOT NULL,
        RetributionTypeId int NOT NULL,
        Amount decimal(18,2) NOT NULL,
        EffectiveFrom datetime2 NOT NULL,
        EffectiveTo datetime2 NULL,
        IsActive bit NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CreatedBy nvarchar(max) NOT NULL,
        UpdatedAt datetime2 NULL,
        UpdatedBy nvarchar(max) NULL,
        CONSTRAINT FK_MstTariff_MstMarket_MarketId FOREIGN KEY(MarketId) REFERENCES dbo.MstMarket(Id),
        CONSTRAINT FK_MstTariff_MstRetributionType_RetributionTypeId FOREIGN KEY(RetributionTypeId) REFERENCES dbo.MstRetributionType(Id)
    );
    CREATE INDEX IX_MstTariff_MarketId_RetributionTypeId_EffectiveFrom ON dbo.MstTariff(MarketId,RetributionTypeId,EffectiveFrom);
END;
GO

IF OBJECT_ID('dbo.MstCollector','U') IS NULL
BEGIN
    CREATE TABLE dbo.MstCollector(
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MstCollector PRIMARY KEY,
        Code nvarchar(30) NOT NULL,
        Name nvarchar(120) NOT NULL,
        MarketId int NOT NULL,
        Phone nvarchar(30) NULL,
        IsActive bit NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CreatedBy nvarchar(max) NOT NULL,
        UpdatedAt datetime2 NULL,
        UpdatedBy nvarchar(max) NULL,
        CONSTRAINT FK_MstCollector_MstMarket_MarketId FOREIGN KEY(MarketId) REFERENCES dbo.MstMarket(Id)
    );
    CREATE UNIQUE INDEX UX_MstCollector_Code ON dbo.MstCollector(Code);
    CREATE INDEX IX_MstCollector_MarketId ON dbo.MstCollector(MarketId);
END;
GO

IF OBJECT_ID('dbo.MstRetributionUnit','U') IS NULL
BEGIN
    CREATE TABLE dbo.MstRetributionUnit(
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MstRetributionUnit PRIMARY KEY,
        MarketId int NOT NULL,
        Code nvarchar(50) NOT NULL,
        RetributionTypeId int NOT NULL,
        PayerName nvarchar(150) NOT NULL,
        CollectorId int NULL,
        Status nvarchar(40) NOT NULL,
        EffectiveFrom datetime2 NOT NULL,
        IsActive bit NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CreatedBy nvarchar(max) NOT NULL,
        UpdatedAt datetime2 NULL,
        UpdatedBy nvarchar(max) NULL,
        CONSTRAINT FK_MstRetributionUnit_MstMarket_MarketId FOREIGN KEY(MarketId) REFERENCES dbo.MstMarket(Id),
        CONSTRAINT FK_MstRetributionUnit_MstRetributionType_RetributionTypeId FOREIGN KEY(RetributionTypeId) REFERENCES dbo.MstRetributionType(Id),
        CONSTRAINT FK_MstRetributionUnit_MstCollector_CollectorId FOREIGN KEY(CollectorId) REFERENCES dbo.MstCollector(Id)
    );
    CREATE UNIQUE INDEX UX_MstRetributionUnit_MarketId_Code ON dbo.MstRetributionUnit(MarketId,Code);
    CREATE INDEX IX_MstRetributionUnit_RetributionTypeId ON dbo.MstRetributionUnit(RetributionTypeId);
    CREATE INDEX IX_MstRetributionUnit_CollectorId ON dbo.MstRetributionUnit(CollectorId);
END;
GO

IF OBJECT_ID('dbo.TrxStockBatch','U') IS NULL
BEGIN
    CREATE TABLE dbo.TrxStockBatch(
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TrxStockBatch PRIMARY KEY,
        MediaType nvarchar(20) NOT NULL,
        ReceivedDate datetime2 NOT NULL,
        StartSerial int NOT NULL,
        EndSerial int NOT NULL,
        DocumentNo nvarchar(100) NULL,
        Notes nvarchar(500) NULL,
        IsActive bit NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CreatedBy nvarchar(max) NOT NULL,
        UpdatedAt datetime2 NULL,
        UpdatedBy nvarchar(max) NULL
    );
    CREATE INDEX IX_TrxStockBatch_MediaType_StartSerial_EndSerial ON dbo.TrxStockBatch(MediaType,StartSerial,EndSerial);
END;
GO

IF OBJECT_ID('dbo.TrxMediaDistribution','U') IS NULL
BEGIN
    CREATE TABLE dbo.TrxMediaDistribution(
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TrxMediaDistribution PRIMARY KEY,
        DistributionNo nvarchar(50) NOT NULL,
        MarketId int NOT NULL,
        MediaType nvarchar(20) NOT NULL,
        DistributionDate datetime2 NOT NULL,
        StartSerial int NOT NULL,
        EndSerial int NOT NULL,
        ReceiverName nvarchar(150) NOT NULL,
        RequestLetterNo nvarchar(100) NULL,
        Notes nvarchar(500) NULL,
        IsActive bit NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CreatedBy nvarchar(max) NOT NULL,
        UpdatedAt datetime2 NULL,
        UpdatedBy nvarchar(max) NULL,
        CONSTRAINT FK_TrxMediaDistribution_MstMarket_MarketId FOREIGN KEY(MarketId) REFERENCES dbo.MstMarket(Id)
    );
    CREATE UNIQUE INDEX UX_TrxMediaDistribution_DistributionNo ON dbo.TrxMediaDistribution(DistributionNo);
    CREATE INDEX IX_TrxMediaDistribution_MediaType_StartSerial_EndSerial ON dbo.TrxMediaDistribution(MediaType,StartSerial,EndSerial);
    CREATE INDEX IX_TrxMediaDistribution_MarketId ON dbo.TrxMediaDistribution(MarketId);
END;
GO

IF OBJECT_ID('dbo.TrxReceipt','U') IS NULL
BEGIN
    CREATE TABLE dbo.TrxReceipt(
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TrxReceipt PRIMARY KEY,
        TransactionNo nvarchar(50) NOT NULL,
        ReceiptDate datetime2 NOT NULL,
        MarketId int NOT NULL,
        UnitId int NULL,
        RetributionTypeId int NOT NULL,
        CollectorId int NULL,
        StartSerial int NULL,
        EndSerial int NULL,
        PeriodStart datetime2 NOT NULL,
        Months int NOT NULL,
        ExpectedAmount decimal(18,2) NOT NULL,
        ReceivedAmount decimal(18,2) NOT NULL,
        PaymentMethod nvarchar(30) NOT NULL,
        PaymentReference nvarchar(100) NULL,
        Notes nvarchar(500) NULL,
        Status nvarchar(20) NOT NULL,
        DeleteReason nvarchar(300) NULL,
        IsActive bit NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CreatedBy nvarchar(max) NOT NULL,
        UpdatedAt datetime2 NULL,
        UpdatedBy nvarchar(max) NULL,
        CONSTRAINT FK_TrxReceipt_MstMarket_MarketId FOREIGN KEY(MarketId) REFERENCES dbo.MstMarket(Id),
        CONSTRAINT FK_TrxReceipt_MstRetributionUnit_UnitId FOREIGN KEY(UnitId) REFERENCES dbo.MstRetributionUnit(Id),
        CONSTRAINT FK_TrxReceipt_MstRetributionType_RetributionTypeId FOREIGN KEY(RetributionTypeId) REFERENCES dbo.MstRetributionType(Id),
        CONSTRAINT FK_TrxReceipt_MstCollector_CollectorId FOREIGN KEY(CollectorId) REFERENCES dbo.MstCollector(Id)
    );
    CREATE UNIQUE INDEX UX_TrxReceipt_TransactionNo ON dbo.TrxReceipt(TransactionNo);
    CREATE INDEX IX_TrxReceipt_ReceiptDate_MarketId ON dbo.TrxReceipt(ReceiptDate,MarketId);
    CREATE INDEX IX_TrxReceipt_StartSerial_EndSerial ON dbo.TrxReceipt(StartSerial,EndSerial);
    CREATE INDEX IX_TrxReceipt_UnitId ON dbo.TrxReceipt(UnitId);
    CREATE INDEX IX_TrxReceipt_RetributionTypeId ON dbo.TrxReceipt(RetributionTypeId);
    CREATE INDEX IX_TrxReceipt_CollectorId ON dbo.TrxReceipt(CollectorId);
END;
GO

IF OBJECT_ID('dbo.TrxDeposit','U') IS NULL
BEGIN
    CREATE TABLE dbo.TrxDeposit(
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TrxDeposit PRIMARY KEY,
        DepositNo nvarchar(50) NOT NULL,
        DepositDate datetime2 NOT NULL,
        MarketId int NOT NULL,
        FromDate datetime2 NOT NULL,
        ToDate datetime2 NOT NULL,
        BankName nvarchar(150) NOT NULL,
        ReferenceNo nvarchar(100) NULL,
        Amount decimal(18,2) NOT NULL,
        Notes nvarchar(500) NULL,
        Status nvarchar(30) NOT NULL,
        IsActive bit NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CreatedBy nvarchar(max) NOT NULL,
        UpdatedAt datetime2 NULL,
        UpdatedBy nvarchar(max) NULL,
        CONSTRAINT FK_TrxDeposit_MstMarket_MarketId FOREIGN KEY(MarketId) REFERENCES dbo.MstMarket(Id)
    );
    CREATE UNIQUE INDEX UX_TrxDeposit_DepositNo ON dbo.TrxDeposit(DepositNo);
    CREATE INDEX IX_TrxDeposit_MarketId ON dbo.TrxDeposit(MarketId);
END;
GO

IF OBJECT_ID('dbo.TrxDepositReceipt','U') IS NULL
BEGIN
    CREATE TABLE dbo.TrxDepositReceipt(
        DepositId int NOT NULL,
        ReceiptId int NOT NULL,
        CONSTRAINT PK_TrxDepositReceipt PRIMARY KEY(DepositId,ReceiptId),
        CONSTRAINT FK_TrxDepositReceipt_TrxDeposit_DepositId FOREIGN KEY(DepositId) REFERENCES dbo.TrxDeposit(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TrxDepositReceipt_TrxReceipt_ReceiptId FOREIGN KEY(ReceiptId) REFERENCES dbo.TrxReceipt(Id)
    );
    CREATE UNIQUE INDEX UX_TrxDepositReceipt_ReceiptId ON dbo.TrxDepositReceipt(ReceiptId);
END;
GO

IF OBJECT_ID('dbo.AppUser','U') IS NULL
BEGIN
    CREATE TABLE dbo.AppUser(
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppUser PRIMARY KEY,
        Username nvarchar(80) NOT NULL,
        DisplayName nvarchar(150) NOT NULL,
        Role nvarchar(30) NOT NULL,
        MarketId int NULL,
        PasswordHash nvarchar(max) NOT NULL,
        IsActive bit NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CreatedBy nvarchar(max) NOT NULL,
        UpdatedAt datetime2 NULL,
        UpdatedBy nvarchar(max) NULL,
        CONSTRAINT FK_AppUser_MstMarket_MarketId FOREIGN KEY(MarketId) REFERENCES dbo.MstMarket(Id)
    );
    CREATE UNIQUE INDEX UX_AppUser_Username ON dbo.AppUser(Username);
    CREATE INDEX IX_AppUser_MarketId ON dbo.AppUser(MarketId);
END;
GO

IF OBJECT_ID('dbo.AccountingPeriod','U') IS NULL
BEGIN
    CREATE TABLE dbo.AccountingPeriod(
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AccountingPeriod PRIMARY KEY,
        Period nvarchar(7) NOT NULL,
        Status nvarchar(20) NOT NULL,
        ClosedAt datetime2 NULL,
        ClosedBy nvarchar(max) NULL,
        ReopenReason nvarchar(300) NULL,
        IsActive bit NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CreatedBy nvarchar(max) NOT NULL,
        UpdatedAt datetime2 NULL,
        UpdatedBy nvarchar(max) NULL
    );
    CREATE UNIQUE INDEX UX_AccountingPeriod_Period ON dbo.AccountingPeriod(Period);
END;
GO

IF OBJECT_ID('dbo.AuditLog','U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLog(
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLog PRIMARY KEY,
        Timestamp datetime2 NOT NULL,
        UserName nvarchar(80) NOT NULL,
        Action nvarchar(30) NOT NULL,
        EntityName nvarchar(80) NOT NULL,
        EntityId int NOT NULL,
        Description nvarchar(1000) NOT NULL,
        IsActive bit NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CreatedBy nvarchar(max) NOT NULL,
        UpdatedAt datetime2 NULL,
        UpdatedBy nvarchar(max) NULL
    );
    CREATE INDEX IX_AuditLog_EntityName_EntityId_Timestamp ON dbo.AuditLog(EntityName,EntityId,Timestamp);
END;
GO
