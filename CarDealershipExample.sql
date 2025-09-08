-- Create the database if it doesn't exist
IF DB_ID('CarDealership') IS NULL
    CREATE DATABASE CarDealership;
GO

USE CarDealership;
GO

-- Create Manufacturers table
IF OBJECT_ID('Manufacturers', 'U') IS NULL
BEGIN
    CREATE TABLE Manufacturers (
        ManufacturerID INT PRIMARY KEY IDENTITY(1,1),
        Name VARCHAR(100) NOT NULL,
        Country VARCHAR(50)
    );
END
GO

-- Create Cars table
IF OBJECT_ID('Cars', 'U') IS NULL
BEGIN
    CREATE TABLE Cars (
        CarID INT PRIMARY KEY IDENTITY(1,1),
        ManufacturerID INT FOREIGN KEY REFERENCES Manufacturers(ManufacturerID),
        Model VARCHAR(100) NOT NULL,
        Year INT,
        Price DECIMAL(10,2),
        Color VARCHAR(50)
    );
END
GO

-- Insert into Manufacturers only if empty
IF NOT EXISTS (SELECT 1 FROM Manufacturers)
BEGIN
    INSERT INTO Manufacturers (Name, Country)
    VALUES
        ('Toyota', 'Japan'),
        ('Ford', 'USA'),
        ('BMW', 'Germany');
END
GO

-- Insert into Cars only if empty
IF NOT EXISTS (SELECT 1 FROM Cars)
BEGIN
    INSERT INTO Cars (ManufacturerID, Model, Year, Price, Color)
    VALUES
        ((SELECT ManufacturerID FROM Manufacturers WHERE Name = 'Toyota'), 'Corolla', 2020, 20000, 'White'),
        ((SELECT ManufacturerID FROM Manufacturers WHERE Name = 'Toyota'), 'Camry', 2021, 25000, 'Black'),
        ((SELECT ManufacturerID FROM Manufacturers WHERE Name = 'Ford'), 'Mustang', 2022, 30000, 'Red'),
        ((SELECT ManufacturerID FROM Manufacturers WHERE Name = 'BMW'), 'X5', 2023, 50000, 'Blue');
END
GO
