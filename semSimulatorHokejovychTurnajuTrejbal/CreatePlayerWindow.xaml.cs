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
    /// Interakční logika pro CreatePlayerWindow.xaml
    /// </summary>
    public partial class CreatePlayerWindow : Window {
        public CreatePlayerWindow(ObservableCollection<Team> allTeams) {
            InitializeComponent();
            DataContext = new CreatePlayerViewModel(allTeams: allTeams, onSave: player => DialogResult = true);
        }
    }
}
