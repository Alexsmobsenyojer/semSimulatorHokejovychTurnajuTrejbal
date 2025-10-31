using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace semSimulatorHokejovychTurnajuTrejbal
{
    public enum Position { C, LW, RW, LD, RD, Goalie }
    public enum Role { Tvurce, Sniper, Vsestranny, Ofenzivni, Defenzivni }
    public sealed class PlayerStats {
        public int Goals { get; set; } = 0;
        public int Assists { get; set; } = 0;
        public int Points => Goals + Assists;
        public int Shots { get; set; } = 0;
        /*možnosti dalšího rozšíření statistik (hity(záleží na výšce a váze), čas na ledě(podle hodnocení), 
          +- (na ledě při gólu), trestné minuty, )*/
    }
    public sealed class GoalStats {
        public int Saves { get; set; } = 0;
        public int GoalsAgainst { get; set; } = 0;
        public double SavePercentage => Saves + GoalsAgainst == 0 ? 100.0 : (double)Saves / (Saves + GoalsAgainst) * 100;
        public int Shutouts { get; set; } = 0;
    }
    public abstract class Player {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public int Number { get; set; }

    }
    class Skater : Player {
        public Position Position { get; set; }
        public Role Role { get; set; }
        public int Shooting { get; set; }
        public int Passing { get; set; }
        public int Defending { get; set; }
        public int Skating { get; set; }
        public int Overall => (Shooting + Passing + Defending + Skating) / 4;
        public PlayerStats Stats { get; set; }= new();
        public List<Role> getRoles(Position selectedPosition) {
            return Enum.GetValues(typeof(Role)).Cast<Role>().Where(role => isForPosition(role, selectedPosition)).ToList();
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
                default:
                    return false;
            }
        }
    }
    class Goalie : Player {
        public int Overall { get; set; }
        public GoalStats Stats { get; set; } = new();
    }
}
