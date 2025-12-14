using LiteDB.Async;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace semSimulatorHokejovychTurnajuTrejbal.Model {
    public class HockeyService {
        private LiteDatabaseAsync _db;
        private readonly string _dbPath = "HockeyDatabase.db";

        public ILiteCollectionAsync<Player> PlayersTable => _db.GetCollection<Player>("players");
        public ILiteCollectionAsync<Team> TeamsTable => _db.GetCollection<Team>("teams");
        public ILiteCollectionAsync<Tournament> TournamentsTable => _db.GetCollection<Tournament>("tournaments");

        public ObservableCollection<Player> Players { get; } = new();
        public ObservableCollection<Team> Teams { get; } = new();
        public ObservableCollection<Tournament> Tournaments { get; } = new();

        public HockeyService() {
            _db = new LiteDatabaseAsync($"Filename={_dbPath}");
            _ = LoadAllAsync();
        }

        public async Task LoadAllAsync() {
            var teams = await TeamsTable.FindAllAsync();
            var players = await PlayersTable.Include(p => p.Team).FindAllAsync();
            var tournaments = await TournamentsTable.FindAllAsync();
            Players.Clear();
            Teams.Clear();
            Tournaments.Clear();
            foreach (var team in teams) Teams.Add(team);
            foreach (var player in players) Players.Add(player);
            foreach (var tournament in tournaments) Tournaments.Add(tournament);
        }

        public async Task ExportToJsonAsync(IEnumerable<Player> players, IEnumerable<Team> teams, IEnumerable<Tournament> tournaments) {
            var dialog = new SaveFileDialog { FileName = "hockey_data.json", Filter = "JSON soubory (*.json)|*.json|Všechny soubory (*.*)|*.*" };
            if (dialog.ShowDialog() == true) {
                foreach (var p in players) p.TeamId = teams.FirstOrDefault(t => t.Id == p.Team!.Id)!.Id;
                var data = new JsonData {
                    Players = players.ToList(),
                    Teams = teams.ToList(),
                    Tournaments = tournaments.ToList()
                };
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(dialog.FileName, json);
            }
        }

        public async Task ImportFromJsonAsync() {
            var dialog = new OpenFileDialog { Filter = "JSON soubory (*.json)|*.json|Všechny soubory (*.*)|*.*" };
            if (dialog.ShowDialog() == true) {
                try {
                    var json = await File.ReadAllTextAsync(dialog.FileName);
                    var data = JsonSerializer.Deserialize<JsonData>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (data is not null) {
                        if (data.Players?.Count != 0) {
                            data.Players!.ForEach(p => p.Team = data.Teams.FirstOrDefault(t => t.Id == p.TeamId));
                            await PlayersTable.InsertBulkAsync(data.Players); 
                        }
                        if (data.Teams?.Count != 0) await TeamsTable.InsertBulkAsync(data.Teams);
                        if (data.Tournaments?.Count != 0) await TournamentsTable.InsertBulkAsync(data.Tournaments);
                    }
                } catch (Exception ex) {
                    MessageBox.Show($"Chyba při načítání: {ex.Message}{ex.InnerException?.Message}");
                }
            }
        }

        public void DeleteDatabase() {
            _db.Dispose();
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);
            _db = new LiteDatabaseAsync($"Filename={_dbPath}");
        }

        public async Task AddPlayerAsync(Player player) {
            await PlayersTable.InsertAsync(player);
            Players.Add(player);
        }
        public async Task AddTeamAsync(Team team) {
            await TeamsTable.InsertAsync(team);
            Teams.Add(team);
        }
        public async Task AddTournamentAsync(Tournament tournament) {
            await TournamentsTable.InsertAsync(tournament);
            Tournaments.Add(tournament);
        }
        public async Task UpdatePlayerAsync(Player player) {
            await PlayersTable.UpdateAsync(player);
        }
        public async Task UpdateTeamAsync(Team team) {
            await TeamsTable.UpdateAsync(team);
        }
        public async Task UpdateTournamentAsync(Tournament tournament) {
            await TournamentsTable.UpdateAsync(tournament);
        }
        public async Task DeletePlayerAsync(Player player) {
            await PlayersTable.DeleteAsync(player.Id);
            Players.Remove(player);
        }
        public async Task DeleteTeamAsync(Team team) {
            await TeamsTable.DeleteAsync(team.Id);
            Teams.Remove(team);
        }
        public async Task DeleteTournamentAsync(Tournament tournament) {
            await TournamentsTable.DeleteAsync(tournament.Id);
            Tournaments.Remove(tournament);
        }
    }
}
