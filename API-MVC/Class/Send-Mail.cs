using System;
using System.Text;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Net;

namespace API_MVC.Class
{
    public class Send_Mail : IDisposable
    {
        #region Properties
        private string SMTP;
        private int Port;
        private bool IsSSL;
        private string AccountName;
        private string AccountUser;
        private string AccountPassword;
        private string EmailReceipt;

        private string[] EmailTo;
        private string[] EmailCC;
        private string[] EmailBCC;
        private string Subject;
        private string Content;
        private string[] FileList;

        MailMessage message;
        #endregion

        #region Method Contruction
        public Send_Mail(string _SMTP, int _Port, bool _IsSSL,
            string _AccountName, string _EmailReceipt, string _AccountUser, string _AccountPassword,
            string[] _EmailTo, string[] _EmailCC, string[] _EmailBCC,
            string _Subject, string _Content, string[] _FileList)
        {
            SMTP = _SMTP; Port = _Port; IsSSL = _IsSSL;
            AccountName = _AccountName; AccountUser = _AccountUser; AccountPassword = _AccountPassword; EmailReceipt = _EmailReceipt;
            EmailTo = _EmailTo; EmailCC = _EmailCC; EmailBCC = _EmailBCC;
            Subject = _Subject; Content = _Content; FileList = _FileList;
            Utils.WriteLogAsync(string.Format("Start mail [{0}] processing!", Subject));
        }
        #endregion

        #region Method
        public async Task<bool> EmailSendingAsync()
        {
            var task = await  Task.Run(async () =>
            {
                bool IsSendOk = false;
                try
                {
                    Utils.WriteLogAsync(string.Format("Send [Subject: {0} Content: {1}]", Subject, Content));
                    message = new MailMessage();
                    message.Subject = Subject;
                    message.Body = Content;
                    message.BodyEncoding = Encoding.UTF8;
                    message.IsBodyHtml = true;
                    Utils.WriteLogAsync(string.Format("Send [From: {0} Name: {1}]", AccountUser, AccountName));
                    message.From = new MailAddress(AccountUser, AccountName);
                    try
                    {
                        Utils.WriteLogAsync(string.Format("Sender [EmailReceipt: {0} AccountName: {1}]", EmailReceipt, AccountName));
                        message.Sender = new MailAddress(EmailReceipt, AccountName);
                    }
                    catch
                    {
                        message.Sender = new MailAddress(AccountUser, AccountName);
                        Utils.WriteLogAsync(string.Format("Sender [EmailReceipt: {0} AccountName: {1}] fail", EmailReceipt, AccountName));
                    }

                    bool kt = await EmailToSetAsync();
                    if (!kt) return false; // ko co email to tu choi
                    await EmailCCSetAsync();
                    await EmailBCCSetAsync();
                    await AttachmentSetAsync();

                    SmtpClient client = new SmtpClient(SMTP, Port); //Gmail smtp    
                    NetworkCredential basicCredential1 = new NetworkCredential(AccountUser, AccountPassword);
                    client.EnableSsl = IsSSL;
                    client.UseDefaultCredentials = false;
                    client.Credentials = basicCredential1;

                    client.Send(message);

                    IsSendOk = true;
                    Utils.WriteLogAsync(string.Format("Sendmail [Subject: {0}] OK", Subject));
                }
                catch (Exception ex)
                {
                    IsSendOk = false;
                    Utils.WriteLogAsync(string.Format("Sendmail [Subject: {0}] Error {1}", Subject, ex.ToString()));
                }
                return IsSendOk;
            });
            return task;
        }
        #endregion

        #region Private Method
        private async Task<bool> EmailToSetAsync()
        {
            var task = await Task.Run(() =>
            {
                bool IsAddOk = false;
                if (EmailTo != null)
                {
                    if (EmailTo.Length > 0)
                    {
                        for (int i = 0; i < EmailTo.Length; i++)
                        {
                            try
                            {
                                message.To.Add(new MailAddress(EmailTo[i]));
                                IsAddOk = true;
                                Utils.WriteLogAsync(string.Format("Sendmail [Subject: {0}] Add mail-to:{1}", Subject, EmailTo[i]));
                            }
                            catch (Exception ex)
                            {
                                Utils.WriteLogAsync(string.Format("Sendmail [Subject: {0}] Add mail-to:{1}; Error {2}", Subject, EmailTo[i], ex.ToString()));
                            }
                        }
                    }
                }
                return IsAddOk;
            });
            return task;
        }
        private async Task<bool> EmailCCSetAsync()
        {
            var task = await Task.Run(() =>
            {
                bool IsAddOk = false;
                if (EmailCC != null)
                {
                    if (EmailCC.Length > 0)
                    {
                        for (int i = 0; i < EmailCC.Length; i++)
                        {
                            try
                            {
                                message.CC.Add(new MailAddress(EmailCC[i]));
                                IsAddOk = true;
                                Utils.WriteLogAsync(string.Format("Sendmail [Subject: {0}] Add mail-cc:{1}", Subject, EmailCC[i]));
                            }
                            catch (Exception ex)
                            {
                                Utils.WriteLogAsync(string.Format("Sendmail [Subject: {0}] Add mail-to:{1}; Error {2}", Subject, EmailCC[i], ex.ToString()));
                            }
                        }
                    }
                }
                return IsAddOk;
            });
            return task;
        }
        private async Task<bool> EmailBCCSetAsync()
        {
            var task = await Task.Run(() =>
            {
                bool IsAddOk = false;
                if (EmailBCC != null)
                {
                    if (EmailBCC.Length > 0)
                    {
                        for (int i = 0; i < EmailBCC.Length; i++)
                        {
                            try
                            {
                                message.Bcc.Add(new MailAddress(EmailBCC[i]));
                                IsAddOk = true;
                                Utils.WriteLogAsync(string.Format("Sendmail [Subject: {0}] Add mail-cc:{1}", Subject, EmailBCC[i]));
                            }
                            catch (Exception ex)
                            {
                                Utils.WriteLogAsync(string.Format("Sendmail [Subject: {0}] Add mail-to:{1}; Error {2}", Subject, EmailBCC[i], ex.ToString()));
                            }
                        }
                    }
                }
                return IsAddOk;
            });
            return task;
        }
        private async Task<bool> AttachmentSetAsync()
        {
            var task = await Task.Run(() =>
            {
                bool IsAddOk = false;
                if (FileList != null)
                {
                    if (FileList.Length > 0)
                    {
                        for (int i = 0; i < FileList.Length; i++)
                        {
                            try
                            {
                                message.Attachments.Add(new Attachment(FileList[i]));
                                IsAddOk = true;
                                Utils.WriteLogAsync(string.Format("Sendmail [Subject: {0}] Add Attachment-file:{1}", Subject, FileList[i]));
                            }
                            catch (Exception ex)
                            {
                                Utils.WriteLogAsync(string.Format("Sendmail [Subject: {0}] Add Attachment-file:{1}; Error {2}", Subject, FileList[i], ex.ToString()));
                            }
                        }
                    }
                }
                return IsAddOk;
            });
            return task;
        }

        public void Dispose()
        {
            message.Dispose();
        }
        #endregion
    }
}
