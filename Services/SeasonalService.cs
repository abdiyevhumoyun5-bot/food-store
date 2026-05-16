using Jahongir_diplomIshi.Models;

namespace Jahongir_diplomIshi.Services
{
    /// <summary>
    /// Hozirgi faslni avtomatik aniqlovchi servis.
    /// Bahor: 3-5 oy | Yoz: 6-8 oy | Kuz: 9-11 oy | Qish: 12-2 oy
    /// </summary>
    public class SeasonalService
    {
        public Season GetCurrentSeason()
        {
            int month = DateTime.Now.Month;
            return month switch
            {
                3 or 4 or 5   => Season.Spring,
                6 or 7 or 8   => Season.Summer,
                9 or 10 or 11 => Season.Autumn,
                _             => Season.Winter   // 12, 1, 2
            };
        }

        public string GetSeasonName(Season season) => season switch
        {
            Season.Spring => "Bahor",
            Season.Summer => "Yoz",
            Season.Autumn => "Kuz",
            Season.Winter => "Qish",
            _ => "Bahor"
        };

        public string GetSeasonEmoji(Season season) => season switch
        {
            Season.Spring => "🌸",
            Season.Summer => "☀️",
            Season.Autumn => "🍂",
            Season.Winter => "❄️",
            _ => "🌸"
        };

        public string GetCurrentSeasonName() => GetSeasonName(GetCurrentSeason());
        public string GetCurrentSeasonEmoji() => GetSeasonEmoji(GetCurrentSeason());
    }
}
