using semSimulatorHokejovychTurnajuTrejbal.ModelView;
using System;
using System.Collections.Generic;
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
    /// Interakční logika pro CreateTeamWindow.xaml
    /// </summary>
    public partial class CreateTeamWindow : Window {
        public CreateTeamWindow() {
            InitializeComponent();
            DataContext = new CreateTeamViewModel(onSave: team => Close(), onCancel: () => Close());
        }
    }
}
