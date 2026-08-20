using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Construction.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Adds a NOT NULL <c>Title</c> column (max 300 chars) to the <c>Submittals</c>
    /// table, plus an index on it. Existing rows are seeded with an empty string —
    /// the <c>DatabaseSeeder.BackfillRfiSubmittalMetadataAsync</c> pass then fills
    /// any blank <c>Title</c> with a valid construction-submittal noun phrase on the
    /// next seed run, so the UI's Submittals "Title" column is never blank.
    /// </summary>
    public partial class AddSubmittalTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // DEFAULT '' keeps the ALTER NOT NULL safe for existing rows; the seeder
            // backfills real titles afterwards.
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Submittals",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.CreateIndex(
                name: "IX_Submittals_Title",
                table: "Submittals",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Submittals_Title",
                table: "Submittals");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Submittals");
        }
    }
}
