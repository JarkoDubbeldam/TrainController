using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainAPI.Migrations
{
    public partial class generickeyvalues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackSection_TrackBoundary_FromBoundaryId",
                table: "TrackSection");

            migrationBuilder.DropForeignKey(
                name: "FK_TrackSection_TrackBoundary_ToBoundaryId",
                table: "TrackSection");

            migrationBuilder.CreateTable(
                name: "KeyValue",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyValue", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_TrackSection_TrackBoundary_FromBoundaryId",
                table: "TrackSection",
                column: "FromBoundaryId",
                principalTable: "TrackBoundary",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackSection_TrackBoundary_ToBoundaryId",
                table: "TrackSection",
                column: "ToBoundaryId",
                principalTable: "TrackBoundary",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackSection_TrackBoundary_FromBoundaryId",
                table: "TrackSection");

            migrationBuilder.DropForeignKey(
                name: "FK_TrackSection_TrackBoundary_ToBoundaryId",
                table: "TrackSection");

            migrationBuilder.DropTable(
                name: "KeyValue");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackSection_TrackBoundary_FromBoundaryId",
                table: "TrackSection",
                column: "FromBoundaryId",
                principalTable: "TrackBoundary",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrackSection_TrackBoundary_ToBoundaryId",
                table: "TrackSection",
                column: "ToBoundaryId",
                principalTable: "TrackBoundary",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
