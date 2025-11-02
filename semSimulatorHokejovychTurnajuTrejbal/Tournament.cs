using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace semSimulatorHokejovychTurnajuTrejbal {
    public class Tournament {
        public int Id { get; init; }
        public required string Title { get; set; }
        public required List<Team> Teams { get; set; }
        public List<Match> Matches { get; set; } = new();
    }
    public class Match {
        public int Id { get; init; }
        public Team HomeTeam { get; init; }
        public Team AwayTeam { get; init; }
        public int HomeScore { get; set; } = 0;
        public int AwayScore { get; set; } = 0;
        public int HomeShots { get; set; } = 0;
        public int AwayShots { get; set; } = 0;
        public bool wasPlayed { get; set; } = false;

        public override string ToString() => $"{HomeTeam.Name} vs {AwayTeam.Name}";
    }
}
