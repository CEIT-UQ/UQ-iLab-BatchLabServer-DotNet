using System;

namespace LabClientHtml
{
    public partial class LabClient : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("~/Home.aspx");
        }
    }
}
