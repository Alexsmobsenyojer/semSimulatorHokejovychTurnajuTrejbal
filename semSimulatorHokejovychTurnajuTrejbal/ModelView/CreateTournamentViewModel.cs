using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;


namespace semSimulatorHokejovychTurnajuTrejbal.ModelView {
    public partial class TeamSelection : ObservableObject {
        public Team Team { get; }
        [ObservableProperty] private bool isSelected;
        public TeamSelection(Team team) => Team = team;
    };
    public partial class CreateTournamentViewModel : ObservableObject {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string title = "";
        public ObservableCollection<Team> AllTeams { get; }
        private readonly Action<Tournament> _onSave;
        public ObservableCollection<TeamSelection> TeamSelections { get; }


        public CreateTournamentViewModel( IEnumerable<Team> allTeams, Action<Tournament> onSave) {
            _onSave = onSave;
            AllTeams = new ObservableCollection<Team>(allTeams);
            TeamSelections = new ObservableCollection<TeamSelection>(allTeams.Select(t => new TeamSelection(t)));
        }
        private bool CanSave() => !string.IsNullOrWhiteSpace(Title);
        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save(Window window) {
            if( TeamSelections.Count(x => x.IsSelected) < 2) {
                System.Windows.MessageBox.Show("Vyberte alespoň dva týmy", "Nedostatečný počet týmů", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            var t = new Tournament{ Title = Title, TeamIds = TeamSelections.Where(x => x.IsSelected).Select(x => x.Team.Id).ToList()};

            int matchId = 1;
            for (int i = 0; i < t.TeamIds.Count; i++)
                for (int j = i + 1; j < t.TeamIds.Count; j++)
                    t.Matches.Add(new Match {
                        Id = matchId++,
                        HomeTeamId = t.TeamIds[i],
                        AwayTeamId = t.TeamIds[j],
                        Title = $"{AllTeams.First(team => team.Id == t.TeamIds[i]).Name} vs {AllTeams.First(team => team.Id == t.TeamIds[j]).Name}"
                    });

            _onSave.Invoke(t);
            window.Close();
        }

        [RelayCommand]
        private void Cancel(Window window) => window.Close();
    }
}
