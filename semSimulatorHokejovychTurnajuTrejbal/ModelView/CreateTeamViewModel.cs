using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace semSimulatorHokejovychTurnajuTrejbal.ModelView {
    public partial class CreateTeamViewModel : ObservableObject {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))] 
        private string name = "";
        private readonly Action<Team> _onSave;

        public CreateTeamViewModel( Action<Team> onSave) {
            _onSave = onSave;
        }
        private bool CanSave() => !string.IsNullOrWhiteSpace(Name);
        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save(Window window) {
            _onSave?.Invoke(new Team { Name = Name.Trim() });
            window.Close();
        }

        [RelayCommand]
        private void Cancel(Window window) => window.Close();
    }
}
