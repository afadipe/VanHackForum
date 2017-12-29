﻿using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SleekSoftMVCFramework.Models;
using SleekSoftMVCFramework.Data.IdentityModel;
using SleekSoftMVCFramework.Data.IdentityService;
using SleekSoftMVCFramework.Repository.CoreRepositories;
using SleekSoftMVCFramework.Utilities;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Collections.Generic;
using SleekSoftMVCFramework.Data.Constant;
using SleekSoftMVCFramework.Data.Core;
using SleekSoftMVCFramework.Data.IdentityService.Extensions;
using log4net;
using SleekSoftMVCFramework.Data.Entities;
using SleekSoftMVCFramework.ViewModel;
using System.IO;
using System.Text;

namespace SleekSoftMVCFramework.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        //private ApplicationSignInManager _signInManager;
        //private ApplicationUserManager UserManager;
        private readonly IRepositoryQuery<ApplicationUser,long> _applicationUser;
        private readonly IRepositoryQuery<Application,int> _applicationQuery;
        private readonly IRepositoryCommand<ApplicationUser, long> _applicationUserCommand;
        private readonly IRepositoryCommand<ApplicationUserPasswordHistory, long> _passwordCommand;
        private readonly ILog _log;
        private readonly Utility _utility;

        //ApplicationUserManager userManager, ApplicationSignInManager signInManager, 
        public AccountController(IRepositoryQuery<Application, int> applicationQuery, 
            IRepositoryQuery<ApplicationUser, long>  applicationUser, 
            IRepositoryCommand<ApplicationUserPasswordHistory, long> passwordCommand,
            IRepositoryCommand<ApplicationUser, long> applicationUserCommand,
            Utility utility, ILog log)
        {
            //UserManager = userManager;
            //_signInManager = signInManager;
            _applicationUser = applicationUser;
            _passwordCommand = passwordCommand;
            _applicationQuery=applicationQuery;
            _applicationUserCommand = applicationUserCommand;
            _log = log;
            _utility = utility;
        }

        //public ApplicationSignInManager SignInManager
        //{
        //    get
        //    {
        //        return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
        //    }
        //    private set 
        //    { 
        //        _signInManager = value; 
        //    }
        //}

        //public ApplicationUserManager UserManager
        //{
        //    get
        //    {
        //        return UserManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        //    }
        //    private set
        //    {
        //        UserManager = value;
        //    }
        //}

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewData["ApplicationName"] = _applicationQuery.GetAll().FirstOrDefault();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
       // [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                ApplicationUser usermodel = UserManager.Users.FirstOrDefault(m => m.UserName.Trim() == model.Email);
                if (usermodel != null)
                {
                    var userIdentity = await UserManager.CreateIdentityAsync(usermodel, DefaultAuthenticationTypes.ApplicationCookie);
                    userIdentity.AddClaim(new Claim("FullName", usermodel.FullName));
                    userIdentity.AddClaim(new Claim("Email", usermodel.Email));
                    userIdentity.AddClaim(new Claim("DateCreated", usermodel.DateCreated.ToString("MM/dd/yyyy")));
                    userIdentity.AddClaim(new Claim("ProfileAvatar", usermodel.ProfileImage ?? " "));
                    var listIdentity = new List<ClaimsIdentity>();
                    listIdentity.Add(userIdentity);
                    ClaimsPrincipal c = new ClaimsPrincipal(listIdentity);
                    AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = model.RememberMe }, userIdentity);
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, change to shouldLockout: true
                var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
                switch (result)
                {
                    case SignInStatus.Success:

                       

                        if (User.Identity.GetFullName() == string.Empty)
                        {
                            await usermodel.GenerateUserIdentityAsync(UserManager);
                        }
                        if (usermodel.IsFirstLogin)
                        {
                            return RedirectToAction("SetFirstlogin", new { code = usermodel.Id.EncryptID() });
                        }

                        return RedirectToLocal(returnUrl);
                    case SignInStatus.LockedOut:
                        return View("Lockout");
                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return View(model);
            }
        }



        [Authorize]
        public async Task<ActionResult> MyProfile()
        {
            try
            {
                long UserId = User.Identity.GetUserId<Int64>();
                ApplicationUser usermodel = await _applicationUser.GetAsync(UserId);
                UserViewModel userdataModel = UserViewModel.EntityToModels(usermodel);
                if (userdataModel == null)
                {
                    return HttpNotFound();
                }
                return View(userdataModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return View("Error");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MyProfile(UserViewModel model)
        {
            long UserId = User.Identity.GetUserId<Int64>();
            try
            {
                if (ModelState.IsValid)
                {
                    var emailExist = _applicationUser.GetAllList(m => m.Email.ToLower().Trim() == model.Email.ToLower().Trim() && m.Id != model.Id).ToList();
                    if (emailExist.Any())
                    {
                        ModelState.AddModelError("", "email address already exist");
                        ViewBag.ErrMsg = "email address already exist";
                        return View(model);
                    }

                    //checking if username does not exist b4
                    var usernameExist = _applicationUser.GetAllList(m => m.UserName.ToLower().Trim() == model.UserName.ToLower().Trim() && m.Id != model.Id).ToList();
                    if (usernameExist.Any())
                    {
                        ModelState.AddModelError("", "username already exist");
                        ViewBag.ErrMsg = "username already exist";
                        return View(model);
                    }

                    ApplicationUser usermodel = await _applicationUser.GetAsync(model.Id);
                    {
                        usermodel.FirstName = model.FirstName;
                        usermodel.LastName = model.LastName;
                        usermodel.MiddleName = model.MiddleName;
                        usermodel.MobileNumber = model.MobileNumber;
                        usermodel.PhoneNumber = model.PhoneNumber;
                        await _applicationUserCommand.UpdateAsync(usermodel);
                        await _applicationUserCommand.SaveChangesAsync();
                        ViewBag.Msg = "User Profile was successfully updated";
                        ModelState.Clear();
                        return View(model);
                    }
                }
                else
                {
                    StringBuilder errorMsg = new StringBuilder();

                    foreach (var modelError in ModelState.Values.SelectMany(modelState => modelState.Errors))
                    {
                        errorMsg.AppendLine(modelError.ErrorMessage);
                        ModelState.AddModelError(string.Empty, modelError.ErrorMessage);
                    }
                    ViewBag.ErrMsg = errorMsg.ToString();
                    return View(model);
                }
                //  return View(model);

            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return View("Error");
            }
        }






        public ActionResult SetFirstlogin(string code)
        {
            try
            {
                SetFirstPasswordViewModel model = new SetFirstPasswordViewModel();
                model.code = code;
                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return View("Error");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetFirstlogin(SetFirstPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                long UserId = model.code.DecryptID();
                if (HasNewPasswordBeenUsedRecently(UserId, model.Password))
                {
                    //return false;
                    ModelState.AddModelError("A previous password can't be used as your new password", "Kindly provide a new password this password ");
                    return View(model);

                }else
                {
                    var result = await UserManager.ChangePasswordAsync(UserId, model.CurrentPassword, model.Password);
                    if (result.Succeeded)
                    {

                        ApplicationUserPasswordHistory passwordModel = new ApplicationUserPasswordHistory();
                        passwordModel.UserId = UserId;
                        passwordModel.DateCreated = DateTime.Now;
                        passwordModel.HashPassword = ExtentionUtility.Encrypt(model.Password);
                        _passwordCommand.Insert(passwordModel);
                        _passwordCommand.SaveChanges();

                        ApplicationUser xmodel = UserManager.FindById(UserId);
                        xmodel.IsFirstLogin = false;
                        UserManager.Update(xmodel);
                        return RedirectToAction("SetPasswordConfirmation", "Account");
                    }
                    AddErrors(result);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return View("Error");
            }
        }


        private string SetUserPermissions(long userId)
        {
            try
            {
                var mUserPremissionRolemodel = _applicationUser.StoreprocedureQueryFor<UserPremissionAndRole>(AppConstant.FetchUserPermissionAndRole + " @UserId", new SqlParameter("UserId", userId)).ToList();

                string userPermissions = "";
                if (mUserPremissionRolemodel != null)
                {
                    int i = 0;
                    foreach (var item in mUserPremissionRolemodel)
                    {
                        i = i + 1;
                        if (i == 0)
                        {
                            userPermissions = item.PermissionCode + ",";
                        }
                        else
                        {
                            userPermissions = userPermissions + item.PermissionCode + ",";
                        }
                    }
                }
                return userPermissions;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return string.Empty;
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await _signInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(UserVm model, HttpPostedFileBase profileImage)
        {
            string code = string.Empty;
            string profilePath = string.Empty;
            if (ModelState.IsValid)
            {

                if (profileImage != null && profileImage.ContentLength > 0)
                {
                    var ext = Path.GetExtension(profileImage.FileName).Trim().ToLower();
                    string[] allowedExtension = new string[] { ".jpeg", ".jpg", ".png" };
                    if (allowedExtension.Contains(ext))
                    {
                        profilePath = _utility.Upload(profileImage, ExtentionUtility.GetAppSetting("AppUploadFolder"));

                    }
                    else
                    {
                        //System.IO
                        ModelState.AddModelError("", string.Format("Invalid image extension,allowed extension are: .jpeg,.jpg,.png ", allowedExtension));
                        return View(model);
                    }
                }


                //checking if emailaddress does not exist b4
                var emailExist = _applicationUser.GetAllList(m => m.Email.ToLower().Trim() == model.Email.ToLower().Trim()).ToList();
                if (emailExist.Any())
                {
                    ModelState.AddModelError("", "email address already exist");
                    return View(model);
                }

                //checking if username does not exist b4
                var usernameExist = _applicationUser.GetAllList(m => m.UserName.ToLower().Trim() == model.UserName.ToLower().Trim()).ToList();
                if (usernameExist.Any())
                {
                    ModelState.AddModelError("", "username already exist");
                    return View(model);
                }

                ApplicationUser usermodel = UserVm.ModeltoEntity(model);
                usermodel.ProfileImage= Path.GetFileName(profilePath);

                var result = await UserManager.CreateAsync(usermodel, "passwo2345");
                if (result.Succeeded)
                {
                    ApplicationUserPasswordHistory passwordModel = new ApplicationUserPasswordHistory();
                    passwordModel.UserId = usermodel.Id;
                    passwordModel.DateCreated = DateTime.Now;
                    passwordModel.HashPassword = ExtentionUtility.Encrypt("passwo2345");
                    passwordModel.CreatedBy = usermodel.Id;
                    _passwordCommand.Insert(passwordModel);
                    _passwordCommand.Save();

                    var addRoleResult = await UserManager.AddToRoleAsync(usermodel.Id, AppConstant.ForumUserRole);
                    if (addRoleResult.Succeeded)
                    {
                        //send user reset mail
                        code = await UserManager.GeneratePasswordResetTokenAsync(usermodel.Id);
                        string portalUrl = System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Authority + System.Web.HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/";

                        var callbackUrl = Url.Action("ResetPassword", "Account", new { Area = "", userCode = usermodel.Id.EncryptID(), code = code });
                        string mPre = portalUrl + callbackUrl;
                        _log.Info(string.Format("Reset URL:{0}", mPre));
                        if (!String.IsNullOrEmpty(usermodel.Email))
                        {
                            _utility.SendWelcomeAndPasswordResetEmail(usermodel, mPre);
                        }
                        return RedirectToAction("RegisterConfirmation");
                        //redirect to success page
                    }

                }
                else
                {
                   
                    ModelState.AddModelError("", result.Errors.FirstOrDefault().ToString());
                    AddErrors(result);
                }
                
               
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult RegisterConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(long userId, string code)
        {
            if (userId.ToString() == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }



        [HttpGet]
        [Authorize]
        public ActionResult ChangePassword()
        {
            CreateViewBagParams();
            return PartialView();
        }


        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            CreateViewBagParams();
            if (!ModelState.IsValid)
            {
                return PartialView(model);
            }
            long UserId = User.Identity.GetUserId<Int64>();
            if (HasNewPasswordBeenUsedRecently(UserId, model.NewPassword))
            {
                ModelState.AddModelError("A previous password can't be used as your new password", "Kindly provide a new password this password ");
                ViewBag.ErrMsg = "A previous password can't be used as your new password, Kindly provide a new password this password ";
                return PartialView(model);

            }
            else
            {

                var result = await UserManager.ChangePasswordAsync(UserId, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    ApplicationUserPasswordHistory passwordModel = new ApplicationUserPasswordHistory();
                    passwordModel.UserId = UserId;
                    passwordModel.DateCreated = DateTime.Now;
                    passwordModel.HashPassword = ExtentionUtility.Encrypt(model.NewPassword);
                    _passwordCommand.Insert(passwordModel);
                    _passwordCommand.SaveChanges();

                    TempData["MESSAGE"] = "Password change successfully";
                    ModelState.Clear();
                    return Json(new { success = true });
                }
                else
                {
                    AddErrors(result);
                    return PartialView(model);
                }

            }
        }


        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await UserManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        // Don't reveal that the user does not exist or is not confirmed
                        return View("ForgotPasswordConfirmation");
                    }
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link

                    string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    string portalUrl = System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Authority + System.Web.HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/";

                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userCode = user.Id.EncryptID(), code = code });
                    string mPre = portalUrl + callbackUrl;
                    _log.Info(string.Format("Reset URL:{0}", mPre));
                    _utility.SendPasswordResetEmail(user, mPre);
                    return RedirectToAction("ForgotPasswordConfirmation", "Account");
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return View(model);
            }
            
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string userCode,string code)
        {
            if (code == null)
                return View("Error");
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            model.userCode = userCode;
            model.Code = code;
            return View(model);
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                Int64 UserId = model.userCode.DecryptID();
                var user = await UserManager.FindByIdAsync(UserId);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }
                //check here if password exist b4
                if (HasNewPasswordBeenUsedRecently(user.Id, model.Password))
                {
                    //return false;
                    ModelState.AddModelError("A previous password can't be used as your new password", "Kindly provide a new password this password ");
                    return View(model);

                }
                else
                {

                    var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
                    if (result.Succeeded)
                    {
                        ApplicationUserPasswordHistory passwordModel = new ApplicationUserPasswordHistory();
                        passwordModel.UserId = user.Id;
                        passwordModel.DateCreated = DateTime.Now;
                        passwordModel.HashPassword = ExtentionUtility.Encrypt(model.Password);
                        _passwordCommand.Insert(passwordModel);
                        _passwordCommand.SaveChanges();

                        ApplicationUser xmodel = UserManager.FindById(user.Id);
                        xmodel.IsFirstLogin = false;
                        UserManager.Update(xmodel);

                        return RedirectToAction("ResetPasswordConfirmation", "Account");
                    }
                    AddErrors(result);
                    return View();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return View("Error");
            }
          
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult SetPasswordConfirmation()
        {
            return View();
        }


        public bool HasNewPasswordBeenUsedRecently(Int64 userId, string proposedNewPassword)
        {
            try
            {
                int numberOfRecentPasswordsToKeep = ExtentionUtility.GetIntAppSetting("NoOfPasswordCheck");
                //first delete old record
                _applicationUser.ExecuteStoreprocedure("spPasswordHistoryDeleteNonRecentPasswords   @UserId,@numberOfRecentPasswordsToKeep", new SqlParameter("UserId", userId), new SqlParameter("numberOfRecentPasswordsToKeep", numberOfRecentPasswordsToKeep));
                //Now get recent password details
                var hashedPasswordDetails = _applicationUser.ExecuteStoredProdure<HashedPasswordDetails>("spPasswordHistorySelect  @UserId,@numberOfRecentPasswordsToKeep", new SqlParameter("UserId", userId), new SqlParameter("numberOfRecentPasswordsToKeep", numberOfRecentPasswordsToKeep));
                foreach (HashedPasswordDetails passwordDetails in hashedPasswordDetails)
                {
                    var encodePassword = ExtentionUtility.Encrypt(proposedNewPassword);
                    if (ProposedNewPasswordMatchesAPreviousPassword(passwordDetails, encodePassword))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return false;
            }
            
        }

        private bool ProposedNewPasswordMatchesAPreviousPassword(HashedPasswordDetails passwordDetails, string encodePassword)
        {
            return encodePassword == passwordDetails.HashPassword;
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await _signInManager.GetVerifiedUserIdAsync();
            if (userId.ToString() == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await _signInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await _signInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (UserManager != null)
                {
                    UserManager.Dispose();
                    //UserManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                   // _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}