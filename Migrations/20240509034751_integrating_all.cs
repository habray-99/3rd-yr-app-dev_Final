using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication6.Migrations
{
    /// <inheritdoc />
    public partial class integrating_all : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogMetrics_AspNetUsers_CustomUserId",
                table: "BlogMetrics");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogMetrics_Blogs_BlogID",
                table: "BlogMetrics");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMetrics_AspNetUsers_UserID",
                table: "UserMetrics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserMetrics",
                table: "UserMetrics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogMetrics",
                table: "BlogMetrics");

            migrationBuilder.DropIndex(
                name: "IX_BlogMetrics_CustomUserId",
                table: "BlogMetrics");

            migrationBuilder.DropColumn(
                name: "CustomUserId",
                table: "BlogMetrics");

            migrationBuilder.RenameTable(
                name: "UserMetrics",
                newName: "UserMetric");

            migrationBuilder.RenameTable(
                name: "BlogMetrics",
                newName: "BlogMetric");

            migrationBuilder.RenameIndex(
                name: "IX_UserMetrics_UserID",
                table: "UserMetric",
                newName: "IX_UserMetric_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_BlogMetrics_BlogID",
                table: "BlogMetric",
                newName: "IX_BlogMetric_BlogID");

            migrationBuilder.AlterColumn<string>(
                name: "ProfilePicture",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldMaxLength: 3145728,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserMetric",
                table: "UserMetric",
                column: "UserMetricID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogMetric",
                table: "BlogMetric",
                column: "BlogMetricID");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogMetric_Blogs_BlogID",
                table: "BlogMetric",
                column: "BlogID",
                principalTable: "Blogs",
                principalColumn: "BlogID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMetric_CustomUser_UserID",
                table: "UserMetric",
                column: "UserID",
                principalTable: "CustomUser",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogMetric_Blogs_BlogID",
                table: "BlogMetric");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMetric_CustomUser_UserID",
                table: "UserMetric");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserMetric",
                table: "UserMetric");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogMetric",
                table: "BlogMetric");

            migrationBuilder.RenameTable(
                name: "UserMetric",
                newName: "UserMetrics");

            migrationBuilder.RenameTable(
                name: "BlogMetric",
                newName: "BlogMetrics");

            migrationBuilder.RenameIndex(
                name: "IX_UserMetric_UserID",
                table: "UserMetrics",
                newName: "IX_UserMetrics_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_BlogMetric_BlogID",
                table: "BlogMetrics",
                newName: "IX_BlogMetrics_BlogID");

            migrationBuilder.AlterColumn<byte[]>(
                name: "ProfilePicture",
                table: "AspNetUsers",
                type: "varbinary(max)",
                maxLength: 3145728,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomUserId",
                table: "BlogMetrics",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserMetrics",
                table: "UserMetrics",
                column: "UserMetricID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogMetrics",
                table: "BlogMetrics",
                column: "BlogMetricID");

            migrationBuilder.CreateIndex(
                name: "IX_BlogMetrics_CustomUserId",
                table: "BlogMetrics",
                column: "CustomUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogMetrics_AspNetUsers_CustomUserId",
                table: "BlogMetrics",
                column: "CustomUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogMetrics_Blogs_BlogID",
                table: "BlogMetrics",
                column: "BlogID",
                principalTable: "Blogs",
                principalColumn: "BlogID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMetrics_AspNetUsers_UserID",
                table: "UserMetrics",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
