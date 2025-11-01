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
        private  ILiteCollectionAsync<Tournament> TournamentsCol => _db.GetCollection<Tournament>("tournaments");
        private  ILiteCollectionAsync<Team> TeamsCol => _db.GetCollection<Team>("teams");
        private  ILiteCollectionAsync<Player> PlayersCol => _db.GetCollection<Player>("players");

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
            EntityTypeCombo.SelectionChanged += (_, __) => UpdateDataList();
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
            Tournaments = new ObservableCollection<Tournament>((await TournamentsCol.FindAllAsync()).ToList());
            Teams = new ObservableCollection<Team>((await TeamsCol.FindAllAsync()).ToList());
            Players = new ObservableCollection<Player>((await PlayersCol.FindAllAsync()).ToList());
            UpdateDataList();
        }

        private async Task SaveDataAsync() {
            try {
                StatusText.Text = "Ukládání…";
                    await Task.WhenAll(
                        TournamentsCol.UpsertAsync(Tournaments),
                        TeamsCol.UpsertAsync(Teams),
                        PlayersCol.UpsertAsync(Players)
                    );
                StatusText.Text = "Data uložena.";
            } catch (Exception ex){
                MessageBox.Show($"Chyba při ukládání: {ex.Message}");
            }
        }

        private void UpdateDataList() {
            string? selectedType = EntityTypeCombo.SelectedItem as string;
            string filter = FilterTextBox.Text.ToLower();
            DataListView.ItemsSource = selectedType switch {
                "Hráč" => Players.Where(p => p.FullName.ToLower().Contains(filter)).Select(p => new { Id = p.Id, Name = p.FullName }),
                "Tým" => Teams.Where(t => t.Name.ToLower().Contains(filter)).Select(t => new { Id = t.Id, Name = t.Name }),
                "Turnaj" => Tournaments.Where(t => t.Title.ToLower().Contains(filter)).Select(t => new { Id = t.Id, Name = t.Title }),
                _ => Enumerable.Empty<object>()
            };
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

        private void CreateEntityClick(object sender, RoutedEventArgs e) {
            var selectedType = EntityTypeCombo.SelectedItem.ToString();
            if (selectedType != null) {
                ShowEntityForm(selectedType);
            } else {
                MessageBox.Show("Vyberte typ entity.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DataListView_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (DataListView.SelectedItem != null) {
                dynamic selected = DataListView.SelectedItem;
                int id = selected.Id;
                string? selEntity = EntityTypeCombo.SelectedItem as string;
                if (selEntity == "Turnaj") {
                    var tournament = Tournaments.FirstOrDefault(t => t.Id == id);
                    if (tournament != null)
                        ShowEntityForm("Turnaj", tournament);
                } else if (selEntity == "Tým") {
                    var team = Teams.FirstOrDefault(t => t.Id == id);
                    if (team != null)
                        ShowEntityForm("Tým", team);
                } else if (selEntity == "Hráč") {
                    var player = Players.FirstOrDefault(p => p.Id == id);
                    if (player != null)
                        ShowEntityForm("Hráč", player);
                }
            }
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

        private void ShowEntityForm(string entityType, object entityToEdit = null) {
            var isEdit = entityToEdit != null;
            var window = new Window {
                Title = $"{(isEdit ? "Editovat" : "Vytvořit")} {entityType}",
                Width = 450, Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var stack = new StackPanel { Margin = new Thickness(15) };
            window.Content = stack;

            var saveBtn = new Button { Content = "Uložit", Width = 80, Margin = new Thickness(0, 0, 10, 0) };
            var cancelBtn = new Button { Content = "Zrušit", Width = 80 };
            cancelBtn.Click += (_, __) => window.Close();

            var btnPanel = new StackPanel {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0),
                Children = { saveBtn, cancelBtn }
            };

            stack.Children.Add(new TextBlock {
                Text = window.Title,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 15)
            });

            void AddLabel(string label, UIElement control) {
                stack.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 5, 0, 2) });
                stack.Children.Add(control);
            }

            switch (entityType) {
                case "Hráč":
                    BuildPlayerForm();
                    break;
                case "Tým":
                    BuildTeamForm();
                    break;
                case "Turnaj":
                    BuildTournamentForm();
                    break;
            }

            stack.Children.Add(btnPanel);
            window.ShowDialog();

            //Player Form
            void BuildPlayerForm() {
                var player = entityToEdit as Player;
                var isGoalie = player is Goalie;
                var skater = player as Skater;

                var nameBox = new TextBox { Text = player?.FullName ?? "Nový hráč" };
                var numberBox = new TextBox { Text = (player?.Number ?? 99).ToString() };
                var positionCombo = new ComboBox { ItemsSource = Enum.GetValues<Position>() };
                var roleCombo = new ComboBox();
                var skaterPanel = new StackPanel();
                var goaliePanel = new StackPanel();

                AddLabel("Jméno a Příjmení:", nameBox);
                AddLabel("Číslo dresu:", numberBox);
                AddLabel("Pozice:", positionCombo);
                AddLabel("Role:", roleCombo);
                var shooting = new TextBox { Text = skater?.Shooting.ToString() ?? "50" };
                var passing = new TextBox { Text = skater?.Passing.ToString() ?? "50" };
                var defending = new TextBox { Text = skater?.Defending.ToString() ?? "50" };
                var skating = new TextBox { Text = skater?.Skating.ToString() ?? "50" };
                var overall = new TextBox { Text = (player as Goalie)?.Overall.ToString() ?? "50" };
                skaterPanel.Children.Add(new TextBlock { Text = "Střela(1-99):" });
                skaterPanel.Children.Add(shooting);
                skaterPanel.Children.Add(new TextBlock { Text = "Přihrávky(1-99):" });
                skaterPanel.Children.Add(passing);
                skaterPanel.Children.Add(new TextBlock { Text = "Obrana(1-99):" });
                skaterPanel.Children.Add(defending);
                skaterPanel.Children.Add(new TextBlock { Text = "Bruslení(1-99):" });
                skaterPanel.Children.Add(skating);
                goaliePanel.Children.Add(new TextBlock { Text = "Celkové hodnocení(1-99):" });
                goaliePanel.Children.Add(overall);
                stack.Children.Add(skaterPanel);
                stack.Children.Add(goaliePanel);
                positionCombo.SelectedIndex = 0;
                roleCombo.SelectedIndex = 0;

                void UpdateUI() {
                    var pos = (Position?)positionCombo.SelectedItem;
                    bool isPosG = pos == Position.G;
                    skaterPanel.Visibility = isPosG ? Visibility.Collapsed : Visibility.Visible;
                    goaliePanel.Visibility = isPosG ? Visibility.Visible : Visibility.Collapsed;
                    roleCombo.IsEnabled = !isPosG;

                    if (isPosG) {
                        roleCombo.ItemsSource = new[] { "Žádná role" };
                        roleCombo.SelectedIndex = 0;
                    } else if (pos != null) {
                        var roles = Skater.GetValidRoles(pos.Value);
                        roleCombo.ItemsSource = roles;
                        roleCombo.SelectedItem ??= roles.FirstOrDefault();
                    } else {
                        roleCombo.ItemsSource = null;
                    }
                }

                positionCombo.SelectionChanged += (_, __) => UpdateUI();
                //edit
                if (player != null) {
                    if (isGoalie) {
                        positionCombo.SelectedItem = Position.G;
                        positionCombo.IsEnabled = false;
                    } else {
                        positionCombo.SelectedItem = skater.Position;
                        roleCombo.SelectedItem = skater.Role;
                    }
                }
                UpdateUI();

                saveBtn.Click += async(_, __) =>{
                    try {
                        Player player;
                        if (isEdit)
                            player = (Player)entityToEdit;
                        else {
                            if (positionCombo.SelectedItem is Position pos && pos == Position.G)
                                player = new Goalie();
                            else
                                player = new Skater();
                        }
                        player.FullName = nameBox.Text.Trim();
                        player.Number = int.Parse(numberBox.Text);

                        if (player is Skater s) {
                            s.Position = (Position)positionCombo.SelectedItem;
                            s.Role = (Role)roleCombo.SelectedItem;
                            s.Shooting = int.Parse(shooting.Text);
                            s.Passing = int.Parse(passing.Text);
                            s.Defending = int.Parse(defending.Text);
                            s.Skating = int.Parse(skating.Text);
                        } else if (player is Goalie g)
                            g.Overall = int.Parse(overall.Text);

                        if (isEdit)
                            await PlayersCol.UpdateAsync(player);
                        else {
                            await PlayersCol.InsertAsync(player);
                            Players.Add(player);
                        }
                        UpdateDataList();
                        StatusText.Text = $"Hráč '{player.FullName}' uložen.";
                        window.Close();
                    } catch (Exception ex) {
                        MessageBox.Show($"Špatně vyplněné hodnoty: {ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
            }

            // Team Form
            void BuildTeamForm() {
                var team = (Team)(entityToEdit ?? new Team { Name = "Nový Tým", Players = new() });

                var nameBox = new TextBox { Text = team.Name };
                var playersListBox = new ListBox {ItemsSource = Players,SelectionMode = SelectionMode.Multiple,DisplayMemberPath = "FullName",Height = 180};
                foreach (var p in team.Players) {
                    if (Players.Contains(p))
                        playersListBox.SelectedItems.Add(p);
                }
                AddLabel("Název týmu:", nameBox);
                AddLabel("Výhry / Prohry:", new TextBlock { Text = $"{team.Wins} / {team.Losses}" });
                AddLabel("Soupiska:", playersListBox);

                saveBtn.Click += async (_, __) =>
                {
                    try {
                        team.Name = nameBox.Text.Trim();
                        team.Players = playersListBox.SelectedItems.Cast<Player>().ToList();
                        if (!isEdit) {
                            await TeamsCol.InsertAsync(team);
                            Teams.Add(team);
                        } else {
                            await TeamsCol.UpdateAsync(team);
                        }

                        UpdateDataList();
                        StatusText.Text = $"Tým '{team.Name}' uložen.";
                        window.Close();
                    } catch (Exception ex) {
                        MessageBox.Show($"Chyba: {ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
            }

            // Tournament Form
            void BuildTournamentForm() {
                var t = (Tournament)(entityToEdit ?? new Tournament {
                    Title = "Nový Turnaj",
                    MatchCount = 0,
                    Teams = new()
                });

                var titleBox = new TextBox { Text = t.Title };
                var countBox = new TextBox { Text = t.MatchCount.ToString() };

                AddLabel("Název turnaje:", titleBox);
                AddLabel("Počet zápasů:", countBox);
                AddLabel("Týmy:", new ListBox {
                    ItemsSource = Teams,
                    DisplayMemberPath = "Name",
                    Height = 180
                });

                saveBtn.Click += async (_, __) => {
                    try {
                        t.Title = titleBox.Text.Trim();
                        if (int.TryParse(countBox.Text, out int c)) t.MatchCount = c;

                        if (!isEdit) {
                            await TournamentsCol.InsertAsync(t);
                            Tournaments.Add(t);
                        } else {
                            await TournamentsCol.UpdateAsync(t);
                        }

                        UpdateDataList();
                        StatusText.Text = $"Turnaj '{t.Title}' uložen.";
                        window.Close();
                    } catch (Exception ex) {
                        MessageBox.Show($"Chyba: {ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
            }
        }


    }
}