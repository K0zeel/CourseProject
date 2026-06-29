-- Расширение колонки Password для хранения PBKDF2-хеша (~80+ символов).
-- Выполните в SSMS на базе kokos, если вход admin/admin падает с «Ошибка входа».

USE [kokos];
GO

IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'Users'
      AND COLUMN_NAME = 'Password'
      AND (CHARACTER_MAXIMUM_LENGTH IS NULL OR CHARACTER_MAXIMUM_LENGTH < 256)
)
BEGIN
    ALTER TABLE [dbo].[Users] ALTER COLUMN [Password] NVARCHAR(256) NOT NULL;
    PRINT 'Колонка Password расширена до NVARCHAR(256).';
END
ELSE
    PRINT 'Колонка Password уже достаточной длины.';
GO
