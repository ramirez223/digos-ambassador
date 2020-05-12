﻿// <auto-generated />

#pragma warning disable CS1591
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantUsingDirective

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DIGOS.Ambassador.Plugins.Autorole.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AddServerToAutoroleConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "MessageCount",
                schema: "AutoroleModule",
                table: "UserChannelStatistics",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "ServerID",
                schema: "AutoroleModule",
                table: "AutoroleConfigurations",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_AutoroleConfigurations_ServerID",
                schema: "AutoroleModule",
                table: "AutoroleConfigurations",
                column: "ServerID");

            migrationBuilder.AddForeignKey(
                name: "FK_AutoroleConfigurations_Servers_ServerID",
                schema: "AutoroleModule",
                table: "AutoroleConfigurations",
                column: "ServerID",
                principalSchema: "Core",
                principalTable: "Servers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AutoroleConfigurations_Servers_ServerID",
                schema: "AutoroleModule",
                table: "AutoroleConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_AutoroleConfigurations_ServerID",
                schema: "AutoroleModule",
                table: "AutoroleConfigurations");

            migrationBuilder.DropColumn(
                name: "ServerID",
                schema: "AutoroleModule",
                table: "AutoroleConfigurations");

            migrationBuilder.AlterColumn<long>(
                name: "MessageCount",
                schema: "AutoroleModule",
                table: "UserChannelStatistics",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);
        }
    }
}

