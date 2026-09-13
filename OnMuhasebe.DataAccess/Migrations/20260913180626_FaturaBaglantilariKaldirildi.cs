using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnMuhasebe.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FaturaBaglantilariKaldirildi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CariIslemler_Faturalar_FaturaId",
                table: "CariIslemler");

            migrationBuilder.DropForeignKey(
                name: "FK_StokHareketleri_Faturalar_FaturaId",
                table: "StokHareketleri");

            migrationBuilder.DropIndex(
                name: "IX_StokHareketleri_FaturaId",
                table: "StokHareketleri");

            migrationBuilder.DropIndex(
                name: "IX_StokHareketleri_StokId",
                table: "StokHareketleri");

            migrationBuilder.DropIndex(
                name: "IX_CariIslemler_CariId",
                table: "CariIslemler");

            migrationBuilder.DropIndex(
                name: "IX_CariIslemler_FaturaId",
                table: "CariIslemler");

            migrationBuilder.DropColumn(
                name: "FaturaId",
                table: "StokHareketleri");

            migrationBuilder.DropColumn(
                name: "FaturaId",
                table: "CariIslemler");

            migrationBuilder.CreateIndex(
                name: "IX_StokHareketleri_StokId_Tarih",
                table: "StokHareketleri",
                columns: new[] { "StokId", "Tarih" });

            migrationBuilder.CreateIndex(
                name: "IX_CariIslemler_CariId_Tarih",
                table: "CariIslemler",
                columns: new[] { "CariId", "Tarih" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StokHareketleri_StokId_Tarih",
                table: "StokHareketleri");

            migrationBuilder.DropIndex(
                name: "IX_CariIslemler_CariId_Tarih",
                table: "CariIslemler");

            migrationBuilder.AddColumn<int>(
                name: "FaturaId",
                table: "StokHareketleri",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaturaId",
                table: "CariIslemler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StokHareketleri_FaturaId",
                table: "StokHareketleri",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_StokHareketleri_StokId",
                table: "StokHareketleri",
                column: "StokId");

            migrationBuilder.CreateIndex(
                name: "IX_CariIslemler_CariId",
                table: "CariIslemler",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_CariIslemler_FaturaId",
                table: "CariIslemler",
                column: "FaturaId");

            migrationBuilder.AddForeignKey(
                name: "FK_CariIslemler_Faturalar_FaturaId",
                table: "CariIslemler",
                column: "FaturaId",
                principalTable: "Faturalar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StokHareketleri_Faturalar_FaturaId",
                table: "StokHareketleri",
                column: "FaturaId",
                principalTable: "Faturalar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
