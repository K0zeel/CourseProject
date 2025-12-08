-- Создание базы данных WrestlingDB
-- Выполните этот скрипт в SQL Server Management Studio (SSMS)

-- Создаем базу данных
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'WrestlingDB')
BEGIN
    CREATE DATABASE WrestlingDB;
END
GO

USE WrestlingDB;
GO

-- Таблица ролей
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE Roles (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(50) NOT NULL
    );
    
    -- Начальные роли
    INSERT INTO Roles (Name) VALUES ('Администратор');
    INSERT INTO Roles (Name) VALUES ('Пользователь');
END
GO

-- Таблица пользователей
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Password NVARCHAR(100) NOT NULL,
        RoleId INT NOT NULL,
        FOREIGN KEY (RoleId) REFERENCES Roles(Id)
    );
    
    -- Начальный администратор (логин: admin, пароль: admin)
    INSERT INTO Users (Username, Password, RoleId) VALUES ('admin', 'admin', 1);
END
GO

-- Таблица весовых категорий
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WeightCategories' AND xtype='U')
BEGIN
    CREATE TABLE WeightCategories (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Weight INT NOT NULL
    );
    
    -- Стандартные весовые категории
    INSERT INTO WeightCategories (Weight) VALUES (31);
    INSERT INTO WeightCategories (Weight) VALUES (34);
    INSERT INTO WeightCategories (Weight) VALUES (38);
    INSERT INTO WeightCategories (Weight) VALUES (42);
    INSERT INTO WeightCategories (Weight) VALUES (46);
    INSERT INTO WeightCategories (Weight) VALUES (50);
    INSERT INTO WeightCategories (Weight) VALUES (54);
    INSERT INTO WeightCategories (Weight) VALUES (58);
    INSERT INTO WeightCategories (Weight) VALUES (62);
    INSERT INTO WeightCategories (Weight) VALUES (66);
    INSERT INTO WeightCategories (Weight) VALUES (70);
    INSERT INTO WeightCategories (Weight) VALUES (74);
    INSERT INTO WeightCategories (Weight) VALUES (79);
    INSERT INTO WeightCategories (Weight) VALUES (84);
    INSERT INTO WeightCategories (Weight) VALUES (89);
    INSERT INTO WeightCategories (Weight) VALUES (97);
    INSERT INTO WeightCategories (Weight) VALUES (125);
END
GO

-- Таблица борцов
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Wrestlers' AND xtype='U')
BEGIN
    CREATE TABLE Wrestlers (
        Id INT PRIMARY KEY IDENTITY(1,1),
        LastName NVARCHAR(100) NOT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        MiddleName NVARCHAR(100) NULL,
        BirthDate DATE NOT NULL,
        Country NVARCHAR(100) NULL,
        Region NVARCHAR(100) NULL,
        Club NVARCHAR(200) NULL,
        WeightCategoryId INT NOT NULL,
        FOREIGN KEY (WeightCategoryId) REFERENCES WeightCategories(Id)
    );
    
    -- Тестовые данные борцов
    INSERT INTO Wrestlers (LastName, FirstName, MiddleName, BirthDate, Country, Region, Club, WeightCategoryId)
    VALUES ('Иванов', 'Иван', 'Иванович', '2005-03-15', 'Россия', 'Архангельская область', 'СК "Борец"', 5);
    
    INSERT INTO Wrestlers (LastName, FirstName, MiddleName, BirthDate, Country, Region, Club, WeightCategoryId)
    VALUES ('Петров', 'Петр', 'Петрович', '2006-07-22', 'Россия', 'Архангельская область', 'СК "Чемпион"', 7);
    
    INSERT INTO Wrestlers (LastName, FirstName, MiddleName, BirthDate, Country, Region, Club, WeightCategoryId)
    VALUES ('Сидоров', 'Сидор', NULL, '2004-11-10', 'Россия', 'Вологодская область', 'ДЮСШ №1', 10);
END
GO

PRINT 'База данных WrestlingDB успешно создана!';
GO

