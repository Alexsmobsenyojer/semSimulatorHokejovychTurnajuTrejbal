using LiteDB.Async;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        private int _currentMinute, _currentPeriod;
        private Simulation _sim;
        public MainWindow() {
            InitializeComponent();
            this.DataContext = this;
            _db = new("Filename=HockeyDatabase.db");
            StatusText.Text = "Připraveno";
            _simulationTimer = new DispatcherTimer();
            _simulationTimer.Tick += SimulationTimer_Tick;
            EntityTypeCombo.SelectionChanged += (_, __) => UpdateDataList();
            _ = LoadDataAsync();
            UpdateDataList();
        }

        private async Task LoadDataAsync() {
            Tournaments = new ObservableCollection<Tournament>((await TournamentsCol.FindAllAsync()));
            Teams = new ObservableCollection<Team>((await TeamsCol.FindAllAsync()));
            Players = new ObservableCollection<Player>((await PlayersCol.FindAllAsync()));
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

        private async void SaveDataClick(object sender, RoutedEventArgs e) {
            var dialog = new SaveFileDialog { FileName = "hockey_data.json", Filter = "JSON soubory (*.json)|*.json|Všechny soubory (*.*)|*.*" };
            if (dialog.ShowDialog() == true) {
                var data = new JsonData {
                    Players = Players.ToList(),
                    Teams = Teams.ToList(),
                    Tournaments = Tournaments.ToList()
                };

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(dialog.FileName, json);
                StatusText.Text = $"Uloženo: {System.IO.Path.GetFileName(dialog.FileName)}";
            }
        }

        private async void LoadDataClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog { Filter = "JSON soubory (*.json)|*.json|Všechny soubory (*.*)|*.*" };
            if (dialog.ShowDialog() == true) {
                try {
                    var json = await File.ReadAllTextAsync(dialog.FileName);
                    var data = JsonSerializer.Deserialize<JsonData>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (data is not null) {
                        Players = new ObservableCollection<Player>(data.Players);
                        Teams = new ObservableCollection<Team>(data.Teams);
                        Tournaments = new ObservableCollection<Tournament>(data.Tournaments);
                        UpdateDataList();
                        StatusText.Text = $"Načteno: {System.IO.Path.GetFileName(dialog.FileName)}";
                    }
                } catch (Exception ex) {
                    MessageBox.Show($"Chyba při načítání: {ex.Message}");
                }
            }
        }

        private async void ExitClick(object sender, RoutedEventArgs e) {
            await SaveDataAsync();
            Application.Current.Shutdown();
        }

        private void DeleteDatabaseClick(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show("Opravdu chcete smazat databázi? Tato akce je nevratná.", "Potvrzení smazání", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes) {
                _db.Dispose();
                if (File.Exists("HockeyDatabase.db")) {
                    File.Delete("HockeyDatabase.db");
                }
                _db = new("Filename=HockeyDatabase.db");
                Tournaments.Clear();
                Teams.Clear();
                Players.Clear();
                UpdateDataList();
                StatusText.Text = "Databáze smazána.";
            }
        }

        private void UpdateDataList() {
            string? selectedEntity = EntityTypeCombo.SelectedItem as string;
            string filter = FilterTextBox.Text.ToLower();
            DataListView.ItemsSource = selectedEntity switch {
                "Hráč" => Players.Where(p => p.FullName.ToLower().Contains(filter)).Select(p => new { Id = p.Id, Name = p.FullName }),
                "Tým" => Teams.Where(t => t.Name.ToLower().Contains(filter)).Select(t => new { Id = t.Id, Name = t.Name }),
                "Turnaj" => Tournaments.Where(t => t.Title.ToLower().Contains(filter)).Select(t => new { Id = t.Id, Name = t.Title }),
                _ => Enumerable.Empty<object>()
            };
        }
        private void FilterDataClick(object sender, RoutedEventArgs e) => UpdateDataList();

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

        private void DataListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (DataListView.SelectedItem != null) {
                dynamic selected = DataListView.SelectedItem;
                int id = selected.Id;
                string? selEntity = EntityTypeCombo.SelectedItem as string;
                if (selEntity == "Turnaj") {
                    var tournament = Tournaments.FirstOrDefault(t => t.Id == id);
                    if (tournament != null) {
                        MatchesListBox.ItemsSource = tournament.Matches;
                    }
                } 
            }
        }

        private void MatchesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (MatchesListBox.SelectedItem is not Match match) return;
            
            _sim = new Simulation(match, Players, Teams);
            _sim.MatchUpdated += OnMatchUpdated;
            _sim.GoalScored += OnGoalScored;
            _sim.HomeStatsUpdated += players => UpdateStats(players, true);
            _sim.AwayStatsUpdated += players => UpdateStats(players, false);
            _sim.DrawPlayerRequested += DrawPlayer;
            _sim.ClearCanvasRequested += ClearPlayers;
            _sim.StartMatch();
            BtnStartSim.IsEnabled = true;
            BtnStopSim.IsEnabled = true;
            BtnSkipSim.IsEnabled = true;
        }

        private void OnMatchUpdated(Match m) {
            TextTeamA.Text = Teams.First(t => t.Id == m.HomeTeamId).Name;
            TextTeamB.Text = Teams.First(t => t.Id == m.AwayTeamId).Name;
        }

        private void OnGoalScored(GoalEvent goal) {
            if(goal.IsHomeGoal)TextScoreA.Text = goal.NewScore.ToString();
            else TextScoreB.Text = goal.NewScore.ToString();

            ScorerText.Text = $"{goal.Scorer.FullName} ({goal.Scorer.Stats.Goals})";
            Assist1Text.Text = goal.Assist1?.FullName ?? "Bez asistence";
            Assist2Text.Text = goal.Assist2?.FullName ?? "";
            _simulationTimer.Stop();
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            var stay = new DoubleAnimation(1, 1, TimeSpan.FromSeconds(3));
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
            fadeIn.Completed += (_, __) => GoalPopup.BeginAnimation(UIElement.OpacityProperty, stay);
            stay.Completed += (_, __) => GoalPopup.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            fadeOut.Completed += (_, __) => _simulationTimer.Start();
            GoalPopup.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        private void UpdateStats(List<Player> players, bool isHome) {
            if (isHome && HomeStatsRadio.IsChecked == true) return;
            if (!isHome && HomeStatsRadio.IsChecked == false) return;
            SkatersStatsListView.ItemsSource = players.OfType<Skater>().Select(s => new { s.FullName, s.Stats.Goals, s.Stats.Assists, s.Stats.Shots }).ToList();
            GoaliesStatsListView.ItemsSource = players.OfType<Goalie>().Select(g => new { g.FullName, g.Stats.Saves, g.Stats.GoalsAgainst, SavePct = g.Stats.SavePercentage.ToString("F2") }).ToList();
        }

        private void DrawPlayer(Player player, double centerX, double centerY, Brush fill) {
            const double radius = 18;
            const double fontSize = 20;

            var circle = new Ellipse {
                Width = radius * 2, Height = radius * 2, Fill = fill,
                Stroke = Brushes.Black, StrokeThickness = 2, Tag = "Player"
            };
            var text = new TextBlock {
                Text = player.Number.ToString(), Foreground = Brushes.White,
                FontWeight = FontWeights.Bold, FontSize = fontSize, Tag = "Player"
            };
            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double textWidth = text.DesiredSize.Width;
            double textHeight = text.DesiredSize.Height;
            Canvas.SetLeft(circle, centerX - radius);
            Canvas.SetTop(circle, centerY - radius);
            Canvas.SetLeft(text, centerX - textWidth / 2);
            Canvas.SetTop(text, centerY - textHeight / 2);
            RinkCanvas.Children.Add(circle);
            RinkCanvas.Children.Add(text);
        }

        private void ClearPlayers() {
            for (int i = RinkCanvas.Children.Count - 1; i >= 0; i--) {
                if (RinkCanvas.Children[i] is FrameworkElement el && el.Tag?.ToString() == "Player")
                    RinkCanvas.Children.RemoveAt(i);
            }
        }

        private void StartSimulationClick(object sender, RoutedEventArgs e){
            _simulationTimer.Interval = TimeSpan.FromSeconds(1);
            SimulationStart();
        }

        private void StopSimulationClick(object sender, RoutedEventArgs e) {
            _simulationTimer.Stop();
            _isSimulating = false;
        }

        private void SkipSimulationClick(object sender, RoutedEventArgs e) {
            _simulationTimer.Interval = TimeSpan.FromMilliseconds(10);
            SimulationStart();
        }

        private void SimulationStart() {
            if (_sim == null) {
                MessageBox.Show("Vyberte zápas k simulaci.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _currentPeriod = 1;
            _currentMinute = 20;
            TextPeriod.Text = "1st";
            TextTime.Text = "20:00";
            _simulationTimer.Start();
            _isSimulating = true;
            _sim.StartMatch();
            MatchesListBox.IsEnabled = false;
        }

        private void SimulationTimer_Tick(object? sender, EventArgs e) {
            _currentMinute--;
            TextTime.Text = $"{_currentMinute.ToString():00}:00";
            _sim.SimulateMinute();
            if (_currentMinute <= 0) {
                _simulationTimer.Stop();
                if (_currentPeriod >= 3) {
                    _isSimulating = false;
                    StatusText.Text = "Zápas skončil.";
                    MatchesListBox.IsEnabled = true;
                    BtnStartSim.IsEnabled = false;
                    BtnSkipSim.IsEnabled = false;
                    BtnStopSim.IsEnabled = false;
                } else {
                    _currentMinute = 20;
                    _currentPeriod++;
                    if (_currentPeriod == 1) TextPeriod.Text = "1st";
                    if (_currentPeriod == 2) TextPeriod.Text = "2nd";
                    if (_currentPeriod == 3) TextPeriod.Text = "3rd";
                    _simulationTimer.Start();
                }
            }
        }

        private void TeamStatsChecked(object sender, RoutedEventArgs e) => _sim?.RefreshStats();

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
                        if (string.IsNullOrWhiteSpace(player.FullName))
                            throw new Exception("Zadejte jméno hráče.");
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
                var team = (Team)(entityToEdit ?? new Team { Name = "Nový Tým", PlayerIds = new() });

                var nameBox = new TextBox { Text = team.Name };
                var playersListBox = new ListBox { ItemsSource = Players, SelectionMode = SelectionMode.Multiple, DisplayMemberPath = "FullName", Height = 180};
                foreach (var p in team.PlayerIds) {
                    var teamPlayer = Players.FirstOrDefault(x => x.Id == p);
                    if (teamPlayer!=null)
                        playersListBox.SelectedItems.Add(teamPlayer);
                }
                AddLabel("Název týmu:", nameBox);
                AddLabel("Výhry / Prohry:", new TextBlock { Text = $"{team.Wins} / {team.Losses}" });
                AddLabel("Soupiska:", playersListBox);

                saveBtn.Click += async (_, __) =>
                {
                    try {
                        team.Name = nameBox.Text.Trim();
                        if (string.IsNullOrWhiteSpace(team.Name))
                            throw new Exception("Zadejte název týmu.");
                        team.PlayerIds = playersListBox.SelectedItems.Cast<Player>().Select(p => p.Id).ToList();
                        //TODO: Player bool isAssignedToTeam false, neukazovat v seznamu hračů při tvorbě týmu
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
                    TeamIds = new()
                });

                var titleBox = new TextBox { Text = t.Title };
                var teamsListBox = new ListBox { ItemsSource = Teams, SelectionMode = SelectionMode.Multiple, DisplayMemberPath = "Name", Height = 180 };
                foreach (var teamId in t.TeamIds) {
                    var tournTeam = Teams.FirstOrDefault(x => x.Id == teamId);
                    if (tournTeam != null)
                        teamsListBox.SelectedItems.Add(tournTeam);
                }

                AddLabel("Název turnaje:", titleBox);
                AddLabel("Týmy:", teamsListBox);

                saveBtn.Click += async (_, __) => {
                    try {
                        t.Title = titleBox.Text.Trim();
                        if (string.IsNullOrWhiteSpace(t.Title))
                            throw new Exception("Zadejte název turnaje.");
                        var selectedTeams = teamsListBox.SelectedItems.Cast<Team>().Select(t => t.Id).ToList();
                        if (selectedTeams.Count < 2)
                            throw new Exception("Vyberte alespoň 2 týmy.");
                        t.TeamIds = selectedTeams;
                        //generace zapasu
                        t.Matches.Clear();
                        int matchId = 1;
                        for (int i = 0; i < t.TeamIds.Count; i++) {
                            for (int j = i + 1; j < t.TeamIds.Count; j++) {
                                t.Matches.Add(new Match {
                                    Id = matchId++,
                                    HomeTeamId = t.TeamIds[i],
                                    AwayTeamId = t.TeamIds[j],
                                    Title = $"{Teams.First(te => te.Id == t.TeamIds[i]).Name} vs {Teams.First(te => te.Id == t.TeamIds[j]).Name}"
                                });
                            }
                        }

                        if (!isEdit) {
                            await TournamentsCol.InsertAsync(t);
                            Tournaments.Add(t);
                        } else {
                            await TournamentsCol.UpdateAsync(t);
                        }

                        UpdateDataList();
                        StatusText.Text = $"Turnaj '{t.Title}' uložen ({t.Matches.Count} zápasů).";
                        window.Close();
                    } catch (Exception ex) {
                        MessageBox.Show($"Chyba: {ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
            }
        }

    }
}