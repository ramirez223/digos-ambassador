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
    public partial class AddReactionCondition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChannelID",
                schema: "AutoroleModule",
                table: "AutoroleConditions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmoteName",
                schema: "AutoroleModule",
                table: "AutoroleConditions",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MessageID",
                schema: "AutoroleModule",
                table: "AutoroleConditions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelID",
                schema: "AutoroleModule",
                table: "AutoroleConditions");

            migrationBuilder.DropColumn(
                name: "EmoteName",
                schema: "AutoroleModule",
                table: "AutoroleConditions");

            migrationBuilder.DropColumn(
                name: "MessageID",
                schema: "AutoroleModule",
                table: "AutoroleConditions");
        }
    }
}
