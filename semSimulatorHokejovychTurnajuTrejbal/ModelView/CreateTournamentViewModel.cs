using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;


namespace semSimulatorHokejovychTurnajuTrejbal.ModelView {
    public partial class CreateTournamentViewModel : ObservableObject {
        [ObservableProperty] private string title = "";
        [ObservableProperty] private List<Team> selectedTeams = new();
        public ObservableCollection<Team> AllTeams { get; }
        private readonly Action<Tournament> _onSave;
        private readonly Action _onCancel;

        public CreateTournamentViewModel( IEnumerable<Team> allTeams, Action<Tournament> onSave, Action onCancel) {
            _onSave = onSave;
            _onCancel = onCancel;
            AllTeams = new ObservableCollection<Team>(allTeams);
        }

        [RelayCommand]
        private void Save() {
            var t = new Tournament{ Title = Title, TeamIds = SelectedTeams.Select(x => x.Id).ToList()};

            int matchId = 1;
            for (int i = 0; i < t.TeamIds.Count; i++)
                for (int j = i + 1; j < t.TeamIds.Count; j++)
                    t.Matches.Add(new Match {
                        Id = matchId++,
                        HomeTeamId = t.TeamIds[i],
                        AwayTeamId = t.TeamIds[j]
                    });

            _onSave(t);
        }

        [RelayCommand]
        private void Cancel() => _onCancel();
    }
}
