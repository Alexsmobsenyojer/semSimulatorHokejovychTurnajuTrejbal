using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace semSimulatorHokejovychTurnajuTrejbal
{
    public enum Position {
        C, LW, RW, LD, RD, Goalie
    }
    public enum Role {
        Tvurce, Sniper, Vsestranny, Ofenzivni, Defenzivni
    }
    class Player {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public int Number { get; set; }
        public Position Position { get; set; }
        public Role Role { get; set; }
        public int Shooting { get; set; }
        public int Passing { get; set; }
        public int Defending { get; set; }
        public int Skating { get; set; }
        public int Overall { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int Shots { get; set; }
        public List<Role> getRoles(Position selectedPosition) {
            return Enum.GetValues(typeof(Role)).Cast<Role>().Where(role=>isForPosition(role, selectedPosition)).ToList();
        }
        public static bool isForPosition(Role role, Position position) {
            switch (position) {
                case Position.C:
                case Position.LW:
                case Position.RW:
                    return role == Role.Tvurce || role == Role.Sniper || role == Role.Vsestranny;
                case Position.LD:
                case Position.RD:
                    return role == Role.Ofenzivni || role == Role.Defenzivni || role == Role.Vsestranny;
                case Position.Goalie:
                    return false;
                default:
                    return false;
            }
        }
    }
}
