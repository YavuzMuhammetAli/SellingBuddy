using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogService.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StorefrontCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //old demo data can't satisfy the new unique slugs, storefront seed replaces it
            migrationBuilder.Sql("DELETE FROM [catalog].[Catalog]; DELETE FROM [catalog].[CatalogType]; DELETE FROM [catalog].[CatalogBrand];");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "catalog",
                table: "Catalog");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "catalog",
                table: "Catalog");

            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "catalog",
                table: "CatalogType",
                newName: "Slug");

            migrationBuilder.CreateSequence(
                name: "catalog_campaign_hilo",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "catalog_offer_hilo",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "catalog_review_hilo",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "catalog_seller_hilo",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "catalog_spec_hilo",
                incrementBy: 10);

            migrationBuilder.AddColumn<string>(
                name: "Description_En",
                schema: "catalog",
                table: "CatalogType",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description_Tr",
                schema: "catalog",
                table: "CatalogType",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                schema: "catalog",
                table: "CatalogType",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                schema: "catalog",
                table: "CatalogType",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name_En",
                schema: "catalog",
                table: "CatalogType",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name_Tr",
                schema: "catalog",
                table: "CatalogType",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentCatalogTypeId",
                schema: "catalog",
                table: "CatalogType",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Badge",
                schema: "catalog",
                table: "Catalog",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "catalog",
                table: "Catalog",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description_En",
                schema: "catalog",
                table: "Catalog",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description_Tr",
                schema: "catalog",
                table: "Catalog",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name_En",
                schema: "catalog",
                table: "Catalog",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name_Tr",
                schema: "catalog",
                table: "Catalog",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OldPrice",
                schema: "catalog",
                table: "Catalog",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                schema: "catalog",
                table: "Catalog",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                schema: "catalog",
                table: "Catalog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                schema: "catalog",
                table: "Catalog",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CatalogCampaign",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaxDiscountPercent = table.Column<int>(type: "int", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HeroFileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    BadgeText_En = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BadgeText_Tr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description_En = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description_Tr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Title_En = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Title_Tr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogCampaign", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogItemSpec",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CatalogItemId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Label_En = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Label_Tr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Value_En = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Value_Tr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogItemSpec", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogItemSpec_Catalog_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalSchema: "catalog",
                        principalTable: "Catalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogReview",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CatalogItemId = table.Column<int>(type: "int", nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment_En = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Comment_Tr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogReview_Catalog_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalSchema: "catalog",
                        principalTable: "Catalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogSeller",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Initials = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Rating = table.Column<double>(type: "float", nullable: false),
                    ReviewCount = table.Column<int>(type: "int", nullable: false),
                    SalesCount = table.Column<int>(type: "int", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    OnTimeShippingRate = table.Column<int>(type: "int", nullable: false),
                    BannerFileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Tagline_En = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Tagline_Tr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogSeller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogCampaignItem",
                schema: "catalog",
                columns: table => new
                {
                    CampaignsId = table.Column<int>(type: "int", nullable: false),
                    ItemsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogCampaignItem", x => new { x.CampaignsId, x.ItemsId });
                    table.ForeignKey(
                        name: "FK_CatalogCampaignItem_CatalogCampaign_CampaignsId",
                        column: x => x.CampaignsId,
                        principalSchema: "catalog",
                        principalTable: "CatalogCampaign",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogCampaignItem_Catalog_ItemsId",
                        column: x => x.ItemsId,
                        principalSchema: "catalog",
                        principalTable: "Catalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogOffer",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CatalogItemId = table.Column<int>(type: "int", nullable: false),
                    CatalogSellerId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OldPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    ShippingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DeliveryDaysMin = table.Column<int>(type: "int", nullable: false),
                    DeliveryDaysMax = table.Column<int>(type: "int", nullable: false),
                    Condition_En = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Condition_Tr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogOffer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogOffer_CatalogSeller_CatalogSellerId",
                        column: x => x.CatalogSellerId,
                        principalSchema: "catalog",
                        principalTable: "CatalogSeller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogOffer_Catalog_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalSchema: "catalog",
                        principalTable: "Catalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogType_ParentCatalogTypeId",
                schema: "catalog",
                table: "CatalogType",
                column: "ParentCatalogTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogType_Slug",
                schema: "catalog",
                table: "CatalogType",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_Slug",
                schema: "catalog",
                table: "Catalog",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogCampaign_Slug",
                schema: "catalog",
                table: "CatalogCampaign",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogCampaignItem_ItemsId",
                schema: "catalog",
                table: "CatalogCampaignItem",
                column: "ItemsId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItemSpec_CatalogItemId",
                schema: "catalog",
                table: "CatalogItemSpec",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogOffer_CatalogItemId_CatalogSellerId",
                schema: "catalog",
                table: "CatalogOffer",
                columns: new[] { "CatalogItemId", "CatalogSellerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogOffer_CatalogSellerId",
                schema: "catalog",
                table: "CatalogOffer",
                column: "CatalogSellerId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogReview_CatalogItemId",
                schema: "catalog",
                table: "CatalogReview",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogSeller_Slug",
                schema: "catalog",
                table: "CatalogSeller",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogType_CatalogType_ParentCatalogTypeId",
                schema: "catalog",
                table: "CatalogType",
                column: "ParentCatalogTypeId",
                principalSchema: "catalog",
                principalTable: "CatalogType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogType_CatalogType_ParentCatalogTypeId",
                schema: "catalog",
                table: "CatalogType");

            migrationBuilder.DropTable(
                name: "CatalogCampaignItem",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "CatalogItemSpec",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "CatalogOffer",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "CatalogReview",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "CatalogCampaign",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "CatalogSeller",
                schema: "catalog");

            migrationBuilder.DropIndex(
                name: "IX_CatalogType_ParentCatalogTypeId",
                schema: "catalog",
                table: "CatalogType");

            migrationBuilder.DropIndex(
                name: "IX_CatalogType_Slug",
                schema: "catalog",
                table: "CatalogType");

            migrationBuilder.DropIndex(
                name: "IX_Catalog_Slug",
                schema: "catalog",
                table: "Catalog");

            migrationBuilder.DropColumn(
                name: "Description_En",
                schema: "catalog",
                table: "CatalogType");

            migrationBuilder.DropColumn(
                name: "Description_Tr",
                schema: "catalog",
                table: "CatalogType");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                schema: "catalog",
                table: "CatalogType");

            migrationBuilder.DropColumn(
                name: "ImageFileName",
                schema: "catalog",
                table: "CatalogType");

            migrationBuilder.DropColumn(
                name: "Name_En",
                schema: "catalog",
                table: "CatalogType");

            migrationBuilder.DropColumn(
                name: "Name_Tr",
                schema: "catalog",
                table: "CatalogType");

            migrationBuilder.DropColumn(
                name: "ParentCatalogTypeId",
                schema: "catalog",
                table: "CatalogType");

            migrationBuilder.DropColumn(
                name: "Badge",
                schema: "catalog",
                table: "Catalog");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "catalog",
                table: "Catalog");

            migrationBuilder.DropColumn(
                name: "Description_En",
                schema: "catalog",
                table: "Catalog");

            migrationBuilder.DropColumn(
                name: "Description_Tr",
                schema: "catalog",
                table: "Catalog");

            migrationBuilder.DropColumn(
                name: "Name_En",
                schema: "catalog",
                table: "Catalog");

            migrationBuilder.DropColumn(
                name: "Name_Tr",
                schema: "catalog",
                table: "Catalog");

            migrationBuilder.DropColumn(
                name: "OldPrice",
                schema: "catalog",
                table: "Catalog");

            migrationBuilder.DropColumn(
                name: "Rating",
                schema: "catalog",
                table: "Catalog");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                schema: "catalog",
                table: "Catalog");

            migrationBuilder.DropColumn(
                name: "Slug",
                schema: "catalog",
                table: "Catalog");

            migrationBuilder.DropSequence(
                name: "catalog_campaign_hilo");

            migrationBuilder.DropSequence(
                name: "catalog_offer_hilo");

            migrationBuilder.DropSequence(
                name: "catalog_review_hilo");

            migrationBuilder.DropSequence(
                name: "catalog_seller_hilo");

            migrationBuilder.DropSequence(
                name: "catalog_spec_hilo");

            migrationBuilder.RenameColumn(
                name: "Slug",
                schema: "catalog",
                table: "CatalogType",
                newName: "Type");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "catalog",
                table: "Catalog",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "catalog",
                table: "Catalog",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
