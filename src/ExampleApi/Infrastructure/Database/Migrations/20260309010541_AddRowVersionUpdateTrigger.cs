using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExampleApi.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionUpdateTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create trigger to automatically update RowVersion on UPDATE
            migrationBuilder.Sql(@"
                CREATE TRIGGER UpdateArticleRowVersion
                AFTER UPDATE ON Articles
                FOR EACH ROW
                BEGIN
                    UPDATE Articles 
                    SET RowVersion = randomblob(8) 
                    WHERE rowid = NEW.rowid AND RowVersion = OLD.RowVersion;
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the trigger when rolling back
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS UpdateArticleRowVersion;");
        }
    }
}
