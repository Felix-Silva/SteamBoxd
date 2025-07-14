using SteamBoxd.Services;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Create SteamService instance
var steamService = new SteamService(builder.Configuration);

var games = await steamService.FetchOwnedGames();

// TEST PRINTING

foreach (var g in games)
{
    Console.WriteLine(g.Name);
    Console.WriteLine("\t Playtime Last Two Weeks: " + g.PlaytimeMinutesTwoWeeks);
    Console.WriteLine("\t Playtime: " + g.PlaytimeMinutes);
    Console.WriteLine("\t Header Image URL: " + g.HeaderImageUrl);
    Console.WriteLine("\t Icon URL: " + g.IconUrl);
    Console.WriteLine("\t Logo URL: " + g.LogoUrl);
}
            
// END TEST PRINTING

app.Run();
