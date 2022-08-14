using Microsoft.EntityFrameworkCore.Migrations;

namespace AlcantaraNew.Migrations
{
    public partial class HT_Update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CultureDatas_HomePageSectionData_FK_HP_DescriptionId",
                table: "CultureDatas");

            migrationBuilder.DropForeignKey(
                name: "FK_CultureDatas_HomePageSectionData_FK_HP_TitleId",
                table: "CultureDatas");

            migrationBuilder.DropForeignKey(
                name: "FK_HomePageSectionData_Catalogs_CatalogId",
                table: "HomePageSectionData");

            migrationBuilder.DropForeignKey(
                name: "FK_HomePageSectionData_HomePageSections_FK_HomePageSectionId",
                table: "HomePageSectionData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HomePageSectionData",
                table: "HomePageSectionData");

            migrationBuilder.RenameTable(
                name: "HomePageSectionData",
                newName: "HomePageSectionDatas");

            migrationBuilder.RenameIndex(
                name: "IX_HomePageSectionData_FK_HomePageSectionId",
                table: "HomePageSectionDatas",
                newName: "IX_HomePageSectionDatas_FK_HomePageSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_HomePageSectionData_CatalogId",
                table: "HomePageSectionDatas",
                newName: "IX_HomePageSectionDatas_CatalogId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HomePageSectionDatas",
                table: "HomePageSectionDatas",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CultureDatas_HomePageSectionDatas_FK_HP_DescriptionId",
                table: "CultureDatas",
                column: "FK_HP_DescriptionId",
                principalTable: "HomePageSectionDatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CultureDatas_HomePageSectionDatas_FK_HP_TitleId",
                table: "CultureDatas",
                column: "FK_HP_TitleId",
                principalTable: "HomePageSectionDatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HomePageSectionDatas_Catalogs_CatalogId",
                table: "HomePageSectionDatas",
                column: "CatalogId",
                principalTable: "Catalogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HomePageSectionDatas_HomePageSections_FK_HomePageSectionId",
                table: "HomePageSectionDatas",
                column: "FK_HomePageSectionId",
                principalTable: "HomePageSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CultureDatas_HomePageSectionDatas_FK_HP_DescriptionId",
                table: "CultureDatas");

            migrationBuilder.DropForeignKey(
                name: "FK_CultureDatas_HomePageSectionDatas_FK_HP_TitleId",
                table: "CultureDatas");

            migrationBuilder.DropForeignKey(
                name: "FK_HomePageSectionDatas_Catalogs_CatalogId",
                table: "HomePageSectionDatas");

            migrationBuilder.DropForeignKey(
                name: "FK_HomePageSectionDatas_HomePageSections_FK_HomePageSectionId",
                table: "HomePageSectionDatas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HomePageSectionDatas",
                table: "HomePageSectionDatas");

            migrationBuilder.RenameTable(
                name: "HomePageSectionDatas",
                newName: "HomePageSectionData");

            migrationBuilder.RenameIndex(
                name: "IX_HomePageSectionDatas_FK_HomePageSectionId",
                table: "HomePageSectionData",
                newName: "IX_HomePageSectionData_FK_HomePageSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_HomePageSectionDatas_CatalogId",
                table: "HomePageSectionData",
                newName: "IX_HomePageSectionData_CatalogId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HomePageSectionData",
                table: "HomePageSectionData",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CultureDatas_HomePageSectionData_FK_HP_DescriptionId",
                table: "CultureDatas",
                column: "FK_HP_DescriptionId",
                principalTable: "HomePageSectionData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CultureDatas_HomePageSectionData_FK_HP_TitleId",
                table: "CultureDatas",
                column: "FK_HP_TitleId",
                principalTable: "HomePageSectionData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HomePageSectionData_Catalogs_CatalogId",
                table: "HomePageSectionData",
                column: "CatalogId",
                principalTable: "Catalogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HomePageSectionData_HomePageSections_FK_HomePageSectionId",
                table: "HomePageSectionData",
                column: "FK_HomePageSectionId",
                principalTable: "HomePageSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
