using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_school_system.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$N34uJz1O0EP1HrwUHzEJBeTg0d6ylLA5s7x0KWr3folKP4cMRJZve");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$vnkUERwLBehtnM71ETWF2.5tOO4ffXck1kFCur2D9Q0PNU8Dn7D2q");
        }
    }
}
