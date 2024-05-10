using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication6.Migrations
{
    /// <inheritdoc />
    public partial class _2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ReactionTypes",
                columns: new[] { "ReactionTypeID", "ReactionName" },
                values: new object[,]
                {
                    { 1, "Upvote" },
                    { 2, "Downvote" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ReactionTypes",
                keyColumn: "ReactionTypeID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ReactionTypes",
                keyColumn: "ReactionTypeID",
                keyValue: 2);
        }
    }
}
