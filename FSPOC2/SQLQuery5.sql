-- get real table name
CREATE PROCEDURE getTableRealNameWithMeta	@applicationName	NVARCHAR(50),
											@tableName			NVARCHAR(50),
											@tableRealName		NVARCHAR(50)	OUTPUT,
											@DbTablePrefix		NVARCHAR(50)	OUTPUT,
											@DbMetaTables		NVARCHAR(50)	OUTPUT					
AS
DECLARE	@tableId INT,
		@sql NVARCHAR(MAX);
		
SELECT @DbTablePrefix = DbTablePrefix, @DbMetaTables = DbMetaTables FROM dbo.Applications WHERE Name = @applicationName;
SET @sql = CONCAT('SELECT @tableId=Id FROM ', @DbTablePrefix, ' WHERE Name = @tableName');
exec sp_executesql @sql, N'@tableId Int output, @tableName NVARCHAR(50)', @tableId output, @tableName = @tableName; SET @tableRealName = CONCAT(@DbTablePrefix, @tableId); 
GO

CREATE PROCEDURE getTableRealName	@applicationName	NVARCHAR(50),
									@tableName			NVARCHAR(50),
									@tableRealName		NVARCHAR(50)	OUTPUT
									
AS
DECLARE	@DbTablePrefix NVARCHAR(50),
		@DbMetaTables NVARCHAR(50);

exec getTableRealNameWithMeta @applicationName, @tableName, @tableRealName OUTPUT, @DbTablePrefix OUTPUT, @DbMetaTables OUTPUT;


DROP PROCEDURE getTableRealName;
DROP PROCEDURE getTableRealNameWithMeta;


-- create table
DECLARE @applicationName NVARCHAR(50) = 'test';
DECLARE @tableName NVARCHAR(50) = 'aa4';
DECLARE @columnDefinition NVARCHAR(200) = 'Id INT NOT NULL, Name NVARCHAR(50) NULL';

DECLARE @id INT;
DECLARE @prefix TABLE(DbTablePrefix NVARCHAR(50), DbMetaTables NVARCHAR(50));INSERT INTO @prefix SELECT DbTablePrefix, DbMetaTables FROM dbo.Applications WHERE Name = @applicationName;
DECLARE @sql NVARCHAR(MAX) = CONCAT('INSERT INTO ', (SELECT DbMetaTables FROM @prefix), '(Name) VALUES(@table); SELECT @id=Id FROM ', (SELECT DbMetaTables FROM @prefix), ' WHERE Name = @table;' );
exec sp_executesql @sql, N'@id int output, @table NVARCHAR(50)', @id output, @table = @tableName;

SET @sql = CONCAT('CREATE TABLE ', (SELECT DbTablePrefix FROM @prefix), @id, '(', @columnDefinition, ');');
exec (@sql);


-- get columns
DECLARE @applicationName NVARCHAR(50) = 'test';
DECLARE @tableName NVARCHAR(50) = 'testTable';

DECLARE @tableId INT;
DECLARE @prefix TABLE(DbTablePrefix NVARCHAR(50), DbMetaTables NVARCHAR(50));INSERT INTO @prefix SELECT DbTablePrefix, DbMetaTables FROM dbo.Applications WHERE Name = @applicationName;
DECLARE @sql NVARCHAR(MAX) = CONCAT('SELECT @tableId=Id FROM ', (SELECT DbMetaTables FROM @Prefix), ' WHERE Name = @tableName');
exec sp_executesql @sql, N'@tableId Int output, @tableName NVARCHAR(50)', @tableId output, @tableName = @tableName
DECLARE @realTableName NVARCHAR(50) = CONCAT((SELECT DbTablePrefix FROM @prefix), @tableId);

SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(@realTableName);


-- add column
DECLARE @applicationName NVARCHAR(50) = 'test';
DECLARE @tableName NVARCHAR(50) = 'aa4';
DECLARE @columnDefinition NVARCHAR(200) = 'Name3 NVARCHAR(50) NULL';

DECLARE @tableId INT;
DECLARE @prefix TABLE(DbTablePrefix NVARCHAR(50), DbMetaTables NVARCHAR(50));INSERT INTO @prefix SELECT DbTablePrefix, DbMetaTables FROM dbo.Applications WHERE Name = @applicationName;
DECLARE @sql NVARCHAR(MAX) = CONCAT('SELECT @tableId=Id FROM ', (SELECT DbMetaTables FROM @prefix), ' WHERE Name = @tableName');
exec sp_executesql @sql, N'@tableId int output, @tableName NVARCHAR(50)', @tableId output, @tableName = @tableName;

SET @sql = CONCAT('ALTER TABLE ', (SELECT DbTablePrefix FROM @prefix), @tableId, ' ADD ', @columnDefinition);
exec (@sql);


-- DROP TABLE
DECLARE @applicationName NVARCHAR(50) = 'test';
DECLARE @tableName NVARCHAR(50) = 'aa4';

DECLARE @tableId INT;
DECLARE @prefix TABLE(DbTablePrefix NVARCHAR(50), DbMetaTables NVARCHAR(50));INSERT INTO @prefix SELECT DbTablePrefix, DbMetaTables FROM dbo.Applications WHERE Name = @applicationName;
DECLARE @sql NVARCHAR(MAX) = CONCAT('SELECT @tableId=Id FROM ', (SELECT DbMetaTables FROM @Prefix), ' WHERE Name = @tableName');
exec sp_executesql @sql, N'@tableId int output, @tableName NVARCHAR(50)', @tableId output, @tableName = @tableName;

SET @sql = CONCAT('DROP TABLE ', (SELECT DbTablePrefix FROM @prefix), @tableId, ';DELETE FROM ', (SELECT DbMetaTables FROM @prefix), ' WHERE Id = @tableId');
exec sp_executesql @sql, N'@tableId int', @tableId = @tableId;


-- RENAME Table
DECLARE @applicationName NVARCHAR(50) = 'test';
DECLARE @tableName NVARCHAR(50) = 'aa4';
DECLARE @newName NVARCHAR(50) = 'uu';

DECLARE @MetaTables NVARCHAR(100) = (SELECT DbMetaTables FROM dbo.Applications WHERE Name = @applicationName);
DECLARE @sql NVARCHAR(MAX) = CONCAT('UPDATE ', @MetaTables, ' SET Name = @newName WHERE Name = @tableName;');
exec sp_executesql @sql, N'@tableName NVARCHAR(50) output, @newName NVARCHAR(50)', @tableName = @tableName, @newName = @newName;


-- SELECT table list
DECLARE @applicationName NVARCHAR(50) = 'test';
DECLARE @columnNames NVARCHAR(200) = '*';

DECLARE @tableName NVARCHAR(50) = (SELECT DbMetaTables FROM dbo.Applications WHERE Name = @applicationName);
DECLARE @sql NVARCHAR(MAX) = CONCAT('SELECT ', @columnNames, ' FROM ', @tableName, ';');
exec (@sql);


-- SELECT
DECLARE @applicationName NVARCHAR(50) = 'test';
DECLARE @tableName NVARCHAR(50) = 'testTable';
DECLARE @columnNames NVARCHAR(200) = '*';
DECLARE @additional NVARCHAR(500) = '';

DECLARE @tableId INT;
DECLARE @prefix TABLE(DbTablePrefix NVARCHAR(50), DbMetaTables NVARCHAR(50));INSERT INTO @prefix SELECT DbTablePrefix, DbMetaTables FROM dbo.Applications WHERE Name = @applicationName;
DECLARE @sql NVARCHAR(MAX) = CONCAT('SELECT @tableId=Id FROM ', (SELECT DbMetaTables FROM @prefix), ' WHERE Name = @tableName');
exec sp_executesql @sql, N'@tableId Int output, @tableName NVARCHAR(50)', @tableId output, @tableName = @tableName;

SET @sql = CONCAT('SELECT ', @columnNames, ' FROM ', (SELECT DbTablePrefix FROM @prefix), @tableId, ' ', @additional, ';');
exec (@sql);


-- DROP COLUMN
DECLARE @applicationName NVARCHAR(50) = 'test';
DECLARE @tableName NVARCHAR(50) = 'uuu';
DECLARE @columnName NVARCHAR(50) = 'aa';

DECLARE @tableId INT;
DECLARE @prefix TABLE(DbTablePrefix NVARCHAR(50), DbMetaTables NVARCHAR(50));INSERT INTO @prefix SELECT DbTablePrefix, DbMetaTables FROM dbo.Applications WHERE Name = @applicationName;
DECLARE @sql NVARCHAR(MAX) = CONCAT('SELECT @tableId=Id FROM ', (SELECT DbMetaTables FROM @prefix), ' WHERE Name = @tableName');
exec sp_executesql @sql, N'@tableId Int output, @tableName NVARCHAR(50)', @tableId output, @tableName = @tableName;

SET @sql = CONCAT('ALTER TABLE ', (SELECT DbTablePrefix FROM @prefix), @tableId, ' DROP COLUMN ', @columnName);
exec (@sql);

