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
        Blip destination;

        public Client()
        {

        }
        [Tick]
        private async Task RaceWatcher()
        {
            if (race != null)
            {
                Vector3 position = Game.PlayerPed.Position;
                string serverId = Game.Player.ServerId.ToString();

                if (!race.Ready())
                {
                    if (race.ID == serverId)
                    {
                        if (race.StartPoint == Vector3.Zero) { race.StartPoint = position; World.RemoveWaypoint(); Debug.WriteLine("Setup start point!"); }
                        else
                        {
                            Blip waypointBlip = World.GetWaypointBlip();

                            API.ClearPrints();
                            API.BeginTextCommandPrint("STRING");                            
                            API.AddTextComponentSubstringPlayerName("Mark the finish line on your gps!");
                            API.EndTextCommandPrint(0, true);

                            if (waypointBlip != null)
                            {
                                race.EndPoint = waypointBlip.Position;
                                race.EndPoint = new Vector3(race.EndPoint.X, race.EndPoint.Y, World.GetGroundHeight(new Vector2(race.EndPoint.X, race.EndPoint.Y)));

                                World.RemoveWaypoint();

                                TriggerServerEvent("Race.Setup", race.StartPoint, race.EndPoint);

                                Debug.WriteLine("Setup endpoint!");
                            }
                        }
                    }
                } else
                {
                    if (race.Started && !race.Finished)
                    {
                        if (destination == null)
                        {
                            destination = World.CreateBlip(race.EndPoint);
                            destination.Sprite = BlipSprite.Standard;
                            destination.Scale = 0.8f;
                            destination.Color = BlipColor.Yellow;
                            destination.ShowRoute = true;
                            World.WaypointPosition = race.EndPoint;
                        }

                        if (position.DistanceToSquared(race.EndPoint) < 25)
                        {
                            TriggerServerEvent("Race.End", race.ID);
                            race.Finished = true;
                            destination.Delete();
                            destination = null;
                        }
                    }
                }
            }
        }
            }
        [EventHandler("Race.Sync")]
        private void SyncRace(string _race)
        {
            race = JsonConvert.DeserializeObject<Race>(_race);
        }
    }
}
