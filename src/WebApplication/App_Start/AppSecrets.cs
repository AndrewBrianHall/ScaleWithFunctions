using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebApplication.App_Start
{
    public class Secrets
    {
        Dictionary<string, string> _secrets;

        public string this[string key]
        {
            get
            {
                return _secrets[key];
            }
        }

        public Secrets()
        {
            var rootDir = HttpContext.Current.Server.MapPath("~");
            string secretsJson = rootDir + "\\App_Data\\secrets.json";
            using (var reader = new StreamReader(secretsJson))
            {
                var json = reader.ReadToEnd();
                _secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
        }
    }

    public class AppSecrets
    {
        public static Secrets Secrets { get; private set; } = new Secrets();

    }
}