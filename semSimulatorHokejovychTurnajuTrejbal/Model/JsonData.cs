using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace semSimulatorHokejovychTurnajuTrejbal {
    public class JsonData {
        public List<Player> Players { get; set; } = new();
        public List<Team> Teams { get; set; } = new();
        public List<Tournament> Tournaments { get; set; } = new();
    }
}
