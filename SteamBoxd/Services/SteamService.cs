using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using SteamBoxd.Models;

namespace SteamBoxd.Services
{
    public class SteamService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _steamId;

        public SteamService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["Steam:ApiKey"]!; // Null-forgiving!
            _steamId = config["Steam:SteamId"]!; // Also Null-forgiving!
        }

        public async Task<List<Game>> FetchOwnedGames()
        {
            string url = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={_apiKey}&steamid={_steamId}&include_appinfo=true&format=json";
            var response = await _httpClient.GetStringAsync(url);

            using JsonDocument doc = JsonDocument.Parse(response);

            var games = new List<Game>();

            if (!doc.RootElement.TryGetProperty("response", out var responseElement) ||
                !responseElement.TryGetProperty("games", out var gamesElement))
            {
                return games; // no games found
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