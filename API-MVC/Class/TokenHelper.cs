using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
namespace API_MVC.Class
{
    public static class TokenHelper
    {
        private const string saltValue = "w8yIhE";
        private const int TokenTimeAPI = 43200;

        public static string GenerateToken(string _DeviceID, string _Username)
        {
            DateTime today = DateTime.Now;
            long timeCreated = long.Parse (today.ToString("yyyyMMddHHmmss"));
            long timeExpired = long.Parse(today.AddMinutes (43200).ToString("yyyyMMddHHmmss"));
            string _Guid = Guid.NewGuid().ToString();
            TokenDTO a = new TokenDTO();
            a.DeviceID = _DeviceID;
            a.Username = _Username;
            a.TimeCreated = timeCreated;
            a.TimeExpired = timeExpired;
            a.Guid = _Guid;
            a.Token = EncryptMD5.CreateMD5(string.Format("{0}.{1}.{2}.{3}.{4}.{5}", _DeviceID, _Username, timeCreated.ToString(), timeExpired.ToString(), _Guid, saltValue).ToLower());
            return Utils.Base64Encode(JsonConvert.SerializeObject(a));
        }
        public static bool CheckToken(string Token, out TokenDTO a)
        {
            a = JsonConvert.DeserializeObject<TokenDTO>(Utils.Base64Decode(Token));
            long timeNow = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
            string _token = EncryptMD5.CreateMD5(string.Format("{0}.{1}.{2}.{3}.{4}.{5}", a.DeviceID, a.Username, a.TimeCreated.ToString(), a.TimeExpired.ToString(), a.Guid, saltValue).ToLower());
            if (a.Token != _token) return false;
            if (a.TimeExpired < timeNow) return false;
            return true;
        }
    }
}
