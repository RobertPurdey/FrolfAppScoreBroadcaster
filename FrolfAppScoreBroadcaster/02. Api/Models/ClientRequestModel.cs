using System;

namespace FrolfAppScoreBroadcaster._02._Api.Models
{
    public class ClientRequestModel
    {
        public string Token { get; set; }
        public string Command { get; set; }
        public Guid GameId { get; set; }
    }
}
