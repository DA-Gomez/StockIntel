using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameInsiderTablesToPlural : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "insider_transaction",
                newName: "insider_transactions");

            migrationBuilder.Sql(
                "ALTER TABLE insider_transactions RENAME CONSTRAINT pk_insider_transaction TO pk_insider_transactions;");

            migrationBuilder.Sql(
                "ALTER TABLE insider_transactions RENAME CONSTRAINT fk_insider_transaction_insider_filing_filing_id TO fk_insider_transactions_insider_filing_filing_id;");

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
            migrationBuilder.RenameIndex(
                name: "ix_insider_transactions_transaction_date",
                newName: "ix_insider_transaction_transaction_date",
                table: "insider_transactions");

            migrationBuilder.RenameIndex(
                name: "ix_insider_transactions_filing_id",
                newName: "ix_insider_transaction_filing_id",
                table: "insider_transactions");

            migrationBuilder.Sql(
                "ALTER TABLE insider_transactions RENAME CONSTRAINT fk_insider_transactions_insider_filing_filing_id TO fk_insider_transaction_insider_filing_filing_id;");

            migrationBuilder.Sql(
                "ALTER TABLE insider_transactions RENAME CONSTRAINT pk_insider_transactions TO pk_insider_transaction;");

            migrationBuilder.RenameTable(
                name: "insider_transactions",
                newName: "insider_transaction");
        }
    }
}
