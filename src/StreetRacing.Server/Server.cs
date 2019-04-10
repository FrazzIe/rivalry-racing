using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using StreetRacing.Shared;

namespace StreetRacing.Server
{
    public class Server : BaseScript
    {
        SortedList<string, string> players = new SortedList<string, string>();
        SortedList<string, Race> races = new SortedList<string, Race>();

        public Server()
        {

        }
    }
}
