﻿using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using SleekSoftMVCFramework.Data.Entities;
using SleekSoftMVCFramework.Repository.CoreRepositories;

namespace SleekSoftMVCFramework.HangfireJob
{
    ///---------------------------------------------------------------------------------------------
    /// <summary>
    ///  Hangfire EmailJob class file
    /// </summary>
    /// <copyright>
    /// *****************************************************************************
    ///     ----- Fadipe Wasiu Ayobami . All Rights Reserved. Copyright (c) 2017
    /// *****************************************************************************
    /// </copyright>
    /// <remarks>
    /// *****************************************************************************
    ///     ---- Created For: Public Use (All Products)
    ///     ---- Created By: Fadipe Wasiu Ayobami
    ///     ---- Original Language: C#
    ///     ---- Current Version: v1.0.0.0.1
    ///     ---- Current Language: C#
    /// *****************************************************************************
    /// </remarks>
    /// <history>
    /// *****************************************************************************
    ///     --- Date First Created : 08 - 11 - 2017
    ///     --- Author: Fadipe Wasiu Ayobami
    ///     --- Date First Reviewed: 
    ///     --- Date Last Reviewed:
    /// *****************************************************************************
    /// </history>
    /// <usage>
    /// Class a new class i.e Animal
    /// public class Animal:BaseEntityWithAudit<int>
    /// {
    /// 
    /// }
    /// 
    /// -- Fadipe Wasiu Ayobami
    /// </usage>
    /// ----------------------------------------------------------------------------------------------
    ///

    public class HangfireEmailJob
    {
        private readonly ILog _log;
        private readonly IRepositoryQuery<EmailLog,long> _emailJobQuery;
        private readonly IRepositoryCommand<EmailLog, long> _emailJobCommand;
        private readonly IRepositoryQuery<EmailAttachment, long> _emailAttachmentQuery;
        public HangfireEmailJob()
        {
            _log = log4net.LogManager.GetLogger("HangFireLoggerAppender");
            _emailJobQuery = new RepositoryQuery<EmailLog, long>();
            _emailJobCommand = new RepositoryCommand<EmailLog, long>();
            _emailAttachmentQuery = new RepositoryQuery<EmailAttachment, long>();
        }

        public  void Execute()
        {
            try
            {
                List<EmailLog> emaillogmodellist = _emailJobQuery.GetAllList(m => m.IsSent == false && m.DateToSend <= DateTime.Now).Take(10).ToList();
                if (emaillogmodellist.Any())
                {
                    foreach (EmailLog emaillogmodel in emaillogmodellist)
                    {
                        try
                        {
                            MailMessage msg = GenerateMail(emaillogmodel);
                            bool result = Utilities.EmailHandler.Send(msg);
                            if (result)
                            {
                                emaillogmodel.DateSent= DateTime.Now;
                                emaillogmodel.IsSent = true;
                            }
                            else
                            {
                                emaillogmodel.IsSent = false;
                                emaillogmodel.Retires++;
                            }
                            _emailJobCommand.Update(emaillogmodel);
                            _emailJobCommand.SaveChanges();

                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }



        public MailMessage GenerateMail(EmailLog emaillog)
        {

            try
            {
                var emailfrom = System.Configuration.ConfigurationManager.AppSettings["MailFrom"];
                var logourl = System.Configuration.ConfigurationManager.AppSettings["LogoUrl"];

                String mailTo = emaillog.Receiver;
               // MailMessage mailMessage = new MailMessage();
               // mailMessage.From=(new MailAddress(emailfrom, "no-reply@vatacada.edu"));
                MailMessage mailMessage = new MailMessage(new MailAddress(emailfrom, emailfrom), new MailAddress(mailTo, emaillog.Receiver));
                //string[] receivers = emaillog.Receiver.Split(';');
                //foreach (string s in receivers)
                //{
                //    mailMessage.To.Add(new MailAddress(s));
                //}
                //string[] cc = emaillog.CC.Split(';');
                //foreach (string s in cc)
                //{
                //    if (s.Trim() != "")
                //    {
                //        mailMessage.CC.Add(new MailAddress(s));
                //    }
                //}
                //string[] bcc = emaillog.BCC.Split(';');
                //foreach (string s in bcc)
                //{
                //    if (s.Trim() != "")
                //    {
                //        mailMessage.Bcc.Add(new MailAddress(s));
                //    }
                //}

                if (emaillog.HasAttachment)
                {
                    List<EmailAttachment> attachments = _emailAttachmentQuery.GetAllList(m => m.EmailLogID == emaillog.Id).ToList();
                    if (attachments.Any())
                    {
                        foreach (EmailAttachment attach in attachments)
                        {
                            if (File.Exists(attach.FilePath))
                            {
                                mailMessage.Attachments.Add(new Attachment(attach.FilePath));
                            }
                        }
                    }
                }
                mailMessage.Subject = emaillog.Subject;
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = emaillog.MessageBody;
                return mailMessage;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}