﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace RentalManagementSystem.Migrations
{
    [DbContext(typeof(RentalManagementContext))]
    [Migration("20250122043728_fix")]
    partial class fix
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("House", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("HouseNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<bool>("IsOccupied")
                        .HasColumnType("bit");

                    b.Property<int?>("OccupancyReportId")
                        .HasColumnType("int");

                    b.Property<int>("PropertyId")
                        .HasColumnType("int");

                    b.Property<decimal>("Rent")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime?>("VacantSince")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("OccupancyReportId");

                    b.HasIndex("PropertyId");

                    b.ToTable("Houses", (string)null);
                });

            modelBuilder.Entity("Property", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Properties", (string)null);
                });

            modelBuilder.Entity("RentalManagementSystem.Models.Bank", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Head_Office")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Website")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Year_Of_Establishment")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Banks", (string)null);
                });

            modelBuilder.Entity("RentalManagementSystem.Models.ContactInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BankId")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BankId")
                        .IsUnique();

                    b.ToTable("ContactInfos");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.FinancialReport", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("NetIncome")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("PropertyId")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("TotalExpenses")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalRevenue")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("PropertyId");

                    b.ToTable("FinancialReports", (string)null);
                });

            modelBuilder.Entity("RentalManagementSystem.Models.Lease", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("LeaseReportId")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("TenantId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdatedAt")
                        .ValueGeneratedOnUpdate()
                        .HasColumnType("datetime2");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("LeaseReportId");

                    b.HasIndex("TenantId");

                    b.HasIndex("UserId");

                    b.ToTable("Leases", (string)null);
                });

            modelBuilder.Entity("RentalManagementSystem.Models.LeaseDocument", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DocumentName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("DocumentPath")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<DateTime>("GeneratedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("LeaseId")
                        .HasColumnType("int");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("LeaseId");

                    b.ToTable("LeaseDocuments", (string)null);
                });

            modelBuilder.Entity("RentalManagementSystem.Models.LeaseReport", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ActiveLeases")
                        .HasColumnType("int");

                    b.Property<int>("ExpiringLeases")
                        .HasColumnType("int");

                    b.Property<DateTime>("GeneratedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("PropertyId")
                        .HasColumnType("int");

                    b.Property<int>("RenewedLeases")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PropertyId");

                    b.ToTable("LeaseReports", (string)null);
                });

            modelBuilder.Entity("RentalManagementSystem.Models.MaintenanceReport", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("PendingRequests")
                        .HasColumnType("int");

                    b.Property<int>("PropertyId")
                        .HasColumnType("int");

                    b.Property<int>("ResolvedRequests")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("TotalMaintenanceCost")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("TotalRequests")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PropertyId");

                    b.ToTable("MaintenanceReports", (string)null);
                });

            modelBuilder.Entity("RentalManagementSystem.Models.Notification", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<bool>("IsRead")
                        .HasColumnType("bit");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("TargetUrl")
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Notifications", (string)null);
                });

            modelBuilder.Entity("RentalManagementSystem.Models.OccupancyReport", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("GeneratedDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("OccupancyRate")
                        .HasColumnType("decimal(5,2)");

                    b.Property<int>("OccupiedUnits")
                        .HasColumnType("int");

                    b.Property<int>("PropertyId")
                        .HasColumnType("int");

                    b.Property<int>("TotalUnits")
                        .HasColumnType("int");

                    b.Property<int>("VacantUnits")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PropertyId");

                    b.ToTable("OccupancyReports", (string)null);
                });

            modelBuilder.Entity("RentalManagementSystem.Models.Payment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int?>("FinancialReportId")
                        .HasColumnType("int");

                    b.Property<int?>("HouseId")
                        .HasColumnType("int");

                    b.Property<DateTime>("PaymentDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("PaymentMethod")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("PaymentReference")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("PaymentStatus")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("PaymentType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FinancialReportId");

                    b.HasIndex("HouseId");

                    b.HasIndex("UserId");

                    b.ToTable("Payments", (string)null);
                });

            modelBuilder.Entity("RentalManagementSystem.Models.Profile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AccountHolderName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AccountNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Bank")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("LandlordId")
                        .HasColumnType("int");

                    b.Property<string>("NumberForPayments")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("LandlordId")
                        .IsUnique();

                    b.ToTable("Profiles", (string)null);
                });

            modelBuilder.Entity("RentalManagementSystem.Models.Request", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("LandlordNotes")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<int?>("MaintenanceReportId")
                        .HasColumnType("int");

                    b.Property<string>("Priority")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("TenantId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime?>("UpdatedAt")
                        .ValueGeneratedOnUpdate()
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("MaintenanceReportId");

                    b.HasIndex("TenantId");

                    b.ToTable("Requests", (string)null);
                });

            modelBuilder.Entity("RentalManagementSystem.Models.Utility", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Cost")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Utilities", (string)null);
                });

            modelBuilder.Entity("RentalManagementSystem.Models.UtilityReading", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Consumption")
                        .HasColumnType("int");

                    b.Property<int>("CurrentReading")
                        .HasColumnType("int");

                    b.Property<bool>("IsPaid")
                        .HasColumnType("bit");

                    b.Property<int>("PrevReading")
                        .HasColumnType("int");

                    b.Property<DateTime>("ReadingDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("TenantId")
                        .HasColumnType("int");

                    b.Property<int>("TotalCost")
                        .HasColumnType("int");

                    b.Property<int>("UtilityId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.HasIndex("UtilityId");

                    b.ToTable("UtilityReadings", (string)null);
                });

            modelBuilder.Entity("User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<int>("FailedLoginAttempts")
                        .HasColumnType("int");

                    b.Property<string>("FullName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("HouseId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastLoginDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("PasswordChanged")
                        .HasColumnType("bit");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RefreshTokenExpiryTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("HouseId")
                        .IsUnique()
                        .HasFilter("[HouseId] IS NOT NULL");

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("House", b =>
                {
                    b.HasOne("RentalManagementSystem.Models.OccupancyReport", null)
                        .WithMany("UnitStatus")
                        .HasForeignKey("OccupancyReportId");

                    b.HasOne("Property", "Property")
                        .WithMany("Houses")
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Property");
                });

            modelBuilder.Entity("Property", b =>
                {
                    b.HasOne("User", "User")
                        .WithMany("Properties")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.ContactInfo", b =>
                {
                    b.HasOne("RentalManagementSystem.Models.Bank", "Bank")
                        .WithOne("Contact_Info")
                        .HasForeignKey("RentalManagementSystem.Models.ContactInfo", "BankId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Bank");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.FinancialReport", b =>
                {
                    b.HasOne("Property", "Property")
                        .WithMany()
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Property");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.Lease", b =>
                {
                    b.HasOne("RentalManagementSystem.Models.LeaseReport", null)
                        .WithMany("LeaseDetails")
                        .HasForeignKey("LeaseReportId");

                    b.HasOne("User", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("User", null)
                        .WithMany("Leases")
                        .HasForeignKey("UserId");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.LeaseDocument", b =>
                {
                    b.HasOne("RentalManagementSystem.Models.Lease", "Lease")
                        .WithMany()
                        .HasForeignKey("LeaseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Lease");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.LeaseReport", b =>
                {
                    b.HasOne("Property", "Property")
                        .WithMany()
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Property");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.MaintenanceReport", b =>
                {
                    b.HasOne("Property", "Property")
                        .WithMany()
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Property");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.Notification", b =>
                {
                    b.HasOne("User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.OccupancyReport", b =>
                {
                    b.HasOne("Property", "Property")
                        .WithMany()
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Property");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.Payment", b =>
                {
                    b.HasOne("RentalManagementSystem.Models.FinancialReport", null)
                        .WithMany("Payments")
                        .HasForeignKey("FinancialReportId");

                    b.HasOne("House", "House")
                        .WithMany("Payments")
                        .HasForeignKey("HouseId");

                    b.HasOne("User", "User")
                        .WithMany("Payments")
                        .HasForeignKey("UserId");

                    b.Navigation("House");

                    b.Navigation("User");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.Profile", b =>
                {
                    b.HasOne("User", "Landlord")
                        .WithOne("Profile")
                        .HasForeignKey("RentalManagementSystem.Models.Profile", "LandlordId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Landlord");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.Request", b =>
                {
                    b.HasOne("RentalManagementSystem.Models.MaintenanceReport", null)
                        .WithMany("Requests")
                        .HasForeignKey("MaintenanceReportId");

                    b.HasOne("User", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.UtilityReading", b =>
                {
                    b.HasOne("User", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RentalManagementSystem.Models.Utility", "Utility")
                        .WithMany()
                        .HasForeignKey("UtilityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");

                    b.Navigation("Utility");
                });

            modelBuilder.Entity("User", b =>
                {
                    b.HasOne("House", "House")
                        .WithOne("Tenant")
                        .HasForeignKey("User", "HouseId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("House");
                });

            modelBuilder.Entity("House", b =>
                {
                    b.Navigation("Payments");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("Property", b =>
                {
                    b.Navigation("Houses");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.Bank", b =>
                {
                    b.Navigation("Contact_Info")
                        .IsRequired();
                });

            modelBuilder.Entity("RentalManagementSystem.Models.FinancialReport", b =>
                {
                    b.Navigation("Payments");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.LeaseReport", b =>
                {
                    b.Navigation("LeaseDetails");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.MaintenanceReport", b =>
                {
                    b.Navigation("Requests");
                });

            modelBuilder.Entity("RentalManagementSystem.Models.OccupancyReport", b =>
                {
                    b.Navigation("UnitStatus");
                });

            modelBuilder.Entity("User", b =>
                {
                    b.Navigation("Leases");

                    b.Navigation("Payments");

                    b.Navigation("Profile")
                        .IsRequired();

                    b.Navigation("Properties");
                });
#pragma warning restore 612, 618
        }
    }
}
