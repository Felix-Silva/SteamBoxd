namespace SteamBoxd.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string SteamAppId { get; set; } = null!;  // required, non-nullable with null-forgiving operator
        public string Name { get; set; } = null!;

        // nullable URLs since they might not be present
        public string? IconUrl { get; set; }
        public string? LogoUrl { get; set; }

        // playtime in minutes (total and last two weeks)
        public int PlaytimeMinutes { get; set; }
        public int PlaytimeMinutesTwoWeeks { get; set; }

        public string HeaderImageUrl { get; set; } = null!;

        // Foreign key to Account
        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;
    }

    public class Account
    {
        public int Id { get; set; }
        public string SteamId { get; set; } = string.Empty; // Unique SteamID 64
        public string Username { get; set; } = string.Empty;
        public List<Game> Games { get; set; } = new();
    }
}
