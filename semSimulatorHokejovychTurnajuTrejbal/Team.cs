using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace semSimulatorHokejovychTurnajuTrejbal {
    public class Team {
        public int Id { get; init; }
        public required string Name { get; set; }
        public int Wins { get; private set; } = 0;
        public int Losses { get; private set; } = 0;
        public required List<Player> Players { get; set; }

        public void AddWin() {
            Wins++;
        }
        public void AddLoss() {
            Losses++;
        }
    }
}
