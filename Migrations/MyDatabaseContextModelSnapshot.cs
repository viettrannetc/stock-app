﻿// <auto-generated />
using System;
using DotNetCoreSqlDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DotNetCoreSqlDb.Migrations
{
    [DbContext(typeof(MyDatabaseContext))]
    partial class MyDatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("DotNetCoreSqlDb.Models.StockSymbol", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"), 1L, 1);

                    b.Property<decimal>("_bp_")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("_clp_")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("_cp_")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("_diviend_")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("_fp_")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("_in_")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("_lp_")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("_op_")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("_pc_")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("_sc_")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("_sin_")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("_tval_")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("_tvol_")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("_vhtt_")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("catID")
                        .HasColumnType("int");

                    b.Property<decimal>("change")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("stockName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("StockSymbol");
                });

            modelBuilder.Entity("DotNetCoreSqlDb.Models.StockSymbolHistory", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"), 1L, 1);

                    b.Property<decimal>("C")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("H")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("L")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("O")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("StockSymbol")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("T")
                        .HasColumnType("int");

                    b.Property<decimal>("V")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("ID");

                    b.ToTable("StockSymbolHistory");
                });

            modelBuilder.Entity("DotNetCoreSqlDb.Models.Todo", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"), 1L, 1);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Todo");
                });
#pragma warning restore 612, 618
        }
    }
}
