using System.Text.Json;
using API.Data;
using API.Interfaces;
using API.Middlewares;
using API.Repositories;
using API.Services;
using API.Websocket;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();

builder.Services.AddDbContext<DataContext>(options =>
{
    var config = builder.Configuration;
    var connectionString = config.GetConnectionString("TestConnection");
    options.UseSqlite(connectionString);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddSingleton<CryptoService>();
builder.Services.AddSingleton<CryptoPriceCache>();
builder.Services.AddSingleton<CryptoWsConnectionManager>();
builder.Services.AddSingleton<CryptoWsHandler>();

builder.Services.AddHostedService<CryptoWorker>();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer");

var app = builder.Build();

app.UseWebSockets();

var cryptoService = app.Services.GetRequiredService<CryptoService>();
var priceCache = app.Services.GetRequiredService<CryptoPriceCache>();
var wsManager = app.Services.GetRequiredService<CryptoWsConnectionManager>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    await db.Database.MigrateAsync();
}


cryptoService.PriceUpdated += async (_, dto) =>
{
    priceCache.Update(dto);

    var json = JsonSerializer.Serialize(new
    {
        type = "priceUpdated",
        data = dto
    });

    await wsManager.BroadcastAsync(json);
};

app.UseMiddleware<CryptoWebSocketMiddleware>();
app.UseMiddleware<ExceptionHandling>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/v1.json");
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Demo API V1");
    });
}

app.UseHttpsRedirection();

app.UseCors(options =>
{
    options
          .AllowAnyHeader()
          .AllowAnyMethod().WithOrigins("http://localhost:3000", "http://localhost:5173", "http://p4gko04oc4cg8g84sw0ksowg.138.68.111.206.sslip.io");
});


app.UseAuthorization();

app.MapControllers();

app.Run();
