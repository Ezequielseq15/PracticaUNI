/* =====================================================================
   Sistema de Gestión de Proyectos de Graduación UNI
   Script SQL Server (sección 12 de la guía, ampliado con Reviews y
   ProjectDocuments para soportar los endpoints del README).
   ===================================================================== */

IF DB_ID(N'GraduacionUNI') IS NULL
    CREATE DATABASE GraduacionUNI;
GO

USE GraduacionUNI;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
CREATE TABLE dbo.Users (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Email        NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    Role         NVARCHAR(30)  NOT NULL,
    CONSTRAINT CK_Users_Role CHECK (Role IN (N'Estudiante', N'Tutor', N'Coordinador', N'Administrador'))
);
GO

IF OBJECT_ID(N'dbo.Projects', N'U') IS NULL
CREATE TABLE dbo.Projects (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Title       NVARCHAR(250) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Status      NVARCHAR(30)  NOT NULL
        CONSTRAINT DF_Projects_Status DEFAULT 'Propuesta',
    StudentId   INT NOT NULL,
    TutorId     INT NULL,
    CreatedAt   DATETIME2 NOT NULL
        CONSTRAINT DF_Projects_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Projects_Students FOREIGN KEY (StudentId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Projects_Tutors   FOREIGN KEY (TutorId)   REFERENCES dbo.Users(Id),
    CONSTRAINT CK_Projects_Status CHECK (Status IN
        (N'Propuesta', N'Revision', N'Aprobada', N'EnDesarrollo', N'Defensa', N'Cerrada'))
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Projects_StudentId')
    CREATE INDEX IX_Projects_StudentId ON dbo.Projects(StudentId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Projects_TutorId')
    CREATE INDEX IX_Projects_TutorId ON dbo.Projects(TutorId);
GO

IF OBJECT_ID(N'dbo.Reviews', N'U') IS NULL
CREATE TABLE dbo.Reviews (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    TutorId   INT NOT NULL,
    Comments  NVARCHAR(2000) NOT NULL,
    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Reviews_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Reviews_Projects FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Reviews_Tutors   FOREIGN KEY (TutorId)   REFERENCES dbo.Users(Id)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Reviews_ProjectId')
    CREATE INDEX IX_Reviews_ProjectId ON dbo.Reviews(ProjectId);
GO

IF OBJECT_ID(N'dbo.ProjectDocuments', N'U') IS NULL
CREATE TABLE dbo.ProjectDocuments (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId   INT NOT NULL,
    FileName    NVARCHAR(260) NOT NULL,
    StoredName  NVARCHAR(100) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    SizeBytes   BIGINT NOT NULL,
    UploadedAt  DATETIME2 NOT NULL
        CONSTRAINT DF_ProjectDocuments_UploadedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ProjectDocuments_Projects FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE CASCADE
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProjectDocuments_ProjectId')
    CREATE INDEX IX_ProjectDocuments_ProjectId ON dbo.ProjectDocuments(ProjectId);
GO

/* Los usuarios de prueba NO se insertan aquí: sus contraseñas deben guardarse
   con hash (PasswordHasher de ASP.NET Core). La API los crea sola en Development. */


   SELECT Id, Title, Status, StudentId, TutorId FROM Projects;
   SELECT Email, PasswordHash FROM Users;