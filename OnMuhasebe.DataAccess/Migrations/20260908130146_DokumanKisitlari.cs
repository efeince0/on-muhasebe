using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnMuhasebe.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DokumanKisitlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_StokHareketleri_Miktar",
                table: "StokHareketleri",
                sql: "[HareketTipi] = 'Sayim' OR [Miktar] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FaturaSatirlari_Miktar",
                table: "FaturaSatirlari",
                sql: "[Miktar] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Cariler_VergiNo",
                table: "Cariler",
                column: "VergiNo",
                unique: true,
                filter: "[VergiNo] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CariIslemler_Tutar",
                table: "CariIslemler",
                sql: "[Tutar] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StokHareketleri_Miktar",
                table: "StokHareketleri");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FaturaSatirlari_Miktar",
                table: "FaturaSatirlari");

            migrationBuilder.DropIndex(
                name: "IX_Cariler_VergiNo",
                table: "Cariler");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CariIslemler_Tutar",
                table: "CariIslemler");
        }
    }
}
