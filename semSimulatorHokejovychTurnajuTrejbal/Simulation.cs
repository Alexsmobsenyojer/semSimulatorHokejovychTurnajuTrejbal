using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace semSimulatorHokejovychTurnajuTrejbal {
    public class GoalEvent {
        public Skater Scorer { get;}
        public Skater? Assist1 { get;}
        public Skater? Assist2 { get;}
        public int NewScore { get;}
        public bool IsHomeGoal { get;}
        public GoalEvent(Skater scorer, Skater? assist1, Skater? assist2, int newScore,  bool isHomeGoal) {
            Scorer = scorer;
            Assist1 = assist1;
            Assist2 = assist2;
            NewScore = newScore;
            IsHomeGoal = isHomeGoal;
        }
    }
    partial class Simulation{
        private readonly Match _match;
        private readonly Random _rand = new();
        private Team _homeTeam;
        private Team _awayTeam;
        private List<Player> _homePlayers;
        private List<Player> _awayPlayers;
        private List<Skater> homeCenters, homeLeftWingers, homeRightWingers, homeLeftDefensemen, homeRightDefensemen, 
            awayCenters, awayLeftWingers, awayRightWingers, awayLeftDefensemen, awayRightDefensemen;
        private Goalie homeGoalie, awayGoalie;
        private Skater homeC, homeLW, homeRW, homeLD, homeRD;
        private Skater awayC, awayLW, awayRW, awayLD, awayRD;

        public event Action<Match>? MatchUpdated;
        public event Action<GoalEvent>? GoalScored;
        public event Action<List<Player>>? HomeStatsUpdated;
        public event Action<List<Player>>? AwayStatsUpdated;
        public event Action<Player, double, double, Brush>? DrawPlayerRequested;
        public event Action? ClearCanvasRequested;

        public Simulation(Match match, IEnumerable<Player> allPlayers, IEnumerable<Team> allTeams) {
            _match = match;
            _homeTeam = allTeams.First(t => t.Id == match.HomeTeamId);
            _awayTeam = allTeams.First(t => t.Id == match.AwayTeamId);
            _homePlayers = _homeTeam.PlayerIds.Select(id => allPlayers.First(p => p.Id == id)).ToList();
            _awayPlayers = _awayTeam.PlayerIds.Select(id => allPlayers.First(p => p.Id == id)).ToList();
        }

        public void StartMatch() {
            homeCenters = PickSkaters(_homePlayers, Position.C, 4);
            homeLeftWingers = PickSkaters(_homePlayers, Position.LW, 4);
            homeRightWingers = PickSkaters(_homePlayers, Position.RW, 4);
            homeLeftDefensemen = PickSkaters(_homePlayers, Position.LD, 3);
            homeRightDefensemen = PickSkaters(_homePlayers, Position.RD, 3);
            homeGoalie = PickStartingGoalie(_homePlayers);

            awayCenters = PickSkaters(_awayPlayers, Position.C, 4);
            awayLeftWingers = PickSkaters(_awayPlayers, Position.LW, 4);
            awayRightWingers = PickSkaters(_awayPlayers, Position.RW, 4);
            awayLeftDefensemen = PickSkaters(_awayPlayers, Position.LD, 3);
            awayRightDefensemen = PickSkaters(_awayPlayers, Position.RD, 3);
            awayGoalie = PickStartingGoalie(_awayPlayers);

            ChangeLines();
            StartMatchUpdated();
        }

        public void SimulateMinute() {
            ChangeLines();
            SimulateEvents();
            RaiseStatsUpdated();
        }
        public void RefreshStats() {
            RaiseStatsUpdated();
        }
        private List<Skater> PickSkaters(List<Player> players, Position position, int count) {
            return players
                .OfType<Skater>()
                .Where(Skater => Skater.Position == position)
                .OrderByDescending(Skater => Skater.Overall)
                .Take(count)
                .ToList();
        }
        private Goalie PickStartingGoalie(List<Player> players) {
            var goalies = players
                .OfType<Goalie>()
                .OrderByDescending(g => g.Overall)
                .Take(2)
                .ToList();
            return goalies.Count == 1 ? goalies[0] : _rand.NextDouble() < 0.7 ? goalies[0] : goalies[1];
        }
        private void ChangeLines() {
            int fwd = _rand.Next(100) switch { < 40 => 0, < 70 => 1, < 90 => 2, _ => 3 };
            int def = _rand.Next(100) switch { < 50 => 0, < 80 => 1, _ => 2 };

            homeC = homeCenters[fwd]; homeLW = homeLeftWingers[fwd]; homeRW = homeRightWingers[fwd];
            homeLD = homeLeftDefensemen[def]; homeRD = homeRightDefensemen[def];

            awayC = awayCenters[fwd]; awayLW = awayLeftWingers[fwd]; awayRW = awayRightWingers[fwd];
            awayLD = awayLeftDefensemen[def]; awayRD = awayRightDefensemen[def];

            DrawPlayers();
        }

        private void DrawPlayers() {
            ClearCanvasRequested?.Invoke();

            Draw(homeGoalie, 200, 600, Brushes.Navy);
            Draw(awayGoalie, 200, 100, Brushes.Crimson);

            Draw(homeLW, 120, 420, Brushes.Navy);
            Draw(homeC, 200, 400, Brushes.Navy);
            Draw(homeRW, 280, 420, Brushes.Navy);
            Draw(homeLD, 140, 500, Brushes.Navy);
            Draw(homeRD, 260, 500, Brushes.Navy);

            Draw(awayLW, 120, 280, Brushes.Crimson);
            Draw(awayC, 200, 300, Brushes.Crimson);
            Draw(awayRW, 280, 280, Brushes.Crimson);
            Draw(awayLD, 140, 200, Brushes.Crimson);
            Draw(awayRD, 260, 200, Brushes.Crimson);
        }

        private void Draw(Player p, double x, double y, Brush c) => DrawPlayerRequested?.Invoke(p, x, y, c);

        private void SimulateEvents() {
            //TODO
            awayC.Stats.AddGoal();
            awayLW.Stats.AddAssist();
            _match.AwayScore++;
            GoalScored?.Invoke(new GoalEvent( awayC, awayLW, null, _match.AwayScore, false));
        }

        private void StartMatchUpdated() => MatchUpdated?.Invoke(_match);
        private void RaiseStatsUpdated() {
            HomeStatsUpdated?.Invoke(_homePlayers);
            AwayStatsUpdated?.Invoke(_awayPlayers);
        }

    }
}
