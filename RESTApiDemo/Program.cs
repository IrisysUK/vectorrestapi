/*********************************************************************************************
 * 
 * This demonstration application gives a very simple example of how to get count data from a 
 * device using the Vector REST API.
 * 
 * The code below should be used with the API documentation provided on the device and within
 * Estate Manager.
 * 
 * The following code is not an exhaustive list of the capabilities of the REST API and is only
 * meant as a demonstration on how to get up and running.
 * 
 * 
 * 
 * *******************************************************************************************/

namespace RESTApiDemo
{
    using Newtonsoft.Json.Linq;
    using System.IO;
    using System.Net;

    /// <summary>
    /// Demo application for gathering data using the Vector REST API
    /// </summary>
    class Program
    {
        // Used for the demo, set to true if you are connecting via EM or false if connecting direct to device
        const bool EMConnect = false;

        // Base address of the EM instance or the device you're talking to
        static string baseAddress = "http://<device address here>/";

        // Web API key for the device (can be found on the security page of the device) or EM instance (can be found on the My Account page)
        static string webApiKey = "'<API Key here>'";

        static void Main(string[] args)
        {
            string deviceIdentifier = string.Empty;
            bool live = false;

            if (EMConnect)
            {
                // Connecting to a device in EM

                // Serial number of the device you want to talk to should go here
                deviceIdentifier = "<Device serial number here>";
                live = false;
            }
            else
            {
                // Connecting directly to a device
                deviceIdentifier = "direct";
                live = true;
            }
            
            // Get an access token, only has to be done once per session
            string accessToken = GetAccessToken(baseAddress, webApiKey);

            // Get the connection token, only has to be done once per session
            JToken connectionToken = GetConnectionToken(baseAddress, accessToken, deviceIdentifier, live);

            // At this point multiple calls can be made to the API to get whatever data you want out of the system
            // using the connection token aquired in the above steps

            // Get data out of the system
            JToken countData = GetPageOfCounts(baseAddress, accessToken, (string)connectionToken["Token"]);

            // Same again
            JToken countData2 = GetPageOfCounts(baseAddress, accessToken, (string)connectionToken["Token"]);

            // Get some histograms
            JToken histogramData = GetPageOfHistograms(baseAddress, accessToken, (string)connectionToken["Token"]);

            // When we're done gathering data then disconnect from the device
            Disconnect(baseAddress, accessToken, (string)connectionToken["Token"]);
        }

        /// <summary>
        /// Get the most recent 10 histogram log entrie from a device
        /// </summary>
        /// <param name="baseAddress">Base address of the device</param>
        /// <param name="accessToken">Security access token</param>
        /// <param name="connectionToken">Connection token</param>
        /// <returns></returns>
        static JToken GetPageOfHistograms(string baseAddress, string accessToken, string connectionToken)
        {
            string data;

            string url = baseAddress + "api/rt/v1/histograms/histogramspagerange?token=" + connectionToken + "&count=10&nextIndex=0&newestFirst=true";

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            WebHeaderCollection webHeaderCollection = new WebHeaderCollection();
            webHeaderCollection.Add(HttpRequestHeader.Authorization, accessToken);

            httpWebRequest.Headers = webHeaderCollection;
            httpWebRequest.MediaType = "application/json";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Method = "GET";

            using (var response = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        data = streamReader.ReadToEnd();
                    }
                }
            }

            return JToken.Parse(data);
        }

        /// <summary>
        /// Gets the most recent 10 log entries from a device
        /// </summary>
        /// <param name="baseAddress">Base address of the device</param>
        /// <param name="accessToken">Security access token</param>
        /// <param name="connectionToken">Connection token</param>
        /// <returns></returns>
        static JToken GetPageOfCounts(string baseAddress, string accessToken, string connectionToken)
        {
            string data;

            string url = baseAddress + "api/rt/v1/counts/countspagerange?token=" + connectionToken + "&count=10&nextIndex=0&newestFirst=true";

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            WebHeaderCollection webHeaderCollection = new WebHeaderCollection();
            webHeaderCollection.Add(HttpRequestHeader.Authorization, accessToken);

            httpWebRequest.Headers = webHeaderCollection;
            httpWebRequest.MediaType = "application/json";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Method = "GET";

            using (var response = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        data = streamReader.ReadToEnd();
                    }
                }
            }

            return JToken.Parse(data);
        }

        /// <summary>
        /// Get the connection token used for communications with a device
        /// </summary>
        /// <param name="baseAddress">Base address of the device</param>
        /// <param name="accessToken">Security access token</param>
        /// <param name="deviceName">Name of the device</param>
        /// <param name="live">Whether the connection should be live</param>
        /// <returns></returns>
        static JToken GetConnectionToken(string baseAddress, string accessToken, string deviceName, bool live)
        {
            string data;

            string url = baseAddress + "api/rt/v1/connection/create";

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            WebHeaderCollection webHeaderCollection = new WebHeaderCollection();
            webHeaderCollection.Add(HttpRequestHeader.Authorization, accessToken);

            httpWebRequest.Headers = webHeaderCollection;
            httpWebRequest.MediaType = "application/json";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Method = "POST";

            using (var requestStream = httpWebRequest.GetRequestStream())
            {
                using (var streamWriter = new StreamWriter(requestStream))
                {
                    JObject dataObject = new JObject();
                    dataObject.Add("Device", deviceName);
                    dataObject.Add("Live", live);

                    streamWriter.Write(dataObject.ToString());
                }
            }

            using (var response = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        data = streamReader.ReadToEnd();
                    }
                }
            }

            return JToken.Parse(data);
        }

        /// <summary>
        /// Get a security access token
        /// </summary>
        /// <param name="baseAddress">Base address of the device</param>
        /// <param name="webAPIKey">Web API Key</param>
        /// <returns></returns>
        static string GetAccessToken(string baseAddress, string webAPIKey)
        {
            string data;

            string url = baseAddress + "api/rt/v1/TokenProvider/get_token";

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            httpWebRequest.MediaType = "application/json";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "text/plain";
            httpWebRequest.Method = "POST";

            using (var requestStream = httpWebRequest.GetRequestStream())
            {
                using (var streamWriter = new StreamWriter(requestStream))
                {
                    streamWriter.Write(webAPIKey);
                }
            }

            using (var response = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        data = streamReader.ReadToEnd();
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Disconnect from the device
        /// </summary>
        /// <param name="baseAddress">Base address of the device</param>
        /// <param name="accessToken">Security access token</param>
        /// <param name="connectionToken">Connection token</param>
        /// <returns></returns>
        static void Disconnect(string baseAddress, string accessToken, string connectionToken)
        {
            string url = baseAddress + "api/rt/v1/disconnect?token=" + connectionToken;

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            WebHeaderCollection webHeaderCollection = new WebHeaderCollection();
            webHeaderCollection.Add(HttpRequestHeader.Authorization, accessToken);

            httpWebRequest.Headers = webHeaderCollection;
            httpWebRequest.MediaType = "application/json";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Method = "GET";

            using (var response = (HttpWebResponse)httpWebRequest.GetResponse())
            {
            }
        }
    }
}
