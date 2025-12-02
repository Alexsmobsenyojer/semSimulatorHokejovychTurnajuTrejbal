using semSimulatorHokejovychTurnajuTrejbal.ModelView;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace semSimulatorHokejovychTurnajuTrejbal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
            DataContext = new MainViewModel();
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

    }
}