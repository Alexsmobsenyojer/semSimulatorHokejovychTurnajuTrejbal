using semSimulatorHokejovychTurnajuTrejbal.ModelView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace semSimulatorHokejovychTurnajuTrejbal {
    /// <summary>
    /// Interakční logika pro CreateTournamentWindow.xaml
    /// </summary>
    public partial class CreateTournamentWindow : Window {
        public CreateTournamentWindow(ObservableCollection<Team> allTeams) {
            InitializeComponent();
            DataContext = new CreateTournamentViewModel(allTeams:allTeams, onSave: team => Close(), onCancel: () => Close());
        }
    }
}
