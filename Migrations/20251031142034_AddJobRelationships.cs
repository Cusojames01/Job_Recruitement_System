using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Job_Recruitment_System.Migrations
{
    /// <inheritdoc />
    public partial class AddJobRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosts_Recruiters_RecruiterId",
                table: "JobPosts");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosts_Recruiters_RecruiterId",
                table: "JobPosts",
                column: "RecruiterId",
                principalTable: "Recruiters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosts_Recruiters_RecruiterId",
                table: "JobPosts");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosts_Recruiters_RecruiterId",
                table: "JobPosts",
                column: "RecruiterId",
                principalTable: "Recruiters",
                principalColumn: "Id");
        }
    }
}
