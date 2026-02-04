-- Docker Database Initialization Script
-- This script creates the database and runs schema + seed scripts

-- Create database if not exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'InventarioDB')
BEGIN
    CREATE DATABASE InventarioDB;
END
GO

USE InventarioDB;
GO

-- Check if tables already exist (to avoid re-running on restart)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Person')
BEGIN
    PRINT 'Initializing database schema...';

    -- The schema and seed will be run from mounted scripts
    -- This file serves as the entry point
END
ELSE
BEGIN
    PRINT 'Database already initialized, skipping...';
END
GO
