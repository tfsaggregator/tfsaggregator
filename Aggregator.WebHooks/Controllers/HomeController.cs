using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aggregator.WebHooks.Controllers
{
    public class HomeController : Controller
    {
        // cache in session as working storage
        private Models.ConfigurationModel _configuration;
        private Models.ConfigurationModel Configuration
        {
            get
            {
                if (_configuration == null)
                    _configuration = Session["Configuration"] as Models.ConfigurationModel;
                return _configuration;
            }
            set
            {
                _configuration = value;
                Session["Configuration"] = _configuration;
            }
        }

        // GET: Default
        public ActionResult Index()
        {
            if (Configuration == null)
            {
                Configuration = new Models.ConfigurationModel();
            }
            return View(Configuration);
        }

        [HttpPost]
        public ActionResult Index(Models.ConfigurationModel configuration, string command)
        {
            try
            {
                if (command == "Add User")
                {
                    configuration.Users.Add(new Models.User());
                    Configuration = configuration;
                }
                else if (command == "Remove Selected")
                {
                    int pos = configuration.Users.Count();
                    while (pos > 0)
                    {
                        pos--;
                        if (configuration.Users[pos].Remove)
                            configuration.Users.RemoveAt(pos);
                    }
                    Configuration = configuration;
                }
                else if (command == "Cancel/Refresh")
                {
                    // force reload of data from database
                    Configuration = null;
                }
                else
                {
                    // update file
                    configuration.Save();
                    // force reload of data from database
                    Configuration = null;
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}