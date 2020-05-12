﻿// <auto-generated />

#pragma warning disable CS1591
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantUsingDirective

// <auto-generated />
using System.Diagnostics.CodeAnalysis;
using System;
using DIGOS.Ambassador.Plugins.Autorole.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DIGOS.Ambassador.Plugins.Autorole.Migrations
{
    [DbContext(typeof(AutoroleDatabaseContext))]
    [ExcludeFromCodeCoverage]
    partial class AutoroleDatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("AutoroleModule")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.AutoroleConfiguration", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("DiscordRoleID")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.Property<bool>("RequiresConfirmation")
                        .HasColumnType("boolean");

                    b.Property<long>("ServerID")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.HasIndex("ServerID");

                    b.ToTable("AutoroleConfigurations","AutoroleModule");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.AutoroleCondition", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long?>("AutoroleConfigurationID")
                        .HasColumnType("bigint");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("AutoroleConfigurationID");

                    b.ToTable("AutoroleConditions","AutoroleModule");

                    b.HasDiscriminator<string>("Discriminator").HasValue("AutoroleCondition");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Statistics.UserChannelStatistics", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("ChannelID")
                        .HasColumnType("bigint");

                    b.Property<long?>("MessageCount")
                        .HasColumnType("bigint");

                    b.Property<long?>("UserServerStatisticsID")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.HasIndex("UserServerStatisticsID");

                    b.ToTable("UserChannelStatistics","AutoroleModule");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Statistics.UserServerStatistics", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("ServerID")
                        .HasColumnType("bigint");

                    b.Property<long?>("TotalMessageCount")
                        .HasColumnType("bigint");

                    b.Property<long?>("UserStatisticsID")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.HasIndex("ServerID");

                    b.HasIndex("UserStatisticsID");

                    b.ToTable("UserServerStatistics","AutoroleModule");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Statistics.UserStatistics", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("UserID")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.HasIndex("UserID");

                    b.ToTable("UserStatistics","AutoroleModule");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Core.Model.Servers.Server", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<long>("DiscordID")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsNSFW")
                        .HasColumnType("boolean");

                    b.Property<string>("JoinMessage")
                        .HasColumnType("text");

                    b.Property<bool>("SendJoinMessage")
                        .HasColumnType("boolean");

                    b.Property<bool>("SuppressPermissionWarnings")
                        .HasColumnType("boolean");

                    b.HasKey("ID");

                    b.ToTable("Servers","Core");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Core.Model.Users.ServerUser", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("ServerID")
                        .HasColumnType("bigint");

                    b.Property<long>("UserID")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.HasIndex("ServerID");

                    b.HasIndex("UserID");

                    b.ToTable("ServerUser","Core");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Core.Model.Users.User", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Bio")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("DiscordID")
                        .HasColumnType("bigint");

                    b.Property<int?>("Timezone")
                        .HasColumnType("integer");

                    b.HasKey("ID");

                    b.ToTable("Users","Core");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.MessageCountInSourceCondition<DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.MessageCountInChannelCondition>", b =>
                {
                    b.HasBaseType("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.AutoroleCondition");

                    b.Property<long>("RequiredCount")
                        .HasColumnName("RequiredCount")
                        .HasColumnType("bigint");

                    b.Property<long>("SourceID")
                        .HasColumnName("SourceID")
                        .HasColumnType("bigint");

                    b.ToTable("AutoroleConditions","AutoroleModule");

                    b.HasDiscriminator().HasValue("MessageCountInSourceCondition<MessageCountInChannelCondition>");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.MessageCountInSourceCondition<DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.MessageCountInGuildCondition>", b =>
                {
                    b.HasBaseType("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.AutoroleCondition");

                    b.Property<long>("RequiredCount")
                        .HasColumnName("RequiredCount")
                        .HasColumnType("bigint");

                    b.Property<long>("SourceID")
                        .HasColumnName("SourceID")
                        .HasColumnType("bigint");

                    b.ToTable("AutoroleConditions","AutoroleModule");

                    b.HasDiscriminator().HasValue("MessageCountInSourceCondition<MessageCountInGuildCondition>");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.TimeSinceEventCondition<DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.TimeSinceJoinCondition>", b =>
                {
                    b.HasBaseType("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.AutoroleCondition");

                    b.Property<TimeSpan>("RequiredTime")
                        .HasColumnName("RequiredTime")
                        .HasColumnType("interval");

                    b.ToTable("AutoroleConditions","AutoroleModule");

                    b.HasDiscriminator().HasValue("TimeSinceEventCondition<TimeSinceJoinCondition>");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.TimeSinceEventCondition<DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.TimeSinceLastActivityCondition>", b =>
                {
                    b.HasBaseType("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.AutoroleCondition");

                    b.Property<TimeSpan>("RequiredTime")
                        .HasColumnName("RequiredTime")
                        .HasColumnType("interval");

                    b.ToTable("AutoroleConditions","AutoroleModule");

                    b.HasDiscriminator().HasValue("TimeSinceEventCondition<TimeSinceLastActivityCondition>");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.ReactionCondition", b =>
                {
                    b.HasBaseType("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.AutoroleCondition");

                    b.Property<long>("ChannelID")
                        .HasColumnType("bigint");

                    b.Property<string>("EmoteName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("MessageID")
                        .HasColumnType("bigint");

                    b.ToTable("AutoroleConditions","AutoroleModule");

                    b.HasDiscriminator().HasValue("ReactionCondition");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.RoleCondition", b =>
                {
                    b.HasBaseType("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.AutoroleCondition");

                    b.Property<long>("RoleID")
                        .HasColumnType("bigint");

                    b.ToTable("AutoroleConditions","AutoroleModule");

                    b.HasDiscriminator().HasValue("RoleCondition");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.MessageCountInChannelCondition", b =>
                {
                    b.HasBaseType("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.MessageCountInSourceCondition<DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.MessageCountInChannelCondition>");

                    b.ToTable("AutoroleConditions","AutoroleModule");

                    b.HasDiscriminator().HasValue("MessageCountInChannelCondition");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.MessageCountInGuildCondition", b =>
                {
                    b.HasBaseType("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.MessageCountInSourceCondition<DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.MessageCountInGuildCondition>");

                    b.ToTable("AutoroleConditions","AutoroleModule");

                    b.HasDiscriminator().HasValue("MessageCountInGuildCondition");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.TimeSinceJoinCondition", b =>
                {
                    b.HasBaseType("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.TimeSinceEventCondition<DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.TimeSinceJoinCondition>");

                    b.ToTable("AutoroleConditions","AutoroleModule");

                    b.HasDiscriminator().HasValue("TimeSinceJoinCondition");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.TimeSinceLastActivityCondition", b =>
                {
                    b.HasBaseType("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.TimeSinceEventCondition<DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.TimeSinceLastActivityCondition>");

                    b.ToTable("AutoroleConditions","AutoroleModule");

                    b.HasDiscriminator().HasValue("TimeSinceLastActivityCondition");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.AutoroleConfiguration", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Plugins.Core.Model.Servers.Server", "Server")
                        .WithMany()
                        .HasForeignKey("ServerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases.AutoroleCondition", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Plugins.Autorole.Model.AutoroleConfiguration", null)
                        .WithMany("Conditions")
                        .HasForeignKey("AutoroleConfigurationID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Statistics.UserChannelStatistics", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Plugins.Autorole.Model.Statistics.UserServerStatistics", null)
                        .WithMany("ChannelStatistics")
                        .HasForeignKey("UserServerStatisticsID");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Statistics.UserServerStatistics", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Plugins.Core.Model.Servers.Server", "Server")
                        .WithMany()
                        .HasForeignKey("ServerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DIGOS.Ambassador.Plugins.Autorole.Model.Statistics.UserStatistics", null)
                        .WithMany("ServerStatistics")
                        .HasForeignKey("UserStatisticsID");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Autorole.Model.Statistics.UserStatistics", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Plugins.Core.Model.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Core.Model.Users.ServerUser", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Plugins.Core.Model.Servers.Server", "Server")
                        .WithMany("KnownUsers")
                        .HasForeignKey("ServerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DIGOS.Ambassador.Plugins.Core.Model.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}

