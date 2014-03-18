using System.Net.Mail;

namespace LogMon
{
    public class AlertEmail
    {
        private const string MailMsgFrom = "alps-alert@macu.com";
        private const string MailMsgSubj = "ALPS Symcon service failure";

        public void Send(string exceptionMsg, int occurrences, string[] recipients)
        {
            string alertMsg = string.Format("SymCon.Service.ALPS appears to be failing. The following error has occurred {0} times:\r\n{1}", occurrences, exceptionMsg);

            var msg = new MailMessage { From = new MailAddress(MailMsgFrom), Subject = MailMsgSubj, Body = alertMsg };

            foreach (var recipient in recipients)
            {
                msg.To.Add(new MailAddress(recipient));
            }
            var client = new SmtpClient();
            client.Send(msg);
        }


    }


}
