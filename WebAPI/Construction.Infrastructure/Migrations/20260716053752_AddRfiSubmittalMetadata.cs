using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Construction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRfiSubmittalMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discipline",
                table: "Submittals",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecificationSection",
                table: "Submittals",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittalType",
                table: "Submittals",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discipline",
                table: "RFIs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Impact",
                table: "RFIs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discipline",
                table: "Submittals");

            migrationBuilder.DropColumn(
                name: "SpecificationSection",
                table: "Submittals");

            migrationBuilder.DropColumn(
                name: "SubmittalType",
                table: "Submittals");

            migrationBuilder.DropColumn(
                name: "Discipline",
                table: "RFIs");

            migrationBuilder.DropColumn(
                name: "Impact",
                table: "RFIs");
        }
    }
}
