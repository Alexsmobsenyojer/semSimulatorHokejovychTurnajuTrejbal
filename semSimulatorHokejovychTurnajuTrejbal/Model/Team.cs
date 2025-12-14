using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace semSimulatorHokejovychTurnajuTrejbal {
    public partial class Team : ObservableObject
    {
        public int Id { get; init; }
        public required string Name { get; set; }

        [ObservableProperty]
        private int wins = 0;

        [ObservableProperty]
        private int losses = 0;

        public void AddWin() => Wins++;

        public void AddLoss() => Losses++;
    }
}
