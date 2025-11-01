using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace semSimulatorHokejovychTurnajuTrejbal
{
    public enum Position { C, LW, RW, LD, RD}
    public enum Role { Playmaker, Sniper, TwoWay, Offensive, Defensive }
    public sealed class SkaterStats {
        public int Goals { get; private set; } = 0;
        public int Assists { get; private set; } = 0;
        public int Points => Goals + Assists;
        public int Shots { get; private set; } = 0;

        public void addGoal() {
            Goals++;
            Shots++;
        }
        public void addAssist() {
            Assists++;
        }
        public void addShot() {
            Shots++;
        }
        /*možnosti dalšího rozšíření statistik (hity(záleží na výšce a váze), čas na ledě(podle hodnocení), 
          +- (na ledě při gólu), trestné minuty, )*/
    }
    public sealed class GoalieStats {
        public int Saves { get; private set; } = 0;
        public int GoalsAgainst { get; private set; } = 0;
        public double SavePercentage => Saves + GoalsAgainst == 0 ? 100.0 : (double)Saves / (Saves + GoalsAgainst) * 100;
        public int Shutouts { get; private set; } = 0;

        public void addSave() {
            Saves++;
        }
        public void addGoalAgainst() {
            GoalsAgainst++;
        }
        public void addShutout() {
            Shutouts++;
        }
    }
    public abstract class Player {
        public int Id { get; init; }
        public required string FullName { get; init; }
        public int Number { get; init; }

    }
    class Skater : Player {
        public Position Position { get; set; }
        public Role Role { get; set; }
        public int Shooting { get; init; }
        public int Passing { get; init; }
        public int Defending { get; init; }
        public int Skating { get; init; }
        public int Overall => (Shooting + Passing + Defending + Skating) / 4;
        public SkaterStats Stats { get; set; }= new();
        public static List<Role> getValidRoles(Position selectedPosition) {
            return Enum.GetValues<Role>().Where(role => isForPosition(role, selectedPosition)).ToList();
        }
        public static bool isForPosition(Role role, Position position) {
            switch (position) {
                case Position.C:
                case Position.LW:
                case Position.RW:
                    return role == Role.Playmaker || role == Role.Sniper || role == Role.TwoWay;
                case Position.LD:
                case Position.RD:
                    return role == Role.Offensive || role == Role.Defensive || role == Role.TwoWay;
                default:
                    return false;
            }
        }
    }
    class Goalie : Player {
        public int Overall { get; init; }
        public GoalieStats Stats { get; set; } = new();
    }
}
