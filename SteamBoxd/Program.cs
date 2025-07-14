using SteamBoxd.Services;
using Microsoft.EntityFrameworkCore;
using SteamBoxd.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=steamboxd.db"));

// Register SteamService as scoped so DI can inject dependencies
builder.Services.AddScoped<SteamService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var steamService = scope.ServiceProvider.GetRequiredService<SteamService>();

    string steamId = builder.Configuration["Steam:SteamId"]!; // Hardcoded for testing

    var account = await steamService.ImportAccountAndGames(steamId);

    foreach (var g in account.Games)
    {
        Console.WriteLine(g.Name);
        Console.WriteLine("\t Playtime Last Two Weeks: " + g.PlaytimeMinutesTwoWeeks);
        Console.WriteLine("\t Playtime: " + g.PlaytimeMinutes);
        Console.WriteLine("\t Header Image URL: " + g.HeaderImageUrl);
        Console.WriteLine("\t Icon URL: " + g.IconUrl);
        Console.WriteLine("\t Logo URL: " + g.LogoUrl);

        if (!db.Games.Any(existing => existing.SteamAppId == g.SteamAppId))
        {
            db.Games.Add(g);
        }
    }

    await db.SaveChangesAsync();
}

app.Run();