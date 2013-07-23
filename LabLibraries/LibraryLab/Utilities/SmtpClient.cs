using System;
using System.Collections;
using System.Text;
using System.Net.Mail;

namespace Library.Lab.Utilities
{
    public class SmtpClient
    {
        #region Constants
        private const string STR_ClassName = "SmtpClient";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        /*
         * String constants
         */
        private const String STR_LocalhostIPAddress = "127.0.0.1";
        #endregion

        #region Properties

        private String from;
        private String subject;
        private String body;
        private ArrayList to;
        private ArrayList cc;
        private ArrayList bcc;

        public String From
        {
            get { return from; }
            set { from = value; }
        }

        public String Subject
        {
            get { return subject; }
            set { subject = value; }
        }

        public String Body
        {
            get { return body; }
            set { body = value; }
        }

        public ArrayList To
        {
            get { return to; }
            set { to = value; }
        }

        public ArrayList Cc
        {
            get { return cc; }
            set { cc = value; }
        }

        public ArrayList Bcc
        {
            get { return bcc; }
            set { bcc = value; }
        }

        #endregion


        //-------------------------------------------------------------------------------------------------//

        public SmtpClient()
        {
            /*
             * Initialise email address lists
             */
            this.to = new ArrayList();
            this.cc = new ArrayList();
            this.bcc = new ArrayList();
        }

        //-------------------------------------------------------------------------------------------------//

        public bool Send()
        {
            const String methodName = "Send";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Assume this will fail
             */
            bool success = false;

            try
            {
                MailMessage mailMessage = new MailMessage();

                /*
                 * Set the list of 'to' email addresses
                 */
                foreach (String address in this.to)
                {
                    mailMessage.To.Add(new MailAddress(address));
                }

                /*
                 * Set the list of 'cc' email addresses
                 */
                foreach (String address in this.cc)
                {
                    mailMessage.CC.Add(new MailAddress(address));
                }

                /*
                 * Set the list of 'bcc' email addresses
                 */
                foreach (String address in this.bcc)
                {
                    mailMessage.Bcc.Add(new MailAddress(address));
                }

                /*
                 * Set the remainder of the message information
                 */
                mailMessage.From = new MailAddress(this.from);
                mailMessage.Subject = this.subject;
                mailMessage.Body = this.body;

                /*
                 * Send the message
                 */
                System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient(STR_LocalhostIPAddress);
                smtpClient.Send(mailMessage);

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return success;
        }
    }
}
