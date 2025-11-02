using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace semSimulatorHokejovychTurnajuTrejbal {
    partial class Simulation(Match match, Canvas canvas) {
        private readonly Random _rand = new();
        private List<Skater> homeCenters, homeLeftWingers, homeRightWingers, homeLeftDefensemen, homeRightDefensemen, 
            awayCenters, awayLeftWingers, awayRightWingers, awayLeftDefensemen, awayRightDefensemen;
        private Goalie homeGoalie, awayGoalie;

        private List<Skater> PickSkaters(Team team, short number, Position position) {
            return team.Players
                .OfType<Skater>()
                .Where(Skater => Skater.Position == position)
                .OrderByDescending(Skater => Skater.Overall)
                .Take(number)
                .ToList();
        }
        private Goalie PickStartingGoalie(Team team) {
            var goalies = team.Players
                .OfType<Goalie>()
                .OrderByDescending(g => g.Overall)
                .Take(2)
                .ToList();
            return goalies.Count == 1 ? goalies[0] : _rand.NextDouble() < 0.7 ? goalies[0] : goalies[1];
        }
        private void DrawPlayer(Player player, double centerX, double centerY, Brush fill) {
            const double radius = 18;
            const double fontSize = 20;

            var circle = new Ellipse {Width = radius * 2,Height = radius * 2,Fill = fill,
                Stroke = Brushes.Black,StrokeThickness = 2,Tag = "Player"};
            var text = new TextBlock {Text = player.Number.ToString(),Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,FontSize = fontSize,Tag = "Player"};
            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double textWidth = text.DesiredSize.Width;
            double textHeight = text.DesiredSize.Height;
            Canvas.SetLeft(circle, centerX - radius);
            Canvas.SetTop(circle, centerY - radius);
            Canvas.SetLeft(text, centerX - textWidth / 2);
            Canvas.SetTop(text, centerY - textHeight / 2);
            canvas.Children.Add(circle);
            canvas.Children.Add(text);
        }

        private void ClearPlayers() {
            for (int i = canvas.Children.Count - 1; i >= 0; i--) {
                if (canvas.Children[i] is FrameworkElement el && el.Tag?.ToString() == "Player")
                    canvas.Children.RemoveAt(i);
            }
        }

        public void SimulateMinute() {
            ClearPlayers();
            DrawPlayer(homeGoalie, 200, 600, Brushes.Navy);
            DrawPlayer(awayGoalie, 200, 100, Brushes.Crimson);
            int forwardLine = _rand.Next(100) switch {
                < 40 => 0,
                < 70 => 1,
                < 90 => 2,
                _ => 3
            };
            int defenseLine = _rand.Next(100) switch {
                < 50 => 0,
                < 80 => 1,
                _ => 2
            };
            DrawPlayer(homeLeftWingers[forwardLine], 120, 520, Brushes.Navy);
            DrawPlayer(homeCenters[forwardLine], 200, 540, Brushes.Navy);
            DrawPlayer(homeRightWingers[forwardLine], 280, 520, Brushes.Navy);
            DrawPlayer(homeLeftDefensemen[forwardLine], 140, 600, Brushes.Navy);
            DrawPlayer(homeRightDefensemen[forwardLine], 260, 600, Brushes.Navy);
            DrawPlayer(awayLeftWingers[forwardLine], 120, 180, Brushes.Crimson);
            DrawPlayer(awayCenters[forwardLine], 200, 160, Brushes.Crimson);
            DrawPlayer(awayRightWingers[forwardLine], 280, 180, Brushes.Crimson);
            DrawPlayer(awayLeftDefensemen[defenseLine], 140, 100, Brushes.Crimson);
            DrawPlayer(awayRightDefensemen[defenseLine], 260, 100, Brushes.Crimson);
        }
        public void StartMatch() {
            homeCenters = PickSkaters(match.HomeTeam, 4, Position.C);
            homeLeftWingers = PickSkaters(match.HomeTeam, 4, Position.LW);
            homeRightWingers = PickSkaters(match.HomeTeam, 4, Position.RW);
            homeLeftDefensemen = PickSkaters(match.HomeTeam, 3, Position.LD);
            homeRightDefensemen = PickSkaters(match.HomeTeam, 3, Position.RD);
            homeGoalie = PickStartingGoalie(match.HomeTeam);
            awayCenters = PickSkaters(match.AwayTeam, 4, Position.C);
            awayLeftWingers = PickSkaters(match.AwayTeam, 4, Position.LW);
            awayRightWingers = PickSkaters(match.AwayTeam, 4, Position.RW);
            awayLeftDefensemen = PickSkaters(match.AwayTeam, 3, Position.LD);
            awayRightDefensemen = PickSkaters(match.AwayTeam, 3, Position.RD);
            awayGoalie = PickStartingGoalie(match.AwayTeam);
        }

    }
}
