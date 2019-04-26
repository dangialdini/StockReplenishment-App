using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StockReplenishment.ErplyService {

    public class EAPIException : Exception {

        public int errorCode;
        public EAPIException(string message, int errorCode) : base(message) {
            this.errorCode = errorCode;
        }
    }

    public class EAPI {

        public const int VERIFY_USER_FAILURE = 2001;
        public const int MISSING_PARAMETERS = 2004;
        public const int WEBREQUEST_ERROR = 2002;
        public string url {
            get {
                return ConfigurationManager.AppSettings["url"];
            }
            set {
                value = ConfigurationManager.AppSettings["url"];
            }

        }
        public string clientCode {
            get {
                return ConfigurationManager.AppSettings["clientcode"];
            }
            set {
                value = ConfigurationManager.AppSettings["clientcode"];
            }
        }
        public string username {
            get {
                return ConfigurationManager.AppSettings["username"];
            }
        }
        public string password {
            get {
                return ConfigurationManager.AppSettings["password"];
            }
        }

        public string sslCaCertPath;
        private string EAPISessionKey;
        private int EAPISessionKeyExpires;

        public JObject sendRequest(string request, Dictionary<string, object> parameters = null) {
            if (parameters == null) parameters = new Dictionary<string, object>();

            if (this.clientCode == null || this.url == null || this.username == null || this.password == null) {
                throw new EAPIException("Missing paramaters", 2004);
            }

            // Add extra parameters
            parameters.Add("request", request);
            parameters.Add("clientCode", this.clientCode);
            parameters.Add("version", "1.0");
            if (request != "verifyUser") parameters.Add("sessionKey", this.getSessionKey());

            // Create web request and post data
            try {
                WebRequest wrequest = WebRequest.Create(this.url);
                wrequest.Method = "POST";
                string postData = createQueryString(parameters);
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                wrequest.ContentType = "application/x-www-form-urlencoded";
                wrequest.ContentLength = byteArray.Length;

                Stream dataStream = wrequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = wrequest.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                JObject json = JObject.Parse(responseFromServer);
                reader.Close();
                dataStream.Close();
                response.Close();
                return json;
            } catch(Exception e) {
                throw new EAPIException("WebRequest error: " + e.ToString(), 2002);
            }
        }

        public String getSessionKey() {
            // If key doesnt exist or expired
            if (EAPISessionKey == null || EAPISessionKeyExpires == 0 || EAPISessionKeyExpires < time()) {
                JObject result = this.sendRequest("verifyUser", new Dictionary<string, object>(){
                    { "username", this.username},
                    { "password", this.password }
                });
                // Failure Check
                int errorCode = (int)result["status"]["errorCode"];
                if(errorCode != 0) {
                    this.EAPISessionKey = null;
                    this.EAPISessionKeyExpires = 0;
                    throw new EAPIException("Verify user failure: error code: " + errorCode.ToString(), errorCode);
                }
                this.EAPISessionKey = (String)result["records"][0]["sessionKey"];
                this.EAPISessionKeyExpires = time() + (int)result["records"][0]["sessionLength"] - 30;
            }
            return this.EAPISessionKey;
        }

        // Return time since Unix epoch
        public int time() {
            return (int)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        // The function that makes a query string out of Dictionary
        // Args: parameters
        public string createQueryString(Dictionary<string, object> parameters) {
            var stringBuilder = new StringBuilder();
            foreach(KeyValuePair<string, object> entry in parameters) {
                stringBuilder.Append(entry.Key + "=" + entry.Value + "&");
            }
            stringBuilder.Length -= 1;
            return stringBuilder.ToString();
        }
    }
}
