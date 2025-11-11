using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RideLab.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredWorkshop",
                table: "AspNetUsers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Bikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Vin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Engine = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bikes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DtcCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Recommendation = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DtcCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObdSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BikeId = table.Column<int>(type: "int", nullable: false),
                    SessionDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceFileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    StoredFilePath = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AverageRpm = table.Column<double>(type: "float", nullable: true),
                    MaxThrottlePosition = table.Column<double>(type: "float", nullable: true),
                    AnomalySummary = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObdSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObdSessions_Bikes_BikeId",
                        column: x => x.BikeId,
                        principalTable: "Bikes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceReminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BikeId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueMileage = table.Column<int>(type: "int", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceReminders_Bikes_BikeId",
                        column: x => x.BikeId,
                        principalTable: "Bikes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BikeDtcs",
                columns: table => new
                {
                    BikeId = table.Column<int>(type: "int", nullable: false),
                    DtcCodeId = table.Column<int>(type: "int", nullable: false),
                    DetectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BikeDtcs", x => new { x.BikeId, x.DtcCodeId });
                    table.ForeignKey(
                        name: "FK_BikeDtcs_Bikes_BikeId",
                        column: x => x.BikeId,
                        principalTable: "Bikes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BikeDtcs_DtcCodes_DtcCodeId",
                        column: x => x.DtcCodeId,
                        principalTable: "DtcCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObdDataPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObdSessionId = table.Column<int>(type: "int", nullable: false),
                    Metric = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObdDataPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObdDataPoints_ObdSessions_ObdSessionId",
                        column: x => x.ObdSessionId,
                        principalTable: "ObdSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Bikes",
                columns: new[] { "Id", "CreatedAtUtc", "Engine", "Manufacturer", "Model", "Name", "Notes", "Vin", "Year" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 10, 8, 30, 0, 0, DateTimeKind.Utc), "636cc", "Kawasaki", "ZX-6R", "Kawasaki Ninja ZX-6R", "Primary track bike", "JKAZX636AAA000001", 2023 },
                    { 2, new DateTime(2024, 3, 5, 9, 0, 0, 0, DateTimeKind.Utc), "1254cc", "BMW", "R1250 GS", "BMW R1250 GS Adventure", "Adventure touring setup", "WB10J2305N6Z00002", 2022 }
                });

            migrationBuilder.InsertData(
                table: "DtcCodes",
                columns: new[] { "Id", "Code", "Description", "Recommendation", "Severity" },
                values: new object[,]
                {
                    { 1, "P0135", "O2 Sensor Heater Circuit Malfunction", "Inspect sensor wiring and replace sensor if necessary", "High" },
                    { 2, "P0301", "Cylinder 1 Misfire Detected", "Check spark plug, coil, and fuel injector", "Critical" },
                    { 3, "C1234", "ABS Pressure Sensor Range/Performance", "Verify ABS pressure sensor calibration", "Medium" }
                });

            migrationBuilder.InsertData(
                table: "BikeDtcs",
                columns: new[] { "BikeId", "DtcCodeId", "DetectedAtUtc", "IsResolved", "Notes" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 5, 12, 7, 0, 0, 0, DateTimeKind.Utc), false, "Pending inspection" },
                    { 1, 2, new DateTime(2024, 5, 12, 7, 10, 0, 0, DateTimeKind.Utc), true, "Spark plug replaced" },
                    { 2, 3, new DateTime(2024, 6, 18, 15, 5, 0, 0, DateTimeKind.Utc), false, "Monitor after ABS bleed" }
                });

            migrationBuilder.InsertData(
                table: "ObdSessions",
                columns: new[] { "Id", "AnomalySummary", "AverageRpm", "BikeId", "MaxThrottlePosition", "Notes", "SessionDateUtc", "SourceFileName", "StoredFilePath" },
                values: new object[,]
                {
                    { 1, "Short lean condition detected between lap 3-4", 9800.0, 1, 96.0, "Morning warm-up session", new DateTime(2024, 5, 12, 6, 45, 0, 0, DateTimeKind.Utc), "session-trackday.csv", "/uploads/session-trackday.csv" },
                    { 2, "Detected intermittent knock sensor spike", 5200.0, 2, 78.0, "Alpine pass run", new DateTime(2024, 6, 18, 14, 15, 0, 0, DateTimeKind.Utc), "canbus-tour.json", "/uploads/canbus-tour.json" }
                });

            migrationBuilder.InsertData(
                table: "ServiceReminders",
                columns: new[] { "Id", "BikeId", "CreatedAtUtc", "Description", "DueDate", "DueMileage", "IsCompleted", "Title" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 5, 13, 8, 0, 0, 0, DateTimeKind.Utc), "Use racing spec oil", new DateTime(2024, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10000, false, "Oil and filter change" },
                    { 2, 2, new DateTime(2024, 6, 20, 9, 30, 0, 0, DateTimeKind.Utc), "Check shaft drive play", new DateTime(2024, 8, 10, 0, 0, 0, 0, DateTimeKind.Utc), 18000, false, "Final drive inspection" }
                });

            migrationBuilder.InsertData(
                table: "ObdDataPoints",
                columns: new[] { "Id", "Metric", "ObdSessionId", "RecordedAtUtc", "Value" },
                values: new object[,]
                {
                    { 1, "RPM", 1, new DateTime(2024, 5, 12, 6, 47, 0, 0, DateTimeKind.Utc), 11000.0 },
                    { 2, "Throttle", 1, new DateTime(2024, 5, 12, 6, 47, 30, 0, DateTimeKind.Utc), 92.0 },
                    { 3, "EngineTemp", 2, new DateTime(2024, 6, 18, 14, 20, 0, 0, DateTimeKind.Utc), 98.0 },
                    { 4, "Speed", 2, new DateTime(2024, 6, 18, 14, 25, 0, 0, DateTimeKind.Utc), 110.0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BikeDtcs_DtcCodeId",
                table: "BikeDtcs",
                column: "DtcCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ObdDataPoints_ObdSessionId",
                table: "ObdDataPoints",
                column: "ObdSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ObdSessions_BikeId",
                table: "ObdSessions",
                column: "BikeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReminders_BikeId",
                table: "ServiceReminders",
                column: "BikeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BikeDtcs");

            migrationBuilder.DropTable(
                name: "ObdDataPoints");

            migrationBuilder.DropTable(
                name: "ServiceReminders");

            migrationBuilder.DropTable(
                name: "DtcCodes");

            migrationBuilder.DropTable(
                name: "ObdSessions");

            migrationBuilder.DropTable(
                name: "Bikes");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PreferredWorkshop",
                table: "AspNetUsers");
        }
    }
}
