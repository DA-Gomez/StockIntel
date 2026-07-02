using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFilingsContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "company",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cik = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_ticker",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticker = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_ticker", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_ticker_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "insider_filing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    accession_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    filing_date = table.Column<DateOnly>(type: "date", nullable: false),
                    insider_cik = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    insider_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_director = table.Column<bool>(type: "boolean", nullable: false),
                    is_officer = table.Column<bool>(type: "boolean", nullable: false),
                    is_ten_percent_owner = table.Column<bool>(type: "boolean", nullable: false),
                    officer_title = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ingested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_insider_filing", x => x.id);
                    table.ForeignKey(
                        name: "fk_insider_filing_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "insider_transaction",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    filing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    shares = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    price_per_share = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    is_acquisition = table.Column<bool>(type: "boolean", nullable: false),
                    shares_owned_after = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    is_direct_ownership = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_insider_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_insider_transaction_insider_filing_filing_id",
                        column: x => x.filing_id,
                        principalTable: "insider_filing",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_name",
                table: "company",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_ticker_company_id",
                table: "company_ticker",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ticker_ticker",
                table: "company_ticker",
                column: "ticker",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_insider_filing_accession_number",
                table: "insider_filing",
                column: "accession_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_insider_filing_company_id_filing_date",
                table: "insider_filing",
                columns: new[] { "company_id", "filing_date" });

            migrationBuilder.CreateIndex(
                name: "ix_insider_transaction_filing_id",
                table: "insider_transaction",
                column: "filing_id");

            migrationBuilder.CreateIndex(
                name: "ix_insider_transaction_transaction_date",
                table: "insider_transaction",
                column: "transaction_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_ticker");

            migrationBuilder.DropTable(
                name: "insider_transaction");

            migrationBuilder.DropTable(
                name: "insider_filing");

            migrationBuilder.DropTable(
                name: "company");
        }
    }
}
