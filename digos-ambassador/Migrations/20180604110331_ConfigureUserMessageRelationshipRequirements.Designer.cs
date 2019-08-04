﻿// <auto-generated />
#pragma warning disable CS1591
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantUsingDirective
using DIGOS.Ambassador.Database;
using DIGOS.Ambassador.Database.Kinks;
using DIGOS.Ambassador.Database.Roleplaying;
using DIGOS.Ambassador.Database.Transformations;
using DIGOS.Ambassador.Transformations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace DIGOS.Ambassador.Migrations
{
    [DbContext(typeof(AmbyDatabaseContext))]
    [Migration("20180604110331_ConfigureUserMessageRelationshipRequirements")]
    partial class ConfigureUserMessageRelationshipRequirements
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.3-rtm-10026");

            modelBuilder.Entity("DIGOS.Ambassador.Database.Appearances.Appearance", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("GenderScale");

                    b.Property<double>("Height");

                    b.Property<double>("Muscularity");

                    b.Property<double>("Weight");

                    b.HasKey("ID");

                    b.ToTable("Appearance");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Appearances.AppearanceComponent", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("AppearanceID");

                    b.Property<long?>("BaseColourID");

                    b.Property<int>("Chirality");

                    b.Property<int?>("Pattern");

                    b.Property<long?>("PatternColourID");

                    b.Property<int>("Size");

                    b.Property<long?>("TransformationID");

                    b.HasKey("ID");

                    b.HasIndex("AppearanceID");

                    b.HasIndex("BaseColourID");

                    b.HasIndex("PatternColourID");

                    b.HasIndex("TransformationID");

                    b.ToTable("AppearanceComponent");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Appearances.Colour", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("Modifier");

                    b.Property<int>("Shade");

                    b.HasKey("ID");

                    b.ToTable("Colour");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Characters.Character", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AvatarUrl");

                    b.Property<long?>("CurrentAppearanceID");

                    b.Property<long?>("DefaultAppearanceID");

                    b.Property<string>("Description");

                    b.Property<bool>("IsCurrent");

                    b.Property<bool>("IsNSFW");

                    b.Property<string>("Name");

                    b.Property<string>("Nickname");

                    b.Property<long>("OwnerID");

                    b.Property<string>("PronounProviderFamily");

                    b.Property<long>("ServerID");

                    b.Property<string>("Summary");

                    b.HasKey("ID");

                    b.HasIndex("CurrentAppearanceID");

                    b.HasIndex("DefaultAppearanceID");

                    b.HasIndex("OwnerID");

                    b.ToTable("Characters");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Data.Image", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Caption");

                    b.Property<long?>("CharacterID");

                    b.Property<bool>("IsNSFW");

                    b.Property<string>("Name");

                    b.Property<string>("Url");

                    b.HasKey("ID");

                    b.HasIndex("CharacterID");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Dossiers.Dossier", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Path");

                    b.Property<string>("Summary");

                    b.Property<string>("Title");

                    b.HasKey("ID");

                    b.ToTable("Dossiers");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Kinks.Kink", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Category");

                    b.Property<string>("Description");

                    b.Property<uint>("FListID");

                    b.Property<string>("Name");

                    b.HasKey("ID");

                    b.ToTable("Kinks");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Permissions.GlobalPermission", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Permission");

                    b.Property<int>("Target");

                    b.Property<long>("UserDiscordID");

                    b.HasKey("ID");

                    b.ToTable("GlobalPermissions");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Permissions.LocalPermission", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Permission");

                    b.Property<long>("ServerDiscordID");

                    b.Property<int>("Target");

                    b.Property<long>("UserDiscordID");

                    b.HasKey("ID");

                    b.ToTable("LocalPermissions");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Roleplaying.Roleplay", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ActiveChannelID");

                    b.Property<bool>("IsActive");

                    b.Property<bool>("IsNSFW");

                    b.Property<bool>("IsPublic");

                    b.Property<string>("Name");

                    b.Property<long?>("OwnerID");

                    b.Property<long>("ServerID");

                    b.Property<string>("Summary");

                    b.HasKey("ID");

                    b.HasIndex("OwnerID");

                    b.ToTable("Roleplays");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Roleplaying.RoleplayParticipant", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("RoleplayID");

                    b.Property<int>("Status");

                    b.Property<long>("UserID");

                    b.HasKey("ID");

                    b.HasIndex("RoleplayID");

                    b.HasIndex("UserID");

                    b.ToTable("RoleplayParticipant");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Roleplaying.UserMessage", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("AuthorDiscordID");

                    b.Property<string>("AuthorNickname");

                    b.Property<string>("Contents");

                    b.Property<long>("DiscordMessageID");

                    b.Property<long?>("RoleplayID")
                        .IsRequired();

                    b.Property<DateTimeOffset>("Timestamp");

                    b.HasKey("ID");

                    b.HasIndex("RoleplayID");

                    b.ToTable("UserMessage");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.ServerInfo.Server", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("DiscordID");

                    b.Property<bool>("IsNSFW");

                    b.Property<bool>("SuppressPermissonWarnings");

                    b.HasKey("ID");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Transformations.GlobalUserProtection", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("DefaultOptIn");

                    b.Property<int>("DefaultType");

                    b.Property<long>("UserID");

                    b.HasKey("ID");

                    b.HasIndex("UserID");

                    b.ToTable("GlobalUserProtections");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Transformations.ServerUserProtection", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("HasOptedIn");

                    b.Property<long>("ServerID");

                    b.Property<int>("Type");

                    b.Property<long>("UserID");

                    b.HasKey("ID");

                    b.HasIndex("ServerID");

                    b.HasIndex("UserID");

                    b.ToTable("ServerUserProtections");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Transformations.Species", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<long?>("ParentID");

                    b.HasKey("ID");

                    b.HasIndex("ParentID");

                    b.ToTable("Species");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Transformations.Transformation", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("DefaultBaseColourID");

                    b.Property<int?>("DefaultPattern");

                    b.Property<long?>("DefaultPatternColourID");

                    b.Property<string>("Description")
                        .IsRequired();

                    b.Property<string>("GrowMessage")
                        .IsRequired();

                    b.Property<bool>("IsNSFW");

                    b.Property<int>("Part");

                    b.Property<string>("ShiftMessage")
                        .IsRequired();

                    b.Property<string>("SingleDescription")
                        .IsRequired();

                    b.Property<long>("SpeciesID");

                    b.Property<string>("UniformDescription");

                    b.HasKey("ID");

                    b.HasIndex("DefaultBaseColourID");

                    b.HasIndex("DefaultPatternColourID");

                    b.HasIndex("SpeciesID");

                    b.ToTable("Transformations");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Transformations.UserProtectionEntry", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("GlobalProtectionID");

                    b.Property<int>("Type");

                    b.Property<long>("UserID");

                    b.HasKey("ID");

                    b.HasIndex("GlobalProtectionID");

                    b.HasIndex("UserID");

                    b.ToTable("UserProtectionEntry");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Users.User", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Bio");

                    b.Property<int>("Class");

                    b.Property<long?>("DefaultCharacterID");

                    b.Property<long>("DiscordID");

                    b.Property<long?>("ServerID");

                    b.Property<int?>("Timezone");

                    b.HasKey("ID");

                    b.HasIndex("DefaultCharacterID")
                        .IsUnique();

                    b.HasIndex("ServerID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Users.UserKink", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("KinkID");

                    b.Property<int>("Preference");

                    b.Property<long?>("UserID");

                    b.HasKey("ID");

                    b.HasIndex("KinkID");

                    b.HasIndex("UserID");

                    b.ToTable("UserKink");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Appearances.AppearanceComponent", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Database.Appearances.Appearance")
                        .WithMany("Components")
                        .HasForeignKey("AppearanceID");

                    b.HasOne("DIGOS.Ambassador.Database.Appearances.Colour", "BaseColour")
                        .WithMany()
                        .HasForeignKey("BaseColourID");

                    b.HasOne("DIGOS.Ambassador.Database.Appearances.Colour", "PatternColour")
                        .WithMany()
                        .HasForeignKey("PatternColourID");

                    b.HasOne("DIGOS.Ambassador.Database.Transformations.Transformation", "Transformation")
                        .WithMany()
                        .HasForeignKey("TransformationID");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Characters.Character", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Database.Appearances.Appearance", "CurrentAppearance")
                        .WithMany()
                        .HasForeignKey("CurrentAppearanceID");

                    b.HasOne("DIGOS.Ambassador.Database.Appearances.Appearance", "DefaultAppearance")
                        .WithMany()
                        .HasForeignKey("DefaultAppearanceID");

                    b.HasOne("DIGOS.Ambassador.Database.Users.User", "Owner")
                        .WithMany("Characters")
                        .HasForeignKey("OwnerID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Data.Image", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Database.Characters.Character")
                        .WithMany("Images")
                        .HasForeignKey("CharacterID");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Roleplaying.Roleplay", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Database.Users.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerID");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Roleplaying.RoleplayParticipant", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Database.Roleplaying.Roleplay", "Roleplay")
                        .WithMany("ParticipatingUsers")
                        .HasForeignKey("RoleplayID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DIGOS.Ambassador.Database.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Roleplaying.UserMessage", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Database.Roleplaying.Roleplay")
                        .WithMany("Messages")
                        .HasForeignKey("RoleplayID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Transformations.GlobalUserProtection", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Database.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Transformations.ServerUserProtection", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Database.ServerInfo.Server", "Server")
                        .WithMany()
                        .HasForeignKey("ServerID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DIGOS.Ambassador.Database.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Transformations.Species", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Database.Transformations.Species", "Parent")
                        .WithMany()
                        .HasForeignKey("ParentID");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Transformations.Transformation", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Database.Appearances.Colour", "DefaultBaseColour")
                        .WithMany()
                        .HasForeignKey("DefaultBaseColourID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DIGOS.Ambassador.Database.Appearances.Colour", "DefaultPatternColour")
                        .WithMany()
                        .HasForeignKey("DefaultPatternColourID");

                    b.HasOne("DIGOS.Ambassador.Database.Transformations.Species", "Species")
                        .WithMany()
                        .HasForeignKey("SpeciesID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Transformations.UserProtectionEntry", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Database.Transformations.GlobalUserProtection", "GlobalProtection")
                        .WithMany("UserListing")
                        .HasForeignKey("GlobalProtectionID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DIGOS.Ambassador.Database.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Users.User", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Database.Characters.Character", "DefaultCharacter")
                        .WithOne()
                        .HasForeignKey("DIGOS.Ambassador.Database.Users.User", "DefaultCharacterID");

                    b.HasOne("DIGOS.Ambassador.Database.ServerInfo.Server")
                        .WithMany("KnownUsers")
                        .HasForeignKey("ServerID");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Database.Users.UserKink", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Database.Kinks.Kink", "Kink")
                        .WithMany()
                        .HasForeignKey("KinkID");

                    b.HasOne("DIGOS.Ambassador.Database.Users.User")
                        .WithMany("Kinks")
                        .HasForeignKey("UserID");
                });
#pragma warning restore 612, 618
        }
    }
}
