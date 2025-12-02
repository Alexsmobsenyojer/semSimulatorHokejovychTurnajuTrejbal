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
        [ObservableProperty] private Match? selectedMatch;
        [ObservableProperty] private bool canStartSimulation = false;
        [ObservableProperty] private bool isSimulationRunning = false;

        [ObservableProperty] private string selectedEntityType = "Hráč";
        [ObservableProperty] private Player? selectedPlayer;
        [ObservableProperty] private Team? selectedTeam;
        [ObservableProperty] private Tournament? selectedTournament;

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

        [ObservableProperty] private bool showGoalAnimation = false;
        [ObservableProperty] private GoalEvent? currentGoal;

        [ObservableProperty] private ObservableCollection<object> skaterStats = new();
        [ObservableProperty] private ObservableCollection<object> goalieStats = new();
        [ObservableProperty] private bool showHomeStats = true;
        [ObservableProperty] private bool showAwayStats = false;

        public ObservableCollection<string> EntityTypes { get; } = new() { "Hráč", "Tým", "Turnaj" };

        public MainViewModel() {
            _service = new HockeyService();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => SimulateMinute();
        }

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

        [RelayCommand]
        private void EditSelected(object? item) {
            if (item != null) {
                int? id = null;
                switch (SelectedEntityType) {
                    case "Hráč":
                        OpenEditWindow(SelectedEntityType, id.Value);
                        break;
                    case "Tým":
                        OpenEditWindow(SelectedEntityType, id.Value);
                        break;
                    case "Turnaj":
                        OpenEditWindow(SelectedEntityType, id.Value);
                        break;
                }       
            }
        }

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
        [RelayCommand (CanExecute = nameof(CanStartSimulation))]
        private void StartSimulation() {
            CurrentMinute = 20;
            CurrentPeriod = 1;
            PeriodText = "1st";
            GameTime = "20:00";
            _timer.Start();
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
                if (CurrentPeriod >= 3) {
                    StatusText = "Zápas skončil";
                    return;
                }
                CurrentPeriod++;
                CurrentMinute = 20;
                PeriodText = CurrentPeriod switch { 1 => "1st", 2 => "2nd", 3 => "3rd", _ => $"{CurrentPeriod}th" };
                _timer.Start();
            }
        }

        partial void OnSelectedMatchChanged(Match? match) {
            if (match == null) return;

            _simulation = new Simulation(match, Players, Teams);
            _simulation.MatchUpdated += m => { HomeScore = m.HomeScore; AwayScore = m.AwayScore; };
            _simulation.ShotAttempted += s => { HomeShots = s.IsHome ? HomeShots + 1 : HomeShots; AwayShots = s.IsHome ? AwayShots : AwayShots + 1; };
            _simulation.GoalScored += g => { CurrentGoal = g; ShowGoalAnimation = true; Task.Delay(3500).ContinueWith(_ => ShowGoalAnimation = false); };
            _simulation.HomeStatsUpdated += p => UpdateStats(p, true);
            _simulation.AwayStatsUpdated += p => UpdateStats(p, false);
            //_simulation.DrawPlayerRequested += App.Current.MainWindow.DrawPlayer;
            //_simulation.ClearCanvasRequested += App.Current.MainWindow.ClearPlayers;
            _simulation.StartMatch();
            CanStartSimulation = true;
        }

        private void UpdateStats(List<Player> players, bool isHome) {

            SkaterStats = new(players.OfType<Skater>().Select(s => new
            {
                s.FullName,
                s.Stats.Goals,
                s.Stats.Assists,
                s.Stats.Shots
            }));

            GoalieStats = new(players.OfType<Goalie>().Select(g => new
            {
                g.FullName,
                g.Stats.Saves,
                g.Stats.GoalsAgainst,
                SavePct = g.Stats.SavePercentage.ToString("F2")
            }));
        }
    }
}
