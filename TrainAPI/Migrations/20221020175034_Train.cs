using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainAPI.Migrations {
  public partial class Train : Migration {
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.CreateTable(
          name: "Train",
          columns: table => new {
            Id = table.Column<int>(type: "int", nullable: false),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
          },
          constraints: table => {
            table.PrimaryKey("PK_Train", x => x.Id);
          });
    }

    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropTable(name: "Train");
    }
  }
}
