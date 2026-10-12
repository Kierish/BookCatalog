using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookCatalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveLoanCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActiveLoanCount",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE "Users" AS "user"
                SET "ActiveLoanCount" = (
                    SELECT COUNT(*)::integer
                    FROM "Loans" AS "loan"
                    WHERE "loan"."UserId" = "user"."Id"
                      AND "loan"."ReturnedAt" IS NULL
                );
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_ActiveLoanCount",
                table: "Users",
                sql: "\"ActiveLoanCount\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_ActiveLoanCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ActiveLoanCount",
                table: "Users");
        }
    }
}
