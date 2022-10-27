using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainAPI.Migrations
{
    public partial class TrackSections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackBoundary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackBoundary", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackSection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForeignId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    FromBoundaryId = table.Column<int>(type: "int", nullable: false),
                    ToBoundaryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackSection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackSection_TrackBoundary_FromBoundaryId",
                        column: x => x.FromBoundaryId,
                        principalTable: "TrackBoundary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TrackSection_TrackBoundary_ToBoundaryId",
                        column: x => x.ToBoundaryId,
                        principalTable: "TrackBoundary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "TurnoutConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackSectionId = table.Column<int>(type: "int", nullable: false),
                    TurnoutId = table.Column<int>(type: "int", nullable: false),
                    TurnoutPosition = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurnoutConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TurnoutConfiguration_TrackSection_TrackSectionId",
                        column: x => x.TrackSectionId,
                        principalTable: "TrackSection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TurnoutConfiguration_Turnout_TurnoutId",
                        column: x => x.TurnoutId,
                        principalTable: "Turnout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackSection_FromBoundaryId",
                table: "TrackSection",
                column: "FromBoundaryId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackSection_ToBoundaryId",
                table: "TrackSection",
                column: "ToBoundaryId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnoutConfiguration_TrackSectionId",
                table: "TurnoutConfiguration",
                column: "TrackSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnoutConfiguration_TurnoutId",
                table: "TurnoutConfiguration",
                column: "TurnoutId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TurnoutConfiguration");

            migrationBuilder.DropTable(
                name: "TrackSection");

            migrationBuilder.DropTable(
                name: "TrackBoundary");
        }
    }
}
