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
        [Command("race")]
        private void OnRaceCommand([FromSource] Player player, string[] args)
        {
            var messageObject = new
            {
                color = new[] { 255, 0, 0 },
                multiline = true,
                args = new[] { "Race", "" },
            };
            if (args.Length > 0) {
                switch (args[0])
                {
                    case "create":
                        break;
                    case "start":
                        break;
                    case "join":
                        break;
                    case "cancel":
                        break;
                    case "leave":
                        break;
                    default:
                        messageObject.args[1] = "Invalid syntax: ^*^1create^r^0, ^*^1start, ^*^1join, ^*^1cancel ^r^0or ^*^1leave ^r^0are the only accepted arguments!!";
                        player.TriggerEvent("chat:addMessage", messageObject);
                        break;
            } else
            {
                messageObject.args[1] = "Invalid syntax: ^*^1create^r^0, ^*^1start, ^*^1join, ^*^1cancel ^r^0or ^*^1leave ^r^0are the only accepted arguments!!";
                player.TriggerEvent("chat:addMessage", messageObject);
            }
        }
    }
}
