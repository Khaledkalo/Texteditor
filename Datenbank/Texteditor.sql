
USE master;
GO

IF DB_ID(N'Editor') IS NULL
  CREATE DATABASE Editor;
GO

USE Editor;
GO 

IF OBJECT_ID('Data') IS NOT NULL
  DROP TABLE Data;
GO

CREATE TABLE Data (
 ID_Text INT IDENTITY PRIMARY KEY, 
 Text Text,
 Zeitstempel datetime,
);