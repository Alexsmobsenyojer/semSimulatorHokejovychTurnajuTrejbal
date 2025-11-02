using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace semSimulatorHokejovychTurnajuTrejbal {
    partial class Simulation {
        // jen match misto main
        private readonly MainWindow _main;
        private readonly Random _rand = new();
        public Simulation(MainWindow main) {
            _main = main;
        }
        private void DrawPlayers() {
        }
        private void DrawGoalie(Goalie startingGoalie) { 
            
        }
        public void SimulateMinute() { 
            DrawPlayers();
        }
        public void StartMatch() {
            var centers = _main.match.HomeTeam.Players
                .OfType<Skater>()
                .Where(s => s.Position == Position.C)
                .OrderByDescending(s => s.Overall)
                .Take(4)
                .ToList();
            var leftWingers = _main.match.HomeTeam.Players
                .OfType<Skater>()
                .Where(s => s.Position == Position.LW)
                .OrderByDescending(s => s.Overall)
                .Take(4)
                .ToList();
            var rightWingers = _main.match.HomeTeam.Players
                .OfType<Skater>()
                .Where(s => s.Position == Position.RW)
                .OrderByDescending(s => s.Overall)
                .Take(4)
                .ToList();
            var leftDefensemen = _main.match.HomeTeam.Players
                .OfType<Skater>()
                .Where(s => s.Position == Position.LD)
                .OrderByDescending(s => s.Overall)
                .Take(3)
                .ToList();
            var rightDefensemen = _main.match.HomeTeam.Players
                .OfType<Skater>()
                .Where(s => s.Position == Position.RD)
                .OrderByDescending(s => s.Overall)
                .Take(3)
                .ToList();
            var goalies = _main.match.HomeTeam.Players
                .OfType<Goalie>()
                .OrderByDescending(g => g.Overall)
                .Take(2)
                .ToList();
            var startingGoalie = goalies.Count == 1? goalies[0]: _rand.NextDouble() < 0.7 ? goalies[0] : goalies[1];
            DrawGoalie(startingGoalie);
        }
    }
}
