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
        string[] commandHelp = new[] {
            "^*^3/race ^4create^0^r [bet amount (optional)] [password (optional)]",
            "^*^3/race ^4start^0^r -> starts a race",
            "^*^3/race ^4join^0^r [race id] [race password (optional)]",
            "^*^3/race ^4lock^0^r -> prevent people from joining a race (toggle)",
            "^*^3/race ^4leave^0^r -> disband your race or leave a race that you joined",
            "^*^3/race ^4info^0^r -> displays information about a race",
        };

        public Server()
        {

        }

        public static string GetOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return "th";
            }

            switch (num % 10)
            {
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
                default:
                    return "th";
            }

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
                dynamic name = Exports["core"].GetCharacterName(player.Handle);

                race.Placements.Add(player.Handle);
                race.Participants.Remove(player.Handle);

                if (!race.Finished)
                {
                    race.Finished = true;

                    messageObject.args[1] = string.Format("^*^3{0}^r^0 won the race!", name);

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

                    if (race.Bet > 0)
                    {
                        Exports["core"].AddPlayerCash(player.Handle, race.Pot);

                        messageObject.args[1] = string.Format("You got ^*^2$^0{0}^r^0 for winning", race.Pot);

                        player.TriggerEvent("chat:addMessage", messageObject);
                    }
                }
                else
                {
                    messageObject.args[1] = string.Format("^*^3{0}^r^0 came in ^*^1{1}^0{2}!", name, race.Placements.Count, GetOrdinal(race.Placements.Count));

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
                player.TriggerEvent("Race.Reset");

                if (race.Participants.Count == 0)
                {
                    messageObject.args[1] = string.Format("Race ^*^1{0} ^r^0has concluded", raceId);

                    for (int i = 0; i < race.Placements.Count; i++)
                    {
                        Player _player = Players[int.Parse(race.Placements[i])];

                        _player.TriggerEvent("chat:addMessage", messageObject);
                    }

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

                if (!race.Started)
                {
                    for (int i = 0; i < race.Participants.Count; i++)
                    {
                        Player _player = Players[int.Parse(race.Participants[i])];

                        players.Remove(race.Participants[i]);

                        Exports["core"].AddPlayerCash(_player.Handle, race.Bet);

                        _player.TriggerEvent("chat:addMessage", messageObject);
                        _player.TriggerEvent("Race.Reset");
                    }

                    races.Remove(player.Handle);
                }
                else
                {
                    bool finished = race.Finished;

                    for (int i = 0; i < race.Participants.Count; i++)
                    {
                        Player _player = Players[int.Parse(race.Participants[i])];

                        players.Remove(race.Participants[i]);

                        if (!finished)
                        {
                            Exports["core"].AddPlayerCash(_player.Handle, race.Bet);
                        }

                        _player.TriggerEvent("chat:addMessage", messageObject);
                        _player.TriggerEvent("Race.Reset");
                    }

                    for (int i = 0; i < race.Placements.Count; i++)
                    {
                        Player _player = Players[int.Parse(race.Placements[i])];

                        _player.TriggerEvent("chat:addMessage", messageObject);
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
                    dynamic name = Exports["core"].GetCharacterName(player.Handle);

                    if (name == null) name = "Someone";

                    playerRace.Participants.Remove(raceId);

                    messageObject.args[1] = string.Format("^*^3{0} ^r^0left the race!", name);

                    for (int i = 0; i < playerRace.Participants.Count; i++)
                    {
                        Player _player = Players[int.Parse(playerRace.Participants[i])];

                        _player.TriggerEvent("chat:addMessage", messageObject);
                    }

                    for (int i = 0; i < playerRace.Placements.Count; i++)
                    {
                        Player _player = Players[int.Parse(playerRace.Placements[i])];

                        _player.TriggerEvent("chat:addMessage", messageObject);
                    }

                    players.Remove(player.Handle);

                    if (playerRace.Participants.Count == 0)
                    {
                        messageObject.args[1] = string.Format("Race ^*^1{0} ^r^0has concluded", raceId);
                        for (int i = 0; i < playerRace.Placements.Count; i++)
                        {
                            Player _player = Players[int.Parse(playerRace.Placements[i])];

                            _player.TriggerEvent("chat:addMessage", messageObject);
                        }

                        races.Remove(raceId);
                    }
                }
            }
        }
        [Command("race")]
        private void OnRaceCommand([FromSource] Player player, string[] args)
        {
            Race race = null;
            string raceId = null;
            dynamic playerCash = Exports["core"].GetPlayerCash(player.Handle);

            if (races.ContainsKey(player.Handle))
            {
                race = races[player.Handle];
            }

            if(players.ContainsKey(player.Handle))
            {
                raceId = players[player.Handle];
            }

            if (playerCash == null) playerCash = 0;

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
                                string password = args.Length > 2 ? args[2] : "";

                                if (playerCash >= bet)
                                {
                                    if (bet > 0)
                                    {
                                        Exports["core"].RemovePlayerCash(player.Handle, bet);
                                    } else if (bet < 0)
                                    {
                                        bet = 0;
                                    }

                                    race = new Race(player.Handle, bet, password);
                                    races.Add(player.Handle, race);
                                    players.Add(player.Handle, player.Handle);

                                    player.TriggerEvent("Race.Sync", JsonConvert.SerializeObject(race));

                                    if (string.IsNullOrEmpty(password)) messageObject.args[1] = string.Format("Successfully created, share your code (^*^3{0}^r^0) with the participants!", player.Handle);
                                    else messageObject.args[1] = string.Format("Successfully created, share your code (^*^3{0}^r^0) and password (^*^3{1}^r^0) with the participants!", player.Handle, race.Password);

                                    player.TriggerEvent("chat:addMessage", messageObject);
                                } else
                                {
                                    messageObject.args[1] = "You don't have enough money for the bet!";
                                    player.TriggerEvent("chat:addMessage", messageObject);
                                }
                            }
                            else
                            {
                                messageObject.args[1] = "You are already in someone elses race!";
                                player.TriggerEvent("chat:addMessage", messageObject);
                            }
                        } else if (race.Participants.Count == 0)
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
                        if (race != null && raceId != null)
                        {
                            if (race.Participants.Count > 1)
                            {
                                if (race.Ready())
                                {
                                    messageObject.args[1] = "Starting the race!";
                                    player.TriggerEvent("chat:addMessage", messageObject);

                                    string raceJson = JsonConvert.SerializeObject(race);

                                    for (int i = 0; i < race.Participants.Count; i++)
                                    {
                                        Player _player = Players[int.Parse(race.Participants[i])];

                                        _player.TriggerEvent("Race.Start", raceJson);
                                    }

                                    race.Started = true;
                                } else
                                {
                                    messageObject.args[1] = "You need to pick a finish line!";
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
                                if (races.ContainsKey(args[1]))
                                {
                                    Race playerRace = races[args[1]];
                                    string password = args.Length > 2 ? args[2] : "";
                                    dynamic name = Exports["core"].GetCharacterName(player.Handle);

                                    if (name == null) name = "Someone";

                                    if (playerRace.Password == password) {
                                        if (!playerRace.Locked)
                                        {
                                            if (playerCash >= playerRace.Bet)
                                            {
                                                if (playerRace.Bet > 0)
                                                {
                                                    Exports["core"].RemovePlayerCash(player.Handle, playerRace.Bet);
                                                    playerRace.Pot = playerRace.Bet;
                                                }
                                                string raceJson = JsonConvert.SerializeObject(playerRace);

                                                messageObject.args[1] = string.Format("^*^3{0}^r^0 joined the race!", name);

                                                for (int i = 0; i < playerRace.Participants.Count; i++)
                                                {
                                                    Player _player = Players[int.Parse(playerRace.Participants[i])];

                                                    _player.TriggerEvent("chat:addMessage", messageObject);
                                                }

                                                for (int i = 0; i < playerRace.Placements.Count; i++)
                                                {
                                                    Player _player = Players[int.Parse(playerRace.Placements[i])];

                                                    _player.TriggerEvent("chat:addMessage", messageObject);
                                                }

                                                players.Add(player.Handle, args[1]);
                                                playerRace.Participants.Add(player.Handle);
                                                player.TriggerEvent("Race.Sync", raceJson);

                                                messageObject.args[1] = string.Format("You joined Race ^*^1{0}^r^0!", args[1]);
                                                player.TriggerEvent("chat:addMessage", messageObject);
                                            } else
                                            {
                                                messageObject.args[1] = string.Format("You need at least ^*^2$^0{0}^r to join this race!", playerRace.Bet);
                                                player.TriggerEvent("chat:addMessage", messageObject);
                                            }
                                        } else
                                        {
                                            messageObject.args[1] = "The race has stopped accepting participants!";
                                            player.TriggerEvent("chat:addMessage", messageObject);
                                        }
                                    } else
                                    {
                                        messageObject.args[1] = string.Format("You entered the wrong password!", args[1]);
                                        player.TriggerEvent("chat:addMessage", messageObject);
                                    }
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
                    case "lock":
                        if (race != null)
                        {
                            race.Locked = !race.Locked;

                            if (race.Locked) messageObject.args[1] = string.Format("Race ^*^1{0} ^r^0has now been ^*^1locked^r^0, no one will be able to join!", player.Handle);
                            else messageObject.args[1] = string.Format("Race ^*^1{0} ^r^0has now been ^*^2unlocked^r^0, people will be able to join!", player.Handle);

                            player.TriggerEvent("chat:addMessage", messageObject);
                        } else
                        {
                            messageObject.args[1] = "You aren't in a race!";
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
                                    dynamic name = Exports["core"].GetCharacterName(player.Handle);

                                    if (name == null) name = "Someone";

                                    playerRace.Participants.Remove(player.Handle);

                                    messageObject.args[1] = "You left the race!";
                                    player.TriggerEvent("chat:addMessage", messageObject);

                                    messageObject.args[1] = string.Format("^*^3{0} ^r^0left the race!", name);

                                    for (int i = 0; i < playerRace.Participants.Count; i++)
                                    {
                                        Player _player = Players[int.Parse(playerRace.Participants[i])];

                                        _player.TriggerEvent("chat:addMessage", messageObject);
                                    }

                                    for (int i = 0; i < playerRace.Placements.Count; i++)
                                    {
                                        Player _player = Players[int.Parse(playerRace.Placements[i])];

                                        _player.TriggerEvent("chat:addMessage", messageObject);
                                    }

                                    players.Remove(player.Handle);
                                    player.TriggerEvent("Race.Reset");

                                    if (playerRace.Participants.Count == 0)
                                    {
                                        messageObject.args[1] = string.Format("Race ^*^1{0} ^r^0has concluded", raceId);
                                        for (int i = 0; i < playerRace.Placements.Count; i++)
                                        {
                                            Player _player = Players[int.Parse(playerRace.Placements[i])];

                                            _player.TriggerEvent("chat:addMessage", messageObject);
                                        }

                                        races.Remove(raceId);
                                    }
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

                            if (!race.Started)
                            {
                                for (int i = 0; i < race.Participants.Count; i++)
                                {
                                    Player _player = Players[int.Parse(race.Participants[i])];

                                    players.Remove(race.Participants[i]);

                                    Exports["core"].AddPlayerCash(_player.Handle, race.Bet);

                                    _player.TriggerEvent("chat:addMessage", messageObject);
                                    _player.TriggerEvent("Race.Reset");
                                }

                                races.Remove(player.Handle);
                            } else
                            {
                                bool finished = race.Finished;

                                for (int i = 0; i < race.Participants.Count; i++)
                                {
                                    Player _player = Players[int.Parse(race.Participants[i])];

                                    players.Remove(race.Participants[i]);

                                    if(!finished)
                                    {
                                        Exports["core"].AddPlayerCash(_player.Handle, race.Bet);
                                    }

                                    _player.TriggerEvent("chat:addMessage", messageObject);
                                    _player.TriggerEvent("Race.Reset");
                                }

                                for (int i = 0; i < race.Placements.Count; i++)
                                {
                                    Player _player = Players[int.Parse(race.Placements[i])];

                                    players.Remove(race.Placements[i]);

                                    _player.TriggerEvent("chat:addMessage", messageObject);
                                }

                                races.Remove(player.Handle);
                            }
                        }
                        break;
                    case "info":
                        if (args.Length > 1) {
                            if (races.ContainsKey(args[1])) {
                                Race playerRace = races[args[1]];
                                string name = null;

                                if (playerRace.Finished) name = Exports["core"].GetCharacterName(playerRace.Placements[0]);
                                if (name == null) name = "Unavailable";

                                string[] info = new string[] {
                                    "Code: ^3" + playerRace.ID,
                                    "Passworded: ^*" + (playerRace.Password == "" ? "^1Yes" : "^2No"),
                                    "Buy in: ^2$^0^*" + playerRace.Bet,
                                    "Pot: ^2$^0^*" + playerRace.Pot,
                                    "Racers: ^3^*" + (playerRace.Participants.Count + playerRace.Placements.Count),
                                    "Open: ^*" + (playerRace.Locked ? "^1No" : "^2Yes"),
                                    "Started: ^*" + (playerRace.Started ? "^3Yes" : "^3No"),
                                    "Winner: ^*^3" + name,
                                };

                                for (int i = 0; i < info.Length; i++)
                                {
                                    messageObject.args[1] = info[i];
                                    player.TriggerEvent("chat:addMessage", messageObject);
                                }
                            } else
                            {
                                messageObject.args[1] = "A race with this id doesn't not exist!";
                                player.TriggerEvent("chat:addMessage", messageObject);
                            }
                        } else
                        {
                            messageObject.args[1] = "/race info [id]!";
                            player.TriggerEvent("chat:addMessage", messageObject);
                        }
                        break;
                    default:
                        for (int i = 0; i < commandHelp.Length; i++)
                        {
                            messageObject.args[1] = commandHelp[i];
                            player.TriggerEvent("chat:addMessage", messageObject);
                        }
                        break;
                }
            } else
            {
                for (int i = 0; i < commandHelp.Length; i++)
                {
                    messageObject.args[1] = commandHelp[i];
                    player.TriggerEvent("chat:addMessage", messageObject);
                }
            }
        }
    }
}
