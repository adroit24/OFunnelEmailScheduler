using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OFunnelEmailScheduler.OFunnelUtilities
{
    /// <summary>
    /// This class execute request and return response.
    /// </summary>
    public class WebService
    {
        /// <summary>
        /// This method execute web request to get response.
        /// </summary>
        /// <param name="request">request string to execute.</param>
        /// <returns>Response stream</returns>
        public Stream ExecuteWebRequest(string request, ref Int32 statusCode)
        {
            Stream responseStream = null;
            try
            {
                HttpWebRequest httpRequest = WebRequest.Create(request) as HttpWebRequest;
                int requestTimeoutTime = 30 * 60 * 1000;// i.e. 30 mins
                httpRequest.Timeout = requestTimeoutTime;

                HttpWebResponse response = httpRequest.GetResponse() as HttpWebResponse;
                statusCode = Convert.ToInt32(response.StatusCode);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    statusCode = Convert.ToInt32(response.StatusCode);

                    System.Diagnostics.Debug.WriteLine("Http Request Error:  StatusCode: " +
                        response.StatusCode + "Description : " + response.StatusDescription);

                    HelperMethods.AddLogs(string.Format("ExecuteWebRequest: Http Request Error:  StatusCode = {0}, Description = {1}", response.StatusCode, response.StatusDescription));
                }

                // Get response stream
                responseStream = response.GetResponseStream();

                // Dispose the response stream.
                //responseStream.Dispose();
            }
            catch (WebException webException)
            {
                HttpWebResponse webResponse = webException.Response as HttpWebResponse;
                if (webResponse != null)
                {
                    statusCode = Convert.ToInt32(webResponse.StatusCode);
                }
                else
                {
                    statusCode = Convert.ToInt32(webException.Status);
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("ExecuteWebRequest: Exception = {0}", ex.Message));
            }

            return responseStream;
        }

        /// <summary>
        /// This method execute web request to check server status.
        /// </summary>
        /// <param name="request">request string to execute.</param>
        /// <returns>Response stream</returns>
        public Stream ExecuteWebRequest(string request, ref int statusCode, ref string statusDescription, ref string errorMessage)
        {
            Stream responseStream = null;
            try
            {
                statusCode = 1000;
                statusDescription = "Unknown";
                errorMessage = "Unknown error has occured.";

                HttpWebRequest httpRequest = WebRequest.Create(request) as HttpWebRequest;

                HttpWebResponse response = httpRequest.GetResponse() as HttpWebResponse;
                statusCode = Convert.ToInt32(response.StatusCode);
                statusDescription = response.StatusDescription;
                errorMessage = string.Empty;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    statusCode = Convert.ToInt32(response.StatusCode);
                    statusDescription = response.StatusDescription;
                    errorMessage = string.Empty;

                    System.Diagnostics.Debug.WriteLine("Http Request Error:  StatusCode: " +
                        response.StatusCode + "Description : " + response.StatusDescription);

                    HelperMethods.AddLogs(string.Format("ExecuteWebRequest: Http Request Error:  StatusCode = {0}, Description = {1}", response.StatusCode, response.StatusDescription));
                }

                // Get response stream
                responseStream = response.GetResponseStream();

                // Dispose the response stream.
                //responseStream.Dispose();
            }
            catch (WebException webException)
            {
                try
                {
                    HttpWebResponse webResponse = webException.Response as HttpWebResponse;
                    if (webResponse != null)
                    {
                        statusCode = Convert.ToInt32(webResponse.StatusCode);
                        statusDescription = webResponse.StatusDescription;

                        Stream stream = webResponse.GetResponseStream();
                        StreamReader streamReader = new StreamReader(stream);
                        string textErrorMessage = streamReader.ReadToEnd();
                        errorMessage = textErrorMessage;
                    }
                    else
                    {
                        statusCode = Convert.ToInt32(webException.Status);
                        statusDescription = "WebException Status Messgae =>" + webException.Message;
                        errorMessage = webException.StackTrace;
                    }
                }
                catch (Exception ex)
                {
                    statusCode = 1008;
                    statusDescription = "ExceptionOccurred";
                    errorMessage = ex.Message;
                    HelperMethods.AddLogs(string.Format("ExecuteWebRequest: Exception occurred when handling WebException Exception = {0}", ex.Message));
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("ExecuteWebRequest: Exception = {0}", ex.Message));
                statusCode = 1008;
                statusDescription = "ExceptionOccurred";
                errorMessage = ex.Message;
            }

            return responseStream;
        }
    }
}
