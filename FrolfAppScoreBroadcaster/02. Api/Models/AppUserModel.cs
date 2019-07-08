using System;

namespace FrolfAppScoreBroadcaster._02._Api.Models
{
    public class AppUserModel
    {
        public Guid IdKey { get; set; }
        public string LoginName { get; set; }
        public string Handle { get; set; }
        public string FriendCode { get; set; }
        public string RsaPubXml { get; set; }
    }
}
