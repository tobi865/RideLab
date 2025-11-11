using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RideLab.Models;

namespace RideLab.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Bike> Bikes => Set<Bike>();
    public DbSet<ObdSession> ObdSessions => Set<ObdSession>();
    public DbSet<ObdDataPoint> ObdDataPoints => Set<ObdDataPoint>();
    public DbSet<DtcCode> DtcCodes => Set<DtcCode>();
    public DbSet<BikeDtc> BikeDtcs => Set<BikeDtc>();
    public DbSet<ServiceReminder> ServiceReminders => Set<ServiceReminder>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<BikeDtc>().HasKey(x => new { x.BikeId, x.DtcCodeId });

        builder.Entity<BikeDtc>()
            .HasOne(x => x.Bike)
            .WithMany(b => b.ActiveDtcs)
            .HasForeignKey(x => x.BikeId);

        builder.Entity<BikeDtc>()
            .HasOne(x => x.DtcCode)
            .WithMany(d => d.Bikes)
            .HasForeignKey(x => x.DtcCodeId);

        SeedDomain(builder);
    }

    private static void SeedDomain(ModelBuilder builder)
    {
        builder.Entity<Bike>().HasData(
            new Bike
            {
                Id = 1,
                Name = "Kawasaki Ninja ZX-6R",
                Manufacturer = "Kawasaki",
                Model = "ZX-6R",
                Year = 2023,
                Vin = "JKAZX636AAA000001",
                Engine = "636cc",
                Notes = "Primary track bike",
                CreatedAtUtc = new DateTime(2024, 1, 10, 8, 30, 0, DateTimeKind.Utc)
            },
            new Bike
            {
                Id = 2,
                Name = "BMW R1250 GS Adventure",
                Manufacturer = "BMW",
                Model = "R1250 GS",
                Year = 2022,
                Vin = "WB10J2305N6Z00002",
                Engine = "1254cc",
                Notes = "Adventure touring setup",
                CreatedAtUtc = new DateTime(2024, 3, 5, 9, 0, 0, DateTimeKind.Utc)
            });

        builder.Entity<DtcCode>().HasData(
            new DtcCode
            {
                Id = 1,
                Code = "P0135",
                Description = "O2 Sensor Heater Circuit Malfunction",
                Severity = "High",
                Recommendation = "Inspect sensor wiring and replace sensor if necessary"
            },
            new DtcCode
            {
                Id = 2,
                Code = "P0301",
                Description = "Cylinder 1 Misfire Detected",
                Severity = "Critical",
                Recommendation = "Check spark plug, coil, and fuel injector"
            },
            new DtcCode
            {
                Id = 3,
                Code = "C1234",
                Description = "ABS Pressure Sensor Range/Performance",
                Severity = "Medium",
                Recommendation = "Verify ABS pressure sensor calibration"
            });

        builder.Entity<ObdSession>().HasData(
            new ObdSession
            {
                Id = 1,
                BikeId = 1,
                SessionDateUtc = new DateTime(2024, 5, 12, 6, 45, 0, DateTimeKind.Utc),
                SourceFileName = "session-trackday.csv",
                StoredFilePath = "/uploads/session-trackday.csv",
                Notes = "Morning warm-up session",
                AverageRpm = 9800,
                MaxThrottlePosition = 96,
                AnomalySummary = "Short lean condition detected between lap 3-4"
            },
            new ObdSession
            {
                Id = 2,
                BikeId = 2,
                SessionDateUtc = new DateTime(2024, 6, 18, 14, 15, 0, DateTimeKind.Utc),
                SourceFileName = "canbus-tour.json",
                StoredFilePath = "/uploads/canbus-tour.json",
                Notes = "Alpine pass run",
                AverageRpm = 5200,
                MaxThrottlePosition = 78,
                AnomalySummary = "Detected intermittent knock sensor spike"
            });

        builder.Entity<ObdDataPoint>().HasData(
            new ObdDataPoint
            {
                Id = 1,
                ObdSessionId = 1,
                Metric = "RPM",
                Value = 11000,
                RecordedAtUtc = new DateTime(2024, 5, 12, 6, 47, 0, DateTimeKind.Utc)
            },
            new ObdDataPoint
            {
                Id = 2,
                ObdSessionId = 1,
                Metric = "Throttle",
                Value = 92,
                RecordedAtUtc = new DateTime(2024, 5, 12, 6, 47, 30, DateTimeKind.Utc)
            },
            new ObdDataPoint
            {
                Id = 3,
                ObdSessionId = 2,
                Metric = "EngineTemp",
                Value = 98,
                RecordedAtUtc = new DateTime(2024, 6, 18, 14, 20, 0, DateTimeKind.Utc)
            },
            new ObdDataPoint
            {
                Id = 4,
                ObdSessionId = 2,
                Metric = "Speed",
                Value = 110,
                RecordedAtUtc = new DateTime(2024, 6, 18, 14, 25, 0, DateTimeKind.Utc)
            });

        builder.Entity<BikeDtc>().HasData(
            new BikeDtc
            {
                BikeId = 1,
                DtcCodeId = 1,
                DetectedAtUtc = new DateTime(2024, 5, 12, 7, 0, 0, DateTimeKind.Utc),
                IsResolved = false,
                Notes = "Pending inspection"
            },
            new BikeDtc
            {
                BikeId = 1,
                DtcCodeId = 2,
                DetectedAtUtc = new DateTime(2024, 5, 12, 7, 10, 0, DateTimeKind.Utc),
                IsResolved = true,
                Notes = "Spark plug replaced"
            },
            new BikeDtc
            {
                BikeId = 2,
                DtcCodeId = 3,
                DetectedAtUtc = new DateTime(2024, 6, 18, 15, 5, 0, DateTimeKind.Utc),
                IsResolved = false,
                Notes = "Monitor after ABS bleed"
            });

        builder.Entity<ServiceReminder>().HasData(
            new ServiceReminder
            {
                Id = 1,
                BikeId = 1,
                Title = "Oil and filter change",
                Description = "Use racing spec oil",
                DueDate = new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                DueMileage = 10000,
                IsCompleted = false,
                CreatedAtUtc = new DateTime(2024, 5, 13, 8, 0, 0, DateTimeKind.Utc)
            },
            new ServiceReminder
            {
                Id = 2,
                BikeId = 2,
                Title = "Final drive inspection",
                Description = "Check shaft drive play",
                DueDate = new DateTime(2024, 8, 10, 0, 0, 0, DateTimeKind.Utc),
                DueMileage = 18000,
                IsCompleted = false,
                CreatedAtUtc = new DateTime(2024, 6, 20, 9, 30, 0, DateTimeKind.Utc)
            });
    }
}
