using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ForgeryPOC
{
    public partial class SiteMaster : MasterPage
    {
        #region Anti_Xsrf

        private string _antiXsrfTokenValue;

        protected void Page_Init(object sender, EventArgs e)
        {
            _antiXsrfTokenValue = Init_AntiXsrf(Page, Request, Response);
            Page.PreLoad += master_Page_PreLoad;
        }

        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Set_AntiXsrf(ViewState, Page, Context);
            }
            else
            {
                var isValidAntiXsrfToken = Validate_AntiXsrf(ViewState, Context, _antiXsrfTokenValue);
                if (isValidAntiXsrfToken == false)
                {
                    throw new InvalidOperationException("Validation of Anti-XSRF token failed.");
                }
            }
        }

        #endregion Anti_Xsrf

        /// <summary>
        /// Prevent Cross-Site Request Forgery by generating a token to be validated.
        /// </summary>
        /// <param name="thisPage">The current page.</param>
        /// <param name="thisRequest">The request being submitted.</param>
        /// <param name="thisResponse">The response being returned.</param>
        public static string Init_AntiXsrf(Page thisPage, HttpRequest thisRequest, HttpResponse thisResponse)
        {
            const string AntiXsrfTokenKey = "__AntiXsrfToken";
            string _antiXsrfTokenValue;
            var requestCookie = thisRequest.Cookies[AntiXsrfTokenKey];
            Guid requestCookieGuidValue;

            if (requestCookie != null && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
            {
                // Use the Anti-XSRF token from the cookie
                _antiXsrfTokenValue = requestCookie.Value;
                thisPage.ViewStateUserKey = _antiXsrfTokenValue;
            }
            else
            {
                // Generate a new Anti-XSRF token and save to the cookie
                _antiXsrfTokenValue = Guid.NewGuid().ToString("N");
                thisPage.ViewStateUserKey = _antiXsrfTokenValue;

                var responseCookie = new HttpCookie(AntiXsrfTokenKey)
                { HttpOnly = true, Value = _antiXsrfTokenValue };

                if (/*FormsAuthentication.RequireSSL &&*/ thisRequest.IsSecureConnection)
                {
                    responseCookie.Secure = true;
                }
                thisResponse.Cookies.Set(responseCookie);
            }
            return _antiXsrfTokenValue;
        }

        /// <summary>
        /// Set the Anti-Cross Site token.
        /// </summary>
        /// <param name="viewState"></param>
        /// <param name="thisPage"></param>
        /// <param name="thisContext"></param>
        public static void Set_AntiXsrf(StateBag viewState, Page thisPage, HttpContext thisContext)
        {
            viewState["__AntiXsrfToken"] = thisPage.ViewStateUserKey;
            viewState["__AntiXsrfUserName"] = thisContext.User.Identity.Name ?? String.Empty;
        }

        /// <summary>
        /// Validate the Anti-Cross-Site token.
        /// </summary>
        /// <param name="viewState"></param>
        /// <param name="thisContext"></param>
        /// <param name="tokenValue"></param>
        public bool Validate_AntiXsrf(StateBag viewState, HttpContext thisContext, string tokenValue)
        {
            if ((string)viewState["__AntiXsrfToken"] != tokenValue || (string)viewState["__AntiXsrfUserName"] != (thisContext.User.Identity.Name ?? String.Empty))
            {
                return false;
                //throw new InvalidOperationException("Validation of Anti-XSRF token failed.");
            }
            else
            {
                return true;
            }
        }
    }
}