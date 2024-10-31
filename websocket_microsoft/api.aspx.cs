using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace websocket_microsoft
{
    public partial class api : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = this.Request["action"];
            switch (action)
            {
                case "hello":
                    Response.Write("hello tắc kè");
                    break;
                default:
                    Response.Write("không biết ");
                    break;
            }
        }
    }
}