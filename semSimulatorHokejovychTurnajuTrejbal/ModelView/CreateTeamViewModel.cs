using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace semSimulatorHokejovychTurnajuTrejbal.ModelView {
    public partial class CreateTeamViewModel : ObservableObject {
        [ObservableProperty] private string name = "";
        private readonly Action<Team> _onSave;
        private readonly Action _onCancel;

        public CreateTeamViewModel( Action<Team> onSave, Action onCancel) {
            _onSave = onSave;
            _onCancel = onCancel;
        }

        [RelayCommand]
        private void Save() {
            _onSave(new Team { Name = Name });
        }

        [RelayCommand]
        private void Cancel() => _onCancel();
    }
}
