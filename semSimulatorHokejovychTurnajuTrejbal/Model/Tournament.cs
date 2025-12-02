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
        public required List<int> TeamIds { get; set; }
        public List<Match> Matches { get; set; } = new();
    }
    public class Match {
        public int Id { get; init; }
        public int HomeTeamId { get; init; }
        public int AwayTeamId { get; init; }
        public string Title { get; init; }
        public int HomeScore { get; set; } = 0;
        public int AwayScore { get; set; } = 0;
        public int HomeShots { get; set; } = 0;
        public int AwayShots { get; set; } = 0;
        public bool WasPlayed { get; set; } = false;

        public override string ToString() => Title;
    }
}
