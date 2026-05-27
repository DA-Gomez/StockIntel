using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "watchlists",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_watchlists", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "watchlist_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    watchlist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticker = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_watchlist_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_watchlist_items_watchlists_watchlist_id",
                        column: x => x.watchlist_id,
                        principalTable: "watchlists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_watchlist_items_watchlist_id",
                table: "watchlist_items",
                column: "watchlist_id");

            migrationBuilder.CreateIndex(
                name: "ix_watchlist_items_watchlist_id_ticker",
                table: "watchlist_items",
                columns: new[] { "watchlist_id", "ticker" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_watchlists_user_id",
                table: "watchlists",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "watchlist_items");

            migrationBuilder.DropTable(
                name: "watchlists");
        }
    }
}
