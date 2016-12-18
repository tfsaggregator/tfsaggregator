using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Collections.Specialized;

namespace Aggregator.WebHooks.Models
{
    public class ConfigurationModel
    {
        public ConfigurationModel()
        {
            var config = WebConfigurationManager.OpenWebConfiguration("~");
            var usersSection = config.GetSection("Users") as AppSettingsSection;
            var usersCollection = usersSection.Settings;
            this.Users = new List<User>();
            foreach (KeyValueConfigurationElement kv in usersCollection)
            {
                this.Users.Add(new User() { Username = kv.Key, Password = kv.Value });
            }
            this.PolicyFilePath = config.AppSettings.Settings["policyFilePath"].Value;
        }

        public void Save()
        {
            var config = WebConfigurationManager.OpenWebConfiguration("~");
            config.AppSettings.Settings["policyFilePath"].Value = this.PolicyFilePath;
            var usersSection = config.GetSection("Users") as AppSettingsSection;
            var usersCollection = usersSection.Settings;
            usersCollection.Clear();
            foreach (var user in this.Users)
            {
                usersCollection.Add(user.Username, user.Password);
            }
            config.Save();
        }

        [DisplayName("Authorized users")]
        public List<User> Users { get; private set; }
        [DisplayName("Path to Policy File")]
        public string PolicyFilePath { get; set; }
    }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        // HACK
        public bool Remove { get; set; }
    }
}