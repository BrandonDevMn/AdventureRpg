using AdventureRpg.Data;
using AdventureRpg.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=adventure_rpg.db"));

builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IFishingService, FishingService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
