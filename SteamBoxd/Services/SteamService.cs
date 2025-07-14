using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SteamBoxd.Models;
using SteamBoxd.Data;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace SteamBoxd.Services
{
    public class SteamService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _steamId;
        private readonly AppDbContext _db;

        public SteamService(IConfiguration config, AppDbContext db)
        {
            _httpClient = new HttpClient();
            _apiKey = config["Steam:ApiKey"]!;
            _steamId = config["Steam:SteamId"]!; // Hardcoded for now
            _db = db;
        }

        public async Task<Account> ImportAccountAndGames(string steamId)
        {
            var accountInfo = await GetAccountInfo(steamId);

            var account = await _db.Accounts.Include(a => a.Games)
                                           .FirstOrDefaultAsync(a => a.SteamId == steamId);

            if (account == null)
            {
                account = accountInfo;
                _db.Accounts.Add(account);
            }
            else
            {
                account.Username = accountInfo.Username;
            }

            var games = await GetOwnedGames(steamId);

            foreach (var game in games)
            {
                if (!account.Games.Any(g => g.SteamAppId == game.SteamAppId))
                {
                    game.AccountId = account.Id;
                    account.Games.Add(game);
                }
            }

            await _db.SaveChangesAsync();

            return account;
        }

        public async Task<Account> GetAccountInfo(string steamId)
        {
            string url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={_apiKey}&steamids={steamId}";
            var response = await _httpClient.GetStringAsync(url);

            using var doc = JsonDocument.Parse(response);
            var players = doc.RootElement.GetProperty("response").GetProperty("players");

            if (players.GetArrayLength() == 0)
                throw new System.Exception("No user found with given SteamID.");

            var player = players[0];
            var username = player.GetProperty("personaname").GetString() ?? "Unknown";

            return new Account
            {
                SteamId = steamId,
                Username = username,
                Games = new List<Game>()
            };
        }

        public async Task<List<Game>> GetOwnedGames(string steamId)
        {
            string url = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={_apiKey}&steamid={steamId}&include_appinfo=true&format=json";
            var response = await _httpClient.GetStringAsync(url);

            using JsonDocument doc = JsonDocument.Parse(response);
            var games = new List<Game>();

            if (!doc.RootElement.TryGetProperty("response", out var responseElement) ||
                !responseElement.TryGetProperty("games", out var gamesElement))
            {
                return games;
            }

            foreach (var g in gamesElement.EnumerateArray())
            {
                string appid = g.GetProperty("appid").GetInt32().ToString();

                string? iconHash = null;
                if (g.TryGetProperty("img_icon_url", out var iconProp))
                    iconHash = iconProp.GetString();

                string? logoHash = null;
                if (g.TryGetProperty("img_logo_url", out var logoProp))
                    logoHash = logoProp.GetString();

                int playtime2Weeks = 0;
                if (g.TryGetProperty("playtime_2weeks", out var playtime2Prop))
                    playtime2Weeks = playtime2Prop.GetInt32();

                games.Add(new Game
                {
                    SteamAppId = appid,
                    Name = g.GetProperty("name").GetString()!,
                    PlaytimeMinutesTwoWeeks = playtime2Weeks,
                    PlaytimeMinutes = g.GetProperty("playtime_forever").GetInt32(),
                    HeaderImageUrl = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appid}/header.jpg",
                    IconUrl = !string.IsNullOrEmpty(iconHash)
                        ? $"http://media.steampowered.com/steamcommunity/public/images/apps/{appid}/{iconHash}.jpg"
                        : null,
                    LogoUrl = !string.IsNullOrEmpty(logoHash)
                        ? $"http://media.steampowered.com/steamcommunity/public/images/apps/{appid}/{logoHash}.jpg"
                        : null
                });
            }

            return games;
        }
    }
}
