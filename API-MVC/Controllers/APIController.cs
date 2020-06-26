using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using API_MVC.Class;

namespace API_MVC.Controllers
{
    
    public class APIController : Controller
    {
        private const string ClientID = "HRMMOBILE";
        private IMemoryCache _cache;

        public APIController (IMemoryCache cache)
        {
            _cache = cache;
        }
        [Route("API/Mobile/{functionName}")]
        [HttpPost]
        public async Task<IActionResult> Mobile(string functionName)
        {
            var task = await Task.Run(async () =>
            {
                dynamic d = null; string clientContentType; string clientAccept; string token = "";
                int httpStatusCode = 404;  int ResponseCode = -600; string ResponseMessage = "Truy vấn API thất bại"; string Data = "";                
                string taskReturn;
                d = GetConfig();
                if (d == null)
                {
                    ResponseCode = -99;
                    ResponseMessage = "Lỗi file config API";
                }
                else
                {
                    if (d.Https.ToString() == "1" && !Request.IsHttps)
                    {
                        ResponseCode = -600;
                        ResponseMessage = "Hệ thống yêu cầu SSL";
                    }
                    else
                    {
                        if(Request.Method != "POST")
                        {
                            ResponseCode = -600;
                            ResponseMessage = "Hệ thống yêu cầu phương thức POST";
                        }
                        else
                        {
                            clientContentType = Request.Headers["Content-Type"];
                            clientAccept = Request.Headers["Accept"];
                            if (clientAccept.ToLower() != "application/json")
                            {
                                ResponseCode = -600;
                                ResponseMessage = "Hệ thống yêu cầu Accept: application/json";
                            }
                            else
                            {
                                if (clientContentType.ToLower() != "application/json")
                                {
                                    ResponseCode = -600;
                                    ResponseMessage = "Hệ thống yêu cầu Content-Type: application/json";
                                }
                                else
                                {
                                    int i = 0; bool kt = false;
                                    while (i < d.config.Count && !kt)
                                    {
                                        if (d.config[i].clientId.ToString().ToUpper() == ClientID.ToUpper()) kt = true;
                                        i = i + 1;
                                    }
                                    if (kt)
                                    {
                                        i = i - 1; kt = false;
                                        string StoreName = ""; int j = 0;
                                        dynamic d1 = d.config[i].functionListName;
                                        while (j < d1.Count && !kt)
                                        {
                                            if (d1[j].FunctionName.ToString().ToUpper() == functionName.ToUpper()) kt = true;
                                            j = j + 1;
                                        }
                                        if (kt)
                                        {
                                            dynamic dRequest = null;
                                            StreamReader reader = new StreamReader(Request.Body );                                            
                                            try
                                            {
                                                string content = await reader.ReadToEndAsync();
                                                dRequest = JObject.Parse(content);
                                            }
                                            catch (Exception ex)
                                            {
                                                dRequest = null;
                                            }
                                            if (functionName.ToUpper() != "LOGIN")
                                            {
                                                token = Request.Headers["Authorization"];
                                                if (token == null) token = "";
                                                try
                                                {
                                                    // Check token
                                                    TokenDTO objToken;
                                                    bool IsValid = TokenHelper.CheckToken(token, out objToken);
                                                    // Thực hiện call nghiệp vụ với token nhận được
                                                    if (!IsValid)
                                                    {
                                                        ResponseCode = -600;
                                                        ResponseMessage = "Token không hợp lệ";
                                                    }
                                                    else
                                                    {
                                                        // Điều hướng nghiệp vụ
                                                        switch (functionName.ToLower())
                                                        {
                                                            case "sendmail":
                                                                string _SMTP = "smtp.gmail.com"; int _Port = 587; bool _IsSSL = true;
                                                                string _AccountName = "Nguyen Van A"; string _EmailReceipt = "info@abc.com";
                                                                string _AccountUser = "anv@abc.com"; string _AccountPassword = "abcPass";
                                                                string[] _EmailTo = {"khach@cbd.com"}; string[] _EmailCC = { "sep1@abc.com" , "sep2@abc.com" };
                                                                string[] _EmailBCC = { };
                                                                string _Subject = "Test mail"; string _Content = "Test content";
                                                                string[] _FileList = { };
                                                                Send_Mail send_Mail = new Send_Mail(_SMTP, _Port, _IsSSL,
                                                                _AccountName, _EmailReceipt, _AccountUser, _AccountPassword,
                                                                _EmailTo, _EmailCC, _EmailBCC,
                                                                _Subject, _Content, _FileList);
                                                                bool IsSended = await send_Mail.EmailSendingAsync();
                                                                if (IsSended)
                                                                {
                                                                    ResponseCode = 1;
                                                                    ResponseMessage = string.Format("Gửi email từ API thành công");
                                                                    Data = "";
                                                                    httpStatusCode = 200;
                                                                }
                                                                else
                                                                {
                                                                    ResponseCode = -600;
                                                                    ResponseMessage = string.Format("Gửi email từ API thất bại");
                                                                    Data = "";
                                                                }
                                                                break;
                                                            default:
                                                                // Create param store proceduce
                                                                List<string> arStoreName = new List<string>();
                                                                Dictionary<string, object> paramObj = new Dictionary<string, object>();
                                                                arStoreName.Add(functionName.ToLower());
                                                                arStoreName.Add(StoreName.ToLower());
                                                                dynamic d2 = d1[j].ParamIn;
                                                                for (var l = 0; l < d2.Count; l++)
                                                                {
                                                                    string a = ""; string b = "";
                                                                    a = d2[l].LocalName.ToString();
                                                                    if (dRequest != null)
                                                                    {
                                                                        if (dRequest[d2[l].ParamName.ToString()] != null)
                                                                        {
                                                                            b = dRequest[d2[l].ParamName.ToString()].ToString();
                                                                        }
                                                                    }
                                                                    // IsHidden = 1
                                                                    if (d2[l].IsHidden.ToString() == "1")
                                                                    {
                                                                        // Username
                                                                        if ((a.ToUpper() == "P_USERNAME") && (objToken != null))
                                                                        {
                                                                            b = objToken.Username;
                                                                        }
                                                                    }
                                                                    paramObj.Add(a, b);
                                                                }
                                                                d2 = d1[j].ParamOut;
                                                                for (var l = 0; l < d2.Count; l++)
                                                                {
                                                                    string a = ""; string b = "";
                                                                    a = d2[l].LocalName.ToString();
                                                                    b = d2[l].LocalValue.ToString();
                                                                    paramObj.Add(a, b);
                                                                }
                                                                // Call store with Param
                                                                break;
                                                        }
                                                        ResponseCode = 1;
                                                        ResponseMessage = string.Format("Truy vấn API thành công");
                                                        Data = "";
                                                        httpStatusCode = 200;
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    ResponseCode = -600;
                                                    ResponseMessage = string.Format ("Token không hợp lệ: {0}", ex.ToString ());
                                                }
                                            }
                                            else
                                            {
                                                // Thực hiện call Login
                                                if (dRequest == null)
                                                {
                                                    ResponseCode = -600;
                                                    ResponseMessage = "Hệ thống không nhận được request Data";
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        bool isValid;
                                                        switch (int.Parse(dRequest.Provider.ToString()))
                                                        {
                                                            case 2: // Login Email
                                                                string smtp = "mail.abc.com"; int port = 587;
                                                                string Email = dRequest.UserName.ToString(); string UserPassword = dRequest.Password.ToString();
                                                                isValid = await Utils.EmailValidateAsync(smtp, port, Email, UserPassword);
                                                                break;
                                                            case 3: // Login LDAP
                                                                Email = dRequest.UserName.ToString(); UserPassword = dRequest.Password.ToString();
                                                                string LdapURL = "LDAP://abc.com"; string LDAPDomain = "abc.com"; string LDAPBaseDN = "//ldap://abc.com/path";
                                                                LDAP m = new LDAP(LdapURL, LDAPDomain, LDAPBaseDN, 0, Email, UserPassword); // 0 - ContextType.Machine
                                                                isValid = await m.ValidateUserAsync();
                                                                break;
                                                            default: // Login bang store proceduce
                                                                // Create param store proceduce
                                                                List<string> arStoreName = new List<string>();
                                                                Dictionary<string, object> paramObj = new Dictionary<string, object>();
                                                                arStoreName.Add(functionName.ToLower());
                                                                arStoreName.Add(StoreName.ToLower());
                                                                dynamic d2 = d1[j].ParamIn;
                                                                for (var l = 0; l < d2.Count; l++)
                                                                {
                                                                    string a = ""; string b = "";
                                                                    a = d2[l].LocalName.ToString();
                                                                    if (dRequest != null)
                                                                    {
                                                                        if (dRequest[d2[l].ParamName.ToString()] != null)
                                                                        {
                                                                            b = dRequest[d2[l].ParamName.ToString()].ToString();
                                                                        }
                                                                    }
                                                                    // Encrypt Password
                                                                    //if(a.ToUpper == "P_PASSWORD")
                                                                    //{
                                                                    //    using (encry = new EncryptData)
                                                                    //    {
                                                                    //        b = encry.EncryptString(b)
                                                                    //    }
                                                                    //}
                                                                    paramObj.Add(a, b);
                                                                }
                                                                d2 = d1[j].ParamOut;
                                                                for (var l = 0; l < d2.Count; l++)
                                                                {
                                                                    string a = ""; string b = "";
                                                                    a = d2[l].LocalName.ToString();
                                                                    b = d2[l].LocalValue.ToString();
                                                                    paramObj.Add(a, b);
                                                                }
                                                                // Call store with Param
                                                                isValid = true;
                                                                break;
                                                        }
                                                        if (!isValid)
                                                        {
                                                            ResponseCode = -600;
                                                            ResponseMessage = "Login không thành công";
                                                            httpStatusCode = 200;
                                                        }
                                                        else
                                                        {
                                                            ResponseCode = 1;
                                                            ResponseMessage = "Login thành công";
                                                            httpStatusCode = 200;
                                                            token = TokenHelper.GenerateToken(dRequest["DeviceID"].ToString(), dRequest["UserName"].ToString());
                                                            Data = string.Format (",\"Token\":\"{0}\"", token);
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        ResponseCode = -600;
                                                        ResponseMessage = string.Format("Login không thành công {0}", ex.ToString());
                                                    }                                                    
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ResponseCode = -600;
                                            ResponseMessage = "Hệ thống không tìm được nghiệp vụ: " + functionName;
                                        }
                                    }
                                    else
                                    {
                                        ResponseCode = -600;
                                        ResponseMessage = "Hệ thống không tìm được ClientID";
                                    }
                                }
                            }
                        }
                    }
                }
                Utils.WriteLogAsync(string.Format("ResponseCode: {0}, ResponseMessage: {1}", ResponseCode, ResponseMessage));
                taskReturn = httpStatusCode.ToString () + "^{\"Status\": " + ResponseCode + ", \"Message\": \"" + ResponseMessage + "\"" + Data + "}";
                return taskReturn; 
            });
            string[] a = task.Split(new string[] { "^" }, StringSplitOptions.None);
            int _StatusCode = int.Parse(a[0]);
            dynamic  r = JObject.Parse(a[1]);
            if (_StatusCode == 200)
            {
                //return Json(a[1]);
                return new ContentResult { Content = a[1], ContentType = "application/json" };
            }
            else
            {
                return StatusCode (_StatusCode);
            }            
        }

        private dynamic GetConfig() {
            dynamic d = null;
            try
            {
                 string _Key = "_ConfigJson";
                bool kt = _cache.TryGetValue(_Key, out d);
                if (!kt || d == null)
                {
                    string fileConfigName = Directory.GetCurrentDirectory() + @"\config.json";
                    d = JObject.Parse(System.IO.File.ReadAllText(fileConfigName));
                    Set(_Key, d, 31104000);
                }
            }
            catch (Exception ex)
            {
                Utils.WriteLogAsync(ex.ToString());
            }
            return d;
        }
        private void Set(string Key, object Data, double iTime = 31104000)
        {
            Key = Key.ToLower();
            if (Key == "keylist") return;
            //SetListKeyAdd(Key);
            _cache.Set(Key, Data, DateTime.Now.AddSeconds(iTime));
        }
    }
}
