using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogService.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReviewOriginalLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                schema: "catalog",
                table: "CatalogReview",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                schema: "catalog",
                table: "CatalogReview",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            //existing reviews were written in turkish, the english copy was a translation
            migrationBuilder.Sql("UPDATE [catalog].[CatalogReview] SET [Comment] = COALESCE([Comment_Tr], [Comment_En], ''), [LanguageCode] = CASE WHEN [Comment_Tr] IS NULL THEN 'en' ELSE 'tr' END;");

            migrationBuilder.DropColumn(
                name: "Comment_En",
                schema: "catalog",
                table: "CatalogReview");

            migrationBuilder.DropColumn(
                name: "Comment_Tr",
                schema: "catalog",
                table: "CatalogReview");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment_En",
                schema: "catalog",
                table: "CatalogReview",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comment_Tr",
                schema: "catalog",
                table: "CatalogReview",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.Sql("UPDATE [catalog].[CatalogReview] SET [Comment_En] = CASE WHEN [LanguageCode] = 'en' THEN [Comment] END, [Comment_Tr] = CASE WHEN [LanguageCode] = 'tr' THEN [Comment] END;");

            migrationBuilder.DropColumn(
                name: "Comment",
                schema: "catalog",
                table: "CatalogReview");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                schema: "catalog",
                table: "CatalogReview");
        }
    }
}
