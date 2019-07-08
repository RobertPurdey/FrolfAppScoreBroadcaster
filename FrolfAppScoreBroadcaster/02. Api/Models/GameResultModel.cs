using System.Collections.Generic;

namespace FrolfAppScoreBroadcaster._02._Api.Models
{
    public class GameResultModel
    {
        public string CourseName { get; set; }
        public int CoursePar { get; set; }
        public int HoleCount { get; set; }
        public IDictionary<int, int> HolePars { get; set; }
        public IEnumerable<PlayerGameResultModel> PlayerResults { get; set; }
    }
}
