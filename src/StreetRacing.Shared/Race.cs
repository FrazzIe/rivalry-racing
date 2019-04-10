using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace StreetRacing.Shared
{
    public class Race
    {
        [JsonProperty]
        private string id;
        [JsonProperty]
        private int bet;
        private Vector3 start;
        [JsonProperty]
        private Vector3 end;
        [JsonProperty]
        private List<string> participants;
        [JsonProperty]
        private List<string> placements;

        public Race(string _id, int _bet)
        {
            id = _id;
            bet = _bet;
            start = Vector3.Zero;
            end = Vector3.Zero;

            participants = new List<string>();
            placements = new List<string>();

            participants.Add(_id);
        }

        public string ID { get => id; }
        public int Bet { get => bet; }
        public Vector3 StartPoint { get => start; set => start = value; }
        public Vector3 EndPoint { get => end; set => end = value; }
        public List<string> Participants { get => participants; }
        public List<string> Placements { get => placements; }
        public bool Ready()
        {
            return (StartPoint != Vector3.Zero && EndPoint != Vector3.Zero);
        }
        public bool Started { get; set; }
        public bool Finished { get; set; }
    }
}