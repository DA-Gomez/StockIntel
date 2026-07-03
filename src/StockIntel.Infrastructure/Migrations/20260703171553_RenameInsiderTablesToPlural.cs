using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameInsiderTablesToPlural : Migration
    {
        // Pure rename: the class names changed (InsiderFiling -> InsiderFilings,
        // InsiderTransaction -> InsiderTransactions) so the snake_case naming
        // convention now expects plural table/constraint/index names. We rename
        // in place instead of drop/create so existing rows survive.
        //
        // Order matters: rename the tables first, then the objects that live on
        // them (PK/FK/indexes), because Postgres constraint/index renames target
        // the (already-renamed) table.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tables
            migrationBuilder.RenameTable(
                name: "insider_filing",
                newName: "insider_filings");

            migrationBuilder.RenameTable(
                name: "insider_transaction",
                newName: "insider_transactions");

            // Primary keys (no dedicated MigrationBuilder op — use Postgres DDL)
            migrationBuilder.Sql(
                "ALTER TABLE insider_filings RENAME CONSTRAINT pk_insider_filing TO pk_insider_filings;");
            migrationBuilder.Sql(
                "ALTER TABLE insider_transactions RENAME CONSTRAINT pk_insider_transaction TO pk_insider_transactions;");

            // Foreign keys
            migrationBuilder.Sql(
                "ALTER TABLE insider_filings RENAME CONSTRAINT fk_insider_filing_company_company_id TO fk_insider_filings_company_company_id;");
            migrationBuilder.Sql(
                "ALTER TABLE insider_transactions RENAME CONSTRAINT fk_insider_transaction_insider_filing_filing_id TO fk_insider_transactions_insider_filings_filing_id;");

            // Indexes
            migrationBuilder.RenameIndex(
                name: "ix_insider_filing_accession_number",
                newName: "ix_insider_filings_accession_number",
                table: "insider_filings");

            migrationBuilder.RenameIndex(
                name: "ix_insider_filing_company_id_filing_date",
                newName: "ix_insider_filings_company_id_filing_date",
                table: "insider_filings");

            migrationBuilder.RenameIndex(
                name: "ix_insider_transaction_filing_id",
                newName: "ix_insider_transactions_filing_id",
                table: "insider_transactions");

            migrationBuilder.RenameIndex(
                name: "ix_insider_transaction_transaction_date",
                newName: "ix_insider_transactions_transaction_date",
                table: "insider_transactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Indexes back to singular
            migrationBuilder.RenameIndex(
                name: "ix_insider_transactions_transaction_date",
                newName: "ix_insider_transaction_transaction_date",
                table: "insider_transactions");

            migrationBuilder.RenameIndex(
                name: "ix_insider_transactions_filing_id",
                newName: "ix_insider_transaction_filing_id",
                table: "insider_transactions");

            migrationBuilder.RenameIndex(
                name: "ix_insider_filings_company_id_filing_date",
                newName: "ix_insider_filing_company_id_filing_date",
                table: "insider_filings");

            migrationBuilder.RenameIndex(
                name: "ix_insider_filings_accession_number",
                newName: "ix_insider_filing_accession_number",
                table: "insider_filings");

            // Foreign keys back to singular
            migrationBuilder.Sql(
                "ALTER TABLE insider_transactions RENAME CONSTRAINT fk_insider_transactions_insider_filings_filing_id TO fk_insider_transaction_insider_filing_filing_id;");
            migrationBuilder.Sql(
                "ALTER TABLE insider_filings RENAME CONSTRAINT fk_insider_filings_company_company_id TO fk_insider_filing_company_company_id;");

            // Primary keys back to singular
            migrationBuilder.Sql(
                "ALTER TABLE insider_transactions RENAME CONSTRAINT pk_insider_transactions TO pk_insider_transaction;");
            migrationBuilder.Sql(
                "ALTER TABLE insider_filings RENAME CONSTRAINT pk_insider_filings TO pk_insider_filing;");

            // Tables back to singular
            migrationBuilder.RenameTable(
                name: "insider_transactions",
                newName: "insider_transaction");

            migrationBuilder.RenameTable(
                name: "insider_filings",
                newName: "insider_filing");
        }
    }
}
