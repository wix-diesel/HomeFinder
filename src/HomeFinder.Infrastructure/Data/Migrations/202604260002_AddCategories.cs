using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeFinder.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Categories テーブル作成
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    normalizedName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    color = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true),
                    isReserved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.id);
                });

            // Categories テーブルに UNIQUE インデックス
            migrationBuilder.CreateIndex(
                name: "IX_Categories_NormalizedName",
                table: "Categories",
                column: "normalizedName",
                unique: true);

            // Items テーブルに categoryId カラム追加
            migrationBuilder.AddColumn<Guid>(
                name: "categoryId",
                table: "Items",
                type: "uniqueidentifier",
                nullable: true);

            // Items テーブルに外部キー制約追加
            migrationBuilder.CreateIndex(
                name: "IX_Items_CategoryId",
                table: "Items",
                column: "categoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Categories_CategoryId",
                table: "Items",
                column: "categoryId",
                principalTable: "Categories",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            // 未分類カテゴリーを初期挿入
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "id", "name", "normalizedName", "icon", "color", "isReserved", "createdAt", "updatedAt" },
                values: new object[] {
                    new Guid("550e8400-e29b-41d4-a716-446655440000"),
                    "未分類",
                    "未分類",
                    null,
                    null,
                    true,
                    new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 外部キー制約削除
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Categories_CategoryId",
                table: "Items");

            // インデックス削除
            migrationBuilder.DropIndex(
                name: "IX_Items_CategoryId",
                table: "Items");

            // categoryId カラム削除
            migrationBuilder.DropColumn(
                name: "categoryId",
                table: "Items");

            // Categories テーブル削除
            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
