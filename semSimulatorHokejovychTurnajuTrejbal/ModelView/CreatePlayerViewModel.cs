using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;


namespace semSimulatorHokejovychTurnajuTrejbal.ModelView {
    public partial class CreatePlayerViewModel : ObservableObject {
        [ObservableProperty] private string fullName = "";
        [ObservableProperty] private int number = 99;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AvailableRoles))]
        [NotifyPropertyChangedFor(nameof(IsSkater))] 
        private Position position = Position.C;
        [ObservableProperty] private Role role = Role.Playmaker;
        [ObservableProperty] private int shooting = 50;
        [ObservableProperty] private int passing = 50;
        [ObservableProperty] private int defending = 50;
        [ObservableProperty] private int skating = 50;
        [ObservableProperty] private int overall = 80;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private Team? selectedTeam;
        public ObservableCollection<Team> AllTeams { get; }
        public bool IsSkater => Position != Position.G;
        public bool HasTeam() => SelectedTeam is not null;
        public List<Role> AvailableRoles => Skater.GetValidRoles(Position);
        public List<Position> Positions => Enum.GetValues<Position>().ToList();

        private readonly Action<Player> _onSave;
        private readonly Action _onCancel;

        public CreatePlayerViewModel(IEnumerable<Team> allTeams, Action<Player> onSave, Action onCancel) {
            _onSave = onSave;
            _onCancel = onCancel;
            AllTeams = new ObservableCollection<Team>(allTeams);
        }

        [RelayCommand(CanExecute = nameof(HasTeam))]
        private void Save() {
            Player player = IsSkater ? new Skater() : new Goalie();

            player.FullName = FullName;
            player.Number = Number;
            player.TeamId = SelectedTeam!.Id;
            if (player is Skater skater) {
                skater.Position = Position;
                skater.Role = Role;
                skater.Shooting = Shooting;
                skater.Passing = Passing;
                skater.Defending = Defending;
                skater.Skating = Skating;
            } else if (player is Goalie goalie) {
                goalie.Overall = Overall;
            }

            _onSave(player);
        }

        [RelayCommand]
        private void Cancel() => _onCancel();
    }
}
