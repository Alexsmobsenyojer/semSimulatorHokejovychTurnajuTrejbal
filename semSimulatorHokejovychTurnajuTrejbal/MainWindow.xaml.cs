using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiteDB.Async;

namespace semSimulatorHokejovychTurnajuTrejbal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            LiteDatabaseAsync db = new("Filename=HockeyDate.db;");
            StatusText.Text = "Připraveno";
        }

        private void SaveDataClick(object sender, RoutedEventArgs e) {

        }

        private void LoadDataClick(object sender, RoutedEventArgs e) {

        }

        private void ExitClick(object sender, RoutedEventArgs e) {

        }

        private void CreateEditEntityClick(object sender, RoutedEventArgs e) {

        }

        private void AutoCreateTournamentClick(object sender, RoutedEventArgs e){

        }

        private void StartSimulationClick(object sender, RoutedEventArgs e){

        }

        private void StopSimulationClick(object sender, RoutedEventArgs e) {

        }

        private void SkipSimulationClick(object sender, RoutedEventArgs e) {

        }

        private void FilterDataClick(object sender, RoutedEventArgs e) {

        }
    }
}