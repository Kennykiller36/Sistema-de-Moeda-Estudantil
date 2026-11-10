using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MoedaEstudantil.Controllers;
using MoedaEstudantil.Externos;
using MoedaEstudantil.Persistencia;
using MoedaEstudantil.Persistencia.Repositorios;
using MoedaEstudantil.Servicos;

var builder = WebApplication.CreateBuilder(args);

var cultura = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

builder.Services.AddControllersWithViews(opcoes =>
{
    opcoes.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    var mensagens = opcoes.ModelBindingMessageProvider;
    mensagens.SetValueMustNotBeNullAccessor(_ => "Preencha este campo.");
    mensagens.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"O valor \"{valor}\" não é válido para {campo}.");
    mensagens.SetUnknownValueIsInvalidAccessor(campo => $"Valor inválido para {campo}.");
    mensagens.SetValueIsInvalidAccessor(valor => $"O valor \"{valor}\" não é válido.");
    mensagens.SetValueMustBeANumberAccessor(campo => $"{campo} deve ser um número.");
});

var raiz = builder.Environment.ContentRootPath;
var conexao = ConexaoSqlite.Resolver(builder.Configuration.GetConnectionString("Padrao")!, raiz);
builder.Services.AddDbContext<MoedaEstudantilContext>(opcoes => opcoes.UseSqlite(conexao));
builder.Services.AddScoped<IUnidadeDeTrabalho>(provedor => provedor.GetRequiredService<MoedaEstudantilContext>());
builder.Services.AddScoped<IAlunoRepositorio, AlunoRepositorio>();
builder.Services.AddScoped<IProfessorRepositorio, ProfessorRepositorio>();
builder.Services.AddScoped<IEmpresaParceiraRepositorio, EmpresaParceiraRepositorio>();
builder.Services.AddScoped<IVantagemRepositorio, VantagemRepositorio>();
builder.Services.AddScoped<ITransacaoRepositorio, TransacaoRepositorio>();
builder.Services.AddScoped<ICupomRepositorio, CupomRepositorio>();
builder.Services.AddScoped<INotificacaoRepositorio, NotificacaoRepositorio>();
builder.Services.AddScoped<IInstituicaoRepositorio, InstituicaoRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<AutenticacaoServico>();
builder.Services.AddScoped<AlunoServico>();
builder.Services.AddScoped<EmpresaServico>();
builder.Services.AddScoped<ProfessorServico>();
builder.Services.AddScoped<MoedaServico>();
builder.Services.AddScoped<VantagemServico>();
builder.Services.AddScoped<ResgateServico>();
builder.Services.AddScoped<NotificacaoServico>();

builder.Services.Configure<OpcoesEmail>(builder.Configuration.GetSection("Email"));
builder.Services.PostConfigure<OpcoesEmail>(opcoes => opcoes.PastaSaida = Path.GetFullPath(opcoes.PastaSaida, raiz));
builder.Services.AddSingleton<IServicoEmail, ServicoEmailSmtp>();

var pastaFotos = Path.GetFullPath(builder.Configuration["Fotos:Pasta"] ?? "dados/fotos", raiz);
builder.Services.AddSingleton<IArmazenamentoFotos>(new ArmazenamentoFotosEmDisco(pastaFotos));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opcoes =>
    {
        opcoes.LoginPath = "/entrar";
        opcoes.AccessDeniedPath = "/acesso-negado";
        opcoes.Cookie.Name = "moeda.estudantil";
        opcoes.SlidingExpiration = true;
        opcoes.ExpireTimeSpan = TimeSpan.FromHours(8);
        opcoes.Events.OnValidatePrincipal = SessaoCookie.ValidarAsync;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

await InicializadorBanco.AplicarAsync(app.Services);
using (var escopo = app.Services.CreateScope())
    await escopo.ServiceProvider.GetRequiredService<MoedaServico>().CreditarSemestreDeTodosAsync();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = [cultura],
    SupportedUICultures = [cultura]
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(pastaFotos),
    RequestPath = ArmazenamentoFotosEmDisco.RotaPublica,
    OnPrepareResponse = contexto => contexto.Context.Response.Headers.XContentTypeOptions = "nosniff"
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
