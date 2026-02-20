using BankMore.Fees.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "BankMore.Fees.Api")
        .WriteTo.Console();
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "BankMore Fees API", Version = "v1" }));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "BankMore.Fees" }))
    .WithSummary("Health check")
    .WithDescription("Verifica se o serviço Fees está saudável.");
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
