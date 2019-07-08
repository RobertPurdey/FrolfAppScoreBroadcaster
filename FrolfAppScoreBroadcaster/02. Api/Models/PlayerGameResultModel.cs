using System.Collections.Generic;

namespace FrolfAppScoreBroadcaster._02._Api.Models
{
    public class PlayerGameResultModel
    {
        public string PlayerName { get; set; }
        public int TotalStrokes { get; set; }
        public int TotalScore { get; set; }
        public IDictionary<int, int> Scores { get; set; }
        public IDictionary<int, int> Strokes { get; set; }
    }
}
