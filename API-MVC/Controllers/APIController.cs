﻿using System;
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
            var task = await Task.Run(() =>
            {
                dynamic d = null; string clientContentType; string clientAccept; string ClientToken = ""; dynamic token = null;
                int httpStatusCode = 200;  int ResponseCode = 1; string ResponseMessage = "Truy vấn API thành công"; string Data = "null";                
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
                                        if (d.config.clientId.ToString().ToUpper() == ClientID.ToUpper()) kt = true;
                                        i = i + 1;
                                    }
                                    if (kt)
                                    {
                                        i = i - 1; kt = false;
                                        string StoreName = ""; int j = 0;
                                        dynamic d1 = d.config[i].functionListName;
                                        while (j < d1.Count && !kt)
                                        {
                                            if (d1.FunctionName.ToString().ToUpper() == functionName.ToUpper()) kt = true;
                                            j = j + 1;
                                        }
                                        if (kt)
                                        {
                                            if (functionName.ToUpper() != "LOGIN")
                                            {
                                                ClientToken = Request.Headers["Authorization"];
                                                try
                                                {
                                                    token = JsonConvert.DeserializeObject<dynamic>(ClientToken);
                                                    // Thực hiện call nghiệp vụ với token nhận được
                                                }
                                                catch (Exception ex)
                                                {
                                                    ResponseCode = -600;
                                                    ResponseMessage = "Token không hợp lệ";
                                                    kt = false;
                                                }
                                            }
                                            else
                                            {
                                                // Thực hiện call Login
                                                token = null;
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
                taskReturn = httpStatusCode.ToString () + "^{\"Status\": " + ResponseCode + ", \"Message\": \"" + ResponseMessage + "\", \"Data\": " + Data + "}";
                return taskReturn; 
            });
            string[] a = task.Split(new string[] { "^" }, StringSplitOptions.None);
            int _StatusCode = int.Parse(a[0]);
            if (_StatusCode == 200)
            {
                return Json(JObject.Parse(a[1]));
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
                WriteLog(ex.ToString());
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

        private void WriteLog(string Msg)
        {
            DateTime d = DateTime.Now;
            string filePath = Directory.GetCurrentDirectory() + @"\Logs\API\Mobile";
            if(!Directory.Exists (filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            filePath = filePath + @"\log_" + d.ToString("yyyyMMddHHmm").Substring(0, 11) + @".log";
            using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(Environment.NewLine);
                    sw.Write("Log Entry {0} {1}: ", d.ToLongTimeString(), d.ToLongDateString());
                    sw.Write(Environment.NewLine);
                    sw.Write(Msg);
                    sw.Close();
                }
                fs.Close();
            }
        }
    }
}
