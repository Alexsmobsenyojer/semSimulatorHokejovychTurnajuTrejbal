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

    public class ShotEvent {
        public Skater Shooter { get;}
        public bool IsHome { get;}
        public int NewShotCount { get;}
        public ShotEvent(Skater shooter, Goalie goalie, bool isHome, int newShots) {
            Shooter = shooter;
            IsHome = isHome;
            shooter.Stats.AddShot();
            goalie.Stats.AddSave();
            NewShotCount = newShots;
        }
    }
    public class GoalEvent {
        public Skater Scorer { get;}
        public Skater? Assist1 { get;}
        public Skater? Assist2 { get;}
        public int NewScore { get;}
        public bool IsHomeGoal { get;}
        public GoalEvent(Skater scorer, Goalie goalie, Skater? assist1, Skater? assist2, int newScore,  bool isHomeGoal) {
            Scorer = scorer;
            Assist1 = assist1;
            Assist2 = assist2;
            NewScore = newScore;
            IsHomeGoal = isHomeGoal;
            scorer.Stats.AddGoal();
            goalie.Stats.AddGoalAgainst();
            if (assist1 != null) assist1.Stats.AddAssist();
            if (assist2 != null) assist2.Stats.AddAssist();
        }
    }
    partial class Simulation {
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
        public event Action<ShotEvent>? ShotAttempted;
        public event Action<GoalEvent>? GoalScored;
        public event Action<List<Player>>? HomeStatsUpdated;
        public event Action<List<Player>>? AwayStatsUpdated;
        public event Action<Player, double, double, Brush>? DrawPlayerRequested;
        public event Action? ClearCanvasRequested;

        public Simulation(Match match, IEnumerable<Player> allPlayers, IEnumerable<Team> allTeams) {
            _match = match;
            _homeTeam = allTeams.First(t => t.Id == match.HomeTeamId);
            _awayTeam = allTeams.First(t => t.Id == match.AwayTeamId);
            _homePlayers = allPlayers.Where(p => p.Team!.Id == _homeTeam.Id).ToList();
            _awayPlayers = allPlayers.Where(p => p.Team!.Id == _awayTeam.Id).ToList();
        }

        public void StartMatch() {
            ValidateTeam(_homePlayers, _homeTeam.Name);
            ValidateTeam(_awayPlayers, _awayTeam.Name);

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
        public void recordOutcome() {
            if (_match.HomeScore > _match.AwayScore) {
                _homeTeam.AddWin();
                _awayTeam.AddLoss();
            }
            if (_match.AwayScore > _match.HomeScore) {
                _homeTeam.AddLoss();
                _awayTeam.AddWin();
            }
        }

        private void ValidateTeam(List<Player> players, string teamName) {
            var skaters = players.OfType<Skater>().ToList();
            var goalies = players.OfType<Goalie>().Count();

            if (goalies < 1)
                throw new InvalidOperationException($"{teamName} nemá žádného brankáře – zápas nelze spustit.");
            if (skaters.Count(s => s.Position == Position.C) < 4)
                throw new InvalidOperationException($"{teamName} má méně než 4 centry – zápas nelze spustit.");
            if (skaters.Count(s => s.Position == Position.LW) < 4)
                throw new InvalidOperationException($"{teamName} má méně než 4 levá křídla – zápas nelze spustit.");
            if (skaters.Count(s => s.Position == Position.RW) < 4)
                throw new InvalidOperationException($"{teamName} má méně než 4 pravá křídla – zápas nelze spustit.");
            if (skaters.Count(s => s.Position == Position.LD) < 3)
                throw new InvalidOperationException($"{teamName} má méně než 3 levé obránce – zápas nelze spustit.");
            if (skaters.Count(s => s.Position == Position.RD) < 3)
                throw new InvalidOperationException($"{teamName} má méně než 3 pravé obránce – zápas nelze spustit.");
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
            // Složení hráčů na ledě
            var homeOnIce = new List<Skater> { homeLW, homeC, homeRW, homeLD, homeRD };
            var awayOnIce = new List<Skater> { awayLW, awayC, awayRW, awayLD, awayRD };

            // Výpočet parametrů pro počet střel
            double homeSkating = (homeLW.Skating + homeC.Skating + homeRW.Skating) / 3.0;
            double awaySkating = (awayLW.Skating + awayC.Skating + awayRW.Skating) / 3.0;
            double homeDefense = (homeLD.Defending + homeRD.Defending) / 2.0;
            double awayDefense = (awayLD.Defending + awayRD.Defending) / 2.0;
            double homeExpected = (0.5 + (homeSkating - awayDefense) / 50.0);
            double awayExpected = (0.5 + (awaySkating - homeDefense) / 50.0);

            // Náhodné rozložení kolem očekávané hodnoty
            int homeAttempts = Math.Max(0, (int)Math.Round(homeExpected + (_rand.NextDouble() - 0.5)));
            int awayAttempts = Math.Max(0, (int)Math.Round(awayExpected + (_rand.NextDouble() - 0.5)));

            // Pomocné lokální funkce pro výběr
            Skater PickGoalScorer(List<Skater> list) {
                var weights = list.Select(s => Math.Max(1.0, s.Shooting + 0.25 * s.Skating)).ToArray();
                double total = weights.Sum();
                double r = _rand.NextDouble() * total;
                double cum = 0;
                for (int i = 0; i < list.Count; i++) {
                    cum += weights[i];
                    if (r <= cum) return list[i];
                }
                return list.Last();
            }

            Skater PickAssister(List<Skater> list, Skater exclude, Skater exclude2 = null) {
                var candidates = list.Where(p => p != exclude && p != exclude2).ToList();
                var weights = candidates.Select(s => Math.Max(1.0, 1.5 * s.Passing + 0.5 * s.Overall)).ToArray();
                double total = weights.Sum();
                double r = _rand.NextDouble() * total;
                double cum = 0;
                for (int i = 0; i < candidates.Count; i++) {
                    cum += weights[i];
                    if (r <= cum) return candidates[i];
                }
                return candidates.Last();
            }

            // Simulace pro jednu sadu pokusů týmů
            void SimulateTeamAttempts(int attempts, List<Skater> attackers, Goalie defendingGoalie, bool isHome) {
                for (int i = 0; i < attempts; i++) {
                    var shooter = PickGoalScorer(attackers);
                    // rozhodnutí střela vs přihrávka
                    double shootProb = shooter.Shooting + 0.0001;
                    double passProb = shooter.Passing + 0.0001;
                    bool willShoot = _rand.NextDouble() < (shootProb / (shootProb + passProb));

                    // změna ze střely -> přihrávku
                    if (!willShoot) {
                        // přihrávku provede střelec, ale střelcem na zakončení se stává asistent vybraný váhově
                        var assister = PickAssister(attackers, shooter);
                        shooter = assister;
                    }

                    // střely
                    if (isHome) _match.HomeShots++;
                    else _match.AwayShots++;
                    int newShotCount = isHome ? _match.HomeShots : _match.AwayShots;
                    ShotAttempted?.Invoke(new ShotEvent(shooter, defendingGoalie, isHome, newShotCount));
                    // Výpočet šance na gól: když je hodnocení střelcovi střeli a brankáře stejné -> 10%
                    double chance;
                    if (shooter.Shooting == defendingGoalie.Overall) {
                        chance = 0.10;
                    } else {
                        chance = 0.10 + (shooter.Shooting - defendingGoalie.Overall) * 0.01;
                    }

                    // Omezení do intervalu
                    chance = Math.Max(0.01, Math.Min(chance, 0.8));

                    if (_rand.NextDouble() < chance) {
                        // Gól padl
                        if (isHome) {
                            _match.HomeScore++;
                        } else {
                            _match.AwayScore++;
                        }
                        // Výběr asistencí: první 90%, druhá 80% z první
                        Skater? assist1 = null;
                        Skater? assist2 = null;
                        if (_rand.NextDouble() < 0.90) {
                            assist1 = PickAssister(attackers, shooter);
                            if (_rand.NextDouble() < 0.80) {
                                assist2 = PickAssister(attackers, shooter, assist1);
                            }
                        }
                        int newScore = isHome ? _match.HomeScore : _match.AwayScore;
                        GoalScored?.Invoke(new GoalEvent(shooter, defendingGoalie, assist1, assist2, newScore, isHome));
                    }
                }
            }

            SimulateTeamAttempts(homeAttempts, homeOnIce, awayGoalie, isHome: true);
            SimulateTeamAttempts(awayAttempts, awayOnIce, homeGoalie, isHome: false);
        }
        private void StartMatchUpdated() => MatchUpdated?.Invoke(_match);
        private void RaiseStatsUpdated() {
            HomeStatsUpdated?.Invoke(_homePlayers);
            AwayStatsUpdated?.Invoke(_awayPlayers);
        }

    }
}
