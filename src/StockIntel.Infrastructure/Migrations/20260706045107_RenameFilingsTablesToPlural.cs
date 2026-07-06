using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameFilingsTablesToPlural : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_company_ticker_company_company_id",
                table: "company_ticker");

            migrationBuilder.DropForeignKey(
                name: "fk_insider_filing_company_company_id",
                table: "insider_filing");

            migrationBuilder.DropForeignKey(
                name: "fk_insider_transactions_insider_filing_filing_id",
                table: "insider_transactions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_insider_filing",
                table: "insider_filing");

            migrationBuilder.DropPrimaryKey(
                name: "pk_company_ticker",
                table: "company_ticker");

            migrationBuilder.DropPrimaryKey(
                name: "pk_company",
                table: "company");

            migrationBuilder.RenameTable(
                name: "insider_filing",
                newName: "insider_filings");

            migrationBuilder.RenameTable(
                name: "company_ticker",
                newName: "company_tickers");

            migrationBuilder.RenameTable(
                name: "company",
                newName: "companies");

            migrationBuilder.RenameIndex(
                name: "ix_insider_filing_company_id_filing_date",
                table: "insider_filings",
                newName: "ix_insider_filings_company_id_filing_date");

            migrationBuilder.RenameIndex(
                name: "ix_insider_filing_accession_number",
                table: "insider_filings",
                newName: "ix_insider_filings_accession_number");

            migrationBuilder.RenameIndex(
                name: "ix_company_ticker_ticker",
                table: "company_tickers",
                newName: "ix_company_tickers_ticker");

            migrationBuilder.RenameIndex(
                name: "ix_company_ticker_company_id",
                table: "company_tickers",
                newName: "ix_company_tickers_company_id");

            migrationBuilder.RenameIndex(
                name: "ix_company_name",
                table: "companies",
                newName: "ix_companies_name");

            migrationBuilder.AddPrimaryKey(
                name: "pk_insider_filings",
                table: "insider_filings",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_company_tickers",
                table: "company_tickers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_companies",
                table: "companies",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_tickers_companies_company_id",
                table: "company_tickers",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_insider_filings_companies_company_id",
                table: "insider_filings",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_insider_transactions_insider_filings_filing_id",
                table: "insider_transactions",
                column: "filing_id",
                principalTable: "insider_filings",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_company_tickers_companies_company_id",
                table: "company_tickers");

            migrationBuilder.DropForeignKey(
                name: "fk_insider_filings_companies_company_id",
                table: "insider_filings");

            migrationBuilder.DropForeignKey(
                name: "fk_insider_transactions_insider_filings_filing_id",
                table: "insider_transactions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_insider_filings",
                table: "insider_filings");

            migrationBuilder.DropPrimaryKey(
                name: "pk_company_tickers",
                table: "company_tickers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_companies",
                table: "companies");

            migrationBuilder.RenameTable(
                name: "insider_filings",
                newName: "insider_filing");

            migrationBuilder.RenameTable(
                name: "company_tickers",
                newName: "company_ticker");

            migrationBuilder.RenameTable(
                name: "companies",
                newName: "company");

            migrationBuilder.RenameIndex(
                name: "ix_insider_filings_company_id_filing_date",
                table: "insider_filing",
                newName: "ix_insider_filing_company_id_filing_date");

            migrationBuilder.RenameIndex(
                name: "ix_insider_filings_accession_number",
                table: "insider_filing",
                newName: "ix_insider_filing_accession_number");

            migrationBuilder.RenameIndex(
                name: "ix_company_tickers_ticker",
                table: "company_ticker",
                newName: "ix_company_ticker_ticker");

            migrationBuilder.RenameIndex(
                name: "ix_company_tickers_company_id",
                table: "company_ticker",
                newName: "ix_company_ticker_company_id");

            migrationBuilder.RenameIndex(
                name: "ix_companies_name",
                table: "company",
                newName: "ix_company_name");

            migrationBuilder.AddPrimaryKey(
                name: "pk_insider_filing",
                table: "insider_filing",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_company_ticker",
                table: "company_ticker",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_company",
                table: "company",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_ticker_company_company_id",
                table: "company_ticker",
                column: "company_id",
                principalTable: "company",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_insider_filing_company_company_id",
                table: "insider_filing",
                column: "company_id",
                principalTable: "company",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_insider_transactions_insider_filing_filing_id",
                table: "insider_transactions",
                column: "filing_id",
                principalTable: "insider_filing",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
