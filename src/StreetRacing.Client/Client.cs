using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using StreetRacing.Shared;

namespace StreetRacing.Client
{
    public class Client : BaseScript
    {
        Race race;

        public Client()
        {

        }
        [Tick]
        private async Task RaceWatcher()
        {
            if (race != null)
            {
            }
        }
    }
}
