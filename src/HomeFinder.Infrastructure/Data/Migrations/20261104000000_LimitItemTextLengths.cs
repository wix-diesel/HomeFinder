using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeFinder.Infrastructure.Data.Migrations;

[DbContext(typeof(ItemDbContext))]
[Migration("20261104000000_LimitItemTextLengths")]
public partial class LimitItemTextLengths : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 既存値の走査や切り捨てを行わず保持し、新しい書き込みには制約を適用する。
        migrationBuilder.Sql("ALTER TABLE [Items] WITH NOCHECK ADD CONSTRAINT [CK_Items_Description_Length] CHECK ([Description] IS NULL OR DATALENGTH([Description]) <= 8000);");
        migrationBuilder.Sql("ALTER TABLE [Items] WITH NOCHECK ADD CONSTRAINT [CK_Items_Note_Length] CHECK ([Note] IS NULL OR DATALENGTH([Note]) <= 8000);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(name: "CK_Items_Description_Length", table: "Items");
        migrationBuilder.DropCheckConstraint(name: "CK_Items_Note_Length", table: "Items");
    }
}
