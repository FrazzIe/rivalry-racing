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
        [EventHandler("Race.Setup")]
        private void SetupRace([FromSource] Player player, Vector3 start, Vector3 end)
        {
            if (races.ContainsKey(player.Handle))
            {
                Race race = races[player.Handle];

                race.StartPoint = start;
                race.EndPoint = end;

                string raceJson = JsonConvert.SerializeObject(race);

                for (int i = 0; i < race.Participants.Count; i++)
                {
                    Player _player = Players[int.Parse(race.Participants[i])];

                    _player.TriggerEvent("Race.Sync", raceJson);
                }
            }

        }
        [EventHandler("Race.End")]
        private void EndRace([FromSource] Player player, string raceId)
        {
            if (races.ContainsKey(raceId))
            {
                var messageObject = new
                {
                    color = new[] { 255, 0, 0 },
                    multiline = true,
                    args = new[] { "Race", "" },
                };

                Race race = races[raceId];

                race.Placements.Add(player.Handle);
                race.Participants.Remove(player.Handle);

                if (!race.Finished)
                {
                    race.Finished = true;

                    messageObject.args[1] = player.Name + " won the race!";

                    for (int i = 0; i < race.Participants.Count; i++)
                    {
                        Player _player = Players[int.Parse(race.Participants[i])];

                        _player.TriggerEvent("chat:addMessage", messageObject);
                    }

                    for (int i = 0; i < race.Placements.Count; i++)
                    {
                        Player _player = Players[int.Parse(race.Placements[i])];

                        _player.TriggerEvent("chat:addMessage", messageObject);
                    }
                }
                else
                {
                    messageObject.args[1] = player.Name + " came in " + race.Placements.Count + "!";

                    for (int i = 0; i < race.Participants.Count; i++)
                    {
                        Player _player = Players[int.Parse(race.Participants[i])];

                        _player.TriggerEvent("chat:addMessage", messageObject);
                    }

                    for (int i = 0; i < race.Placements.Count; i++)
                    {
                        Player _player = Players[int.Parse(race.Placements[i])];

                        _player.TriggerEvent("chat:addMessage", messageObject);
                    }
                }

                players.Remove(player.Handle);
                player.TriggerEvent("Race.Sync", JsonConvert.SerializeObject(null));


                if (race.Participants.Count == 0)
                {
                    races.Remove(raceId);
                }
            }
        }
        [EventHandler("playerDropped")]
        private void OnPlayerDropped([FromSource] Player player)
        {
            var messageObject = new
            {
                color = new[] { 255, 0, 0 },
                multiline = true,
                args = new[] { "Race", "" },
            };

            if (races.ContainsKey(player.Handle))
            {
                Race race = races[player.Handle];

                messageObject.args[1] = string.Format("Race ^*^1{0} ^r^0was disolved by the creator!", player.Handle);

                string raceJson = JsonConvert.SerializeObject(null);

                if (!race.Started)
                {
                    for (int i = 0; i < race.Participants.Count; i++)
                    {
                        Player _player = Players[int.Parse(race.Participants[i])];

                        players.Remove(race.Participants[i]);

                        _player.TriggerEvent("chat:addMessage", messageObject);
                        _player.TriggerEvent("Race.Sync", raceJson);
                    }

                    races.Remove(player.Handle);
                }
                else
                {
                    for (int i = 0; i < race.Participants.Count; i++)
                    {
                        Player _player = Players[int.Parse(race.Participants[i])];

                        players.Remove(race.Participants[i]);

                        _player.TriggerEvent("chat:addMessage", messageObject);
                        _player.TriggerEvent("Race.Sync", raceJson);
                    }

                    for (int i = 0; i < race.Placements.Count; i++)
                    {
                        Player _player = Players[int.Parse(race.Placements[i])];

                        players.Remove(race.Placements[i]);

                        _player.TriggerEvent("chat:addMessage", messageObject);
                        _player.TriggerEvent("Race.Sync", raceJson);
                    }

                    races.Remove(player.Handle);
                }
            }

            if (players.ContainsKey(player.Handle))
            {
                string raceId = players[player.Handle];

                if (races.ContainsKey(raceId))
                {
                    Race playerRace = races[raceId];

                    playerRace.Participants.Remove(raceId);

                    string raceJson = JsonConvert.SerializeObject(playerRace);

                    messageObject.args[1] = string.Format("Racer ^*^1{0} ^r^0left the race!", player.Handle);

                    for (int i = 0; i < playerRace.Participants.Count; i++)
                    {
                        Player _player = Players[int.Parse(playerRace.Participants[i])];

                        _player.TriggerEvent("chat:addMessage", messageObject);
                        _player.TriggerEvent("Race.Sync", raceJson);
                    }

                    for (int i = 0; i < playerRace.Placements.Count; i++)
                    {
                        Player _player = Players[int.Parse(playerRace.Placements[i])];

                        _player.TriggerEvent("chat:addMessage", messageObject);
                    }

                    players.Remove(player.Handle);
                }
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
                        } else if(race.Participants.Count == 0)
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
                        if(race != null && raceId != null)
                        {
                            if (race.Participants.Count > 1)
                            {
                                if (race.Ready())
                                {
                                    string raceJson = JsonConvert.SerializeObject(race);

                                    for (int i = 0; i < race.Participants.Count; i++)
                                    {
                                        Player _player = Players[int.Parse(race.Participants[i])];

                                        Debug.WriteLine("Starting race for player {0}", race.Participants[i]);

                                        _player.TriggerEvent("Race.Start", raceJson);
                                    }
                                } else
                                {
                                    messageObject.args[1] = "The host needs to pick a finish line!";
                                    player.TriggerEvent("chat:addMessage", messageObject);
                                }
                            } else
                            {
                                messageObject.args[1] = "Not enough participants!";
                                player.TriggerEvent("chat:addMessage", messageObject);
                            }
                        } else
                        {
                            messageObject.args[1] = "You need to be the creator of a race to use this!";
                            player.TriggerEvent("chat:addMessage", messageObject);
                        }
                        break;
                    case "join":
                        if (args.Length > 1)
                        {
                            if (race == null && raceId == null)
                            {
                                if(races.ContainsKey(args[1]))
                                {
                                    Race playerRace = races[args[1]];
                                    players.Add(player.Handle, args[1]);
                                    playerRace.Participants.Add(player.Handle);
                                    player.TriggerEvent("Race.Sync", JsonConvert.SerializeObject(race));
                                }
                            } else
                            {
                                messageObject.args[1] = "You are already in a race!";
                                player.TriggerEvent("chat:addMessage", messageObject);
                            }
                        } else
                        {
                            messageObject.args[1] = "/race join [id]!";
                            player.TriggerEvent("chat:addMessage", messageObject);
                        }
                        break;
                    case "leave":
                        if (race == null)
                        {
                            if (raceId != null)
                            {
                                if (races.ContainsKey(raceId))
                                {
                                    Race playerRace = races[raceId];

                                    playerRace.Participants.Remove(raceId);

                                    string raceJson = JsonConvert.SerializeObject(playerRace);

                                    messageObject.args[1] = string.Format("Racer ^*^1{0} ^r^0left the race!", player.Handle);

                                    for (int i = 0; i < playerRace.Participants.Count; i++)
                                    {
                                        Player _player = Players[int.Parse(playerRace.Participants[i])];

                                        _player.TriggerEvent("chat:addMessage", messageObject);
                                        _player.TriggerEvent("Race.Sync", raceJson);
                                    }

                                    for (int i = 0; i < playerRace.Placements.Count; i++)
                                    {
                                        Player _player = Players[int.Parse(playerRace.Placements[i])];

                                        _player.TriggerEvent("chat:addMessage", messageObject);
                                    }

                                    players.Remove(player.Handle);
                                }
                            }
                            else
                            {
                                messageObject.args[1] = "You aren't in a race!";
                                player.TriggerEvent("chat:addMessage", messageObject);
                            }
                        } else
                        {
                            messageObject.args[1] = string.Format("Race ^*^1{0} ^r^0was disolved by the creator!", player.Handle);

                            string raceJson = JsonConvert.SerializeObject(null);

                            if (!race.Started)
                            {
                                for (int i = 0; i < race.Participants.Count; i++)
                                {
                                    Player _player = Players[int.Parse(race.Participants[i])];

                                    players.Remove(race.Participants[i]);

                                    _player.TriggerEvent("chat:addMessage", messageObject);
                                    _player.TriggerEvent("Race.Sync", raceJson);
                                }

                                races.Remove(player.Handle);
                            } else
                            {
                                for (int i = 0; i < race.Participants.Count; i++)
                                {
                                    Player _player = Players[int.Parse(race.Participants[i])];

                                    players.Remove(race.Participants[i]);

                                    _player.TriggerEvent("chat:addMessage", messageObject);
                                    _player.TriggerEvent("Race.Sync", raceJson);
                                }

                                for (int i = 0; i < race.Placements.Count; i++)
                                {
                                    Player _player = Players[int.Parse(race.Placements[i])];

                                    players.Remove(race.Placements[i]);

                                    _player.TriggerEvent("chat:addMessage", messageObject);
                                    _player.TriggerEvent("Race.Sync", raceJson);
                                }

                                races.Remove(player.Handle);
                            }
                        }
                        break;
                    default:
                        messageObject.args[1] = "Invalid syntax: ^*^1create^r^0, ^*^1start, ^*^1join, ^r^0or ^*^1leave ^r^0are the only accepted arguments!!";
                        player.TriggerEvent("chat:addMessage", messageObject);
                        break;
                }
            } else
            {
                messageObject.args[1] = "Invalid syntax: ^*^1create^r^0, ^*^1start, ^*^1join, ^r^0or ^*^1leave ^r^0are the only accepted arguments!!";
                player.TriggerEvent("chat:addMessage", messageObject);
            }
        }
    }
}
