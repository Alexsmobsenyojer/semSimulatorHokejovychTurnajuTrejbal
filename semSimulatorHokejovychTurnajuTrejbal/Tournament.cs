using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace semSimulatorHokejovychTurnajuTrejbal {
    class Tournament {
        public int Id { get; set; }
        public required string Title { get; set; }
        public int MatchCount { get; set; }
        public required List<Team> Teams { get; set; }
    }
}
