# Sistema de Moeda Estudantil

Protótipo da disciplina Projeto de Software (Lab03), sprint 2.

## O que esta sprint entrega

- O modelo relacional do diagrama `Diagramas/04-modelo-er` criado no banco.
- Acesso aos dados com ORM (Entity Framework Core) e DAO (repositórios + unidade de trabalho).
- CRUD inicial de aluno e de empresa parceira: telas, validação e gravação no banco.
- Login criado no cadastro. Cada pessoa altera e exclui só o próprio registro.

Envio de moedas, vantagens, extrato, cupom e e-mail ficam para a sprint 3. As tabelas dessas partes já existem, alinhadas ao modelo ER, para a próxima entrega continuar em cima do mesmo banco.

## Como executar

É preciso o SDK do .NET 9. Na pasta do repositório:

```powershell
dotnet run --project src/MoedaEstudantil
```

Se esse `dotnet` não enxergar o SDK, use o que foi instalado na pasta do usuário:

```powershell
& "$env:USERPROFILE\.dotnet\dotnet.exe" run --project src/MoedaEstudantil
```

A aplicação abre em `http://localhost:5208`. O banco SQLite fica em `src/MoedaEstudantil/dados/moeda-estudantil.db` e não entra no Git. Na primeira execução o sistema aplica a migração e cadastra três instituições, com CNPJ apenas ilustrativo:

- Pontifícia Universidade Católica de Minas Gerais
- Universidade Federal de Minas Gerais
- Universidade Federal de Ouro Preto

Para rodar os testes:

```powershell
dotnet test
```

## Arquitetura

O código segue o diagrama de componentes, em MVC:

| Camada | Onde está | Responsabilidade nesta sprint |
| --- | --- | --- |
| View | `Views` | Telas de login, aluno e empresa |
| Controller | `Controllers` | Recebe o formulário e devolve a tela |
| Serviço | `Servicos` | Regras de cadastro, unicidade e senha |
| Persistência | `Persistencia` | ORM, repositórios e migração |

## Estratégia de acesso ao banco

O acesso não fica espalhado nos controllers. Há duas peças:

1. **ORM — Entity Framework Core.** As classes de `Dominio` são mapeadas para as tabelas do modelo ER em `Persistencia/ConfiguracaoModelo.cs`. `Usuario`, `Aluno`, `Professor` e `EmpresaParceira` usam uma tabela por tipo: a tabela `usuario` guarda e-mail, senha e tipo; `aluno` e `empresa_parceira` usam `usuario_id` como chave, como no diagrama. A senha é gravada com PBKDF2, nunca em texto puro.
2. **DAO — repositório e unidade de trabalho.** `AlunoRepositorio`, `EmpresaParceiraRepositorio`, `InstituicaoRepositorio` e `UsuarioRepositorio` são o único caminho de leitura e escrita. `MoedaEstudantilContext` implementa `IUnidadeDeTrabalho` e só o serviço chama `SalvarAsync`, para o cadastro inteiro (usuário + aluno, ou usuário + empresa) ir em uma única transação.

O arquivo da migração inicial está em `src/MoedaEstudantil/Migrations`.
