using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnonymousComplaintsAPI.Migrations
{
    /// <inheritdoc />
    public partial class mig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CCAC14030537", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Archived = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Categori__19093A2B51D3BA1B", x => x.CategoryID);
                    table.ForeignKey(
                        name: "FK_Categories_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Frequencies",
                columns: table => new
                {
                    FrequencyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Archived = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Frequenc__592474B82B67A39E", x => x.FrequencyID);
                    table.ForeignKey(
                        name: "FK_Frequencies_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Types",
                columns: table => new
                {
                    TypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Archived = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CategoryID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Types__516F03959BE84BD0", x => x.TypeID);
                    table.ForeignKey(
                        name: "FK_Types_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__Types__CategoryI__151B244E",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID");
                });

            migrationBuilder.CreateTable(
                name: "AnonymousComplaints",
                columns: table => new
                {
                    AnonymousComplaintID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: true),
                    FrequencyID = table.Column<int>(type: "int", nullable: true),
                    FusionWithID = table.Column<int>(type: "int", nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    TypeID = table.Column<int>(type: "int", nullable: true),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Anonymou__54E7974A0BBB4943", x => x.AnonymousComplaintID);
                    table.ForeignKey(
                        name: "FK_AnonymousComplaint_Category",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID");
                    table.ForeignKey(
                        name: "FK_AnonymousComplaint_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_AnonymousComplaint_Frequency",
                        column: x => x.FrequencyID,
                        principalTable: "Frequencies",
                        principalColumn: "FrequencyID");
                    table.ForeignKey(
                        name: "FK_AnonymousComplaint_Fusion",
                        column: x => x.FusionWithID,
                        principalTable: "AnonymousComplaints",
                        principalColumn: "AnonymousComplaintID");
                    table.ForeignKey(
                        name: "FK__Anonymous__TypeI__2739D489",
                        column: x => x.TypeID,
                        principalTable: "Types",
                        principalColumn: "TypeID");
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    AttachmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnonymousComplaintID = table.Column<int>(type: "int", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    FileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Attachme__442C64DE2E016E74", x => x.AttachmentID);
                    table.ForeignKey(
                        name: "FK_Attachments_AnonymousComplaint",
                        column: x => x.AnonymousComplaintID,
                        principalTable: "AnonymousComplaints",
                        principalColumn: "AnonymousComplaintID");
                    table.ForeignKey(
                        name: "FK_Attachments_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Defendants",
                columns: table => new
                {
                    AnonymousComplaintID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Defendan__859F1B80B65EB45E", x => new { x.AnonymousComplaintID, x.UserID });
                    table.ForeignKey(
                        name: "FK_Defendants_AnonymousComplaint",
                        column: x => x.AnonymousComplaintID,
                        principalTable: "AnonymousComplaints",
                        principalColumn: "AnonymousComplaintID");
                    table.ForeignKey(
                        name: "FK_Defendants_User",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Solutions",
                columns: table => new
                {
                    SolutionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnonymousComplaintID = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Archived = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Solution__6B633AF077192C53", x => x.SolutionID);
                    table.ForeignKey(
                        name: "FK_Solutions_AnonymousComplaint",
                        column: x => x.AnonymousComplaintID,
                        principalTable: "AnonymousComplaints",
                        principalColumn: "AnonymousComplaintID");
                    table.ForeignKey(
                        name: "FK_Solutions_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousComplaints_CategoryID",
                table: "AnonymousComplaints",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousComplaints_CreatedBy",
                table: "AnonymousComplaints",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousComplaints_FrequencyID",
                table: "AnonymousComplaints",
                column: "FrequencyID");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousComplaints_FusionWithID",
                table: "AnonymousComplaints",
                column: "FusionWithID");

            migrationBuilder.CreateIndex(
                name: "IX_AnonymousComplaints_TypeID",
                table: "AnonymousComplaints",
                column: "TypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_AnonymousComplaintID",
                table: "Attachments",
                column: "AnonymousComplaintID");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_CreatedBy",
                table: "Attachments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CreatedBy",
                table: "Categories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Defendants_UserID",
                table: "Defendants",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Frequencies_CreatedBy",
                table: "Frequencies",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_AnonymousComplaintID",
                table: "Solutions",
                column: "AnonymousComplaintID");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_CreatedBy",
                table: "Solutions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Types_CategoryID",
                table: "Types",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Types_CreatedBy",
                table: "Types",
                column: "CreatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "Defendants");

            migrationBuilder.DropTable(
                name: "Solutions");

            migrationBuilder.DropTable(
                name: "AnonymousComplaints");

            migrationBuilder.DropTable(
                name: "Frequencies");

            migrationBuilder.DropTable(
                name: "Types");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
