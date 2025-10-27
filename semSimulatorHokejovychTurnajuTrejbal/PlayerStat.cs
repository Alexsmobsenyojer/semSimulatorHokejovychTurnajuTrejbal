using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace semSimulatorHokejovychTurnajuTrejbal{
    class PlayerStat{
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int Points => Goals + Assists;
        public int Shots { get; set; }
        /*možnosti dalšího rozšíření statistik (hity(záleží na výšce a váze), čas na ledě(podle hodnocení), 
          +- (na ledě při gólu), trestné minuty, )*/
    }
}
