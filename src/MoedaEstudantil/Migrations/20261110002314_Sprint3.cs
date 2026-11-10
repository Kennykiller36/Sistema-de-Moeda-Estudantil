using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoedaEstudantil.Migrations
{
    /// <inheritdoc />
    public partial class Sprint3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "assunto",
                table: "notificacao",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "corpo",
                table: "notificacao",
                type: "TEXT",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "data_geracao",
                table: "cupom",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "data_aplicacao",
                table: "credito_semestral",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_notificacao_destinatario",
                table: "notificacao",
                column: "destinatario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_notificacao_destinatario",
                table: "notificacao");

            migrationBuilder.DropColumn(
                name: "assunto",
                table: "notificacao");

            migrationBuilder.DropColumn(
                name: "corpo",
                table: "notificacao");

            migrationBuilder.DropColumn(
                name: "data_geracao",
                table: "cupom");

            migrationBuilder.DropColumn(
                name: "data_aplicacao",
                table: "credito_semestral");
        }
    }
}
