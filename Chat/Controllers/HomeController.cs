using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Chat.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public Connection.Connection connection { get; set; }


        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Authenticate(string username)
        {
            List<string> model = new List<string>();

            if (connection == null)
            {
                connection = new Connection.Connection(username, "#testingxxxf");
                model = connection.InitialConnection();
                Session["connection"] = connection;
            }

            return View("Authenticated", model);
        }

        [HttpPost]
        public JsonResult GetMessages()
        {
            if (connection == null)
            {
                connection = Session["connection"] as Connection.Connection;
            }

            var msg = connection.GetMessages();
            return Json(new { message = msg.Select(s => s) });
        }

        [HttpPost]
        public bool PostMessage(string msg)
        {
            if (connection == null)
            {
                connection = Session["connection"] as Connection.Connection;
            }

            connection.SendMessage(msg);

            return true;
        }
        
        [HttpPost]
        public JsonResult GetNames()
        {
            if (connection == null)
            {
                connection = Session["connection"] as Connection.Connection;
            }

            string finalNames = string.Empty;
            var names = connection.GetNames();
            if (names.Any())
            {
                var split = names.Where(s => s.Contains("353"));
                var subNames = split.First().Substring(1, split.First().Length - 1);
                finalNames = subNames.Substring(subNames.IndexOf(":")+1, subNames.Length - subNames.IndexOf(":")-1);
            }

            var result = finalNames.Split(' ');

            return Json(new { data = result.ToArray() });
        }

    }
}
