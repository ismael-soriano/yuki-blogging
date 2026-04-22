IF DB_ID(N'authors') IS NULL
BEGIN
    CREATE DATABASE [authors];
END
GO

USE [authors];
GO

IF OBJECT_ID(N'dbo.authors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.authors (
        id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        name NVARCHAR(255) NOT NULL,
        surname NVARCHAR(255) NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.authors WHERE id = '9f9df8ca-4314-4d0d-a629-fcb0cead5dae')
BEGIN
    INSERT INTO dbo.authors (id, name, surname)
    VALUES ('9f9df8ca-4314-4d0d-a629-fcb0cead5dae', 'Ada', 'Lovelace');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.authors WHERE id = '78e270c0-1134-4f52-9d90-dc559b1cbec5')
BEGIN
    INSERT INTO dbo.authors (id, name, surname)
    VALUES ('78e270c0-1134-4f52-9d90-dc559b1cbec5', 'Linus', 'Torvalds');
END
GO

