﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetCatHook.Scraper.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NetCatHook.Scraper.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240115190539_AddTableWeatherReports")]
    partial class AddTableWeatherReports
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("NetCatHook.Scraper.App.Entities.TgBotChat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<long>("ChatId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "ChatId" }, "IX_ChatId_Unique")
                        .IsUnique();

                    b.ToTable("TgBotChats");
                });

            modelBuilder.Entity("NetCatHook.Scraper.App.Entities.WeatherReport", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.Property<int?>("Humidity")
                        .HasColumnType("integer");

                    b.Property<int?>("Pressure")
                        .HasColumnType("integer");

                    b.Property<int?>("TemperatureAir")
                        .HasColumnType("integer");

                    b.Property<int?>("WindDirection")
                        .HasColumnType("integer");

                    b.Property<int?>("WindGust")
                        .HasColumnType("integer");

                    b.Property<int?>("WindSpeed")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("WeatherReports");
                });
#pragma warning restore 612, 618
        }
    }
}
