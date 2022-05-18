using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ForgeryPOC
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnCount_Click(object sender, EventArgs e)
        {
            if (ViewState["count"] is null)
            {
                ViewState["count"] = 1;
            }
            labelCount.Text = ViewState["count"].ToString();
            ViewState["count"] = (int)ViewState["count"] + 1;
        }
    }
}