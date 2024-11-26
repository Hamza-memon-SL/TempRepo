using Auto_Alert_2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Auto_Alert_2.Services
{
    public class EmailService
    {
        public async Task<bool> SendEmailAsync(EmailDataModel emailDataModel)
        {
            try
            {

                bool emailSent = false;
                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                //message.Bcc.Add("hamza.memon@systemsltd.com");
                UserDataServices uds = new UserDataServices();
                var emails = await uds.GetEmails();
                foreach (string email in emails)
                {
                    message.Bcc.Add(email);
                }

                //old code
                //message.Bcc.Add("musman.rana@ke.com.pk,Sadia.haq@ke.com.pk, Rameez.raja@ke.com.pk,Faran.ikram@ke.com.pk,aliakhtar.hussain@ke.com.pk,ebad.khalid@ke.com.pk,s.hassanashraf@ke.com.pk,s.alirazahaider@ke.com.pk,junaid.ashhad@ke.com.pk");


                //message.Bcc.Add("musman.rana@ke.com.pk; Sadia.haq@ke.com.pk; Rameez.raja@ke.com.pk; Faran.ikam@ke.com.pk; aliakhtar.hussain@ke.com.pk; ebad.khalid@ke.com.pk; s.hassanashraf@ke.com.pk; s.alirazahaider@ke.com.pk; junaid.ashhad@ke.com.pk");
                message.From = new System.Net.Mail.MailAddress("noreply@ke.com.pk");
                message.Subject = "Auto-Alert-Job-Part-2";
                if (emailDataModel.IsSuccess)
                {
                    message.Body = " Hi Team, <br/> <br/> Here is the result of Job. <br/><strong> Date:</strong> " + emailDataModel.JobDate + "  <br/><strong>Total Records for PNs:</strong> " + emailDataModel.TotalPNS + "<br/><strong> Total Batches Count:</strong> " + emailDataModel.TotalBatch + "<br/><strong> Start Time:</strong> " + emailDataModel.StartTime + " <br/><strong> Completion Time:</strong> " + emailDataModel.EndTime + " <br/><br/><br/> Thanks.<br/> KE.";
                }
                else
                {
                    message.Body = " Hi Team,<br/> <br/> Here is the result of Job. <br/> <br/> Today's job has been failed to complete or did not started. Please contact with the team to resolve the issue earliest. <br/><br/><br/> Thanks. <br/> KE.";
                }
                message.IsBodyHtml = true;
                using (SmtpClient client = new SmtpClient("103.125.141.122", 25))
                {
                    client.EnableSsl = false;
                    client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = true;
                    client.Credentials = new NetworkCredential("noreply@ke.com.pk", "Uiop@1234");   //Decrypt("sGd7MXFrhskqhPLkP9mc9g==", true)
                    //client.Credentials = new NetworkCredential("TestingKElectric22@hotmail.com", "QWERTY12345");

                    client.Send(message);
                };

                emailSent = true;
                return await Task.FromResult(emailSent);
            }
            catch (Exception ex)
            {
                return await Task.FromResult(false);
            }
        }
    }

}
