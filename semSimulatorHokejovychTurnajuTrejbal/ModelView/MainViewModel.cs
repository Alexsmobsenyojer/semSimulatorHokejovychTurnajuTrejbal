using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using semSimulatorHokejovychTurnajuTrejbal.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace semSimulatorHokejovychTurnajuTrejbal.ModelView
{
    public partial class MainViewModel: ObservableObject {
        private readonly HockeyService _service;
        private Simulation? _simulation;
        private DispatcherTimer _timer;

        public ObservableCollection<Player> Players => _service.Players;
        public ObservableCollection<Team> Teams => _service.Teams;
        public ObservableCollection<Tournament> Tournaments => _service.Tournaments;
        [ObservableProperty] private ObservableCollection<object> filteredEntities = new();
        [ObservableProperty] private ObservableCollection<Match> currentMatches = new();
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartSimulationCommand))]
        [NotifyCanExecuteChangedFor(nameof(SkipSimulationCommand))] 
        private Match? selectedMatch;

        [ObservableProperty] private string selectedEntityType = "Turnaj";
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeletePlayerCommand))]
        private Player? selectedPlayer;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteTeamCommand))]
        private Team? selectedTeam;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteTournamentCommand))]
        private Tournament? selectedTournament;

        [ObservableProperty] private string filterText = "";
        [ObservableProperty] private string statusText = "Připraveno";

        [ObservableProperty] private string homeTeamName = "HOME";
        [ObservableProperty] private string awayTeamName = "AWAY";
        [ObservableProperty] private int homeScore;
        [ObservableProperty] private int awayScore;
        [ObservableProperty] private int homeShots;
        [ObservableProperty] private int awayShots;
        [ObservableProperty] private string gameTime = "20:00";
        [ObservableProperty] private string periodText = "1st";
        [ObservableProperty] private int currentMinute = 20;
        [ObservableProperty] private int currentPeriod = 1;
        [ObservableProperty] private bool showHomeStats = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StopSimulationCommand))]
        private bool isSimulationRunning = false;
        [ObservableProperty] private bool isSimulationEnabled = false;//zamezit simulaci po skončení zápasu

        [ObservableProperty] private GoalEvent? currentGoal;

        [ObservableProperty] private ObservableCollection<object> skStatsHome = new();
        [ObservableProperty] private ObservableCollection<object> goStatsHome = new();
        [ObservableProperty] private ObservableCollection<object> skStatsAway = new();
        [ObservableProperty] private ObservableCollection<object> goStatsAway = new();

        public ObservableCollection<string> EntityTypes { get; } = new() { "Turnaj", "Tým", "Hráč" };

        [ObservableProperty] private int visibilityHomeStats = 100;
        [ObservableProperty] private int visibilityAwayStats = 0;
        [ObservableProperty] private int showGoalAnimation = 0;
        [ObservableProperty] private int showPlayers =0;
        [ObservableProperty] private int showTeams = 0;
        [ObservableProperty] private int showTournaments = 100;

        public MainViewModel() {
            _service = new HockeyService();
            _timer = new DispatcherTimer();
            _timer.Tick += (s, e) => SimulateMinute();
        }
        private bool CanStartSimulation() => SelectedMatch != null && !SelectedMatch.WasPlayed && !IsSimulationRunning;
        private async Task Reload() {
            StatusText = "Načítání...";
            await _service.LoadAllAsync();
            StatusText = "Data načtena";
        }

        //partial void OnSelectedEntityTypeChanged(string value) => FilterCommand.Execute(null);
        partial void OnFilterTextChanged(string value) => FilterCommand.Execute(null);

        [RelayCommand]
        private async Task LoadData() { 
            await _service.ImportFromJsonAsync();
            _ = Reload();
        }

        [RelayCommand]
        private async Task SaveData() => await _service.ExportToJsonAsync(Players, Teams, Tournaments);

        [RelayCommand]
        private void Exit() => Application.Current.Shutdown();

        [RelayCommand]
        private void DeleteDatabase() {
            if (MessageBox.Show("Smazat databázi?", "Potvrzení", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                _service.DeleteDatabase();
                Players.Clear(); Teams.Clear(); Tournaments.Clear();
                StatusText = "Databáze smazána";
            }
        }

        [RelayCommand]
        private void CreateEntity(string? type) => OpenEditWindow(type, null);

        private void OpenEditWindow(string? type, int? id) {
            //
        }

        [RelayCommand]
        private void Filter() {
            CollectionViewSource.GetDefaultView(Players).Filter = o => {
                if (o is Player p) return p.FullName.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
                return false;
            };
            CollectionViewSource.GetDefaultView(Teams).Filter = o => {
                if (o is Team t) return t.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
                return false;
            };
            CollectionViewSource.GetDefaultView(Tournaments).Filter = o => {
                if (o is Tournament tr) return tr.Title.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
                return false;
            };
        }

        private bool CanDeleteTournament() => SelectedTournament != null;
        [RelayCommand (CanExecute =nameof(CanDeleteTournament))]
        private async Task DeleteTournament() {
            if (SelectedTournament == null) return;
            if (MessageBox.Show($"Opravdu smazat turnaj {SelectedTournament.Title}?", "Potvrzení", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                await _service.DeleteTournamentAsync(SelectedTournament);
                Tournaments.Remove(SelectedTournament);
                StatusText = $"Turnaj smazán";
            }
        }

        private bool CanDeleteTeam() => SelectedTeam != null;
        [RelayCommand (CanExecute =nameof(CanDeleteTeam))]
        private async Task DeleteTeam() {
            if (SelectedTeam == null) return;
            if (MessageBox.Show($"Opravdu smazat tým {SelectedTeam.Name}?", "Potvrzení", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                foreach (var t in Tournaments) {
                    var matchesToRemove = t.Matches.Where(m => m.HomeTeamId == SelectedTeam.Id || m.AwayTeamId == SelectedTeam.Id).ToList();
                    foreach (var match in matchesToRemove) {
                        t.Matches.Remove(match);
                        CurrentMatches.Remove(match);
                    }
                }
                await _service.DeleteTeamAsync(SelectedTeam);
                Teams.Remove(SelectedTeam);
                StatusText = $"Tým smazán";
            }
        }

        private bool CanDeletePlayer() => SelectedPlayer != null;
        [RelayCommand (CanExecute =nameof(CanDeletePlayer))]
        private async Task DeletePlayer() {
            if (SelectedPlayer == null) return;
            if (MessageBox.Show($"Opravdu smazat hráče {SelectedPlayer.FullName}?", "Potvrzení", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                await _service.DeletePlayerAsync(SelectedPlayer);
                Players.Remove(SelectedPlayer);
                StatusText = $"Hráč smazán";
            }
        }

        [RelayCommand (CanExecute = nameof(CanStartSimulation))]
        private void StartSimulation() {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Start();
            IsSimulationRunning = true;
        }

        [RelayCommand (CanExecute = nameof(IsSimulationRunning))]
        private void StopSimulation() {
            _timer.Stop();
            IsSimulationRunning = false;
        }

        [RelayCommand (CanExecute = nameof(CanStartSimulation))]
        private void SkipSimulation() {
            _timer.Interval = TimeSpan.FromMilliseconds(10);
            _timer.Start();
            IsSimulationRunning = true;
        }

        private void SimulateMinute() {
            CurrentMinute--;
            GameTime = $"{CurrentMinute:D2}:00";
            _simulation?.SimulateMinute();

            if (CurrentMinute <= 0) {
                _timer.Stop();
                if (CurrentPeriod == 3) {
                    StatusText = "Zápas skončil";
                    IsSimulationRunning = false;
                    SelectedMatch!.WasPlayed = true;
                    IsSimulationEnabled = false;
                    return;
                }
                CurrentPeriod++;
                CurrentMinute = 20;
                PeriodText = CurrentPeriod switch { 1 => "1st", 2 => "2nd", 3 => "3rd", _ => $"{CurrentPeriod}th" };
                _timer.Start();
            }
        }

        partial void OnSelectedTournamentChanged(Tournament? value) {
            if (value == null) {
                CurrentMatches.Clear();
                return;
            }
            var matches = value.Matches.Where(m => !m.WasPlayed).ToList();
            CurrentMatches = new ObservableCollection<Match>(matches);
        }

        partial void OnSelectedMatchChanged(Match? value) {
            if (value == null) return;
            HomeTeamName = Teams.FirstOrDefault(t => t.Id == value.HomeTeamId)?.Name ?? "HOME";
            AwayTeamName = Teams.FirstOrDefault(t => t.Id == value.AwayTeamId)?.Name ?? "AWAY";
            if (value!.WasPlayed){
                _simulation = null;
                HomeScore = value.HomeScore;
                AwayScore = value.AwayScore;
                HomeShots = value.HomeShots;
                AwayShots = value.AwayShots;
                PeriodText = "Zápas již byl odehrán";
                return;
            }
            CurrentMinute = 20;
            CurrentPeriod = 1;
            PeriodText = "1st";
            GameTime = "20:00";
            _simulation = new Simulation(value, Players, Teams);
            _simulation.MatchUpdated += m => { HomeScore = m.HomeScore; AwayScore = m.AwayScore; };
            _simulation.ShotAttempted += s => { HomeShots = s.IsHome ? HomeShots + 1 : HomeShots; AwayShots = s.IsHome ? AwayShots : AwayShots + 1; };
            _simulation.GoalScored += g => { CurrentGoal = g; ShowGoalAnimation = 100; Task.Delay(3500).ContinueWith(_ => ShowGoalAnimation = 0); 
                if(g.IsHomeGoal)HomeScore++; else AwayScore++; };
            _simulation.HomeStatsUpdated += p => UpdateStatsHome(p);
            _simulation.AwayStatsUpdated += p => UpdateStatsAway(p);
            if (App.Current.MainWindow is MainWindow mainWindow)
            {
                _simulation.DrawPlayerRequested += mainWindow.DrawPlayer;
                _simulation.ClearCanvasRequested += mainWindow.ClearPlayers;
            }
            _simulation.StartMatch();
            IsSimulationEnabled = true;
        }
        partial void OnShowHomeStatsChanged(bool value) {
            if (value) {
                VisibilityHomeStats = 100;
                VisibilityAwayStats = 0;
            } else {
                VisibilityHomeStats = 0;
                VisibilityAwayStats = 100;
            }
        }

        partial void OnSelectedEntityTypeChanged(string value) {
            ShowPlayers = 0;
            ShowTeams = 0;
            ShowTournaments = 0;
            switch (value) {
                case "Hráč":
                    ShowPlayers = 100;
                    break;
                case "Tým":
                    ShowTeams = 100;
                    break;
                case "Turnaj":
                    ShowTournaments = 100;
                    break;
            }
        }

        private void UpdateStatsHome(List<Player> players) {
            SkStatsHome = new(players.OfType<Skater>().Select(s => new
            {
                s.FullName,
                s.Stats.Goals,
                s.Stats.Assists,
                s.Stats.Shots
            }));

            GoStatsHome = new(players.OfType<Goalie>().Select(g => new
            {
                g.FullName,
                g.Stats.Saves,
                g.Stats.GoalsAgainst,
                g.Stats.SavePercentage
            }));
        }
        private void UpdateStatsAway(List<Player> players) {
            SkStatsAway = new(players.OfType<Skater>().Select(s => new
            {
                s.FullName,
                s.Stats.Goals,
                s.Stats.Assists,
                s.Stats.Shots
            }));
            GoStatsAway = new(players.OfType<Goalie>().Select(g => new
            {
                g.FullName,
                g.Stats.Saves,
                g.Stats.GoalsAgainst,
                g.Stats.SavePercentage
            }));
        }
    }
}
