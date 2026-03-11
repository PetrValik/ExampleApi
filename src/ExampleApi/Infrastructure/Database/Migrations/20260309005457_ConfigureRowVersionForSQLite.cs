using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExampleApi.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureRowVersionForSQLite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Articles",
                type: "BLOB",
                rowVersion: true,
                nullable: true,
                defaultValueSql: "randomblob(8)",
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldRowVersion: true,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Articles",
                type: "BLOB",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldRowVersion: true,
                oldNullable: true,
                oldDefaultValueSql: "randomblob(8)");
        }
    }
}
