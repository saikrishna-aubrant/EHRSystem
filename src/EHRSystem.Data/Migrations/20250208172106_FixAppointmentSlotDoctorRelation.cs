using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHRSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixAppointmentSlotDoctorRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentSlots_AspNetUsers_DoctorId1",
                table: "AppointmentSlots");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentSlots_DoctorId1",
                table: "AppointmentSlots");

            migrationBuilder.DropColumn(
                name: "DoctorId1",
                table: "AppointmentSlots");

            migrationBuilder.CreateTable(
                name: "AppointmentAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OldDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NewDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<string>(type: "nvarchar(85)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentAudits_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppointmentAudits_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentAudits_AppointmentId",
                table: "AppointmentAudits",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentAudits_ModifiedById",
                table: "AppointmentAudits",
                column: "ModifiedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentAudits");

            migrationBuilder.AddColumn<string>(
                name: "DoctorId1",
                table: "AppointmentSlots",
                type: "nvarchar(85)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlots_DoctorId1",
                table: "AppointmentSlots",
                column: "DoctorId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentSlots_AspNetUsers_DoctorId1",
                table: "AppointmentSlots",
                column: "DoctorId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
