using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SleekSoftMVCFramework.Data.IdentityModel;
using SleekSoftMVCFramework.Repository;
using SleekSoftMVCFramework.Repository.CoreRepositories;
using SleekSoftMVCFramework.Data.IdentityService;
using SleekSoftMVCFramework.Data.Entities;
using System.Data.Entity.Validation;
using System.Web;
using System.IO;
using SleekSoftMVCFramework.Data.AppEntities;

namespace SleekSoftMVCFramework.Utilities
{
    public class Utility
    {
        private readonly IRepositoryQuery<Permission,long> _permissionQuery;
        private readonly IRepositoryQuery<ApplicationRole, long> _applicationRoleQuery;
        private readonly ApplicationRoleManager _applicationRoleManager;

        private readonly IRepositoryQuery<EmailTemplate, long> _emailTemplateRepositoryQuery;
        private readonly IRepositoryQuery<EmailToken, long> _emailTokenRepositoryQuery;
        private readonly IRepositoryQuery<TopicCategory, int> _topicCategoryRepoQuery;
        private readonly IRepositoryCommand<EmailLog, long> _emailLogRepositoryCommand;
        public Utility(IRepositoryQuery<Permission, long> permissionQuery, IRepositoryQuery<ApplicationRole, long> applicationRoleQuery,
             IRepositoryQuery<EmailTemplate, long> emailTemplateRepositoryQuery,
            IRepositoryQuery<EmailToken, long> emailTokenRepositoryQuery,
            IRepositoryCommand<EmailLog, long> emailLogRepositoryCommand,
            IRepositoryQuery<TopicCategory, int> topicCategoryRepoQuery,
            ApplicationRoleManager applicationRoleManager)
        {
            _permissionQuery = permissionQuery;
            _applicationRoleManager = applicationRoleManager;
            _applicationRoleQuery = applicationRoleQuery;

            _emailTemplateRepositoryQuery = emailTemplateRepositoryQuery;
            _emailTokenRepositoryQuery = emailTokenRepositoryQuery;
            _emailLogRepositoryCommand = emailLogRepositoryCommand;
            _topicCategoryRepoQuery = topicCategoryRepoQuery;

        }
        public IEnumerable<SelectListItem> GetPermissions()
        {
            var types = _permissionQuery.GetAllList().Select(x =>
                                new SelectListItem
                                {
                                    Value = x.Id.ToString(),
                                    Text = x.Name

                                }).AsEnumerable();
            return new SelectList(types, "Value", "Text");
        }


        public IEnumerable<SelectListItem> GetRoles()
        {
            var types = _applicationRoleQuery.GetAllList().Select(x =>
                                new SelectListItem
                                {
                                    Value = x.Name,
                                    Text = x.Name

                                }).AsEnumerable();
            return new SelectList(types, "Value", "Text");
        }


        public IEnumerable<SelectListItem> GetTopicCategorys()
        {
            var types = _topicCategoryRepoQuery.GetAllList().Select(x =>
                                new SelectListItem
                                {
                                    Value = x.Id.ToString(),
                                    Text = x.Title

                                }).AsEnumerable();
            return new SelectList(types, "Value", "Text");
        }

        public void SendWelcomeAndPasswordResetEmail(ApplicationUser mUser, string resetUrl)
        {
            try
            {
                EmailTemplate emailFormat = _emailTemplateRepositoryQuery.GetAllList(m => m.Code == "Welc").SingleOrDefault();
                List<EmailToken> tokenCol = _emailTokenRepositoryQuery.GetAllList(m => m.EmailCode == emailFormat.Code).ToList();
                foreach (var token in tokenCol)
                {
                    if (token.Token.Equals("{UserName}"))
                    {
                        token.PreviewText = mUser.UserName;
                    }
                    else if (token.Token.Equals("{Email}"))
                    {
                        token.PreviewText = mUser.Email ?? string.Empty;
                    }
                    else if (token.Token.Equals("{Url}"))
                    {
                        token.PreviewText = resetUrl;
                    }
                }
                try
                {
                    EmailLog mlog = new EmailLog();
                    mlog.Receiver = mUser.Email;
                    mlog.Sender = ExtentionUtility.GetAppSetting("MailFrom");
                    mlog.Subject = "Welcome to VanHack Forum Portal";
                    mlog.MessageBody = ExtentionUtility.GeneratePreviewHTML(emailFormat.Body, tokenCol);
                    mlog.DateCreated = mlog.DateToSend = DateTime.Now;
                    mlog.IsSent = mlog.HasAttachment = false;
                    mlog.EmailAttachments = new List<EmailAttachment>();
                    _emailLogRepositoryCommand.Insert(mlog);
                    _emailLogRepositoryCommand.SaveChanges();
                }
                catch (DbEntityValidationException filterContext)
                {
                    if (typeof(DbEntityValidationException) == filterContext.GetType())
                    {
                        foreach (var validationErrors in filterContext.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                System.Diagnostics.Debug.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);

                            }
                        }
                    }
                    throw;
                }

            }
            catch
            {

                throw;
            }
        }


        public void SendDiscussionNotificationEmail(ApplicationUser mUser, string topic, string postContent,string postBy)
        {
            try
            {
                EmailTemplate emailFormat = _emailTemplateRepositoryQuery.GetAllList(m => m.Code == "Discussion").SingleOrDefault();
                List<EmailToken> tokenCol = _emailTokenRepositoryQuery.GetAllList(m => m.EmailCode == emailFormat.Code).ToList();
                foreach (var token in tokenCol)
                {
                    if (token.Token.Equals("{UserName}"))
                    {
                        token.PreviewText = mUser.UserName;
                    }
                    else if (token.Token.Equals("{Topic}"))
                    {
                        token.PreviewText = topic;
                    }
                    else if (token.Token.Equals("{Post}"))
                    {
                        token.PreviewText = postContent;
                    }
                    else if (token.Token.Equals("{PostedBy}"))
                    {
                        token.PreviewText = postBy;
                    }
                }
                try
                {
                    EmailLog mlog = new EmailLog();
                    mlog.Receiver = mUser.Email;
                    mlog.Sender = ExtentionUtility.GetAppSetting("MailFrom");
                    mlog.Subject = "VanHack Forum Discussion Notification";
                    mlog.MessageBody = ExtentionUtility.GeneratePreviewHTML(emailFormat.Body, tokenCol);
                    mlog.DateCreated = mlog.DateToSend = DateTime.Now;
                    mlog.IsSent = mlog.HasAttachment = false;
                    mlog.EmailAttachments = new List<EmailAttachment>();
                    _emailLogRepositoryCommand.Insert(mlog);
                    _emailLogRepositoryCommand.SaveChanges();
                }
                catch (DbEntityValidationException filterContext)
                {
                    if (typeof(DbEntityValidationException) == filterContext.GetType())
                    {
                        foreach (var validationErrors in filterContext.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                System.Diagnostics.Debug.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);

                            }
                        }
                    }
                    throw;
                }

            }
            catch
            {

                throw;
            }
        }


        public void SendPasswordResetEmail(ApplicationUser mUser, string resetUrl)
        {
            try
            {
                EmailTemplate emailFormat = _emailTemplateRepositoryQuery.GetAllList(m => m.Code == "F_PASSWORD").SingleOrDefault();
                List<EmailToken> tokenCol = _emailTokenRepositoryQuery.GetAllList(m => m.EmailCode == emailFormat.Code).ToList();
                foreach (var token in tokenCol)
                {
                    if (token.Token.Equals("{Name}"))
                    {
                        token.PreviewText = mUser.FirstName + " " + mUser.LastName;
                    }
                    else if (token.Token.Equals("{Email}"))
                    {
                        token.PreviewText = mUser.Email ?? string.Empty;
                    }
                    else if (token.Token.Equals("{Url}"))
                    {
                        token.PreviewText = resetUrl;
                    }
                }
                try
                {
                    EmailLog mlog = new EmailLog();
                    mlog.Receiver = mUser.Email;
                    mlog.Sender = ExtentionUtility.GetAppSetting("MailFrom");
                    mlog.Subject = "Password Reset Notification";
                    mlog.MessageBody = ExtentionUtility.GeneratePreviewHTML(emailFormat.Body, tokenCol);
                    mlog.DateCreated = mlog.DateToSend = DateTime.Now;
                    mlog.IsSent = mlog.HasAttachment = false;
                    mlog.EmailAttachments = new List<EmailAttachment>();
                    _emailLogRepositoryCommand.Insert(mlog);
                    _emailLogRepositoryCommand.SaveChanges();
                }
                catch (DbEntityValidationException filterContext)
                {
                    if (typeof(DbEntityValidationException) == filterContext.GetType())
                    {
                        foreach (var validationErrors in filterContext.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                System.Diagnostics.Debug.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);

                            }
                        }
                    }
                    throw;
                }

            }
            catch
            {

                throw;
            }
        }

        public IEnumerable<SelectListItem> GetSpecificRoles(Int64[] roleIds)
        {

            var types = _applicationRoleManager.Roles.Where(e =>roleIds.Contains(e.Id)).Select(x =>
                                  new SelectListItem
                                  {
                                      Value = x.Id.ToString(),
                                      Text = x.Name
                                  });

            return new SelectList(types, "Value", "Text");
        }

        public string Upload(HttpPostedFileBase file, string foldername)
        {
            var folder = System.Web.HttpContext.Current.Server.MapPath(foldername);
            var fileName = UploadFile(file, folder);
            return fileName;
        }


        private string UploadFile(HttpPostedFileBase file, string folderName)
        {
            var fileExtension = Path.GetExtension(file.FileName);

            var fileName = Guid.NewGuid() + fileExtension;
            if (!Directory.Exists(folderName)) Directory.CreateDirectory(folderName);
            var folderPath = folderName + "\\" + fileName;
            file.SaveAs(folderPath);
            return folderPath;
        }
    }
}