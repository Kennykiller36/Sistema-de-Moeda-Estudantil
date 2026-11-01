using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoedaEstudantil.Persistencia;
using MoedaEstudantil.Persistencia.Repositorios;
using MoedaEstudantil.Servicos;

var builder = WebApplication.CreateBuilder(args);

var cultura = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

builder.Services.AddControllersWithViews(opcoes =>
    opcoes.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

var conexao = ConexaoSqlite.Resolver(
    builder.Configuration.GetConnectionString("Padrao")!,
    builder.Environment.ContentRootPath);
builder.Services.AddDbContext<MoedaEstudantilContext>(opcoes => opcoes.UseSqlite(conexao));
builder.Services.AddScoped<IUnidadeDeTrabalho>(provedor => provedor.GetRequiredService<MoedaEstudantilContext>());
builder.Services.AddScoped<IAlunoRepositorio, AlunoRepositorio>();
builder.Services.AddScoped<IEmpresaParceiraRepositorio, EmpresaParceiraRepositorio>();
builder.Services.AddScoped<IInstituicaoRepositorio, InstituicaoRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<AlunoServico>();
builder.Services.AddScoped<EmpresaServico>();
builder.Services.AddScoped<AutenticacaoServico>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opcoes =>
    {
        opcoes.LoginPath = "/entrar";
        opcoes.AccessDeniedPath = "/entrar";
        opcoes.Cookie.Name = "moeda.estudantil";
        opcoes.SlidingExpiration = true;
        opcoes.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();

var app = builder.Build();

await InicializadorBanco.AplicarAsync(app.Services);

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = [cultura],
    SupportedUICultures = [cultura]
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
