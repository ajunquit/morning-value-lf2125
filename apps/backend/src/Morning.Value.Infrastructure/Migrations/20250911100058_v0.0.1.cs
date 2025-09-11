using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Morning.Value.Infrastructure.Migrations
{
    public partial class v001 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Quitar FKs hacia Loans.UserId/BookId (si existen)
            migrationBuilder.Sql(@"
DECLARE @fk NVARCHAR(128);

-- UserId
SELECT @fk = fk.name
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id=fkc.constraint_object_id
JOIN sys.columns c ON c.object_id=fkc.parent_object_id AND c.column_id=fkc.parent_column_id
WHERE fk.parent_object_id=OBJECT_ID(N'dbo.Loans') AND c.name='UserId';
IF @fk IS NOT NULL EXEC('ALTER TABLE [dbo].[Loans] DROP CONSTRAINT ['+@fk+']');

-- BookId
SELECT @fk = fk.name
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id=fkc.constraint_object_id
JOIN sys.columns c ON c.object_id=fkc.parent_object_id AND c.column_id=fkc.parent_column_id
WHERE fk.parent_object_id=OBJECT_ID(N'dbo.Loans') AND c.name='BookId';
IF @fk IS NOT NULL EXEC('ALTER TABLE [dbo].[Loans] DROP CONSTRAINT ['+@fk+']');
");

            // 2) Quitar defaults (si los hubiera)
            migrationBuilder.Sql(@"
DECLARE @df NVARCHAR(128);

SELECT @df = d.name
FROM sys.default_constraints d
JOIN sys.columns c ON d.parent_object_id=c.object_id AND d.parent_column_id=c.column_id
WHERE d.parent_object_id=OBJECT_ID(N'dbo.Loans') AND c.name='UserId';
IF @df IS NOT NULL EXEC('ALTER TABLE [dbo].[Loans] DROP CONSTRAINT ['+@df+']');

SELECT @df = d.name
FROM sys.default_constraints d
JOIN sys.columns c ON d.parent_object_id=c.object_id AND d.parent_column_id=c.column_id
WHERE d.parent_object_id=OBJECT_ID(N'dbo.Loans') AND c.name='BookId';
IF @df IS NOT NULL EXEC('ALTER TABLE [dbo].[Loans] DROP CONSTRAINT ['+@df+']');
");

            // 3) Quitar índices no PK sobre esas columnas (si existen)
            migrationBuilder.Sql(@"
DECLARE @ix NVARCHAR(128);
DECLARE ixCur CURSOR FOR
SELECT i.name
FROM sys.indexes i
JOIN sys.index_columns ic ON i.object_id=ic.object_id AND i.index_id=ic.index_id
JOIN sys.columns c ON ic.object_id=c.object_id AND ic.column_id=c.column_id
WHERE i.object_id=OBJECT_ID(N'dbo.Loans')
  AND c.name IN ('UserId','BookId')
  AND i.is_primary_key=0 AND i.is_unique_constraint=0;
OPEN ixCur; FETCH NEXT FROM ixCur INTO @ix;
WHILE @@FETCH_STATUS=0
BEGIN
    EXEC('DROP INDEX ['+@ix+'] ON [dbo].[Loans]');
    FETCH NEXT FROM ixCur INTO @ix;
END
CLOSE ixCur; DEALLOCATE ixCur;
");

            // 4) Borrar columnas INT y volverlas a crear como GUID
            migrationBuilder.DropColumn(name: "UserId", table: "Loans");
            migrationBuilder.DropColumn(name: "BookId", table: "Loans");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Loans",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "BookId",
                table: "Loans",
                type: "uniqueidentifier",
                nullable: false);

            // 5) Re-crear índices + FKs (Books y Users/AspNetUsers si existen)
            migrationBuilder.Sql(@"
CREATE INDEX [IX_Loans_UserId] ON [dbo].[Loans]([UserId]);
CREATE INDEX [IX_Loans_BookId] ON [dbo].[Loans]([BookId]);

-- FK a Books(Id)
IF OBJECT_ID(N'dbo.Books') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Loans] WITH CHECK
    ADD CONSTRAINT [FK_Loans_Books_BookId] FOREIGN KEY([BookId]) REFERENCES [dbo].[Books]([Id]) ON DELETE CASCADE;
END

-- FK a Users/AspNetUsers(Id)
DECLARE @UsersTable sysname;
IF OBJECT_ID(N'dbo.Users') IS NOT NULL SET @UsersTable=N'dbo.Users';
ELSE IF OBJECT_ID(N'dbo.AspNetUsers') IS NOT NULL SET @UsersTable=N'dbo.AspNetUsers';

IF @UsersTable IS NOT NULL
BEGIN
    DECLARE @sql NVARCHAR(MAX) = N'
    ALTER TABLE [dbo].[Loans] WITH CHECK
    ADD CONSTRAINT [FK_Loans_Users_UserId] FOREIGN KEY([UserId]) REFERENCES ' + @UsersTable + N'([Id]) ON DELETE CASCADE;';
    EXEC(@sql);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir a INT (por si acaso)
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.FK_Loans_Books_BookId') IS NOT NULL
    ALTER TABLE [dbo].[Loans] DROP CONSTRAINT [FK_Loans_Books_BookId];
IF OBJECT_ID(N'dbo.FK_Loans_Users_UserId') IS NOT NULL
    ALTER TABLE [dbo].[Loans] DROP CONSTRAINT [FK_Loans_Users_UserId];

IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Loans_BookId' AND object_id=OBJECT_ID(N'dbo.Loans'))
    DROP INDEX [IX_Loans_BookId] ON [dbo].[Loans];
IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Loans_UserId' AND object_id=OBJECT_ID(N'dbo.Loans'))
    DROP INDEX [IX_Loans_UserId] ON [dbo].[Loans];
");

            migrationBuilder.DropColumn(name: "UserId", table: "Loans");
            migrationBuilder.DropColumn(name: "BookId", table: "Loans");

            migrationBuilder.AddColumn<int>(name: "UserId", table: "Loans", type: "int", nullable: false);
            migrationBuilder.AddColumn<int>(name: "BookId", table: "Loans", type: "int", nullable: false);
        }
    }
}
