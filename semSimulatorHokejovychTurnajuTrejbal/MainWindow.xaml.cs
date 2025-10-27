using LiteDB.Async;
using System.Collections.ObjectModel;
using System.Text;
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
        public ObservableCollection<string> EntityTypes { get; } = new() { "Turnaj", "Tým", "Hráč" };
        private List<Tournament> Tournaments = new();
        private List<Team> Teams = new();
        private List<Player> Players = new();

        private DispatcherTimer _simulationTimer;
        private bool _isSimulating = false;
        private int _currentMinute = 20, _currentPeriod = 1;
        private Team? _teamA, _teamB;
        private int _scoreA = 0, _scoreB = 0, _shotsA = 0, _shotsB = 0;
        public MainWindow() {
            InitializeComponent();
            this.DataContext = this;
            _db = new("Filename=HockeyDate.db;");
            StatusText.Text = "Připraveno";
            _simulationTimer = new DispatcherTimer();
            _simulationTimer.Interval = TimeSpan.FromSeconds(1); // 1s = 1 minuta
            _simulationTimer.Tick += SimulationTimer_Tick;
        }
        private void SimulationTimer_Tick(object sender, EventArgs e) {
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

        private void SaveDataClick(object sender, RoutedEventArgs e) {

        }

        private void LoadDataClick(object sender, RoutedEventArgs e) {

        }

        private void ExitClick(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void CreateEditEntityClick(object sender, RoutedEventArgs e) {

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

        }

        private void TeamStatsChecked(object sender, RoutedEventArgs e) {
            if (HomeStatsRadio.IsChecked == true) {
                //PlayerStatsListView.ItemsSource = homeTeamStats;
            } else {
                //PlayerStatsListView.ItemsSource = awayTeamStats;
            }
        }
    }
}