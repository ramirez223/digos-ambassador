﻿// <auto-generated />
#pragma warning disable CS1591
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantUsingDirective
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DIGOS.Ambassador.Migrations
{
    public partial class RemoveCoreAndPermissionsFromPrimaryContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Same as before, just updating the model
            /*
            migrationBuilder.DropTable(
                name: "UserConsents",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "GlobalPermissions",
                schema: "PermissionModule");

            migrationBuilder.DropTable(
                name: "LocalPermissions",
                schema: "PermissionModule");
            */
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Same as before, just updating the model
            /*
            migrationBuilder.EnsureSchema(
                name: "PermissionModule");

            migrationBuilder.CreateTable(
                name: "UserConsents",
                schema: "Core",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    DiscordID = table.Column<long>(nullable: false),
                    HasConsented = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConsents", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GlobalPermissions",
                schema: "PermissionModule",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Permission = table.Column<int>(nullable: false),
                    Target = table.Column<int>(nullable: false),
                    UserDiscordID = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalPermissions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "LocalPermissions",
                schema: "PermissionModule",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Permission = table.Column<int>(nullable: false),
                    ServerDiscordID = table.Column<long>(nullable: false),
                    Target = table.Column<int>(nullable: false),
                    UserDiscordID = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalPermissions", x => x.ID);
                });
            */
        }
    }
}
