using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_MVC.Class
{
    public static  class Utils
    {
        public static void WriteLogAsync(string Msg)
        {
            var task = Task.Run(() =>
            {
                DateTime d = DateTime.Now;
                string filePath = Directory.GetCurrentDirectory() + @"\Logs\API\Mobile";
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                filePath = filePath + @"\log_" + d.ToString("yyyyMMddHHmm").Substring(0, 11) + @".log";
                using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(Environment.NewLine);
                        sw.Write(Environment.NewLine);
                        sw.Write("Log Entry {0} {1}: ", d.ToLongTimeString(), d.ToLongDateString());
                        sw.Write(Environment.NewLine);
                        sw.Write(Msg);
                        sw.Close();
                    }
                    fs.Close();
                }
                return d;
            });
        }
        public static async Task<bool> EmailValidateAsync(string smtp, int port, string Email, string UserPassword)
        {
            var task = await Task.Run(() =>
            {
                MailSocket m = new MailSocket(smtp, port);
                bool kt = false; string res = "";
                m.SendCommand("EHLO " + smtp); // EHLO mail.server.com
                res = m.GetFullResponse();
                m.SendCommand("AUTH LOGIN"); // AUTH LOGIN
                res = m.GetFullResponse();
                m.SendCommand(Base64Encode(Email)); // Base64Encode - Email
                res = m.GetFullResponse();
                m.SendCommand(Base64Encode(UserPassword)); // Base64Encode - Pwd
                res = m.GetFullResponse();
                if (res.IndexOf("Authentication successful") > 0) kt = true;
                m.SendCommand("QUIT"); // QUIT
                res = m.GetFullResponse();
                if (res.IndexOf("Authentication successful") > 0) kt = true;
                return kt;
            });
            return task;
        }
        public static string Base64Encode(string txt)
        {
            byte[] data;
            data = Encoding.UTF8.GetBytes(txt);
            return Convert.ToBase64String(data);
        }

        public static string Base64Decode(string txtBase64Encode)
        {
            byte[] data;
            data = Convert.FromBase64String(txtBase64Encode);
            return Encoding.UTF8.GetString(data);
        }
    }
}
