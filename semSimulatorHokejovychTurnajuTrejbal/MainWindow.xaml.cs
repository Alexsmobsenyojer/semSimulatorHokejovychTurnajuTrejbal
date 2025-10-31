using LiteDB.Async;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace semSimulatorHokejovychTurnajuTrejbal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private LiteDatabaseAsync _db;
        public ObservableCollection<string> EntityTypes { get; } = new() { "Hráč", "Tým", "Turnaj" };
        private ObservableCollection<Tournament> Tournaments = new();
        private ObservableCollection<Team> Teams = new();
        private ObservableCollection<Player> Players = new();

        private DispatcherTimer _simulationTimer;
        private bool _isSimulating = false;
        private int _currentMinute = 20, _currentPeriod = 1;
        private Team? _teamA, _teamB;
        private int _scoreA = 0, _scoreB = 0, _shotsA = 0, _shotsB = 0;
        public MainWindow() {
            InitializeComponent();
            this.DataContext = this;
            _db = new("Filename=HockeyDatabase.db");
            StatusText.Text = "Připraveno";
            _simulationTimer = new DispatcherTimer();
            _simulationTimer.Interval = TimeSpan.FromSeconds(1); // 1s = 1 minuta
            _simulationTimer.Tick += SimulationTimer_Tick;
        }
        private void SimulationTimer_Tick(object? sender, EventArgs e) {
            _currentMinute--;
            TextTime.Text = $"{_currentMinute.ToString():00}:00";
            if (_currentMinute <= 0) {
                _simulationTimer.Stop();
                _currentMinute = 20;
                _currentPeriod++;
                if (_currentPeriod > 3) {
                    _isSimulating = false;
                    StatusText.Text = "Zápas skončil.";
                } else {
                    if (_currentPeriod == 1) TextPeriod.Text = _currentPeriod.ToString()+"st";
                    if (_currentPeriod == 2) TextPeriod.Text = _currentPeriod.ToString()+"nd";
                    if (_currentPeriod == 3) TextPeriod.Text = _currentPeriod.ToString()+"rd";
                    _simulationTimer.Start();
                }
            }
        }

        private async Task LoadDataAsync() {
            var tournamentsCol = _db.GetCollection<Tournament>("tournaments");
            Tournaments = new ObservableCollection<Tournament>((await tournamentsCol.FindAllAsync()).ToList());
            var teamsCol = _db.GetCollection<Team>("teams");
            Teams = new ObservableCollection<Team>((await teamsCol.FindAllAsync()).ToList());
            var playersCol = _db.GetCollection<Player>("players");
            Players = new ObservableCollection<Player>((await playersCol.FindAllAsync()).ToList());
            UpdateDataList();
        }

        private async Task SaveDataAsync() {
            try {
                StatusText.Text = "Ukládání…";
                await Task.Run(async () =>
                {
                    await Task.WhenAll(
                        _db.GetCollection<Tournament>("tournaments").UpsertAsync(Tournaments),
                        _db.GetCollection<Team>("teams").UpsertAsync(Teams),
                        _db.GetCollection<Player>("players").UpsertAsync(Players)
                    );
                });
                StatusText.Text = "Data uložena.";
            } catch (Exception ex){
                MessageBox.Show($"Chyba při ukládání: {ex.Message}");
            }
        }

        private void UpdateDataList() {
            string? selectedType = EntityTypeCombo.SelectedItem as string;
            string filter = FilterTextBox.Text.ToLower();

            if (selectedType == "Turnaj")
                DataListView.ItemsSource = Tournaments.Where(t => t.Title.ToLower().Contains(filter)).Select(t => new { Id = t.Id, Name = t.Title });
            else if (selectedType == "Tým")
                DataListView.ItemsSource = Teams.Where(t => t.Name.ToLower().Contains(filter)).Select(t => new { Id = t.Id, Name = t.Name });
            else if (selectedType == "Hráč")
                DataListView.ItemsSource = Players.Where(p => p.FullName.ToLower().Contains(filter)).Select(p => new { Id = p.Id, Name = p.FullName });
        }


        private async void SaveDataClick(object sender, RoutedEventArgs e) {
            await SaveDataAsync();
        }

        private async void LoadDataClick(object sender, RoutedEventArgs e) {
           await LoadDataAsync();
        }

        private async void ExitClick(object sender, RoutedEventArgs e) {
            await SaveDataAsync();
            Application.Current.Shutdown();
        }

        private void CreateEditEntityClick(object sender, RoutedEventArgs e) {
            ShowEntityForm(EntityTypeCombo.SelectedItem.ToString() ?? "");
        }

        private void AutoCreateTournamentClick(object sender, RoutedEventArgs e){

        }

        private void StartSimulationClick(object sender, RoutedEventArgs e){
            _simulationTimer.Start();
            _isSimulating = true;
        }

        private void StopSimulationClick(object sender, RoutedEventArgs e) {
            _simulationTimer.Stop();
            _isSimulating = false;
        }

        private void SkipSimulationClick(object sender, RoutedEventArgs e) {

        }

        private void FilterDataClick(object sender, RoutedEventArgs e) {
            UpdateDataList();
        }

        private void TeamStatsChecked(object sender, RoutedEventArgs e) {
            if (HomeStatsRadio.IsChecked == true) {
                //PlayerStatsListView.ItemsSource = homeTeamStats;
            } else {
                //PlayerStatsListView.ItemsSource = awayTeamStats;
            }
        }

        private void ShowEntityForm(string entityType) { }
        

    }
}