using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_MVC.Class
{
    public class TokenDTO
    {
        public string Token;
        public string Guid;
        public string DeviceID;
        public string Username;
        public long TimeCreated;
        public long TimeExpired;
    }
}
