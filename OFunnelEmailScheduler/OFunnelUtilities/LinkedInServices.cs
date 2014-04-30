
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFunnelEmailScheduler.OFunnelDbLogic;

namespace OFunnelEmailScheduler.OFunnelUtilities
{
    class LinkedInServices
    {
        /// <summary>
        /// This method returns LinkedIn list for specified userId
        /// </summary>
        /// <param name="userId">userId</param>
        /// <returns>Response string</returns>
        public string GetLinkedInListWithUserId(string userId)
        {
            string responseData = string.Empty;
            try
            {
                NameValueCollection nameValue = new NameValueCollection();
                nameValue["userId"] = userId;

                string message = HelperMethods.GetParametersListForLogMessage(nameValue);

                HelperMethods.AddLogs("Enter into GetLinkedInListWithUserId. Parameters List => " + message);

                if (!string.IsNullOrEmpty(userId))
                {
                    OFunnelDatabaseHandler oFunnelDatabaseHandler = new OFunnelDatabaseHandler();
                    DataSet dataSet = oFunnelDatabaseHandler.GetLinkedInAccessTokenFromUserId("'" + userId + "'");

                    if (HelperMethods.IsValidDataSet(dataSet))
                    {
                        string authToken = Convert.ToString(dataSet.Tables[0].Rows[0]["accessToken"]);

                        if (!string.IsNullOrEmpty(authToken))
                        {
                            // Create request to get profile from LinkedIn.
                            string linkedInListUrl = this.CreateRequestToGetLinkedInListFromLinkedIn(authToken);

                            if (!string.IsNullOrEmpty(linkedInListUrl))
                            {
                                Int32 httpStatusCode = -1;
                                Stream response = this.ExecuteRequestToGetResult(linkedInListUrl, ref httpStatusCode);
                                if (response != null)
                                {
                                    StreamReader streamReader = new StreamReader(response);
                                    responseData = streamReader.ReadToEnd();

                                    response.Dispose();
                                }
                                else
                                {
                                    HelperMethods.AddLogs("GetLinkedInListWithUserId: LinkedIn connections response stream is null so LinkedIn connections not found for this user, either there is no connection for this user or some error occured.");
                                }
                            }
                        }
                    }
                    else
                    {
                        HelperMethods.AddLogs(string.Format("GetLinkedInListWithUserId: Failed to get Access Token from database for userId = {0}.", userId));
                    }
                }
                else
                {
                    HelperMethods.AddLogs("GetLinkedInListWithUserId: BadRequest as Required parameter {userId} is missing.");
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("GetLinkedInListWithUserId: Failed to get LinkedIn connections from LinkedIn for userId = {0}. Exception Occured {1}", userId, ex.Message));
                Debug.WriteLine("Failed to get LinkedIn Connections. Exception: " + ex.Message);
            }

            HelperMethods.AddLogs("Exit from GetLinkedInListWithUserId. \n\n");

            return responseData;
        }

                /// <summary>
        /// This method creates a web request to get LinkedIn list from LinkedIn.
        /// </summary>
        /// <param name="auth2Token">auth2 token for LinkedIn</param>
        /// <returns>Request to get LinkedIn list from LinkedIn</returns>
        public string CreateRequestToGetLinkedInListFromLinkedIn(string auth2Token)
        {
            string linkedInListUrl = string.Empty;

            if (!string.IsNullOrEmpty(auth2Token))
            {
                linkedInListUrl = Constants.LinkedInListUrl;

                linkedInListUrl = linkedInListUrl.Replace("AUTH_2_TOKEN", auth2Token);
            }

            return linkedInListUrl;
        }

        /// <summary>
        /// This method executes request to get results from LinkedIn.
        /// </summary>
        /// <param name="resuest">Request url to get result LinkedIn.</param>
        /// <returns>Return serach result stream</returns>
        public Stream ExecuteRequestToGetResult(string resuest, ref Int32 httpStatusCode)
        {
            string response = string.Empty;

            Stream responseStream = null;

            if (!string.IsNullOrEmpty(resuest))
            {
                WebService webService = new WebService();
                responseStream = webService.ExecuteWebRequest(resuest, ref httpStatusCode);
            }

            return responseStream;
        }
    }
}
