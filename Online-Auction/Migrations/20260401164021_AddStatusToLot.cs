using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Online_Auction.Migrations
{
    public partial class AddStatusToLot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Добавляем колонку Status в таблицу Lots
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "lots",
                type: "integer",
                nullable: false,
                defaultValue: 1); 
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // При откате миграции удаляем колонку Status
            migrationBuilder.DropColumn(
                name: "Status",
                table: "lots");
        }
    }
}