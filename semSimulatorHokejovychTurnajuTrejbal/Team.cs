using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace semSimulatorHokejovychTurnajuTrejbal {
    class Team {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public required List<Player> Players { get; set; }
    }
}
