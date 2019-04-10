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
        [Command("race")]
        private void OnRaceCommand([FromSource] Player player, string[] args)
        {
            Race race = null;
            string raceId = null;

            if (races.ContainsKey(player.Handle))
            {
                race = races[player.Handle];
            }

            if(players.ContainsKey(player.Handle))
            {
                raceId = players[player.Handle];
            }

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
                        if (race == null)
                        {
                            if (raceId == null)
                            {
                                int bet = 0;
                                int.TryParse(args.Length > 1 ? args[1] : "0", out bet);

                                race = new Race(player.Handle, bet);
                                races.Add(player.Handle, race);
                                players.Add(player.Handle, player.Handle);

                                player.TriggerEvent("Race.Sync", JsonConvert.SerializeObject(race));

                                messageObject.args[1] = string.Format("Race ^*^3{0} created, share your code with the participants!", player.Handle);
                                player.TriggerEvent("chat:addMessage", messageObject);

                                Debug.WriteLine("Created race {0}", player.Handle);
                            }
                            else
                            {
                                messageObject.args[1] = "You are already in someone elses race!";
                                player.TriggerEvent("chat:addMessage", messageObject);
                            }
                        } else if(race.Placements.Count >= race.Participants.Count)
                        {
                            races.Remove(player.Handle);
                            messageObject.args[1] = "Try again!";
                            player.TriggerEvent("chat:addMessage", messageObject);
                        } else
                        {
                            messageObject.args[1] = "You already created a race!";
                            player.TriggerEvent("chat:addMessage", messageObject);
                        }
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
