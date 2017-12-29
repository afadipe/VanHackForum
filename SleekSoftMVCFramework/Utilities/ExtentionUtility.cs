using SleekSoftMVCFramework.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.Routing;
using System.Text.RegularExpressions;

namespace SleekSoftMVCFramework.Utilities
{
    public static class SerializeDeserializeExtension
    {
        public static string Serialize(this object o)
        {
            var sw = new StringWriter();
            var formatter = new LosFormatter();
            formatter.Serialize(sw, o);

            return sw.ToString();
        }

        public static object Deserialize(this string data)
        {
            if (String.IsNullOrEmpty(data))
                return null;

            var formatter = new LosFormatter();
            return formatter.Deserialize(data);
        }
    }

    public static class ExtentionUtility
    {
        public static string GetAppSetting(string key)
        {
            try
            {
                return System.Configuration.ConfigurationManager.AppSettings[key].ToString();
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static int GetIntAppSetting(string key)
        {
            try
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings[key].ToString());
            }
            catch (Exception ex)
            {
                return 2;
            }
        }

        public static string GetApiContollerName(this ApiController controller)
        {
            return HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();

        }
        public static string GetApiActionName(this ApiController controller)
        {
            return HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();

        }
        public static string GetContollerName(this Controller controller)
        {
            return HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
        }
        public static string GetActionName(this Controller controller)
        {
            return HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
        }

        public static String EncryptID(this Int64 ID)
        {
            try
            {
                string EcryptedApplicantID = CryptoClass.EncryptPlainTextToCipherText(ID.ToString());
                EcryptedApplicantID = EcryptedApplicantID.Replace("/", "~");
                EcryptedApplicantID = EcryptedApplicantID.Replace("\\", "`");
                return EcryptedApplicantID;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public static String EncryptIntID(this int ID)
        {
            try
            {
                string EcryptedApplicantID = CryptoClass.EncryptPlainTextToCipherText(ID.ToString());
                EcryptedApplicantID = EcryptedApplicantID.Replace("/", "~");
                EcryptedApplicantID = EcryptedApplicantID.Replace("\\", "`");
                return EcryptedApplicantID;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public static String Encrypt(this string Text)
        {
            try
            {
                String Ecrypted = CryptoClass.EncryptPlainTextToCipherText(Text);
                return Ecrypted;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public static Int64 DecryptID(this string ID)
        {
            try
            {
                ID = ID.Replace("~", "/");
                ID = ID.Replace("`", "\\");
                Int64 DecryptedApplicantID = Convert.ToInt64(CryptoClass.DecryptCipherTextToPlainText(ID));
                return DecryptedApplicantID;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static int DecryptIntID(this string ID)
        {
            try
            {
                ID = ID.Replace("~", "/");
                ID = ID.Replace("`", "\\");
                int DecryptedApplicantID = Convert.ToInt32(CryptoClass.DecryptCipherTextToPlainText(ID));
                return DecryptedApplicantID;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static string GeneratePreviewHTML(string htmlcode, List<EmailToken> lstToken)
        {
            try
            {
                foreach (EmailToken item in lstToken)
                {
                    htmlcode = htmlcode.Replace(item.Token, item.PreviewText);
                }
            }
            catch {}
            return htmlcode;
        }


        public static string GetPrettyDate(string date)
        {
            DateTime time;
            if (DateTime.TryParse(date, out time))
            {
                var span = DateTime.UtcNow.Subtract(time);
                var totalDays = (int)span.TotalDays;
                var totalSeconds = (int)span.TotalSeconds;
                if ((totalDays < 0) || (totalDays >= 0x1f))
                {
                    return DateUtils.FormatDateTime(date, "dd MMMM yyyy");
                }
                if (totalDays == 0)
                {
                    if (totalSeconds < 60)
                    {
                        return DateUtils.GetLocalisedText("Just Now");
                    }
                    if (totalSeconds < 120)
                    {
                        return DateUtils.GetLocalisedText(string.Format("{0} {1}", totalSeconds,"ago"));
                    }
                    if (totalSeconds < 0xe10)
                    {
                        return DateUtils.GetLocalisedText(string.Format("{0} {1}", Math.Floor((double)(((double)totalSeconds) / 60.0)), "Minutes Ago"));
                    }
                    if (totalSeconds < 0x1c20)
                    {
                        return DateUtils.GetLocalisedText("One Hour Ago");
                    }
                    if (totalSeconds < 0x15180)
                    {
                        return DateUtils.GetLocalisedText(string.Format("{0} {1}", Math.Floor((double)(((double)totalSeconds) / 3600.0)),"Hours Ago"));
                    }
                }
                if (totalDays == 1)
                {
                    return DateUtils.GetLocalisedText("Yesterday");
                }
                if (totalDays < 7)
                {
                    return DateUtils.GetLocalisedText(string.Format("{0}  {1}",totalDays, "Days Ago"));
                }
                if (totalDays < 0x1f)
                {
                    return  DateUtils.GetLocalisedText(string.Format("{0} {1}", Math.Ceiling((double)(((double)totalDays) / 7.0)), "Weeks Ago"));
                }
            }
            return date;
        }
        
        public static MvcHtmlString Pager(this HtmlHelper helper, int currentPage, int pageSize, int totalItemCount, object routeValues, string actionOveride = null, string controllerOveride = null)
        {
            // how many pages to display in each page group const  	
            var cGroupSize = 10;
            var pageCount = (int)Math.Ceiling(totalItemCount / (double)pageSize);

            if (pageCount <= 0)
            {
                return null;
            }

            // cleanup any out bounds page number passed  	
            currentPage = Math.Max(currentPage, 1);
            currentPage = Math.Min(currentPage, pageCount);

            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext, helper.RouteCollection);
            var containerdiv = new TagBuilder("nav");
            var container = new TagBuilder("ul");
            container.AddCssClass("pagination");
            var actionName = !string.IsNullOrEmpty(actionOveride) ? actionOveride : helper.ViewContext.RouteData.GetRequiredString("action");
            var controllerName = !string.IsNullOrEmpty(controllerOveride) ? controllerOveride : helper.ViewContext.RouteData.GetRequiredString("controller");

            // calculate the last page group number starting from the current page  	
            // until we hit the next whole divisible number  	
            var lastGroupNumber = currentPage;
            while ((lastGroupNumber % cGroupSize != 0)) lastGroupNumber++;

            // correct if we went over the number of pages  	
            var groupEnd = Math.Min(lastGroupNumber, pageCount);

            // work out the first page group number, we use the lastGroupNumber instead of  	
            // groupEnd so that we don't include numbers from the previous group if we went  	
            // over the page count  	
            var groupStart = lastGroupNumber - (cGroupSize - 1);

            // if we are past the first page  	
            if (currentPage > 1)
            {
                var previousli = new TagBuilder("li");
                var previous = new TagBuilder("a");
                previous.SetInnerText("«");
                previous.AddCssClass("previous");
                var routingValues = new RouteValueDictionary(routeValues) { { "p", currentPage - 1 } };
                previous.MergeAttribute("href", urlHelper.Action(actionName, controllerName, routingValues));
                previousli.InnerHtml = previous.ToString();
                container.InnerHtml += previousli;
            }

            // if we have past the first page group  	
            if (currentPage > cGroupSize)
            {
                var previousDotsli = new TagBuilder("li");
                var previousDots = new TagBuilder("a");
                previousDots.SetInnerText("...");
                previousDots.AddCssClass("previous-dots");
                var routingValues = new RouteValueDictionary(routeValues) { { "p", groupStart - cGroupSize } };
                previousDots.MergeAttribute("href", urlHelper.Action(actionName, controllerName, routingValues));
                previousDotsli.InnerHtml = previousDots.ToString();
                container.InnerHtml += previousDotsli.ToString();
            }

            for (var i = groupStart; i <= groupEnd; i++)
            {
                var pageNumberli = new TagBuilder("li");
                pageNumberli.AddCssClass(((i == currentPage)) ? "active" : "p");
                var pageNumber = new TagBuilder("a");
                pageNumber.SetInnerText((i).ToString());
                var routingValues = new RouteValueDictionary(routeValues) { { "p", i } };
                pageNumber.MergeAttribute("href", urlHelper.Action(actionName, controllerName, routingValues));
                pageNumberli.InnerHtml = pageNumber.ToString();
                container.InnerHtml += pageNumberli.ToString();
            }

            // if there are still pages past the end of this page group  	
            if (pageCount > groupEnd)
            {
                var nextDotsli = new TagBuilder("li");
                var nextDots = new TagBuilder("a");
                nextDots.SetInnerText("...");
                nextDots.AddCssClass("next-dots");
                var routingValues = new RouteValueDictionary(routeValues) { { "p", groupEnd + 1 } };
                nextDots.MergeAttribute("href", urlHelper.Action(actionName, controllerName, routingValues));
                nextDotsli.InnerHtml = nextDots.ToString();
                container.InnerHtml += nextDotsli.ToString();
            }

            // if we still have pages left to show  	
            if (currentPage < pageCount)
            {
                var nextli = new TagBuilder("li");
                var next = new TagBuilder("a");
                next.SetInnerText("»");
                next.AddCssClass("next");
                var routingValues = new RouteValueDictionary(routeValues) { { "p", currentPage + 1 } };
                next.MergeAttribute("href", urlHelper.Action(actionName, controllerName, routingValues));
                nextli.InnerHtml = next.ToString();
                container.InnerHtml += nextli.ToString();
            }
            containerdiv.InnerHtml = container.ToString();
            return MvcHtmlString.Create(containerdiv.ToString());
        }

    }

    public static class DateUtils
    {
        public static string FormatDateTime(string date, string format)
        {
            DateTime time;
            if (DateTime.TryParse(date, out time) && !string.IsNullOrEmpty(format))
            {
                format = Regex.Replace(format, @"(?<!\\)((\\\\)*)(S)", "$1" + GetDayNumberSuffix(time));
                return time.ToString(format);
            }
            return string.Empty;
        }


        private static string GetDayNumberSuffix(DateTime date)
        {
            switch (date.Day)
            {
                case 1:
                case 0x15:
                case 0x1f:
                    return @"\s\t";

                case 2:
                case 0x16:
                    return @"\n\d";

                case 3:
                case 0x17:
                    return @"\r\d";
            }
            return @"\t\h";
        }


        public static string GetLocalisedText(string mval)
        {
            return mval;
        }
    }

}