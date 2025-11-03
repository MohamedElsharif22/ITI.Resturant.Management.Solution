using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.Resturant.Management.Infrastructure._Data.Migrations
{
    /// <inheritdoc />
    public partial class Updated_orderEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First add a new temporary column for EstimatedDeliveryTime
            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedDeliveryTime_New",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            // Convert existing integer minutes to datetime
            migrationBuilder.Sql(@"
                UPDATE Orders 
                SET EstimatedDeliveryTime_New = DATEADD(MINUTE, 
                    CASE 
                        WHEN EstimatedDeliveryTime IS NOT NULL 
                        THEN CAST(EstimatedDeliveryTime as int) 
                        ELSE 0 
                    END, 
                    GETDATE())
                WHERE EstimatedDeliveryTime IS NOT NULL");

            // Drop the old column
            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryTime",
                table: "Orders");

            // Rename the new column to the original name
            migrationBuilder.RenameColumn(
                name: "EstimatedDeliveryTime_New",
                table: "Orders",
                newName: "EstimatedDeliveryTime");

            // Add DailyOrderCount to MenuItems if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE Name = 'DailyOrderCount'
                    AND Object_ID = Object_ID('MenuItems')
                )
                BEGIN
                    ALTER TABLE MenuItems
                    ADD DailyOrderCount int NOT NULL DEFAULT 0
                END");

            // Add LastResetTime to MenuItems if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE Name = 'LastResetTime'
                    AND Object_ID = Object_ID('MenuItems')
                )
                BEGIN
                    ALTER TABLE MenuItems
                    ADD LastResetTime datetime2 NOT NULL DEFAULT GETDATE()
                END");

            // Fix the PreparationTimeInMinutes column name if needed
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE Name = 'PreparationTimeInMinuts'
                    AND Object_ID = Object_ID('MenuItems')
                )
                BEGIN
                    EXEC sp_rename 'MenuItems.PreparationTimeInMinuts', 'PreparationTimeInMinutes', 'COLUMN'
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert estimated delivery time back to minutes
            migrationBuilder.AddColumn<int>(
                name: "EstimatedDeliveryTime_Old",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE Orders 
                SET EstimatedDeliveryTime_Old = 
                    CASE 
                        WHEN EstimatedDeliveryTime IS NOT NULL 
                        THEN DATEDIFF(MINUTE, GETDATE(), EstimatedDeliveryTime)
                        ELSE NULL 
                    END");

            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryTime",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "EstimatedDeliveryTime_Old",
                table: "Orders",
                newName: "EstimatedDeliveryTime");

            // Remove DailyOrderCount if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE Name = 'DailyOrderCount'
                    AND Object_ID = Object_ID('MenuItems')
                )
                BEGIN
                    ALTER TABLE MenuItems
                    DROP COLUMN DailyOrderCount
                END");

            // Remove LastResetTime if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE Name = 'LastResetTime'
                    AND Object_ID = Object_ID('MenuItems')
                )
                BEGIN
                    ALTER TABLE MenuItems
                    DROP COLUMN LastResetTime
                END");

            // Fix the PreparationTimeInMinutes column name back if needed
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE Name = 'PreparationTimeInMinutes'
                    AND Object_ID = Object_ID('MenuItems')
                )
                BEGIN
                    EXEC sp_rename 'MenuItems.PreparationTimeInMinutes', 'PreparationTimeInMinuts', 'COLUMN'
                END");
        }
    }
}
