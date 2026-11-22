# Sistema de Moeda Estudantil

Protótipo da disciplina Projeto de Software. A entrega atual é a sprint 2 do Laboratório 4: diagrama de implantação e publicação na nuvem.

## O que esta sprint entrega

- Diagrama de implantação em `Diagramas/16-diagrama-de-implantacao.drawio`, com a imagem ao lado.
- Contêiner Docker da aplicação e o serviço descrito em `render.yaml`.
- A interface Razor e o backend MVC são o mesmo processo. O nó de execução na nuvem é um único serviço web no Render. A Vercel não executa ASP.NET Core; ela entraria no diagrama se a interface fosse um site estático separado.

## Como executar

É preciso o SDK do .NET 9. Na pasta do repositório:

```powershell
dotnet run --project src/MoedaEstudantil
```

Se esse `dotnet` não enxergar o SDK, use o que foi instalado na pasta do usuário:

```powershell
& "$env:USERPROFILE\.dotnet\dotnet.exe" run --project src/MoedaEstudantil
```

A aplicação abre em `http://localhost:5208`. O banco SQLite fica em `src/MoedaEstudantil/dados/moeda-estudantil.db` e não entra no Git. Na primeira execução o sistema aplica as migrações e cadastra três instituições, com CNPJ apenas ilustrativo:

- Pontifícia Universidade Católica de Minas Gerais
- Universidade Federal de Minas Gerais
- Universidade Federal de Ouro Preto

Também entram quatro professores de demonstração, com a senha `professor123`:

- marina.couto@pucminas.br
- ricardo.lacerda@pucminas.br
- helena.prado@ufmg.br
- tiago.moreira@ufop.edu.br

Alunos e empresas parceiras se cadastram pela tela inicial. Sem servidor SMTP, o e-mail de moedas e de cupom fica em `src/MoedaEstudantil/dados/emails` e também aparece nas notificações da tela.

Para rodar os testes:

```powershell
dotnet test
```

Para subir o mesmo contêiner da nuvem na máquina local:

```powershell
docker build -t moeda-estudantil .
docker run --rm -p 8080:8080 moeda-estudantil
```

A aplicação fica em `http://localhost:8080`. A verificação usada pelo Render é `http://localhost:8080/saude`.

## Como publicar no Render

1. Envie este repositório para o GitHub.
2. No Render, crie um Blueprint apontando para o repositório. O `render.yaml` define o serviço `moeda-estudantil`, a porta `8080` e a verificação em `/saude`.
3. No plano gratuito o disco do contêiner é recriado a cada publicação. Na subida, as migrações rodam de novo e as instituições e os professores de demonstração são cadastrados outra vez.
4. Para conservar banco, fotos e e-mails entre publicações, anexe um disco em `/var/dados` e defina:

```text
ConnectionStrings__Padrao=Data Source=/var/dados/moeda-estudantil.db
Fotos__Pasta=/var/dados/fotos
Email__PastaSaida=/var/dados/emails
```

Para enviar e-mail de verdade, defina `Email__Host`, `Email__Porta`, `Email__Usuario` e `Email__Senha`. Sem host, o sistema grava o `.eml` na pasta de saída.

## Arquitetura

O código segue o diagrama de componentes, em MVC. O diagrama de implantação mostra esse mesmo artefato dentro do contêiner.

| Camada | Onde está | Responsabilidade |
| --- | --- | --- |
| View | `Views` | Telas de login, aluno, professor e empresa |
| Controller | `Controllers` | Recebe o formulário e devolve a tela |
| Serviço | `Servicos` | Cadastro, moedas, vantagens, resgate e extrato |
| Persistência | `Persistencia` | ORM, repositórios e migração |
| Externos | `Externos` | Fotos em disco e envio de e-mail |

## Estratégia de acesso ao banco

O acesso não fica espalhado nos controllers. Há duas peças:

1. **ORM — Entity Framework Core.** As classes de `Dominio` são mapeadas para as tabelas do modelo ER em `Persistencia/ConfiguracaoModelo.cs`. `Usuario`, `Aluno`, `Professor` e `EmpresaParceira` usam uma tabela por tipo: a tabela `usuario` guarda e-mail, senha e tipo; `aluno` e `empresa_parceira` usam `usuario_id` como chave, como no diagrama. A senha é gravada com PBKDF2, nunca em texto puro.
2. **DAO — repositório e unidade de trabalho.** Os repositórios em `Persistencia/Repositorios` são o único caminho de leitura e escrita. `MoedaEstudantilContext` implementa `IUnidadeDeTrabalho` e só o serviço chama `SalvarAsync`, para cada operação ir em uma única transação.

O banco da nuvem continua sendo o SQLite do modelo ER. O arquivo da migração está em `src/MoedaEstudantil/Migrations`.
