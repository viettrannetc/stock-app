﻿// <auto-generated />
using System;
using DotNetCoreSqlDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DotNetCoreSqlDb.Migrations
{
    [DbContext(typeof(MyDatabaseContext))]
    [Migration("20230116031047_addPKAForHours")]
    partial class addPKAForHours
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("DotNetCoreSqlDb.Models.Database.Finance.KLGDMuaBan", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"), 1L, 1);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("StockSymbol")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("TotalBuy")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalSell")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalUnknow")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalVol")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("ID");

                    b.ToTable("KLGDMuaBan");
                });

            modelBuilder.Entity("DotNetCoreSqlDb.Models.History", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"), 1L, 1);

                    b.Property<decimal>("BandsBot")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("BandsMid")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("BandsTop")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("C")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("GiaMA05")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("H")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("IchimokuCloudBot")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("IchimokuCloudTop")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("IchimokuKijun")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("IchimokuTenKan")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("L")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("MACD")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("MACDMomentum")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("MACDPhanKi")
                        .HasColumnType("int");

                    b.Property<decimal>("MACDSignal")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("NenBot")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("NenTop")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("O")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("RSI")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("RSIPhanKi")
                        .HasColumnType("int");

                    b.Property<string>("StockSymbol")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("T")
                        .HasColumnType("int");

                    b.Property<decimal>("V")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("ID");

                    b.ToTable("History");
                });

            modelBuilder.Entity("DotNetCoreSqlDb.Models.HistoryHour", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"), 1L, 1);

                    b.Property<decimal>("BandsBot")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("BandsMid")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("BandsTop")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("C")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("GiaMA05")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("H")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("IchimokuCloudBot")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("IchimokuCloudTop")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("IchimokuKijun")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("IchimokuTenKan")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("L")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("MACD")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("MACDMomentum")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("MACDPhanKi")
                        .HasColumnType("int");

                    b.Property<decimal>("MACDSignal")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("NenBot")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("NenTop")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("O")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("RSI")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("RSIPhanKi")
                        .HasColumnType("int");

                    b.Property<string>("StockSymbol")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("T")
                        .HasColumnType("int");

                    b.Property<decimal>("V")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("ID");

                    b.ToTable("HistoryHour");
                });

            modelBuilder.Entity("DotNetCoreSqlDb.Models.StockSymbol", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"), 1L, 1);

                    b.Property<bool>("BiChanGiaoDich")
                        .HasColumnType("bit");

                    b.Property<decimal>("MA20Vol")
                        .HasColumnType("decimal(18,2)");

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

            modelBuilder.Entity("DotNetCoreSqlDb.Models.StockSymbolFinanceHistory", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"), 1L, 1);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NameEn")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Quarter")
                        .HasColumnType("int");

                    b.Property<string>("StockSymbol")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<decimal?>("Value")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("YearPeriod")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.ToTable("StockSymbolFinanceHistory");
                });

            modelBuilder.Entity("DotNetCoreSqlDb.Models.StockSymbolFinanceYearlyHistory", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"), 1L, 1);

                    b.Property<string>("NameEn")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StockSymbol")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<decimal?>("Value")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("YearPeriod")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.ToTable("StockSymbolFinanceYearlyHistory");
                });

            modelBuilder.Entity("DotNetCoreSqlDb.Models.StockSymbolTradingHistory", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"), 1L, 1);

                    b.Property<decimal>("Change")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsBuy")
                        .HasColumnType("bit");

                    b.Property<bool>("IsTangDotBien")
                        .HasColumnType("bit");

                    b.Property<decimal>("MatchQtty")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("StockSymbol")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("TotalVol")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("ID");

                    b.ToTable("StockSymbolTradingHistory");
                });
#pragma warning restore 612, 618
        }
    }
}
