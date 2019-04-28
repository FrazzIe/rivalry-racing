using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
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
        Vector3 cylinderMarkerPosition = Vector3.Zero;
        Vector3 flagMarkerPosition = Vector3.Zero;
        Vector3 cylinderMarkerScale = new Vector3(25f, 25f, 25f);
        Vector3 flagMarkerScale = new Vector3(20f, 20f, 20f);
        Scaleform countdown = new Scaleform("COUNTDOWN");
        bool showCountdown = false;

        public Client()
        {

        }
        [Tick]
        private async Task OnPlayerReady()
        {
            await Delay(0);

            if (API.NetworkIsSessionStarted())
            {
                TriggerEvent("chat:addSuggestion", "/race", "create, start, join, lock or leave a race", new[]{
                    new { name = "option", help = "create, start, join, lock, leave, info"},
                    new { name = "bet/race", help = "create -> bet amount, join or info -> race id"},
                    new { name = "password", help = "create or join -> race password"}
                });

                Tick -= OnPlayerReady;
            }
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
                                Vector3 waypointPosition = waypointBlip.Position;
                                int startTime = Game.GameTime;
                                float groundZ = 0.0f;
                                bool foundZ = true;
                                
                                World.RemoveWaypoint();

                                while (!API.GetGroundZFor_3dCoord(waypointPosition.X, waypointPosition.Y, 1000.0f, ref groundZ, false)) {
                                    await Delay(0);

                                    API.ClearPrints();
                                    API.BeginTextCommandPrint("STRING");
                                    API.AddTextComponentSubstringPlayerName("Confirming finish line location!");
                                    API.EndTextCommandPrint(0, true);

                                    API.RequestCollisionAtCoord(waypointPosition.X, waypointPosition.Y, 0.0f);

                                    if ((Game.GameTime - startTime) > 10000)
                                    {
                                        foundZ = false;
                                        break;
                                    }
                                }
                                
                                if (foundZ)
                                {
                                    race.EndPoint = new Vector3(waypointPosition.X, waypointPosition.Y, groundZ);

                                    TriggerServerEvent("Race.Setup", race.StartPoint, race.EndPoint);

                                    TriggerEvent("chat:addMessage", new
                                    {
                                        color = new[] { 255, 0, 0 },
                                        multiline = true,
                                        args = new[] { "Race", "Finish line confirmed!" },
                                    });
                                } else
                                {
                                    TriggerEvent("chat:addMessage", new
                                    {
                                        color = new[] { 255, 0, 0 },
                                        multiline = true,
                                        args = new[] { "Race", "Unable to confirm the finish point, please try again!" },
                                    });
                                }
                            }
                        }
                    }
                } else
                {
                    if (showCountdown)
                    {
                        if (countdown.IsLoaded)
                            countdown.Render2D();
                    }

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

                        World.DrawMarker(MarkerType.VerticalCylinder, cylinderMarkerPosition, Vector3.Zero, Vector3.Zero, cylinderMarkerScale, Color.FromArgb(20, 255, 255, 0));
                        World.DrawMarker(MarkerType.CheckeredFlagRect, flagMarkerPosition, Vector3.Zero, Vector3.Zero, flagMarkerScale, Color.FromArgb(125, 255, 0, 0), true, true);

                        if (position.DistanceToSquared(race.EndPoint) < 140.0)
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
        [EventHandler("Race.Start")]
        private async void StartRace(string _race)
        {
            race = JsonConvert.DeserializeObject<Race>(_race);

            cylinderMarkerPosition = race.EndPoint;
            flagMarkerPosition = race.EndPoint;

            cylinderMarkerPosition.Z -= 1.0f;
            flagMarkerPosition.Z += 7.5f;

            int count = 3;

            while(count != 0)
            {
                countdown.CallFunction("SET_MESSAGE", new object[] { count, 255, 0, 0, false });
                API.PlaySoundFrontend(-1, "3_2_1", "HUD_MINI_GAME_SOUNDSET", false);

                showCountdown = true;

                await Delay(1000);

                count--;
            }

            API.PlaySoundFrontend(-1, "Beep_Green", "DLC_HEIST_HACKING_SNAKE_SOUNDS", true);

            showCountdown = false;

            race.Started = true;
        }
        [EventHandler("Race.Sync")]
        private void SyncRace(string _race)
        {
            race = JsonConvert.DeserializeObject<Race>(_race);
        }
        [EventHandler("Race.Reset")]
        private void ResetRace()
        {
            race = null;

            if (race == null && destination != null)
            {
                destination.Delete();
                destination = null;
            }
        }
    }
}
