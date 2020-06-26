using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace API_MVC.Class
{
    public class MailSocket : IDisposable
    {
        #region Properties
        private TcpClient client = null;
        private NetworkStream stream = null;
        private StreamReader reader = null;
        private StreamWriter writer = null;
        private string resp = "";
        private int state = -1;
        #endregion

        /// <summary>
        /// Khởi tạo tạo object Mail Socket
        /// </summary>
        /// <param name="tc">Object TcpClient</param>
        /// <returns></returns>
        public MailSocket(TcpClient tc)
        {
            client = tc;
            stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
        }

        /// <summary>
        /// Khởi tạo tạo object Mail Socket
        /// </summary>
        /// <param name="url">Đường dẫn</param>
        /// <param name="port">Port</param>
        /// <returns></returns>
        public MailSocket(string url, int port)
        {
            client = new TcpClient(url, port);
            stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
        }

        /// <summary>
        /// Gửi dữ liệu Socket tới Mail Server
        /// </summary>
        /// <param name="bts">Data định dạng Byte</param>
        /// <returns></returns>
        public void SendData(byte[] bts)
        {
            if (GetResponseState() != 221)
            {
                stream.Write(bts, 0, bts.Length);
                stream.Flush();
            }
        }

        /// <summary>
        /// Gửi lệnh Socket tới Mail Server
        /// </summary>
        /// <param name="cmd">Chuỗi mã lệnh</param>
        /// <returns></returns>
        public void SendCommand(string cmd)
        {
            if (GetResponseState() != 221)
            {
                writer.WriteLine(cmd);
                writer.Flush();
            }
        }

        /// <summary>
        /// Nhận thông tin từ Mail Server
        /// </summary>
        /// <returns></returns>
        public string GetFullResponse()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(RecvResponse());
            sb.Append("\r\n");
            while (HaveNextResponse())
            {
                sb.Append(RecvResponse());
                sb.Append("\r\n");
            }
            return sb.ToString();
        }
        #region Private Method //HTTP_CODE.Base64Encode(
        private bool IsNumber(char c)
        {
            return c >= '0' && c <= '9';
        }

        /// <summary>
        /// Lấy trạng thái trả lời
        /// </summary>
        /// <returns></returns>
        public int GetResponseState()
        {
            if (resp.Length >= 3 && IsNumber(resp[0]) && IsNumber(resp[1]) && IsNumber(resp[2]))
                state = Convert.ToInt32(resp.Substring(0, 3));

            return state;
        }
        private bool HaveNextResponse()
        {
            if (GetResponseState() > -1)
            {
                if (resp.Length >= 4 && resp[3] != ' ')
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        private string RecvResponse()
        {
            if (GetResponseState() != 221)
                resp = reader.ReadLine();
            else
                resp = "221 closed!";

            return resp;
        }

        public void Dispose()
        {
            reader.Close();
            writer.Close();
            client.Close();
            stream.Close();
        }
        #endregion
    }
}
