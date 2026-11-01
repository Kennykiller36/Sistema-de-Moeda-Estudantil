using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoedaEstudantil.Migrations
{
    /// <inheritdoc />
    public partial class EsquemaInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "instituicao",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nome = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    cnpj = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    cidade = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instituicao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    email = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    senha = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "aluno",
                columns: table => new
                {
                    usuario_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nome = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    cpf = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false),
                    rg = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    endereco = table.Column<string>(type: "TEXT", maxLength: 240, nullable: false),
                    curso = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    saldo = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    instituicao_id = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aluno", x => x.usuario_id);
                    table.CheckConstraint("ck_aluno_saldo", "saldo >= 0");
                    table.ForeignKey(
                        name: "FK_aluno_instituicao_instituicao_id",
                        column: x => x.instituicao_id,
                        principalTable: "instituicao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_aluno_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "empresa_parceira",
                columns: table => new
                {
                    usuario_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nome_fantasia = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    cnpj = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresa_parceira", x => x.usuario_id);
                    table.ForeignKey(
                        name: "FK_empresa_parceira_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "professor",
                columns: table => new
                {
                    usuario_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nome = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    cpf = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false),
                    departamento = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    saldo = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    instituicao_id = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professor", x => x.usuario_id);
                    table.CheckConstraint("ck_professor_saldo", "saldo >= 0");
                    table.ForeignKey(
                        name: "FK_professor_instituicao_instituicao_id",
                        column: x => x.instituicao_id,
                        principalTable: "instituicao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_professor_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vantagem",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    foto = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    custo_moedas = table.Column<int>(type: "INTEGER", nullable: false),
                    ativa = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    empresa_id = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vantagem", x => x.id);
                    table.CheckConstraint("ck_vantagem_custo", "custo_moedas > 0");
                    table.ForeignKey(
                        name: "FK_vantagem_empresa_parceira_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa_parceira",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "credito_semestral",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    professor_id = table.Column<long>(type: "INTEGER", nullable: false),
                    semestre = table.Column<string>(type: "TEXT", maxLength: 6, nullable: false),
                    quantidade = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1000)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credito_semestral", x => x.id);
                    table.CheckConstraint("ck_credito_quantidade", "quantidade = 1000");
                    table.ForeignKey(
                        name: "FK_credito_semestral_professor_professor_id",
                        column: x => x.professor_id,
                        principalTable: "professor",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    aluno_id = table.Column<long>(type: "INTEGER", nullable: false),
                    professor_id = table.Column<long>(type: "INTEGER", nullable: true),
                    vantagem_id = table.Column<long>(type: "INTEGER", nullable: true),
                    tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    motivo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    data_hora = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transacao", x => x.id);
                    table.CheckConstraint("ck_transacao_quantidade", "quantidade > 0");
                    table.ForeignKey(
                        name: "FK_transacao_aluno_aluno_id",
                        column: x => x.aluno_id,
                        principalTable: "aluno",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transacao_professor_professor_id",
                        column: x => x.professor_id,
                        principalTable: "professor",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transacao_vantagem_vantagem_id",
                        column: x => x.vantagem_id,
                        principalTable: "vantagem",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cupom",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    codigo = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    transacao_id = table.Column<long>(type: "INTEGER", nullable: false),
                    aluno_id = table.Column<long>(type: "INTEGER", nullable: false),
                    vantagem_id = table.Column<long>(type: "INTEGER", nullable: false),
                    empresa_id = table.Column<long>(type: "INTEGER", nullable: false),
                    utilizado = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    data_utilizacao = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cupom", x => x.id);
                    table.ForeignKey(
                        name: "FK_cupom_aluno_aluno_id",
                        column: x => x.aluno_id,
                        principalTable: "aluno",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cupom_empresa_parceira_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa_parceira",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cupom_transacao_transacao_id",
                        column: x => x.transacao_id,
                        principalTable: "transacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cupom_vantagem_vantagem_id",
                        column: x => x.vantagem_id,
                        principalTable: "vantagem",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notificacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    transacao_id = table.Column<long>(type: "INTEGER", nullable: false),
                    destinatario = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    data_envio = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificacao", x => x.id);
                    table.ForeignKey(
                        name: "FK_notificacao_transacao_transacao_id",
                        column: x => x.transacao_id,
                        principalTable: "transacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aluno_cpf",
                table: "aluno",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_aluno_instituicao_id",
                table: "aluno",
                column: "instituicao_id");

            migrationBuilder.CreateIndex(
                name: "IX_credito_semestral_professor_id_semestre",
                table: "credito_semestral",
                columns: new[] { "professor_id", "semestre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cupom_aluno_id",
                table: "cupom",
                column: "aluno_id");

            migrationBuilder.CreateIndex(
                name: "IX_cupom_codigo",
                table: "cupom",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cupom_empresa_id",
                table: "cupom",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "IX_cupom_transacao_id",
                table: "cupom",
                column: "transacao_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cupom_vantagem_id",
                table: "cupom",
                column: "vantagem_id");

            migrationBuilder.CreateIndex(
                name: "IX_empresa_parceira_cnpj",
                table: "empresa_parceira",
                column: "cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_instituicao_cnpj",
                table: "instituicao",
                column: "cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notificacao_transacao_id",
                table: "notificacao",
                column: "transacao_id");

            migrationBuilder.CreateIndex(
                name: "IX_professor_cpf",
                table: "professor",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professor_instituicao_id",
                table: "professor",
                column: "instituicao_id");

            migrationBuilder.CreateIndex(
                name: "IX_transacao_aluno_id",
                table: "transacao",
                column: "aluno_id");

            migrationBuilder.CreateIndex(
                name: "IX_transacao_professor_id",
                table: "transacao",
                column: "professor_id");

            migrationBuilder.CreateIndex(
                name: "IX_transacao_vantagem_id",
                table: "transacao",
                column: "vantagem_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_email",
                table: "usuario",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vantagem_empresa_id",
                table: "vantagem",
                column: "empresa_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credito_semestral");

            migrationBuilder.DropTable(
                name: "cupom");

            migrationBuilder.DropTable(
                name: "notificacao");

            migrationBuilder.DropTable(
                name: "transacao");

            migrationBuilder.DropTable(
                name: "aluno");

            migrationBuilder.DropTable(
                name: "professor");

            migrationBuilder.DropTable(
                name: "vantagem");

            migrationBuilder.DropTable(
                name: "instituicao");

            migrationBuilder.DropTable(
                name: "empresa_parceira");

            migrationBuilder.DropTable(
                name: "usuario");
        }
    }
}
