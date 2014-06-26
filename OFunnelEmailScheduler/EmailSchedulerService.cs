using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using OFunnelEmailScheduler.OFunnelDbLogic;
using OFunnelEmailScheduler.OFunnelUtilities;
using System.Collections.Specialized;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Security;
using System.Net.Mail;
using System.Globalization;

namespace OFunnelEmailScheduler
{
    /// <summary>
    /// Class for email schedule service.
    /// </summary>
    public partial class EmailSchedulerService : ServiceBase
    {
        Timer emailSchedulerTimer;
        Timer networkUpdateTimer;
        Timer accessTokenExpiredEmailTimer;
        Timer trialPeriodExpiredEmailTimer;
        Timer similarCompaniesTimer;
        Timer updateCompaniesSchedulerTimer;
        Timer followUpNetworkUpdateTimer;

        private System.Timers.Timer checkServerStatusTimer;
        private System.Timers.Timer sendPushNotificationTimer;

        private int numberOfThreadsNotYetCompleted = -1;
        private int numberOfThreadsForSimilarCompaniesNotYetCompleted = -1;
        private int numberOfThreadsForNetworkUpdateEmailNotYetCompleted = -1;
        private int numberOfThreadsForAccessTokenExpiredEmailNotYetCompleted = -1;
        private int numberOfThreadsToSendTrialPeriodExpiredEmailNotYetCompleted = -1;
        private int numberOfThreadsToUpdateCompanyDetailsNotYetCompleted = -1;
        private int numberOfThreadsToSendPushNotificationNotYetCompleted = -1;
        private int numberOfThreadsForFollowupNetworkUpdateEmailNotYetCompleted = -1;
        
        private ManualResetEvent doneEvent = new ManualResetEvent(false);
        private ManualResetEvent doneEventForSimilarCompanies = new ManualResetEvent(false);
        private ManualResetEvent doneEventForNetworkUpdateEmail = new ManualResetEvent(false);
        private ManualResetEvent doneEventForAccessTokenExpiredEmail = new ManualResetEvent(false);
        private ManualResetEvent doneEventToSendTrialPeriodExpiredEmail = new ManualResetEvent(false);
        private ManualResetEvent doneEventToUpdateCompanies = new ManualResetEvent(false);
        private ManualResetEvent doneEventToSendPushNotification = new ManualResetEvent(false);
        private ManualResetEvent doneEventForFollowUpNetworkUpdateEmail = new ManualResetEvent(false);
        
        private AllArticles allArticles = null;

        /// <summary>
        /// EmailSchedulerService clas constructor.
        /// </summary>
        public EmailSchedulerService()
        {
            InitializeComponent();
            
            int workerThreads = -1;
            int completionPortThreads = -1;
            ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
            HelperMethods.AddLogs(string.Format("EmailSchedulerService: Threads Available =>  workerThreads = {0} and completionPortThreads = {1}.", workerThreads, completionPortThreads));

            int maxThreadRequired = Config.MaxThreadRequired;

            if (workerThreads >= maxThreadRequired)
            {
                workerThreads = maxThreadRequired;
            }

            if (completionPortThreads > maxThreadRequired)
            {
                completionPortThreads = maxThreadRequired;
            }

            bool isThreadLimitSet = ThreadPool.SetMaxThreads(workerThreads, completionPortThreads);
            HelperMethods.AddLogs("EmailSchedulerService: Thread limit set status => isThreadLimitSet = " + isThreadLimitSet);
        }

        /// <summary>
        /// OnStart call back for service start.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            DateTime now = DateTime.Now;

            // weekly request email timer.
            DateTime todayEmailSendTime = now.Date.AddHours(Config.EmailSendTime);
            DateTime nextEmailSendTime = now <= todayEmailSendTime ? todayEmailSendTime : todayEmailSendTime.AddHours(Config.TimerIntervalInHours);
            emailSchedulerTimer = new Timer(RequestEmailSchedulerTimeElapsed, null, nextEmailSendTime - DateTime.Now, TimeSpan.FromHours(Config.TimerIntervalInHours));

            //// Similar companies timer.
            //DateTime todaySimilarCompaniesTime = now.Date.AddHours(Config.SimilarCompaniesTime);
            //DateTime nextSimilarCompaniesTime = now <= todaySimilarCompaniesTime ? todaySimilarCompaniesTime : todaySimilarCompaniesTime.AddHours(Config.SimilarCompaniesTimerIntervalInHours);
            //similarCompaniesTimer = new Timer(SimilarCompaniesSchedulerTimeElapsed, null, nextSimilarCompaniesTime - DateTime.Now, TimeSpan.FromHours(Config.SimilarCompaniesTimerIntervalInHours));

            // Network update email sent timer.
            DateTime todayNetworkUpdateTime = now.Date.AddHours(Config.UpdateNetworkTime);
            DateTime nextNetworkUpdateTime = now <= todayNetworkUpdateTime ? todayNetworkUpdateTime : todayNetworkUpdateTime.AddHours(Config.UpdateNetworkTimerIntervalInHours);
            networkUpdateTimer = new Timer(NetworkUpdateEmailSchedulerTimeElapsed, null, nextNetworkUpdateTime - DateTime.Now, TimeSpan.FromHours(Config.UpdateNetworkTimerIntervalInHours));


            // Access token expired email sent timer.
            DateTime todayAccessTokenExpiredEmailTime = now.Date.AddHours(Config.AccessTokenExpiredEmailTime);
            DateTime nextAccessTokenExpiredEmailTime = now <= todayAccessTokenExpiredEmailTime ? todayAccessTokenExpiredEmailTime : todayAccessTokenExpiredEmailTime.AddHours(Config.AccessTokenExpiredTimerIntervalInHours);
            accessTokenExpiredEmailTimer = new Timer(AccessTokenExpiredEmailSchedulerTimeElapsed, null, nextAccessTokenExpiredEmailTime - DateTime.Now, TimeSpan.FromHours(Config.AccessTokenExpiredTimerIntervalInHours));

            // Trial Period expired email sent timer.
            //DateTime todayTrialPeriodExpiredEmailTime = now.Date.AddHours(Config.TrialPeriodExpiredEmailTime);
            //DateTime nextTrialPeriodExpiredEmailTime = now <= todayTrialPeriodExpiredEmailTime ? todayTrialPeriodExpiredEmailTime : todayTrialPeriodExpiredEmailTime.AddHours(Config.TrialPeriodExpiredTimerIntervalInHours);
            //trialPeriodExpiredEmailTimer = new Timer(TrialPeriodExpiredEmailSchedulerTimeElapsed, null, nextTrialPeriodExpiredEmailTime - DateTime.Now, TimeSpan.FromHours(Config.TrialPeriodExpiredTimerIntervalInHours));

            DateTime todayUpdateCompaniesTime = now.Date.AddHours(Config.UpdateCompaniesTime);
            DateTime nextUpdateCompaniesTime = now <= todayUpdateCompaniesTime ? todayUpdateCompaniesTime : todayUpdateCompaniesTime.AddHours(Config.UpdateCompaniesTimerIntervalInHours);
            updateCompaniesSchedulerTimer = new Timer(UpdateCompaniesSchedulerTimeElapsed, null, nextUpdateCompaniesTime - DateTime.Now, TimeSpan.FromHours(Config.UpdateCompaniesTimerIntervalInHours));

            // weekly request email timer.
            DateTime todayFollowupsEmailTime = now.Date.AddHours(Config.FollowupEmailSendTime);
            DateTime nextFollowupsEmailSendTime = now <= todayFollowupsEmailTime ? todayFollowupsEmailTime : todayFollowupsEmailTime.AddHours(Config.FollowupEmailTimerIntervalInHours);
            followUpNetworkUpdateTimer = new Timer(RequestFollowupsEmailSchedulerTimeElapsed, null, nextFollowupsEmailSendTime - DateTime.Now, TimeSpan.FromHours(Config.FollowupEmailTimerIntervalInHours));

            /////////////////////////////////////////////////////////
            //// Check Server Status
            /////////////////////////////////////////////////////////
            
            // Check server status timer.
            checkServerStatusTimer = new System.Timers.Timer(Config.CheckServerStatusTimerInterval);

            // Hook up the Elapsed event for the timer.
            checkServerStatusTimer.Elapsed += new System.Timers.ElapsedEventHandler(CheckServerStatusSchedulerTimeElapsed);

            // Set the Interval to 5 min (300000 milliseconds).
            checkServerStatusTimer.Interval = Config.CheckServerStatusTimerInterval;
            checkServerStatusTimer.Enabled = true;

            /////////////////////////////////////////////////////////
            //// Send Push Notification
            /////////////////////////////////////////////////////////
            
            // Send push Notification timer.
            sendPushNotificationTimer = new System.Timers.Timer(Config.SendPushNotificationTimerInterval);

            // Hook up the Elapsed event for the timer.
            sendPushNotificationTimer.Elapsed += new System.Timers.ElapsedEventHandler(SendPushNotificationSchedulerTimeElapsed);

            // Set the Interval to 1 hour (3600000 milliseconds).
            sendPushNotificationTimer.Interval = Config.SendPushNotificationTimerInterval;
            sendPushNotificationTimer.Enabled = true;
        }

        /// <summary>
        /// OnStop call back for service stop.
        /// </summary>
        protected override void OnStop()
        {

        }

        /// <summary>
        /// OnPause call back for service pause.
        /// </summary>
        protected override void OnPause()
        {

        }

        /// <summary>
        /// OnContinue call back for service continue.
        /// </summary>
        protected override void OnContinue()
        {

        }

        /// <summary>
        /// OnShutdown call back for service shutdown.
        /// </summary>
        protected override void OnShutdown()
        {

        }

        /// <summary>
        /// Request Email scheduler time elapsed callback.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">Event arguments</param>
        private void RequestEmailSchedulerTimeElapsed(object sender)
        {
            DayOfWeek dayOfWeek =  DateTime.Now.DayOfWeek;

            // To send email on tuesday only.
            if ((DayOfWeek.Tuesday == dayOfWeek))
            {
                //this.SendEmailToOFunnelUsersForOpenRequests();
            }
        }

        /// <summary>
        /// This method send email to all ofunnel user for all open requests.
        /// </summary>
        private void SendEmailToOFunnelUsersForOpenRequests()
        {
            try
            {
                OFunnelUsers oFunnelUsers = this.GetAllOFunnelUsers();

                if (oFunnelUsers != null && oFunnelUsers.oFunnelUser != null && oFunnelUsers.oFunnelUser.Length > 0)
                {
                    this.allArticles = this.GetArticleDetails();

                    this.numberOfThreadsNotYetCompleted = oFunnelUsers.oFunnelUser.Length;

                    foreach (OFunnelUser oFunnelUser in oFunnelUsers.oFunnelUser)
                    {
                        if (oFunnelUser.userId != -1)
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolCallbackToSendEmail), oFunnelUser);
                        }
                        else
                        {
                            numberOfThreadsNotYetCompleted--;
                        }
                    }

                    this.doneEvent.WaitOne();
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("Failed to send emails. \n\n", ex.Message));
            }
        }

        public void ThreadPoolCallbackToSendEmail(Object threadContext)
        {
            int userIndex = -1;

            try
            {
                OFunnelUser oFunnelUser = threadContext as OFunnelUser;
                userIndex = oFunnelUser.userIndex;

                AllOpenRequests allOpenRequests = this.GetRequestsGot(Convert.ToString(oFunnelUser.userId));
                if (allOpenRequests != null && allOpenRequests.openRequestDetails != null && allOpenRequests.openRequestDetails.Length > 0)
                {
                    var openRequests = from openReqDetails in allOpenRequests.openRequestDetails
                                       where openReqDetails.toUserId == -1
                                       select openReqDetails;

                    // Sort request details.
                    OpenRequestDetails[] requestDetailsTosendOnEmail = this.SortRequestDetails(Convert.ToString(oFunnelUser.userId), openRequests.ToArray());

                    string toEmail = oFunnelUser.email;
                    string userName = oFunnelUser.firstName + " " + oFunnelUser.lastName;

                    if (requestDetailsTosendOnEmail != null && requestDetailsTosendOnEmail.Length > 0)
                    {
                        HelperMethods.AddLogs(string.Format("Daily status email for open requests (request count {0}) sending to {1} at EmailId: {2}.", requestDetailsTosendOnEmail.Length, userName, toEmail));

                        // Send email for all public and private request to user.
                        EmailService emailService = new EmailService();

                        emailService.CreateOpenRequestSectionForEmailTemplate(requestDetailsTosendOnEmail);
                        emailService.CreateArticleSectionForEmailTemplate(allArticles);
                        
                        NameValueCollection nameValues = new NameValueCollection();
                        nameValues["userName"] = userName;

                        emailService.SetUsersDetailsToSendEmail(nameValues);

                        string openRequestEmailSubject = Constants.OpenRequestEmailSubject;

                        bool isMailSend = emailService.SendMailToAllUsers(toEmail, string.Empty, openRequestEmailSubject, EmailType.OpenRequestEmail);
                        if (isMailSend)
                        {
                            HelperMethods.AddLogs(string.Format("Daily status email for open requests (request count {0}) sends sucessfully to {1} at EmailId: {2}.\n\n", requestDetailsTosendOnEmail.Length, userName, toEmail));
                        }
                        else
                        {
                            HelperMethods.AddLogs(string.Format("Daily status email for open requests (request count {0}) failed to send to {1} at EmailId: {2}.\n\n", requestDetailsTosendOnEmail.Length, userName, toEmail));
                        }
                    }
                    else
                    {
                        HelperMethods.AddLogs(string.Format("Open requests count is 0 for {0}.", userName));
                    }
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendEmail: Failed to send emails. Exception{0} \n\n", ex.Message));
            }

            if (Interlocked.Decrement(ref numberOfThreadsNotYetCompleted) == 0)
            {
                this.doneEvent.Set();
            }
        }

        /// <summary>
        /// This method sorts request details.
        /// </summary>
        /// <param name="userId">userId</param>
        /// <param name="requestDetails">requestDetails</param>
        /// <returns>Sorted request details.</returns>
        private OpenRequestDetails[] SortRequestDetails(string userId, OpenRequestDetails[] requestDetails)
        {
            try
            {   
                LinkedInServices linkedInServices = new LinkedInServices();
                string responseData = linkedInServices.GetLinkedInListWithUserId(userId);

                if (!string.IsNullOrEmpty(responseData))
                {
                    JObject jsonDataObject = JObject.Parse(responseData);

                    if (jsonDataObject != null)
                    {
                        JArray persons = (JArray)jsonDataObject["values"];

                        if (persons != null && persons.Count > 0)
                        {
                            foreach (OpenRequestDetails request in requestDetails)
                            {
                                string targetName = request.forUserId != -1 ? request.forUserName : string.Empty;
                                string companySearched = request.companySearched;
                                companySearched = HelperMethods.IgnoreBlackListWordFromCompanyName(companySearched);

                                bool isCompanyMatched = false;

                                foreach (var person in persons)
                                {
                                    string personName = (string)person["firstName"] + " " + (string)person["lastName"];

                                    if (!string.IsNullOrEmpty(targetName) && !string.IsNullOrEmpty(personName) && personName.ToUpper().Equals(targetName.ToUpper()))
                                    {
                                        request.matchedFound = true;
                                        break;
                                    }

                                    JObject positions = (JObject)person["positions"];
                                    if (positions != null)
                                    {
                                        JArray values = (JArray)positions["values"];
                                        if (values != null && values.Count > 0)
                                        {
                                            foreach (var value in values)
                                            {
                                                JObject company = (JObject)value["company"];
                                                if (company != null)
                                                {
                                                    string companyName = (string)company["name"];
                                                    if (!string.IsNullOrEmpty(companySearched) && !string.IsNullOrEmpty(companyName))
                                                    {
                                                        isCompanyMatched = HelperMethods.CheckCompanyNameMatched(companySearched, companyName);
                                                        if (isCompanyMatched)
                                                        {
                                                            request.matchedFound = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (isCompanyMatched)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    HelperMethods.AddLogs(string.Format("SortRequestDetails: Failed to get LinkedIn connections for UserId = {0}, So Sorting on basis of person and company name cannot be done.", userId));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("failed to set companyMatched result. Exception: " + ex.Message);
            }

            var sortedRequestDetails = requestDetails.OrderByDescending(x => x.matchedFound).ThenByDescending(x => x.fromUserScore).ThenByDescending(x => Convert.ToDateTime(x.createdAt));

            return sortedRequestDetails.ToArray();
        }

        /// <summary>
        /// This method gets all introduction request user has received based on toUserId.
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>AllRequest</returns>
        public AllOpenRequests GetRequestsGot(string userId)
        {
            AllOpenRequests allOpenRequests = new AllOpenRequests();
  
            try
            {
                NameValueCollection nameValue = new NameValueCollection();
                nameValue["userId"] = userId;

                string message = HelperMethods.GetParametersListForLogMessage(nameValue);

                HelperMethods.AddLogs("Enter In GetRequestsGot. Parameters List => " + message);

                if (!string.IsNullOrEmpty(userId))
                {
                    OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                    DataSet dataSet = databaseHandler.GetRequestsGotForUserId("'" + userId + "'");

                    if (HelperMethods.IsValidDataSet(dataSet))
                    {
                        OpenRequestDetails openRequestDetails = null;
                        List<OpenRequestDetails> requestDetailsList = new List<OpenRequestDetails>();

                        for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                        {
                            openRequestDetails = new OpenRequestDetails();

                            openRequestDetails.querySearched = Convert.ToString(dataSet.Tables[0].Rows[i]["querySearched"]);
                            openRequestDetails.companySearched = Convert.ToString(dataSet.Tables[0].Rows[i]["companySearched"]);

                            openRequestDetails.fromUserId = string.IsNullOrEmpty(Convert.ToString(dataSet.Tables[0].Rows[i]["fromUserId"])) ? -1 : Convert.ToInt32(dataSet.Tables[0].Rows[i]["fromUserId"]);
                            openRequestDetails.fromUserName = Convert.ToString(dataSet.Tables[0].Rows[i]["fromUserName"]);
                            openRequestDetails.fromUserProfilePicUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["fromUserProfilePicUrl"]);
                            openRequestDetails.fromUserProfileUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["fromUserProfileUrl"]);
                            openRequestDetails.fromUserCompany = Convert.ToString(dataSet.Tables[0].Rows[i]["fromUserCompany"]);
                            openRequestDetails.fromUserHeadline = Convert.ToString(dataSet.Tables[0].Rows[i]["fromUserHeadline"]);
                            openRequestDetails.fromUserScore = string.IsNullOrEmpty(Convert.ToString(dataSet.Tables[0].Rows[i]["fromUserScore"])) ? -1 : Convert.ToInt32(dataSet.Tables[0].Rows[i]["fromUserScore"]);

                            openRequestDetails.forUserId = string.IsNullOrEmpty(Convert.ToString(dataSet.Tables[0].Rows[i]["forUserId"])) ? -1 : Convert.ToInt32(dataSet.Tables[0].Rows[i]["forUserId"]);
                            openRequestDetails.forUserName = Convert.ToString(dataSet.Tables[0].Rows[i]["forUserName"]);
                            openRequestDetails.forUserProfilePicUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["forUserProfilePicUrl"]);
                            openRequestDetails.forUserProfileUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["forUserProfileUrl"]);
                            openRequestDetails.forUserCompany = Convert.ToString(dataSet.Tables[0].Rows[i]["forUserCompany"]);
                            openRequestDetails.forUserHeadline = Convert.ToString(dataSet.Tables[0].Rows[i]["forUserHeadline"]);
                            openRequestDetails.forUserScore = string.IsNullOrEmpty(Convert.ToString(dataSet.Tables[0].Rows[i]["forUserScore"])) ? -1 : Convert.ToInt32(dataSet.Tables[0].Rows[i]["forUserScore"]);

                            openRequestDetails.toUserId = string.IsNullOrEmpty(Convert.ToString(dataSet.Tables[0].Rows[i]["toUserId"])) ? -1 : Convert.ToInt32(dataSet.Tables[0].Rows[i]["toUserId"]);
                            openRequestDetails.toUserName = Convert.ToString(dataSet.Tables[0].Rows[i]["toUserName"]);
                            openRequestDetails.toUserProfilePicUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["toUserProfilePicUrl"]);
                            openRequestDetails.toUserProfileUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["toUserProfileUrl"]);
                            openRequestDetails.toUserCompany = Convert.ToString(dataSet.Tables[0].Rows[i]["toUserCompany"]);
                            openRequestDetails.toUserHeadline = Convert.ToString(dataSet.Tables[0].Rows[i]["toUserHeadline"]);
                            openRequestDetails.toUserScore = string.IsNullOrEmpty(Convert.ToString(dataSet.Tables[0].Rows[i]["toUserScore"])) ? -1 : Convert.ToInt32(dataSet.Tables[0].Rows[i]["toUserScore"]);

                            openRequestDetails.status = Convert.ToString(dataSet.Tables[0].Rows[i]["status"]);
                            openRequestDetails.updatedAt = Convert.ToString(dataSet.Tables[0].Rows[i]["updatedAt"]);
                            openRequestDetails.content = Convert.ToString(dataSet.Tables[0].Rows[i]["content"]);
                            openRequestDetails.requestId = string.IsNullOrEmpty(Convert.ToString(dataSet.Tables[0].Rows[i]["requestId"])) ? -1 : Convert.ToInt32(dataSet.Tables[0].Rows[i]["requestId"]);

                            requestDetailsList.Add(openRequestDetails);
                        }

                        allOpenRequests.openRequestDetails = requestDetailsList.ToArray();
                    }
                    else
                    {
                        HelperMethods.AddLogs("GetRequestsGot: (InvalidDataSet) Data received form database is invalid.");
                    }
                }
                else
                {
                    HelperMethods.AddLogs("GetRequestsGot: Required parameter {userId} is missing.");
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("GetRequestsGot: Failed to get request got details from Database for userId = {0}. Exception Occured {1}", userId, ex.Message));

                Debug.WriteLine("Failed to get request got details from Database. Exception: " + ex.Message);
            }

            HelperMethods.AddLogs("Exit from GetRequestsGot.");

            return allOpenRequests;
        }

        /// <summary>
        /// This method gets all articles for weekly email.
        /// </summary>
        /// <returns>AllArticles</returns>
        private AllArticles GetArticleDetails()
        {
            AllArticles allArticles = new AllArticles();

            HelperMethods.AddLogs("Enter in method GetArticleDetails.");
            try
            {
                OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                DataSet dataSet = databaseHandler.GetArticaleDetails(string.Empty);

                if (HelperMethods.IsValidDataSet(dataSet))
                {
                    Article article = null;
                    List<Article> articleList = new List<Article>();

                    for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                    {
                        article = new Article();

                        article.headline = Convert.ToString(dataSet.Tables[0].Rows[i]["headline"]);
                        article.summary = Convert.ToString(dataSet.Tables[0].Rows[i]["summary"]);
                        article.articleUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["articleUrl"]);

                        articleList.Add(article);
                    }

                    allArticles.article = articleList.ToArray();
                }
                else
                {
                    HelperMethods.AddLogs("GetArticleDetails: (InvalidDataSet) Data received form database is invalid.");
                }
                
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("GetArticleDetails: Failed to get article details from database. Exception Occured {0}", ex.Message));
            }

            HelperMethods.AddLogs("Exit from GetArticleDetails.");

            return allArticles;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  Similar Companies functionality for target accounts.
        //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Similar companies scheduler time elapsed callback.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        private void SimilarCompaniesSchedulerTimeElapsed(object sender)
        {
            this.CheckForSimilarCompaniesForTargetAccounts();
        }

        /// <summary>
        /// This method checks for similar companies for target accounts added in last 24 hours.
        /// </summary>
        private void CheckForSimilarCompaniesForTargetAccounts()
        {
            HelperMethods.AddLogs("Enter in CheckForSimilarCompaniesForTargetAccounts.");

            try
            {
                OFunnelUsers oFunnelUsers = this.GetAllOFunnelUsers();

                if (oFunnelUsers != null && oFunnelUsers.oFunnelUser.Length > 0)
                {
                    this.numberOfThreadsForSimilarCompaniesNotYetCompleted = oFunnelUsers.oFunnelUser.Length;

                    foreach (OFunnelUser oFunnelUser in oFunnelUsers.oFunnelUser)
                    {
                        if (oFunnelUser.userId != -1)
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolCallbackForSimilarCompanies), oFunnelUser);
                        }
                        else
                        {
                            numberOfThreadsForSimilarCompaniesNotYetCompleted--;
                        }
                    }

                    this.doneEventForSimilarCompanies.WaitOne();
                }
                else
                {
                    HelperMethods.AddLogs("CheckForSimilarCompaniesForTargetAccounts: No ofunnel user details get from database.");
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("CheckForSimilarCompaniesForTargetAccounts: Failed to check for simlar companies for newly added target acounts. Exception = {0}", ex.Message));
            }

            HelperMethods.AddLogs("Exit from CheckForSimilarCompaniesForTargetAccounts. \n\n");
        }

        /// <summary>
        /// ThreadPool callback for similar companies
        /// </summary>
        /// <param name="threadContext"></param>
        public void ThreadPoolCallbackForSimilarCompanies(Object threadContext)
        {
            string userId = string.Empty;
            try
            {
                OFunnelUser oFunnelUser = threadContext as OFunnelUser;
                userId = Convert.ToString(oFunnelUser.userId);

                HelperMethods.AddLogs("Enter In ThreadPoolCallbackForSimilarCompanies: for userId = " + userId);

                string resuest = Config.SimilarCompaniesUrl;

                resuest = resuest.Replace(":USER_ID", userId);

                Stream responseStream = null;
                if (!string.IsNullOrEmpty(resuest))
                {
                    WebService webService = new WebService();
                    Int32 httpStatusCode = -1;
                    responseStream = webService.ExecuteWebRequest(resuest, ref httpStatusCode);

                    if (responseStream != null)
                    {
                        StreamReader reader = new StreamReader(responseStream);
                        string response = reader.ReadToEnd();

                        HelperMethods.AddLogs(string.Format("ThreadPoolCallbackForSimilarCompanies: Response Message = {0} for userId = {1}.", response, userId));

                        responseStream.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("ThreadPoolCallbackForSimilarCompanies: Failed to add similiar companies For UserId = {0}, Exception = {1} \n\n", userId, ex.Message));
            }

            if (Interlocked.Decrement(ref this.numberOfThreadsForSimilarCompaniesNotYetCompleted) == 0)
            {
                this.doneEventForSimilarCompanies.Set();
            }

            HelperMethods.AddLogs(string.Format("Exit from ThreadPoolCallbackForSimilarCompanies: for userId = {0} \n\n", userId));
        }

        /// <summary>
        /// This methods gets details for all ofunnnel users.
        /// </summary>
        /// <returns>OFunnelUsers</returns>
        private OFunnelUsers GetAllOFunnelUsers()
        {
            OFunnelUsers oFunnelUsers = null;

            try
            {
                OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                DataSet dataSet = databaseHandler.GetAllOFunnelUsers();

                if (HelperMethods.IsValidDataSet(dataSet))
                {
                    oFunnelUsers = new OFunnelUsers();

                    OFunnelUser oFunnelUser = null;

                    List<OFunnelUser> oFunnelUserList = new List<OFunnelUser>();

                    for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                    {
                        oFunnelUser = new OFunnelUser();
                        oFunnelUser.userIndex = i;
                        oFunnelUser.userId = string.IsNullOrEmpty(Convert.ToString(dataSet.Tables[0].Rows[i]["userId"])) ? -1 : Convert.ToInt32(dataSet.Tables[0].Rows[i]["userId"]); ;
                        oFunnelUser.firstName = Convert.ToString(dataSet.Tables[0].Rows[i]["firstName"]);
                        oFunnelUser.lastName = Convert.ToString(dataSet.Tables[0].Rows[i]["lastName"]);
                        oFunnelUser.email = Convert.ToString(dataSet.Tables[0].Rows[i]["email"]);

                        oFunnelUserList.Add(oFunnelUser);
                    }

                    oFunnelUsers.oFunnelUser = oFunnelUserList.ToArray();
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs("GetAllOFunnelUsers: Failed to get all ofunnel users from database." + ex.Message);
            }

            return oFunnelUsers;
        }

        ////////////////////////////////////////////////////////////////
        // Network Update Email send functionality Start
        ///////////////////////////////////////////////////////////////

        /// <summary>
        /// Network update email scheduler time elapsed callback.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">Event arguments</param>
        private void NetworkUpdateEmailSchedulerTimeElapsed(object sender)
        {
            this.SendEmailToOFunnelUsersForNetworkUpdate();   
        }

        /// <summary>
        /// This method send email to all ofunnel user for network updates for company, role, person name.
        /// </summary>
        private void SendEmailToOFunnelUsersForNetworkUpdate()
        {
            HelperMethods.AddLogs("Enter in SendEmailToOFunnelUsersForNetworkUpdate.");

            try
            {
                OFunnelUsers oFunnelUsers = this.GetAllOFunnelUsersForNetworkUpdates();

                if (oFunnelUsers != null && oFunnelUsers.oFunnelUser != null && oFunnelUsers.oFunnelUser.Length > 0)
                {
                    this.numberOfThreadsForNetworkUpdateEmailNotYetCompleted = oFunnelUsers.oFunnelUser.Length;

                    foreach (OFunnelUser oFunnelUser in oFunnelUsers.oFunnelUser)
                    {
                        if (oFunnelUser.userId != -1)
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolCallbackToSendNetworkUpdateEmail), oFunnelUser);
                        }
                        else
                        {
                            numberOfThreadsForNetworkUpdateEmailNotYetCompleted--;
                        }
                    }

                    this.doneEventForNetworkUpdateEmail.WaitOne();
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("SendEmailToOFunnelUsersForNetworkUpdate : Failed to send network update emails for all users. \n\n", ex.Message));
            }

            HelperMethods.AddLogs("Exit from SendEmailToOFunnelUsersForNetworkUpdate.");
        }

        /// <summary>
        /// Thread pool callback to send email for network updates.
        /// </summary>
        /// <param name="threadContext"></param>
        public void ThreadPoolCallbackToSendNetworkUpdateEmail(Object threadContext)
        {
            string userId = string.Empty;

            try
            {
                OFunnelUser oFunnelUser = threadContext as OFunnelUser;
                userId = Convert.ToString(oFunnelUser.userId);
                string toEmail = oFunnelUser.email;
                string userName = oFunnelUser.firstName + " " + oFunnelUser.lastName;

                HelperMethods.AddLogs(string.Format("Enter in ThreadPoolCallbackToSendNetworkUpdateEmail for userId = {0}.", userId));

                OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                DataSet dataSet = databaseHandler.GetRecipientsEmail("'" + userId + "'");

                
                List<string> toEmailsList = new List<string>();
                toEmailsList.Add(toEmail);

                List<string> recipientEmailsList = new List<string>();
                if (HelperMethods.IsValidDataSet(dataSet))
                {
                    if (dataSet.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                        {
                            string email = string.Empty;
                            email = Convert.ToString(dataSet.Tables[0].Rows[i]["email"]);
                            recipientEmailsList.Add(email);
                        }
                    }
                    else
                    {
                        HelperMethods.AddLogs("ThreadPoolCallbackToSendNetworkUpdateEmail: No recipient email for netwrok alerts are available for userId = " + userId);
                    }
                }
                else
                {
                    HelperMethods.AddLogs("ThreadPoolCallbackToSendNetworkUpdateEmail: Failed to get recipient email for netwrok alerts from database for userId = " + userId);
                }

                string[] toEmails = toEmailsList.ToArray();
                string[] recipientEmails = recipientEmailsList.ToArray();

                NetworkUpdates networkUpdates = this.GetNetworkUpdateDetailForUserId(userId);

                //Kushal Hack
                if (Convert.ToInt32(userId) == 2341)
                {
                    SendAlertsInExcel(networkUpdates, toEmail);
                }

                // Send email for all public and private request to user.
                EmailService emailService = new EmailService();

                bool isNetworkUpdateFound = false;
                
                if (networkUpdates != null && networkUpdates.networkAlertsForAlertType != null && networkUpdates.networkAlertsForAlertType.Length > 0)
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendNetworkUpdateEmail: Daily netwrok update & twitter lead email for network updates (Network update count {0}) sending to {1} at EmailId: {2}.", networkUpdates.networkAlertsForAlertType.Length, userName, toEmail));

                    isNetworkUpdateFound = emailService.CreateNetworkUpdateSectionForEmailTemplate(networkUpdates.networkAlertsForAlertType);
                }
                else
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendNetworkUpdateEmail: No network update found for userId = {0}, userName = {1}.", userId, userName));
                }

                if (networkUpdates != null && networkUpdates.networkAlertsForOtherUpdateType != null && networkUpdates.networkAlertsForOtherUpdateType.Length > 0)
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendNetworkUpdateEmail: Daily netwrok update & twitter lead email for network updates (Network update count {0}) sending to {1} at EmailId: {2}.", networkUpdates.networkAlertsForAlertType.Length, userName, toEmail));

                    isNetworkUpdateFound = emailService.CreateAllOtherTypeNetworkUpdateSectionForEmailTemplate(networkUpdates.networkAlertsForOtherUpdateType);
                }
                else
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendNetworkUpdateEmail: No network update found for userId = {0}, userName = {1}.", userId, userName));
                }

                bool isTwitterLeadFound = false;

                TwitterLeads twitterLeads = this.GetTwitterLeadDetailForUserId(userId);
                if (twitterLeads != null && twitterLeads.twitterLeadsForAlertType != null && twitterLeads.twitterLeadsForAlertType.Length > 0)
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendNetworkUpdateEmail: Daily netwrok update & twitter lead  email for twitter lead (Twiter leads count {0}) sending to {1} at EmailId: {2}.", twitterLeads.twitterLeadsForAlertType.Length, userName, toEmail));

                    isTwitterLeadFound = emailService.CreateTwitterLeadSectionForEmailTemplate(twitterLeads.twitterLeadsForAlertType);
                }
                else
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendNetworkUpdateEmail: No twitter lead found for userId = {0}, userName = {1}.", userId, userName));
                }

                if (isNetworkUpdateFound || isTwitterLeadFound)
                {
                    NameValueCollection nameValues = new NameValueCollection();
                    nameValues["userId"] = userId;

                    emailService.SetUsersDetailsToSendEmail(nameValues);

                    string netwrokUpdateAlertEmailSubject = string.Empty;

                    bool isMailSend = false;

                    netwrokUpdateAlertEmailSubject = Constants.NetwrokUpdateAlertEmailSubject;
                    isMailSend = emailService.SendMailToAllUsers(toEmails, recipientEmails, null, netwrokUpdateAlertEmailSubject, EmailType.NetwrokUpdateAlertEmail);
                    
                    if (isMailSend)
                    {
                        HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendNetworkUpdateEmail: Daily network update email for network update and twitter leads sends sucessfully to {0} at EmailId: {1}.\n\n", userName, toEmail));

                        try
                        {
                            DataSet lastTimeDataSet = databaseHandler.SetLastEmailSentTime("'" + userId + "'" + "," + "'" + Constants.AlertEmailType + "'");
                            if (HelperMethods.IsValidDataSet(lastTimeDataSet) && lastTimeDataSet.Tables[0].Rows.Count > 0)
                            {
                                bool isLastEmailSentTimeSet = Convert.ToBoolean(lastTimeDataSet.Tables[0].Rows[0]["isLastEmailSentTimeSet"]);
                                if (!isLastEmailSentTimeSet)
                                {
                                    HelperMethods.AddLogs("ThreadPoolCallbackToSendNetworkUpdateEmail: Failed to set last email sent time for netwrok alert email in database for userId = " + userId);
                                }
                            }
                            else
                            {
                                HelperMethods.AddLogs("ThreadPoolCallbackToSendNetworkUpdateEmail: (DataSet Invalid): Failed to set last email sent time for netwrok alert email in database for userId = " + userId);
                            }
                        }
                        catch (Exception ex)
                        {
                            HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendNetworkUpdateEmail: Failed to set last email sent time for netwrok alert email in database for userId = {0}. Exception = {1} \n\n", userId, ex.Message));
                        }
                    }
                    else
                    {
                        HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendNetworkUpdateEmail: Daily network update email for network update and twitter leads failed to send to {0} at EmailId: {1}.\n\n", userName, toEmail));
                    }
                }
                else
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendNetworkUpdateEmail: Network Update Section is empty so there are no network updates to send email for userId = {0}.", userId));
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendNetworkUpdateEmail: Failed to send network update email for userId = {0}. Exception = {1} \n\n", userId, ex.Message));
            }

            if (Interlocked.Decrement(ref numberOfThreadsForNetworkUpdateEmailNotYetCompleted) == 0)
            {
                this.doneEventForNetworkUpdateEmail.Set();
            }

            HelperMethods.AddLogs(string.Format("Exit from ThreadPoolCallbackToSendNetworkUpdateEmail for userId = {0}.", userId));
        }

        private static void SendAlertsInExcel(NetworkUpdates networkUpdates, string toEmail)
        {

            StringBuilder csvAlertString = new StringBuilder();
            string savedFileName = "";
            if (networkUpdates != null && networkUpdates.networkAlertsForAlertType != null && networkUpdates.networkAlertsForAlertType.Length > 0)
            {

                var cal = System.Globalization.DateTimeFormatInfo.CurrentInfo.Calendar;
                savedFileName = "C:\\users\\adroit\\documents\\Connections" + cal.GetWeekOfYear(DateTime.Now, System.Globalization.CalendarWeekRule.FirstDay, System.DayOfWeek.Saturday).ToString() + DateTime.Now.Date.Year + ".txt";

                if (DateTime.Now.Date.DayOfWeek == DayOfWeek.Saturday)
                {
                    
                    //setting up the headers for the new file
                    csvAlertString.Append("yourConnectName" + "\t" + "yourConnectionTitle" + "\t" + "yourConnectionProfileUrl" + "\t" + "connectedToName" + "\t" + "connectedToTitle" + "\t" + "connectedToProfileUrl" + "\t");
                    csvAlertString.Append(Environment.NewLine);
                }
                
                foreach (NetworkAlertsForAlertType networkAlertsForAlertType in networkUpdates.networkAlertsForAlertType)
                {
                
                    string alertType = networkAlertsForAlertType.alertType;
                    if (networkAlertsForAlertType.networkAlerts != null && networkAlertsForAlertType.networkAlerts.Length > 0)
                    {
                        foreach (NetworkAlerts networkAlert in networkAlertsForAlertType.networkAlerts)
                        {
                            if (networkAlert.networkAlertDetails != null && networkAlert.networkAlertDetails.Length > 0)
                            {
                                string netwrokUpdateDetails = string.Empty;
                                string positionUpdateDetails = string.Empty;

                                foreach (NetworkAlertDetails networkAlertDetail in networkAlert.networkAlertDetails)
                                {
                                    if (!string.IsNullOrEmpty(networkAlertDetail.yourConnectionLinkedInId))
                                    {
                                        string yourConnectionJobTitle = networkAlertDetail.yourConnectionHeadline;
                                        if (!string.IsNullOrEmpty(yourConnectionJobTitle) && !string.IsNullOrEmpty(networkAlertDetail.yourConnectionCompany))
                                        {
                                            yourConnectionJobTitle += ", " + networkAlertDetail.yourConnectionCompany;
                                        }
                                        else
                                        {
                                            yourConnectionJobTitle += networkAlertDetail.yourConnectionCompany;
                                        }

                                        //Kushal - For csv file not needed
                                        //yourConnectionJobTitle = HelperMethods.FormatYourConnectionJobTitle(yourConnectionJobTitle);

                                        string yourConnectionProfilePicUrl = networkAlertDetail.yourConnectionProfilePicUrl;
                                        string yourConnectionProfileUrl = networkAlertDetail.yourConnectionProfileUrl;

                                        if (string.IsNullOrEmpty(yourConnectionProfileUrl))
                                        {
                                            yourConnectionProfileUrl = Constants.ConnectionLinkedInProfileUrl;

                                            yourConnectionProfileUrl = yourConnectionProfileUrl.Replace(":FIRST_NAME", networkAlertDetail.yourConnectionFirstName);
                                            yourConnectionProfileUrl = yourConnectionProfileUrl.Replace(":LAST_NAME", networkAlertDetail.yourConnectionLastName);
                                        }

                                        string connectedToProfileUrl = networkAlertDetail.connectedToProfileUrl;
                                        if (string.IsNullOrEmpty(connectedToProfileUrl))
                                        {
                                            connectedToProfileUrl = Constants.ConnectionLinkedInProfileUrl;

                                            connectedToProfileUrl = connectedToProfileUrl.Replace(":FIRST_NAME", networkAlertDetail.connectedToFirstName);
                                            connectedToProfileUrl = connectedToProfileUrl.Replace(":LAST_NAME", networkAlertDetail.connectedToLastName);
                                        }

                                        if (!alertType.ToUpper().Equals("POSITION") && !alertType.ToUpper().Equals("POSITIONROLE"))
                                        {
                                            csvAlertString.Append(networkAlertDetail.yourConnectionFirstName + " " + networkAlertDetail.yourConnectionLastName);
                                            csvAlertString.Append("\t");
                                            csvAlertString.Append(yourConnectionJobTitle);
                                            csvAlertString.Append("\t");
                                            
                                            
                                            if (!string.IsNullOrEmpty(yourConnectionProfileUrl))
                                            {
                                                csvAlertString.Append(yourConnectionProfileUrl);
                                            }
                                            else
                                            {
                                                csvAlertString.Append("#");
                                            }
                                            csvAlertString.Append("\t");
                                            csvAlertString.Append(networkAlertDetail.connectedToFirstName + " " + networkAlertDetail.connectedToLastName);
                                            csvAlertString.Append("\t");
                                            string connectedToJobTitle = networkAlertDetail.connectedToHeadline;

                                            if (!string.IsNullOrEmpty(connectedToJobTitle) && !string.IsNullOrEmpty(networkAlertDetail.connectedToCompany))
                                            {
                                                connectedToJobTitle += ", " + networkAlertDetail.connectedToCompany;
                                            }
                                            else
                                            {
                                                connectedToJobTitle += networkAlertDetail.connectedToCompany;
                                            }
                                            //connectedToJobTitle = HelperMethods.FormatYourConnectionJobTitle(connectedToJobTitle);        
                                            csvAlertString.Append(connectedToJobTitle);
                                            csvAlertString.Append("\t");
                                            string connectedToProfilePicUrl = networkAlertDetail.connectedToProfilePicUrl;

                                            if (!string.IsNullOrEmpty(connectedToProfileUrl))
                                            {
                                                csvAlertString.Append(connectedToProfileUrl);
                                            }
                                            else
                                            {
                                                csvAlertString.Append("#");
                                            }
                                            csvAlertString.Append("\t");

                                        }

                                        if (alertType.ToUpper().Equals("POSITION") || alertType.ToUpper().Equals("POSITIONROLE"))
                                        {

                                            csvAlertString.Append(networkAlertDetail.yourConnectionFirstName + " " + networkAlertDetail.yourConnectionLastName);
                                            csvAlertString.Append("\t");
                                            csvAlertString.Append(yourConnectionJobTitle);
                                            csvAlertString.Append("\t");
                                            
                                            if (!string.IsNullOrEmpty(yourConnectionProfileUrl))
                                            {
                                                csvAlertString.Append(yourConnectionProfileUrl);
                                            }
                                            else
                                            {
                                                csvAlertString.Append("#");
                                            }
                                            csvAlertString.Append("\t");
                                        }
                                    }

                                    csvAlertString.Append(Environment.NewLine);
                                }
                            }

                        }
                    }

                }
                
            }
            
            
            
            File.AppendAllText(savedFileName, csvAlertString.ToString());

            if (DateTime.Now.Date.DayOfWeek == DayOfWeek.Friday)
            {
                //Send it via email
                string emailUserName = Config.EmailUserName;
                string emailPassword = Config.EmailPassword;
                string emailFromName = Config.EmailFromName;
                string emailHost = Config.EmailHost;
                int emailPort = Config.EmailPort;

                MailMessage message = new MailMessage();
                SmtpClient smtpClient = new SmtpClient();
                MailAddress fromAddress = new MailAddress(emailUserName, emailFromName);
                message.From = fromAddress;

                message.To.Add(toEmail.Trim());
                message.Subject = "Connection Updates";

                message.IsBodyHtml = true;
                message.CC.Add("kushal@ofunnel.com");
                message.Attachments.Add(new Attachment(savedFileName));
                smtpClient.Host = emailHost;
                smtpClient.Port = emailPort;
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = new System.Net.NetworkCredential(emailUserName, emailPassword);
                smtpClient.Send(message);
            }
        }

        private static void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                //MessageBox.Show("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        /// <summary>
        /// This methods gets all ofunnnel users who has any network update in last 24 hours.
        /// </summary>
        /// <returns>OFunnelUsers</returns>
        private OFunnelUsers GetAllOFunnelUsersForNetworkUpdates()
        {
            OFunnelUsers oFunnelUsers = null;

            try
            {
                OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                DataSet dataSet = databaseHandler.GetOFunnelUsersForNetworkUpdate();

                if (HelperMethods.IsValidDataSet(dataSet))
                {
                    oFunnelUsers = new OFunnelUsers();

                    OFunnelUser oFunnelUser = null;

                    List<OFunnelUser> oFunnelUserList = new List<OFunnelUser>();

                    for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                    {
                        oFunnelUser = new OFunnelUser();
                        oFunnelUser.userIndex = i;
                        oFunnelUser.userId = string.IsNullOrEmpty(Convert.ToString(dataSet.Tables[0].Rows[i]["userId"])) ? -1 : Convert.ToInt32(dataSet.Tables[0].Rows[i]["userId"]); ;
                        oFunnelUser.firstName = Convert.ToString(dataSet.Tables[0].Rows[i]["firstName"]);
                        oFunnelUser.lastName = Convert.ToString(dataSet.Tables[0].Rows[i]["lastName"]);
                        oFunnelUser.email = Convert.ToString(dataSet.Tables[0].Rows[i]["email"]);
                        oFunnelUser.accountType = Convert.ToString(dataSet.Tables[0].Rows[i]["accountType"]);

                        oFunnelUserList.Add(oFunnelUser);
                    }

                    oFunnelUsers.oFunnelUser = oFunnelUserList.ToArray();
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs("GetAllOFunnelUsersForTargetAccounts: Failed to get all ofunnel users for target accounts from database." + ex.Message);
            }

            return oFunnelUsers;
        }

        /// <summary>
        /// This method gets network updates for user.
        /// </summary>
        /// <param name="userId">userId</param>
        private NetworkUpdates GetNetworkUpdateDetailForUserId(string userId)
        {
            NetworkUpdates networkUpdates = new NetworkUpdates();

            try
            {
                NameValueCollection nameValue = new NameValueCollection();
                nameValue["userId"] = userId;

                string message = HelperMethods.GetParametersListForLogMessage(nameValue);

                HelperMethods.AddLogs("Enter In GetNetworkUpdateDetailForUserId. Parameters List => " + message);

                if (!string.IsNullOrEmpty(userId))
                {
                    OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                    DataSet dataSet = databaseHandler.GetNetworkUpdateDetailForUserId("'" + userId + "'");

                    if (dataSet != null && dataSet.Tables.Count > 0)
                    {
                        NetworkAlertDetails networkAlertDetails = null;
                        List<NetworkAlertDetails> networkAlertDetailsList = new List<NetworkAlertDetails>();

                        if (dataSet.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                            {
                                networkAlertDetails = new NetworkAlertDetails();

                                networkAlertDetails.networkUpdateId = Convert.ToString(dataSet.Tables[0].Rows[i]["id"]);

                                networkAlertDetails.yourConnectionLinkedInId = Convert.ToString(dataSet.Tables[0].Rows[i]["yourConnectionLinkedInId"]);
                                networkAlertDetails.yourConnectionFirstName = Convert.ToString(dataSet.Tables[0].Rows[i]["yourConnectionFirstName"]);
                                networkAlertDetails.yourConnectionLastName = Convert.ToString(dataSet.Tables[0].Rows[i]["yourConnectionLastName"]);
                                networkAlertDetails.yourConnectionProfileUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["yourConnectionProfileUrl"]);
                                networkAlertDetails.yourConnectionProfilePicUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["yourConnectionProfilePicUrl"]);
                                networkAlertDetails.yourConnectionHeadline = Convert.ToString(dataSet.Tables[0].Rows[i]["yourConnectionHeadline"]);
                                networkAlertDetails.yourConnectionCompany = Convert.ToString(dataSet.Tables[0].Rows[i]["yourConnectionCompany"]);

                                networkAlertDetails.connectedToLinkedInId = Convert.ToString(dataSet.Tables[0].Rows[i]["connectedToLinkedInId"]);
                                networkAlertDetails.connectedToFirstName = Convert.ToString(dataSet.Tables[0].Rows[i]["connectedToFirstName"]);
                                networkAlertDetails.connectedToLastName = Convert.ToString(dataSet.Tables[0].Rows[i]["connectedToLastName"]);
                                networkAlertDetails.connectedToProfileUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["connectedToProfileUrl"]);
                                networkAlertDetails.connectedToProfilePicUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["connectedToProfilePicUrl"]);
                                networkAlertDetails.connectedToHeadline = Convert.ToString(dataSet.Tables[0].Rows[i]["connectedToHeadline"]);
                                networkAlertDetails.connectedToCompany = Convert.ToString(dataSet.Tables[0].Rows[i]["connectedToCompany"]);

                                networkAlertDetails.filterType = Convert.ToString(dataSet.Tables[0].Rows[i]["filterType"]);
                                networkAlertDetails.targetName = Convert.ToString(dataSet.Tables[0].Rows[i]["targetName"]);

                                string filterType = Convert.ToString(dataSet.Tables[0].Rows[i]["filterType"]);

                                if (string.IsNullOrEmpty(networkAlertDetails.connectedToLinkedInId) && filterType.ToUpper().Equals("COMPANY"))
                                {
                                    networkAlertDetails.alertType = "POSITION";
                                }
                                else if (string.IsNullOrEmpty(networkAlertDetails.connectedToLinkedInId) && filterType.ToUpper().Equals("ROLE"))
                                {
                                    networkAlertDetails.alertType = "POSITIONROLE";
                                }
                                else if (string.IsNullOrEmpty(networkAlertDetails.connectedToLinkedInId) && filterType.ToUpper().Equals("PERSON"))
                                {
                                    networkAlertDetails.alertType = "POSITIONPERSON";
                                }
                                else
                                {
                                    networkAlertDetails.alertType = filterType;
                                }

                                networkAlertDetailsList.Add(networkAlertDetails);
                            }

                            if (networkAlertDetailsList != null && networkAlertDetailsList.Count > 0)
                            {
                                List<NetworkAlertsForAlertType> networkAlertsForAlertTypeList = new List<NetworkAlertsForAlertType>();

                                var groupByAlertTypeNetworkAlerts = networkAlertDetailsList.GroupBy(g => g.alertType);
                                foreach (var networkAlertAlertTypeGroup in groupByAlertTypeNetworkAlerts)
                                {
                                    if (networkAlertAlertTypeGroup.Count() > 0)
                                    {
                                        NetworkAlertsForAlertType networkAlertsForAlertType = new NetworkAlertsForAlertType();

                                        List<NetworkAlerts> networkAlertsList = new List<NetworkAlerts>();

                                        var groupByTargetNameNetworkAlerts = networkAlertAlertTypeGroup.GroupBy(g => g.targetName);

                                        foreach (var networkAlertTargetNameGroup in groupByTargetNameNetworkAlerts)
                                        {
                                            NetworkAlerts networkAlerts = new NetworkAlerts();

                                            if (networkAlertTargetNameGroup.Count() > 0)
                                            {
                                                networkAlerts.networkAlertDetails = networkAlertTargetNameGroup.ToArray();

                                                networkAlertsForAlertType.alertType = networkAlerts.networkAlertDetails.ElementAt(0).alertType;
                                                networkAlerts.targetName = networkAlerts.networkAlertDetails.ElementAt(0).targetName;
                                            }

                                            networkAlertsList.Add(networkAlerts);
                                        }

                                        networkAlertsForAlertType.networkAlerts = networkAlertsList.ToArray();

                                        networkAlertsForAlertTypeList.Add(networkAlertsForAlertType);
                                    }
                                }

                                networkUpdates.networkAlertsForAlertType = networkAlertsForAlertTypeList.ToArray();
                            }
                        }

                        List<NetworkAlertDetails> networkAlertDetailsForOtherNetworkUpdateList = new List<NetworkAlertDetails>();

                        // Creating data for All other type of Network updates like CMPY, JGRP, SHAR, PICU, PROF, PREC, VIRL
                        if (dataSet.Tables[1].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataSet.Tables[1].Rows.Count; i++)
                            {
                                networkAlertDetails = new NetworkAlertDetails();

                                networkAlertDetails.networkUpdateId = Convert.ToString(dataSet.Tables[1].Rows[i]["id"]);

                                networkAlertDetails.yourConnectionLinkedInId = Convert.ToString(dataSet.Tables[1].Rows[i]["yourConnectionLinkedInId"]);
                                networkAlertDetails.yourConnectionFirstName = Convert.ToString(dataSet.Tables[1].Rows[i]["yourConnectionFirstName"]);
                                networkAlertDetails.yourConnectionLastName = Convert.ToString(dataSet.Tables[1].Rows[i]["yourConnectionLastName"]);
                                networkAlertDetails.yourConnectionProfileUrl = Convert.ToString(dataSet.Tables[1].Rows[i]["yourConnectionProfileUrl"]);
                                networkAlertDetails.yourConnectionProfilePicUrl = Convert.ToString(dataSet.Tables[1].Rows[i]["yourConnectionProfilePicUrl"]);
                                networkAlertDetails.yourConnectionHeadline = Convert.ToString(dataSet.Tables[1].Rows[i]["yourConnectionHeadline"]);
                                networkAlertDetails.yourConnectionCompany = Convert.ToString(dataSet.Tables[1].Rows[i]["yourConnectionCompany"]);

                                networkAlertDetails.connectedToLinkedInId = Convert.ToString(dataSet.Tables[1].Rows[i]["connectedToLinkedInId"]);
                                networkAlertDetails.connectedToFirstName = Convert.ToString(dataSet.Tables[1].Rows[i]["connectedToFirstName"]);
                                networkAlertDetails.connectedToLastName = Convert.ToString(dataSet.Tables[1].Rows[i]["connectedToLastName"]);
                                networkAlertDetails.connectedToProfileUrl = Convert.ToString(dataSet.Tables[1].Rows[i]["connectedToProfileUrl"]);
                                networkAlertDetails.connectedToProfilePicUrl = Convert.ToString(dataSet.Tables[1].Rows[i]["connectedToProfilePicUrl"]);
                                networkAlertDetails.connectedToHeadline = Convert.ToString(dataSet.Tables[1].Rows[i]["connectedToHeadline"]);
                                networkAlertDetails.connectedToCompany = Convert.ToString(dataSet.Tables[1].Rows[i]["connectedToCompany"]);

                                networkAlertDetails.updateType = Convert.ToString(dataSet.Tables[1].Rows[i]["updateType"]);
                                networkAlertDetails.groupName = Convert.ToString(dataSet.Tables[1].Rows[i]["groupName"]);
                                networkAlertDetails.shortenedUrl = Convert.ToString(dataSet.Tables[1].Rows[i]["shortenedUrl"]);
                                networkAlertDetails.jobTitle = Convert.ToString(dataSet.Tables[1].Rows[i]["jobTitle"]);
                                networkAlertDetails.companyName = Convert.ToString(dataSet.Tables[1].Rows[i]["companyName"]);
                                networkAlertDetails.comment = Convert.ToString(dataSet.Tables[1].Rows[i]["comment"]);

                                networkAlertDetails.filterType = Convert.ToString(dataSet.Tables[1].Rows[i]["filterType"]);
                                networkAlertDetails.targetName = Convert.ToString(dataSet.Tables[1].Rows[i]["targetName"]);

                                string filterType = Convert.ToString(dataSet.Tables[1].Rows[i]["filterType"]);
                                networkAlertDetails.alertType = filterType;

                                networkAlertDetailsForOtherNetworkUpdateList.Add(networkAlertDetails);
                            }

                            if (networkAlertDetailsForOtherNetworkUpdateList != null && networkAlertDetailsForOtherNetworkUpdateList.Count > 0)
                            {
                                List<NetworkAlertsForOtherUpdateType> networkAlertsForOtherUpdateTypeList = new List<NetworkAlertsForOtherUpdateType>();

                                var groupByAlertTypeNetworkAlerts = networkAlertDetailsForOtherNetworkUpdateList.GroupBy(g => g.alertType);
                                foreach (var networkAlertAlertTypeGroup in groupByAlertTypeNetworkAlerts)
                                {
                                    if (networkAlertAlertTypeGroup.Count() > 0)
                                    {
                                        NetworkAlertsForOtherUpdateType networkAlertsForOtherUpdateType = new NetworkAlertsForOtherUpdateType();

                                        List<NetworkAlerts> networkAlertsList = new List<NetworkAlerts>();

                                        var groupByTargetNameNetworkAlerts = networkAlertAlertTypeGroup.GroupBy(g => g.targetName);

                                        foreach (var networkAlertTargetNameGroup in groupByTargetNameNetworkAlerts)
                                        {
                                            NetworkAlerts networkAlerts = new NetworkAlerts();

                                            if (networkAlertTargetNameGroup.Count() > 0)
                                            {
                                                networkAlerts.networkAlertDetails = networkAlertTargetNameGroup.ToArray();

                                                networkAlertsForOtherUpdateType.alertType = networkAlerts.networkAlertDetails.ElementAt(0).alertType;
                                                networkAlerts.targetName = networkAlerts.networkAlertDetails.ElementAt(0).targetName;
                                            }

                                            networkAlertsList.Add(networkAlerts);
                                        }

                                        networkAlertsForOtherUpdateType.networkAlerts = networkAlertsList.ToArray();

                                        networkAlertsForOtherUpdateTypeList.Add(networkAlertsForOtherUpdateType);
                                    }
                                }

                                networkUpdates.networkAlertsForOtherUpdateType = networkAlertsForOtherUpdateTypeList.ToArray();
                            }
                        }
                        else
                        {
                            HelperMethods.AddLogs("GetNetworkUpdateDetailForUserId: Network updates for CMPY, JGRP, SHAR, PICU, PROF, PREC and VIRL is not found.");
                        }
                    }
                    else
                    {
                        HelperMethods.AddLogs("GetNetworkUpdateDetailForUserId: (InvalidDataSet) Data received form database is invalid.");
                    }
                }
                else
                {
                    HelperMethods.AddLogs("GetNetworkUpdateDetailForUserId: Required parameter {userId} is missing.");
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("GetNetworkUpdateDetailForUserId: Failed to get network update details from Database for userId = {0}. Exception Occured = {1}", userId, ex.Message));
            }

            HelperMethods.AddLogs("Exit from GetNetworkUpdateDetailForUserId.");

            return networkUpdates;
        }

        /// <summary>
        /// This method gets twitter leads for user.
        /// </summary>
        /// <param name="userId">userId</param>
        private TwitterLeads GetTwitterLeadDetailForUserId(string userId)
        {
            TwitterLeads twitterLeads = new TwitterLeads();

            try
            {
                NameValueCollection nameValue = new NameValueCollection();
                nameValue["userId"] = userId;

                string message = HelperMethods.GetParametersListForLogMessage(nameValue);

                HelperMethods.AddLogs("Enter In GetTwitterLeadDetailForUserId. Parameters List => " + message);

                if (!string.IsNullOrEmpty(userId))
                {
                    OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                    DataSet dataSet = databaseHandler.GetTwitterLeadsDetailForUserId("'" + userId + "'");

                    if (HelperMethods.IsValidDataSet(dataSet))
                    {
                        TwitterLeadAlertDetails twitterLeadAlertDetails = null;
                        List<TwitterLeadAlertDetails> twitterLeadAlertDetailsList = new List<TwitterLeadAlertDetails>();

                        for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                        {
                            twitterLeadAlertDetails = new TwitterLeadAlertDetails();
                            twitterLeadAlertDetails.leadId = Convert.ToString(dataSet.Tables[0].Rows[i]["leadId"]);
                            twitterLeadAlertDetails.twitterHandle = Convert.ToString(dataSet.Tables[0].Rows[i]["twitterHandle"]);
                            twitterLeadAlertDetails.firstName = Convert.ToString(dataSet.Tables[0].Rows[i]["firstName"]);
                            twitterLeadAlertDetails.lastName = Convert.ToString(dataSet.Tables[0].Rows[i]["lastName"]);
                            twitterLeadAlertDetails.profileUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["profileUrl"]);
                            twitterLeadAlertDetails.profilePicUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["profilePicUrl"]);
                            twitterLeadAlertDetails.twitterBio = Convert.ToString(dataSet.Tables[0].Rows[i]["twitterBio"]);

                            twitterLeadAlertDetails.filterType = Convert.ToString(dataSet.Tables[0].Rows[i]["filterType"]);
                            twitterLeadAlertDetails.targetName = Convert.ToString(dataSet.Tables[0].Rows[i]["targetName"]);

                            string filterType = Convert.ToString(dataSet.Tables[0].Rows[i]["filterType"]);

                            twitterLeadAlertDetails.alertType = filterType;

                            twitterLeadAlertDetailsList.Add(twitterLeadAlertDetails);
                        }

                        if (twitterLeadAlertDetailsList != null && twitterLeadAlertDetailsList.Count > 0)
                        {
                            List<TwitterLeadsForAlertType> twitterLeadsForAlertTypeList = new List<TwitterLeadsForAlertType>();

                            var groupByAlertTypeNetworkAlerts = twitterLeadAlertDetailsList.GroupBy(g => g.alertType);
                            foreach (var networkAlertAlertTypeGroup in groupByAlertTypeNetworkAlerts)
                            {
                                if (networkAlertAlertTypeGroup.Count() > 0)
                                {
                                    TwitterLeadsForAlertType twitterLeadsForAlertType = new TwitterLeadsForAlertType();

                                    List<TwitterLeadAlerts> twitterLeadAlertsList = new List<TwitterLeadAlerts>();

                                    var groupByTargetNameNetworkAlerts = networkAlertAlertTypeGroup.GroupBy(g => g.targetName);

                                    foreach (var networkAlertTargetNameGroup in groupByTargetNameNetworkAlerts)
                                    {
                                        TwitterLeadAlerts twitterLeadAlerts = new TwitterLeadAlerts();

                                        if (networkAlertTargetNameGroup.Count() > 0)
                                        {
                                            twitterLeadAlerts.twitterLeadAlertDetails = networkAlertTargetNameGroup.ToArray();

                                            twitterLeadsForAlertType.alertType = twitterLeadAlerts.twitterLeadAlertDetails.ElementAt(0).alertType;
                                            twitterLeadAlerts.targetName = twitterLeadAlerts.twitterLeadAlertDetails.ElementAt(0).targetName;
                                        }

                                        twitterLeadAlertsList.Add(twitterLeadAlerts);
                                    }

                                    twitterLeadsForAlertType.twitterLeadAlerts = twitterLeadAlertsList.ToArray();

                                    twitterLeadsForAlertTypeList.Add(twitterLeadsForAlertType);
                                }
                            }

                            twitterLeads.twitterLeadsForAlertType = twitterLeadsForAlertTypeList.ToArray();
                        }
                    }
                    else
                    {
                        HelperMethods.AddLogs("GetTwitterLeadDetailForUserId: (InvalidDataSet) Data received form database is invalid.");
                    }
                }
                else
                {
                    HelperMethods.AddLogs("GetTwitterLeadDetailForUserId: Required parameter {userId} is missing.");
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("GetTwitterLeadDetailForUserId: Failed to get twitter lead details from Database for userId = {0}. Exception Occured = {1}", userId, ex.Message));
            }

            HelperMethods.AddLogs("Exit from GetTwitterLeadDetailForUserId.");

            return twitterLeads;
        }

        /// <summary>
        /// This method gets followup twitter leads for user.
        /// </summary>
        /// <param name="userId">userId</param>
        private TwitterLeads GetFollowupTwitterLeadDetailForUserId(string userId)
        {
            TwitterLeads twitterLeads = new TwitterLeads();

            try
            {
                NameValueCollection nameValue = new NameValueCollection();
                nameValue["userId"] = userId;

                string message = HelperMethods.GetParametersListForLogMessage(nameValue);

                HelperMethods.AddLogs("Enter In GetFollowupTwitterLeadDetailForUserId. Parameters List => " + message);

                if (!string.IsNullOrEmpty(userId))
                {
                    OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                    DataSet dataSet = databaseHandler.GetFollowupTwitterLeadsDetailForUserId("'" + userId + "'");

                    if (HelperMethods.IsValidDataSet(dataSet))
                    {
                        TwitterLeadAlertDetails twitterLeadAlertDetails = null;
                        List<TwitterLeadAlertDetails> twitterLeadAlertDetailsList = new List<TwitterLeadAlertDetails>();

                        for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                        {
                            twitterLeadAlertDetails = new TwitterLeadAlertDetails();
                            twitterLeadAlertDetails.leadId = Convert.ToString(dataSet.Tables[0].Rows[i]["leadId"]);
                            twitterLeadAlertDetails.twitterHandle = Convert.ToString(dataSet.Tables[0].Rows[i]["twitterHandle"]);
                            twitterLeadAlertDetails.firstName = Convert.ToString(dataSet.Tables[0].Rows[i]["firstName"]);
                            twitterLeadAlertDetails.lastName = Convert.ToString(dataSet.Tables[0].Rows[i]["lastName"]);
                            twitterLeadAlertDetails.profileUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["profileUrl"]);
                            twitterLeadAlertDetails.profilePicUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["profilePicUrl"]);
                            twitterLeadAlertDetails.twitterBio = Convert.ToString(dataSet.Tables[0].Rows[i]["twitterBio"]);

                            twitterLeadAlertDetails.filterType = Convert.ToString(dataSet.Tables[0].Rows[i]["filterType"]);
                            twitterLeadAlertDetails.targetName = Convert.ToString(dataSet.Tables[0].Rows[i]["targetName"]);

                            string filterType = Convert.ToString(dataSet.Tables[0].Rows[i]["filterType"]);

                            twitterLeadAlertDetails.alertType = filterType;

                            twitterLeadAlertDetailsList.Add(twitterLeadAlertDetails);
                        }

                        if (twitterLeadAlertDetailsList != null && twitterLeadAlertDetailsList.Count > 0)
                        {
                            List<TwitterLeadsForAlertType> twitterLeadsForAlertTypeList = new List<TwitterLeadsForAlertType>();

                            var groupByAlertTypeNetworkAlerts = twitterLeadAlertDetailsList.GroupBy(g => g.alertType);
                            foreach (var networkAlertAlertTypeGroup in groupByAlertTypeNetworkAlerts)
                            {
                                if (networkAlertAlertTypeGroup.Count() > 0)
                                {
                                    TwitterLeadsForAlertType twitterLeadsForAlertType = new TwitterLeadsForAlertType();

                                    List<TwitterLeadAlerts> twitterLeadAlertsList = new List<TwitterLeadAlerts>();

                                    var groupByTargetNameNetworkAlerts = networkAlertAlertTypeGroup.GroupBy(g => g.targetName);

                                    foreach (var networkAlertTargetNameGroup in groupByTargetNameNetworkAlerts)
                                    {
                                        TwitterLeadAlerts twitterLeadAlerts = new TwitterLeadAlerts();

                                        if (networkAlertTargetNameGroup.Count() > 0)
                                        {
                                            twitterLeadAlerts.twitterLeadAlertDetails = networkAlertTargetNameGroup.ToArray();

                                            twitterLeadsForAlertType.alertType = twitterLeadAlerts.twitterLeadAlertDetails.ElementAt(0).alertType;
                                            twitterLeadAlerts.targetName = twitterLeadAlerts.twitterLeadAlertDetails.ElementAt(0).targetName;
                                        }

                                        twitterLeadAlertsList.Add(twitterLeadAlerts);
                                    }

                                    twitterLeadsForAlertType.twitterLeadAlerts = twitterLeadAlertsList.ToArray();

                                    twitterLeadsForAlertTypeList.Add(twitterLeadsForAlertType);
                                }
                            }

                            twitterLeads.twitterLeadsForAlertType = twitterLeadsForAlertTypeList.ToArray();
                        }
                    }
                    else
                    {
                        HelperMethods.AddLogs("GetFollowupTwitterLeadDetailForUserId: (InvalidDataSet) Data received form database is invalid.");
                    }
                }
                else
                {
                    HelperMethods.AddLogs("GetFollowupTwitterLeadDetailForUserId: Required parameter {userId} is missing.");
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("GetFollowupTwitterLeadDetailForUserId: Failed to get followup twitter lead details from Database for userId = {0}. Exception Occured = {1}", userId, ex.Message));
            }

            HelperMethods.AddLogs("Exit from GetFollowupTwitterLeadDetailForUserId.");

            return twitterLeads;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Network Update Email send functionality End.
        /////////////////////////////////////////////////////////////////////////////////////////


        ////////////////////////////////////////////////////////////////
        // Access Token Expired email send functionality Start
        ///////////////////////////////////////////////////////////////

        /// <summary>
        /// Access token expired email scheduler time elapsed callback.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">Event arguments</param>
        private void AccessTokenExpiredEmailSchedulerTimeElapsed(object sender)
        {
            this.SendEmailToOFunnelUsersForAccessTokenExpired();
        }

        /// <summary>
        /// This method send email to users for which linkedIn access token has expired.
        /// </summary>
        private void SendEmailToOFunnelUsersForAccessTokenExpired()
        {
            HelperMethods.AddLogs("Enter in SendEmailToOFunnelUsersForAccessTokenExpired.");

            try
            {
                OFunnelUsers oFunnelUsers = this.GetAllOFunnelUsersForAccessTokenExpired();

                if (oFunnelUsers != null && oFunnelUsers.oFunnelUser != null && oFunnelUsers.oFunnelUser.Length > 0)
                {
                    this.numberOfThreadsForAccessTokenExpiredEmailNotYetCompleted = oFunnelUsers.oFunnelUser.Length;

                    foreach (OFunnelUser oFunnelUser in oFunnelUsers.oFunnelUser)
                    {
                        if (oFunnelUser.userId != -1)
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolCallbackToSendAccessTokenExpiredEmail), oFunnelUser);
                        }
                        else
                        {
                            numberOfThreadsForAccessTokenExpiredEmailNotYetCompleted--;
                        }
                    }

                    this.doneEventForAccessTokenExpiredEmail.WaitOne();
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("SendEmailToOFunnelUsersForAccessTokenExpired : Failed to send access token expired emails for all users. \n\n", ex.Message));
            }

            HelperMethods.AddLogs("Exit from SendEmailToOFunnelUsersForAccessTokenExpired.");
        }

        /// <summary>
        /// Thread pool callback to send email for Access Token Expired.
        /// </summary>
        /// <param name="threadContext">threadContext</param>
        public void ThreadPoolCallbackToSendAccessTokenExpiredEmail(Object threadContext)
        {
            string userId = string.Empty;

            try
            {
                OFunnelUser oFunnelUser = threadContext as OFunnelUser;
                userId = Convert.ToString(oFunnelUser.userId);
                string toEmail = oFunnelUser.email;
                string userName = oFunnelUser.firstName + " " + oFunnelUser.lastName;

                HelperMethods.AddLogs(string.Format("Enter in ThreadPoolCallbackToSendAccessTokenExpiredEmail for userId = {0}.", userId));

                HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendAccessTokenExpiredEmail: start sending reconnect to linkedIn (access token expired) email for userId = {0}, userName = {1} at EmailId: {2}.", userId, userName, toEmail));

                // Send email for all public and private request to user.
                EmailService emailService = new EmailService();

                string accessTokenExpiredEmailSubject = string.Empty;
                bool isMailSend = false;
                
                if(oFunnelUser.accountType.ToUpper() == "PIPELINEUSER")
                {
                    accessTokenExpiredEmailSubject = Constants.AccessTokenExpiredEmailSubjectForPipelineUser;
                    isMailSend = emailService.SendMailToAllUsers(toEmail, null, accessTokenExpiredEmailSubject, EmailType.AccessTokenExpiredEmailForPipelineUser);
                }
                else
                {
                    accessTokenExpiredEmailSubject = Constants.AccessTokenExpiredEmailSubject;
                    isMailSend = emailService.SendMailToAllUsers(toEmail, null, accessTokenExpiredEmailSubject, EmailType.AccessTokenExpiredEmail);
                }
                
                if (isMailSend)
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendAccessTokenExpiredEmail: reconnect to linkedIn (access token expired) email sent sucessfully to {0} at emailId: {1}.\n\n", userName, toEmail));
                }
                else
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendAccessTokenExpiredEmail: Failed to send reconnect to linkedIn (access token expired) email to {0} at emailId: {1}.\n\n", userName, toEmail));
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendAccessTokenExpiredEmail: Failed to send reconnect to linkedIn (access token expired) email for userId = {0}. Exception = {1} \n\n", userId, ex.Message));
            }

            if (Interlocked.Decrement(ref numberOfThreadsForAccessTokenExpiredEmailNotYetCompleted) == 0)
            {
                this.doneEventForAccessTokenExpiredEmail.Set();
            }

            HelperMethods.AddLogs(string.Format("Exit from ThreadPoolCallbackToSendAccessTokenExpiredEmail for userId = {0}.", userId));
        }

        /// <summary>
        /// This methods gets all ofunnnel users for which access token has expired.
        /// </summary>
        /// <returns>OFunnelUsers</returns>
        private OFunnelUsers GetAllOFunnelUsersForAccessTokenExpired()
        {
            OFunnelUsers oFunnelUsers = null;

            HelperMethods.AddLogs("Enter in GetAllOFunnelUsersForAccessTokenExpired.");
            try
            {
                OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                DataSet dataSet = databaseHandler.GetAllOFunnelUsersForAccessTokenExpired();

                if (HelperMethods.IsValidDataSet(dataSet))
                {
                    oFunnelUsers = new OFunnelUsers();

                    OFunnelUser oFunnelUser = null;

                    List<OFunnelUser> oFunnelUserList = new List<OFunnelUser>();

                    for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                    {
                        oFunnelUser = new OFunnelUser();
                        oFunnelUser.userIndex = i;
                        oFunnelUser.userId = string.IsNullOrEmpty(Convert.ToString(dataSet.Tables[0].Rows[i]["userId"])) ? -1 : Convert.ToInt32(dataSet.Tables[0].Rows[i]["userId"]); ;
                        oFunnelUser.firstName = Convert.ToString(dataSet.Tables[0].Rows[i]["firstName"]);
                        oFunnelUser.lastName = Convert.ToString(dataSet.Tables[0].Rows[i]["lastName"]);
                        oFunnelUser.email = Convert.ToString(dataSet.Tables[0].Rows[i]["email"]);
                        oFunnelUser.accountType = Convert.ToString(dataSet.Tables[0].Rows[i]["accountType"]);

                        oFunnelUserList.Add(oFunnelUser);
                    }

                    oFunnelUsers.oFunnelUser = oFunnelUserList.ToArray();
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs("GetAllOFunnelUsersForAccessTokenExpired: Failed to get all ofunnel users for access token expired from database." + ex.Message);
            }

            HelperMethods.AddLogs("Exit from GetAllOFunnelUsersForAccessTokenExpired.");

            return oFunnelUsers;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Access Token Expired email send functionality End.
        /////////////////////////////////////////////////////////////////////////////////////////


        ////////////////////////////////////////////////////////////////
        // Trial Period Expired email send functionality Start
        ///////////////////////////////////////////////////////////////

        /// <summary>
        /// Trial Period expired email scheduler time elapsed callback.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">Event arguments</param>
        private void TrialPeriodExpiredEmailSchedulerTimeElapsed(object sender)
        {
            this.SendEmailToUsersForTrialPeriodExpired();
        }

        /// <summary>
        /// This method send email to users for which trial period has expired.
        /// </summary>
        private void SendEmailToUsersForTrialPeriodExpired()
        {
            HelperMethods.AddLogs("Enter in SendEmailToUsersForTrialPeriodExpired.");

            try
            {
                OFunnelUsers oFunnelUsers = this.GetAllUsersForTrialPeriodExpired();

                if (oFunnelUsers != null && oFunnelUsers.oFunnelUser != null && oFunnelUsers.oFunnelUser.Length > 0)
                {
                    this.numberOfThreadsToSendTrialPeriodExpiredEmailNotYetCompleted = oFunnelUsers.oFunnelUser.Length;

                    foreach (OFunnelUser oFunnelUser in oFunnelUsers.oFunnelUser)
                    {
                        if (oFunnelUser.userId != -1)
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolCallbackToSendTrialPeriodExpiredEmail), oFunnelUser);
                        }
                        else
                        {
                            numberOfThreadsToSendTrialPeriodExpiredEmailNotYetCompleted--;
                        }
                    }

                    this.doneEventToSendTrialPeriodExpiredEmail.WaitOne();
                }
                else
                {
                    HelperMethods.AddLogs(string.Format("SendEmailToUsersForTrialPeriodExpired : No user found for which trial period is expiring in 1 day or 7 days or already ended."));
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("SendEmailToUsersForTrialPeriodExpired : Failed to send trial period expired emails for all users. \n\n", ex.Message));
            }

            HelperMethods.AddLogs("Exit from SendEmailToUsersForTrialPeriodExpired.");
        }

        /// <summary>
        /// Thread pool callback to send email for trial period Expired.
        /// </summary>
        /// <param name="threadContext">threadContext</param>
        public void ThreadPoolCallbackToSendTrialPeriodExpiredEmail(Object threadContext)
        {
            string userId = string.Empty;

            try
            {
                OFunnelUser oFunnelUser = threadContext as OFunnelUser;
                userId = Convert.ToString(oFunnelUser.userId);
                string toEmail = oFunnelUser.email;
                string userName = oFunnelUser.firstName + " " + oFunnelUser.lastName;
                int daysRemainingToExpire = oFunnelUser.daysRemainingToExpire;

                HelperMethods.AddLogs(string.Format("Enter in ThreadPoolCallbackToSendTrialPeriodExpiredEmail for userId = {0}.", userId));

                HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendTrialPeriodExpiredEmail: start sending trial period expired email for userId = {0}, userName = {1} at EmailId: {2}.", userId, userName, toEmail));

                // Send email for all public and private request to user.
                EmailService emailService = new EmailService();

                NameValueCollection nameValues = new NameValueCollection();
                nameValues["daysRemainingToExpire"] = Convert.ToString(daysRemainingToExpire);

                emailService.SetUsersDetailsToSendEmail(nameValues);

                string trialPeriodExpiredEmailSubject = string.Empty;

                switch (daysRemainingToExpire)
                { 
                    case 0:
                        trialPeriodExpiredEmailSubject = Constants.TrialPeriodExpiredEmailSubject;
                        break;
                    case 1:
                        trialPeriodExpiredEmailSubject = Constants.TrialPeriodExpiredInOneDayEmailSubject;
                        break;
                    case 7:
                        trialPeriodExpiredEmailSubject = Constants.TrialPeriodExpiredInSevenDayEmailSubject;
                        break;
                }


                bool isMailSend = emailService.SendMailToAllUsers(toEmail, null, trialPeriodExpiredEmailSubject, EmailType.TrialPeriodExpiredEmail);
                if (isMailSend)
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendTrialPeriodExpiredEmail: trial period expired email sent sucessfully to {0} at emailId: {1}.\n\n", userName, toEmail));
                }
                else
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendTrialPeriodExpiredEmail: Failed to send trial period expired email to {0} at emailId: {1}.\n\n", userName, toEmail));
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendTrialPeriodExpiredEmail: Failed to send trial period expired email for userId = {0}. Exception = {1} \n\n", userId, ex.Message));
            }

            if (Interlocked.Decrement(ref numberOfThreadsToSendTrialPeriodExpiredEmailNotYetCompleted) == 0)
            {
                HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendTrialPeriodExpiredEmail: Process completed."));

                this.doneEventToSendTrialPeriodExpiredEmail.Set();
            }

            HelperMethods.AddLogs(string.Format("Exit from ThreadPoolCallbackToSendTrialPeriodExpiredEmail for userId = {0}.", userId));
        }

        /// <summary>
        /// This methods gets all ofunnnel users for which access token has expired.
        /// </summary>
        /// <returns>OFunnelUsers</returns>
        private OFunnelUsers GetAllUsersForTrialPeriodExpired()
        {
            OFunnelUsers oFunnelUsers = null;

            HelperMethods.AddLogs("Enter in GetAllUsersForTrialPeriodExpired.");
            try
            {
                OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                DataSet dataSet = databaseHandler.GetAllUsersForTrialPeriodExpired();

                if (HelperMethods.IsValidDataSet(dataSet))
                {
                    oFunnelUsers = new OFunnelUsers();

                    OFunnelUser oFunnelUser = null;

                    List<OFunnelUser> oFunnelUserList = new List<OFunnelUser>();

                    for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                    {
                        oFunnelUser = new OFunnelUser();
                        oFunnelUser.userIndex = i;
                        oFunnelUser.userId = string.IsNullOrEmpty(Convert.ToString(dataSet.Tables[0].Rows[i]["userId"])) ? -1 : Convert.ToInt32(dataSet.Tables[0].Rows[i]["userId"]); ;
                        oFunnelUser.firstName = Convert.ToString(dataSet.Tables[0].Rows[i]["firstName"]);
                        oFunnelUser.lastName = Convert.ToString(dataSet.Tables[0].Rows[i]["lastName"]);
                        oFunnelUser.email = Convert.ToString(dataSet.Tables[0].Rows[i]["email"]);
                        oFunnelUser.daysRemainingToExpire = string.IsNullOrEmpty(Convert.ToString(dataSet.Tables[0].Rows[i]["daysRemainingToExpire"])) ? -1 : Convert.ToInt32(dataSet.Tables[0].Rows[i]["daysRemainingToExpire"]); ;

                        oFunnelUserList.Add(oFunnelUser);
                    }

                    oFunnelUsers.oFunnelUser = oFunnelUserList.ToArray();
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs("GetAllUsersForTrialPeriodExpired: Failed to get all users for trial period expired from database." + ex.Message);
            }

            HelperMethods.AddLogs("Exit from in GetAllUsersForTrialPeriodExpired.");

            return oFunnelUsers;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Trial Period Expired email send functionality End.
        /////////////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////////////////////////////////
        // Update Companies functionality Start.
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update companies time elapsed callback.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">Event arguments</param>
        private void UpdateCompaniesSchedulerTimeElapsed(object sender)
        {
            DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;

            // To send email on tuesday only.
            if ((DayOfWeek.Monday == dayOfWeek))
            {
                this.UpdateCompanyDetails();
            }
        }

        /// <summary>
        /// This method update company Details for subindustry, company size, state etc.
        /// </summary>
        private void UpdateCompanyDetails()
        {
            HelperMethods.AddLogs("Enter In UpdateCompanyDetails.");
            try
            {
                OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                DataSet dataSet = databaseHandler.GetAllActiveUsers(string.Empty);

                if (HelperMethods.IsValidDataSet(dataSet) && dataSet.Tables[0].Rows.Count > 0)
                {
                    List<string> allAccessTokenList = new List<string>();

                    for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                    {
                        string accessToken = Convert.ToString(dataSet.Tables[0].Rows[i]["accessToken"]);
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            allAccessTokenList.Add(accessToken);
                        }
                    }

                    DataSet companyDataSet = databaseHandler.GetCompaniesName(string.Empty);
                    List<CompanySearchData> companySearchDataList = null;

                    if (HelperMethods.IsValidDataSet(companyDataSet) && companyDataSet.Tables[0].Rows.Count > 0)
                    {
                        companySearchDataList = new List<CompanySearchData>();
                        for (int j = 0; j < companyDataSet.Tables[0].Rows.Count; j++)
                        {
                            CompanySearchData companySearchData = new CompanySearchData();
                            companySearchData.companyNameToSearch = Convert.ToString(companyDataSet.Tables[0].Rows[j]["name"]);
                            companySearchData.companyId = Convert.ToString(companyDataSet.Tables[0].Rows[j]["id"]);
                            companySearchData.processedName = Convert.ToString(companyDataSet.Tables[0].Rows[j]["processedName"]);

                            companySearchDataList.Add(companySearchData);
                        }
                    }

                    if (allAccessTokenList != null && allAccessTokenList.Count > 0 && companySearchDataList != null && companySearchDataList.Count > 0)
                    {
                        int numberOfThreadsToUpdateCompany = companySearchDataList.Count % Config.MaxCompanyUpdatePerThread == 0 ? companySearchDataList.Count / Config.MaxCompanyUpdatePerThread : ((companySearchDataList.Count / Config.MaxCompanyUpdatePerThread) + 1);
                        int numberOFAccessTokenPerThread = Config.MinAccessTokenPerThread;

                        if(allAccessTokenList.Count < numberOfThreadsToUpdateCompany)
                        {
                            numberOfThreadsToUpdateCompany = allAccessTokenList.Count;
                        }
                        else if(allAccessTokenList.Count / Config.MaxAccessTokenPerThread > numberOfThreadsToUpdateCompany)
                        {
                            numberOFAccessTokenPerThread = Config.MaxAccessTokenPerThread;
                        }

                        List<CompanyUpdateData> companyUpdateDataList = new List<CompanyUpdateData>();
                        int startIndexForCompany = 0;
                        int startIndexForAccessToken = 0;
                        int elementCount = Config.MaxCompanyUpdatePerThread;
                            
                        for(int i = 0; i < numberOfThreadsToUpdateCompany; i++)
                        {
                            if(companySearchDataList.Count < (startIndexForCompany + elementCount))
                            {
                                elementCount = companySearchDataList.Count - startIndexForCompany;
                            }

                            CompanyUpdateData companyUpdateData = new CompanyUpdateData();
                            companyUpdateData.companySearchDataList = companySearchDataList.GetRange(startIndexForCompany, elementCount);
                            companyUpdateData.accessTokenList = allAccessTokenList.GetRange(startIndexForAccessToken, numberOFAccessTokenPerThread);

                            companyUpdateDataList.Add(companyUpdateData);

                            startIndexForCompany += elementCount;
                            startIndexForAccessToken += numberOFAccessTokenPerThread;
                        }

                        if (companyUpdateDataList != null && companyUpdateDataList.Count > 0)
                        {
                            this.numberOfThreadsToUpdateCompanyDetailsNotYetCompleted = companyUpdateDataList.Count;

                            foreach (CompanyUpdateData companyUpdateData in companyUpdateDataList)
                            {
                                ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolCallbackToUpdateCompany), companyUpdateData);
                            }

                            this.doneEventToUpdateCompanies.WaitOne();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("UpdateCompanyDetails: Failed to update company details. exception = {0}", ex.Message));
            }

            HelperMethods.AddLogs("Exit form UpdateCompanyDetails. \n\n");
        }

        /// <summary>
        /// Threadpool callback to update company details from linkedIn.
        /// </summary>
        /// <param name="threadContext">threadContext</param>
        public void ThreadPoolCallbackToUpdateCompany(Object threadContext)
        {
            HelperMethods.AddLogs("Enter In ThreadPoolCallbackToUpdateCompany.");
            try
            {
                CompanyUpdateData companyUpdateData = threadContext as CompanyUpdateData;
                if (companyUpdateData != null && companyUpdateData.accessTokenList != null && companyUpdateData.companySearchDataList != null)
                {
                    this.UpdateCompanyDetailsFromLinkedIn(companyUpdateData.accessTokenList, companyUpdateData.companySearchDataList);
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToUpdateCompany: Failed to update company details. Exception = {0} \n\n", ex.Message));
            }

            if (Interlocked.Decrement(ref numberOfThreadsToUpdateCompanyDetailsNotYetCompleted) == 0)
            {
                this.doneEventToUpdateCompanies.Set();
            }

            HelperMethods.AddLogs("Exit from ThreadPoolCallbackToUpdateCompany.");
        }

        /// <summary>
        /// This method get company details from linkedIn and update in ofunnel database.
        /// </summary>
        /// <param name="asesssTokenList">asesssTokenList</param>
        /// <param name="companySearchDataList">companySearchDataList</param>
        public void UpdateCompanyDetailsFromLinkedIn(List<string> asesssTokenList, List<CompanySearchData> companySearchDataList)
        {
            HelperMethods.AddLogs("Enter In UpdateCompanyDetailsFromLinkedIn.");

            try
            {
                int noOfUpdates = 0;
                bool isCompanyLoopBreaked = false;

                foreach (string accessToken in asesssTokenList)
                {
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        XmlData xmldata = new XmlData();
                        StringBuilder companyDetailsData = new StringBuilder(xmldata.CompanyDetailsData);
                        StringBuilder companyDetailTemplate = new StringBuilder();

                        List<CompanySearchData> proccessCompanySearchDataList = companySearchDataList.GetRange(noOfUpdates, companySearchDataList.Count);
                        if (proccessCompanySearchDataList != null && proccessCompanySearchDataList.Count > 0)
                        {
                            foreach (CompanySearchData companySearchData in proccessCompanySearchDataList)
                            {
                                string companyNameToSearch = companySearchData.companyNameToSearch;
                                string companyId = companySearchData.companyId;
                                string processedName = companySearchData.processedName;

                                if (!string.IsNullOrEmpty(companyNameToSearch) && !string.IsNullOrEmpty(companyId))
                                {
                                    // Create request to get company search  result from LinkedIn.
                                    string companySerachUrl = this.CreateRequestForCompanySearchInLinkedIn(companyNameToSearch, accessToken);

                                    if (!string.IsNullOrEmpty(companySerachUrl))
                                    {
                                        WebService webService = new WebService();
                                        int httpStatusCode = -1;
                                        Stream responseStream = webService.ExecuteWebRequest(companySerachUrl, ref httpStatusCode);

                                        if (responseStream != null)
                                        {
                                            StreamReader reader = new StreamReader(responseStream);
                                            string response = reader.ReadToEnd();
                                            responseStream.Dispose();
                                            if (!string.IsNullOrEmpty(response))
                                            {
                                                JObject jsonDataObject = JObject.Parse(response);
                                                if (jsonDataObject != null)
                                                {
                                                    JObject jsonCompaniesObject = jsonDataObject.Property("companies") != null ? (JObject)jsonDataObject["companies"] : null;
                                                    if (jsonCompaniesObject != null)
                                                    {
                                                        int total = jsonCompaniesObject.Property("_total") != null ? (int)jsonCompaniesObject["_total"] : -1;
                                                        int count = jsonCompaniesObject.Property("_count") != null ? (int)jsonCompaniesObject["_count"] : -1;

                                                        if (total > 0 && count > 0)
                                                        {
                                                            JObject companyValues = null;

                                                            JArray companyArray = jsonCompaniesObject.Property("values") != null ? (JArray)jsonCompaniesObject["values"] : null;
                                                            if (companyArray != null && companyArray.Count > 0)
                                                            {
                                                                foreach (JObject company in companyArray)
                                                                {
                                                                    string companyName = company.Property("name") != null ? (string)company["name"] : string.Empty;

                                                                    if (!string.IsNullOrEmpty(companyName))
                                                                    {
                                                                        string searchedCompany = HelperMethods.IgnoreBlackListWordFromCompanyName(companyName);

                                                                        bool isCompanyNameMatched = processedName.ToUpper().Trim().Equals(searchedCompany.ToUpper().Trim());
                                                                        if (isCompanyNameMatched)
                                                                        {
                                                                            companyValues = (JObject)company;
                                                                            break;
                                                                        }
                                                                    }
                                                                }

                                                                if (companyValues == null)
                                                                {
                                                                    companyValues = (JObject)companyArray.ElementAt(0);
                                                                }

                                                                if (companyValues != null)
                                                                {
                                                                    string websiteUrl = string.Empty;
                                                                    string websiteUrlString = companyValues.Property("websiteUrl") != null ? (string)companyValues["websiteUrl"] : string.Empty;
                                                                    if (!string.IsNullOrEmpty(websiteUrlString))
                                                                    {
                                                                        websiteUrl = websiteUrlString;
                                                                    }

                                                                    string companySize = string.Empty;
                                                                    JObject companySizeObject = companyValues.Property("employeeCountRange") != null ? (JObject)companyValues["employeeCountRange"] : null;
                                                                    if (companySizeObject != null)
                                                                    {
                                                                        string companySizeString = companySizeObject.Property("name") != null ? (string)companySizeObject["name"] : string.Empty;
                                                                        if (!string.IsNullOrEmpty(companySizeString))
                                                                        {
                                                                            int employeeCount = HelperMethods.GetAverageCompanySize(companySizeString);
                                                                            companySize = Convert.ToString(employeeCount);
                                                                        }
                                                                    }

                                                                    string companyType = string.Empty;
                                                                    JObject companyTypeObject = companyValues.Property("companyType") != null ? (JObject)companyValues["companyType"] : null;
                                                                    if (companyTypeObject != null)
                                                                    {
                                                                        string companyTypeString = companyTypeObject.Property("name") != null ? (string)companyTypeObject["name"] : string.Empty;
                                                                        if (!string.IsNullOrEmpty(companyTypeString))
                                                                        {
                                                                            companyType = companyTypeString;
                                                                        }
                                                                    }

                                                                    string state = string.Empty;

                                                                    JObject locationObject = companyValues.Property("locations") != null ? (JObject)companyValues["locations"] : null;
                                                                    if (locationObject != null)
                                                                    {
                                                                        JArray locationArray = locationObject.Property("values") != null ? (JArray)locationObject["values"] : null;

                                                                        if (locationArray != null && locationArray.Count > 0)
                                                                        {
                                                                            JObject location = (JObject)locationArray.ElementAt(0);
                                                                            if (location != null)
                                                                            {
                                                                                JObject address = location.Property("address") != null ? (JObject)location["address"] : null;
                                                                                if (address != null)
                                                                                {
                                                                                    string stateValue = address.Property("state") != null ? (string)address["state"] : string.Empty;
                                                                                    if (!string.IsNullOrEmpty(stateValue))
                                                                                    {
                                                                                        state = stateValue;
                                                                                    }
                                                                                }
                                                                            }

                                                                        }
                                                                    }

                                                                    string subIndustry = string.Empty;
                                                                    JObject industryObject = companyValues.Property("industries") != null ? (JObject)companyValues["industries"] : null;
                                                                    if (industryObject != null)
                                                                    {
                                                                        JArray industryArray = industryObject.Property("values") != null ? (JArray)industryObject["values"] : null;

                                                                        if (industryArray != null && industryArray.Count > 0)
                                                                        {
                                                                            JObject subIndustryObject = (JObject)industryArray.ElementAt(0);
                                                                            if (subIndustryObject != null)
                                                                            {
                                                                                string subIndustryString = subIndustryObject.Property("name") != null ? (string)subIndustryObject["name"] : string.Empty;
                                                                                if (!string.IsNullOrEmpty(subIndustryString))
                                                                                {
                                                                                    subIndustry = subIndustryString;
                                                                                }
                                                                            }
                                                                        }
                                                                    }

                                                                    companyDetailTemplate.Append(xmldata.CompanyDetailTemplate);
                                                                    companyDetailTemplate.Replace(":CompanyIdValue", companyId);
                                                                    companyDetailTemplate.Replace(":WebsiteValue", SecurityElement.Escape(websiteUrl));
                                                                    companyDetailTemplate.Replace(":SubIndustryValue", SecurityElement.Escape(subIndustry));
                                                                    companyDetailTemplate.Replace(":EmployeeCountValue", SecurityElement.Escape(companySize));
                                                                    companyDetailTemplate.Replace(":StateValue", SecurityElement.Escape(state));
                                                                    companyDetailTemplate.Replace(":CompanyTypeValue", SecurityElement.Escape(companyType));

                                                                    noOfUpdates++;
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            HelperMethods.AddLogs("UpdateCompanyDetails: No result found from linkedin for companyName = " + companyNameToSearch);
                                                            companyDetailTemplate.Append(xmldata.CompanyDetailTemplate);
                                                            companyDetailTemplate.Replace(":CompanyIdValue", companyId);
                                                            companyDetailTemplate.Replace(":WebsiteValue", string.Empty);
                                                            companyDetailTemplate.Replace(":SubIndustryValue", string.Empty);
                                                            companyDetailTemplate.Replace(":EmployeeCountValue", string.Empty);
                                                            companyDetailTemplate.Replace(":StateValue", string.Empty);
                                                            companyDetailTemplate.Replace(":CompanyTypeValue", string.Empty);

                                                            noOfUpdates++;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (httpStatusCode != Convert.ToInt32(HttpStatusCode.OK))
                                            {
                                                HelperMethods.AddLogs("UpdateCompanyDetailsFromLinkedIn: LinkedIn Error: errorCode = " + httpStatusCode);

                                                if (httpStatusCode == 403)
                                                {
                                                    isCompanyLoopBreaked = true;
                                                    // As throttle limit has expired for this user (accesstoken) So move to next accesstoken.
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                HelperMethods.AddLogs(string.Format("UpdateCompanyDetailsFromLinkedIn: LinkedIn company search response stream is null so company serach result not found."));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        HelperMethods.AddLogs(string.Format("UpdateCompanyDetails: Number of company updates send to database. noOfUpdates = {0} ", noOfUpdates));

                        string companyDetailTemplateString = Convert.ToString(companyDetailTemplate);
                        if (!string.IsNullOrEmpty(companyDetailTemplateString))
                        {
                            companyDetailsData.Replace(":CompanyDetailTemplate", companyDetailTemplateString);

                            string queryString = Convert.ToString(companyDetailsData);
                                    
                            if (!string.IsNullOrEmpty(queryString))
                            {
                                queryString.Replace("'", "''");

                                OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                                DataSet updateCompanyDataSet = databaseHandler.UpdateCompanyDetails("N" + "'" + queryString + "'");

                                if (HelperMethods.IsValidDataSet(updateCompanyDataSet))
                                {
                                    bool isCompanyDetailsUpdated = Convert.ToBoolean(updateCompanyDataSet.Tables[0].Rows[0]["isCompanyDetailsUpdated"]);
                                            
                                }
                                else
                                {
                                    HelperMethods.AddLogs("UpdateCompanyDetailsFromLinkedIn: (InvalidDataSet) Data received form database is invalid.");
                                }
                            }
                        }

                        if (!isCompanyLoopBreaked)
                        {
                            HelperMethods.AddLogs("UpdateCompanyDetails: All companies has been processed. So break accessToken loop.");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("UpdateCompanyDetailsFromLinkedIn: Failed to update companies in database. Exception Occured {0}", ex.Message));
            }

            HelperMethods.AddLogs("Exit from UpdateCompanyDetailsFromLinkedIn.");
        }

        /// <summary>
        /// This method creates a web request for company search in LinkedIn.
        /// </summary>
        /// <param name="companyName">companyName to be search</param>
        /// <param name="auth2Token">auth2 token for LinkedIn</param>
        /// <returns>Request for company search in LinkedIn</returns>
        public string CreateRequestForCompanySearchInLinkedIn(string companyName, string auth2Token)
        {
            string companySerachUrl = string.Empty;

            if (!string.IsNullOrEmpty(auth2Token))
            {
                companySerachUrl = Constants.CompanySearchLinkedInUrl;

                companySerachUrl = companySerachUrl.Replace(":COMPANY_NAME", companyName);
                companySerachUrl = companySerachUrl.Replace(":AUTH_2_TOKEN", auth2Token);
            }

            return companySerachUrl;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Update Companies functionality End.
        /////////////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////////////////////////////////
        // Check server status functionality start.
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Check server status scheduler time elapsed callback.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        private void CheckServerStatusSchedulerTimeElapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            this.CheckServerStatus();
        }

        /// <summary>
        /// This method checks server status.
        /// </summary>
        private void CheckServerStatus()
        {
            HelperMethods.AddLogs("Enter in CheckServerStatus.");
            string resuest = Config.CheckForServerStatus;

            Stream responseStream = null;
            if (!string.IsNullOrEmpty(resuest))
            {
                WebService webService = new WebService();
                int httpStatusCode = -1;
                string description = string.Empty;
                string errorMessage = string.Empty;

                responseStream = webService.ExecuteWebRequest(resuest, ref httpStatusCode, ref description, ref errorMessage);

                if (responseStream != null && httpStatusCode == Convert.ToInt32(HttpStatusCode.OK))
                {
                    StreamReader reader = new StreamReader(responseStream);
                    string response = reader.ReadToEnd();

                    HelperMethods.AddLogs(string.Format("CheckServerStatus: Response Message = {0}.", response));

                    responseStream.Dispose();
                }
                else
                {
                    if (httpStatusCode != Convert.ToInt32(HttpStatusCode.OK))
                    {
                        HelperMethods.AddLogs(string.Format("CheckServerStatus: httpStatusCode = {0} description = {1} errorMessage = {2}.", httpStatusCode, description, errorMessage));
                        // Send email for all public and private request to user.
                        EmailService emailService = new EmailService();

                        NameValueCollection nameValues = new NameValueCollection();
                        nameValues["httpStatusCode"] = Convert.ToString(httpStatusCode);
                        nameValues["description"] = description;
                        nameValues["errorMessage"] = errorMessage;

                        emailService.SetUsersDetailsToSendEmail(nameValues);

                        string toEmail = Constants.ServerErrorToUserEmailId;
                        string ccEmail = Constants.ServerErrorCcUserEmailId;
                        string serverStatusEmailSubject = Config.OFunnelServerStatusEmailSubject;

                        bool isMailSend = emailService.SendMailToAllUsers(toEmail, ccEmail, serverStatusEmailSubject, EmailType.OFunnelServerError);
                        if (isMailSend)
                        {
                            HelperMethods.AddLogs("CheckServerStatus: Check server Status email sent sucessfully.");
                        }
                        else
                        {
                            HelperMethods.AddLogs("CheckServerStatus: Failed to send Server status email.");
                        }
                    }
                }
            }

            HelperMethods.AddLogs("Exit from CheckServerStatus.");
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Check server status functionality end.
        /////////////////////////////////////////////////////////////////////////////////////////


        /////////////////////////////////////////////////////////////////////////////////////////
        // Send Push Notification functionality start.
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Check server status scheduler time elapsed callback.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        private void SendPushNotificationSchedulerTimeElapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            this.SendPushNotificationToUsers();
        }

        /// <summary>
        /// This method send push Notification to users.
        /// </summary>
        private void SendPushNotificationToUsers()
        {
            HelperMethods.AddLogs("Enter into SendPushNotificationToUsers.");
            try
            {
                OFunnelUsers oFunnelUsers = this.GetAllUserWhoHasNotificationChannelUrl();

                if (oFunnelUsers != null && oFunnelUsers.oFunnelUser != null && oFunnelUsers.oFunnelUser.Length > 0)
                {
                    this.numberOfThreadsToSendPushNotificationNotYetCompleted = oFunnelUsers.oFunnelUser.Length;

                    foreach (OFunnelUser oFunnelUser in oFunnelUsers.oFunnelUser)
                    {
                        if (oFunnelUser.userId != -1)
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolCallbackToSendPushNotification), oFunnelUser);
                        }
                        else
                        {
                            numberOfThreadsToSendPushNotificationNotYetCompleted--;
                        }
                    }

                    this.doneEventToSendPushNotification.WaitOne();
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("Failed to push notification. \n\n", ex.Message));
            }
            HelperMethods.AddLogs("Exit from SendPushNotificationToUsers.");
        }

        /// <summary>
        /// Thread pool callback to send push notification.
        /// </summary>
        /// <param name="threadContext">threadContext</param>
        public void ThreadPoolCallbackToSendPushNotification(Object threadContext)
        {
            OFunnelUser oFunnelUser = threadContext as OFunnelUser;
            string userId = Convert.ToString(oFunnelUser.userId);

            HelperMethods.AddLogs("Enter into ThreadPoolCallbackToSendPushNotification. for userId = " + userId);
            try
            {
                string resuest = Config.SendPushNotificationUrl;
                resuest = resuest.Replace(":USER_ID", userId);
                string channelUrl = Uri.EscapeDataString(oFunnelUser.channelUrl);
                resuest = resuest.Replace(":CHANNEL_URL", channelUrl);

                Stream responseStream = null;
                if (!string.IsNullOrEmpty(resuest))
                {
                    WebService webService = new WebService();
                    int httpStatusCode = -1;
                    string description = string.Empty;
                    string errorMessage = string.Empty;

                    responseStream = webService.ExecuteWebRequest(resuest, ref httpStatusCode, ref description, ref errorMessage);

                    if (responseStream != null && httpStatusCode == Convert.ToInt32(HttpStatusCode.OK))
                    {
                        StreamReader reader = new StreamReader(responseStream);
                        string response = reader.ReadToEnd();

                        HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendPushNotification: Response Message = {0}.", response));

                        responseStream.Dispose();
                    }
                    else
                    {
                        if (httpStatusCode != Convert.ToInt32(HttpStatusCode.OK))
                        {
                            HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendPushNotification: httpStatusCode = {0} description = {1} errorMessage = {2} for userId = {3}.", httpStatusCode, description, errorMessage, userId));
                        }
                        else
                        {
                            HelperMethods.AddLogs("ThreadPoolCallbackToSendPushNotification: Response stream is null for userId = " +  userId);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendPushNotification: Failed to send pushNotification for userId = {0}. Exception = {1}.", userId, ex.Message));
            }

            if (Interlocked.Decrement(ref numberOfThreadsToSendPushNotificationNotYetCompleted) == 0)
            {
                HelperMethods.AddLogs("ThreadPoolCallbackToSendPushNotification: Completed all thread which are sending netwrok updates.");
                this.doneEventToSendPushNotification.Set();
            }

            HelperMethods.AddLogs("Exit from ThreadPoolCallbackToSendPushNotification for userId = " + userId);
        }

        /// <summary>
        /// This method get all users who has setup NotificationChannel.
        /// </summary>
        /// <returns>OFunnelUsers</returns>
        private OFunnelUsers GetAllUserWhoHasNotificationChannelUrl()
        {
            HelperMethods.AddLogs("Enter into GetAllUserWhoHasNotificationChannelUrl.");
            OFunnelUsers oFunnelUsers = null;

            try
            {
                OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                DataSet dataSet = databaseHandler.GetAllUserWhoHasNotificationChannelUrl();

                if (HelperMethods.IsValidDataSet(dataSet) && dataSet.Tables[0].Rows.Count > 0)
                {
                    oFunnelUsers = new OFunnelUsers();

                    OFunnelUser oFunnelUser = null;

                    List<OFunnelUser> oFunnelUserList = new List<OFunnelUser>();

                    for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                    {
                        oFunnelUser = new OFunnelUser();
                        oFunnelUser.userIndex = i;
                        oFunnelUser.userId = string.IsNullOrEmpty(Convert.ToString(dataSet.Tables[0].Rows[i]["userId"])) ? -1 : Convert.ToInt32(dataSet.Tables[0].Rows[i]["userId"]); ;
                        oFunnelUser.firstName = Convert.ToString(dataSet.Tables[0].Rows[i]["firstName"]);
                        oFunnelUser.lastName = Convert.ToString(dataSet.Tables[0].Rows[i]["lastName"]);
                        oFunnelUser.email = Convert.ToString(dataSet.Tables[0].Rows[i]["email"]);
                        oFunnelUser.channelUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["channelUrl"]);

                        oFunnelUserList.Add(oFunnelUser);
                    }

                    oFunnelUsers.oFunnelUser = oFunnelUserList.ToArray();
                }
                else
                {
                    HelperMethods.AddLogs("GetAllUserWhoHasNotificationChannelUrl: No user found in database who has setup notification channel url.");
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs("GetAllUserWhoHasNotificationChannelUrl: Failed to get all ofunnel users from database." + ex.Message);
            }

            HelperMethods.AddLogs("Exit from GetAllUserWhoHasNotificationChannelUrl.");
            return oFunnelUsers;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Send Push Notification functionality end.
        /////////////////////////////////////////////////////////////////////////////////////////


        /////////////////////////////////////////////////////////////////////////////////////////
        // Followup Network updates Email functionality Starts.
        /////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Followup Network Updates Email scheduler time elapsed callback.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">Event arguments</param>
        private void RequestFollowupsEmailSchedulerTimeElapsed(object sender)
        {
            DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;

            // To send email on sunday only.
            if ((DayOfWeek.Sunday == dayOfWeek))
            {
                this.SendEmailToOFunnelUsersForFollowupNetworkUpdate();   
            }
        }

        /// <summary>
        /// This method gets Network Expand Statistics for user.
        /// </summary>
        /// <param name="userId">userId</param>
        private NetworkExpandStatistics GetNetworkExpandStatisticsForUserId(string userId)
        {
            NetworkExpandStatistics networkExpandStatistics = new NetworkExpandStatistics();

            try
            {
                NameValueCollection nameValue = new NameValueCollection();
                nameValue["userId"] = userId;

                string message = HelperMethods.GetParametersListForLogMessage(nameValue);

                HelperMethods.AddLogs("Enter In GetNetworkExpandStatisticsForUserId. Parameters List => " + message);

                if (!string.IsNullOrEmpty(userId))
                {
                    OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                    DataSet dataSet = databaseHandler.GetNetworkExpandDetailsForLocationAndIndustryForUserId("'" + userId + "'");

                    if (dataSet != null && dataSet.Tables.Count > 0)
                    {
                        List<SubIndustryNetworkUpdates> subIndustryNetworkUpdatesList = new List<SubIndustryNetworkUpdates>();
                        List<LocationNetworkUpdates> locationNetworkUpdatesList = new List<LocationNetworkUpdates>();

                        // Fill data for Location network expand
                        if (dataSet.Tables[0].Rows.Count > 0)
                        {
                            LocationNetworkUpdates locationNetworkUpdates = null;

                            for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                            {
                                locationNetworkUpdates = new LocationNetworkUpdates();

                                locationNetworkUpdates.locationName = Convert.ToString(dataSet.Tables[0].Rows[i]["name"]);
                                locationNetworkUpdates.locationUpdateCount = Convert.ToString(dataSet.Tables[0].Rows[i]["networkExpandCount"]);

                                locationNetworkUpdatesList.Add(locationNetworkUpdates);
                            }
                        }

                        // Fill data for subindustry network expand
                        if (dataSet.Tables[1].Rows.Count > 0)
                        {
                            SubIndustryNetworkUpdates subIndustryNetworkUpdates = null;

                            for (int i = 0; i < dataSet.Tables[1].Rows.Count; i++)
                            {
                                subIndustryNetworkUpdates = new SubIndustryNetworkUpdates();

                                subIndustryNetworkUpdates.subindustryName = Convert.ToString(dataSet.Tables[1].Rows[i]["name"]);
                                subIndustryNetworkUpdates.subindustryUpdateCount = Convert.ToString(dataSet.Tables[1].Rows[i]["networkExpandCount"]);

                                subIndustryNetworkUpdatesList.Add(subIndustryNetworkUpdates);
                            }
                        }

                        networkExpandStatistics.subIndustryNetworkUpdates = subIndustryNetworkUpdatesList.ToArray();
                        networkExpandStatistics.locationNetworkUpdates = locationNetworkUpdatesList.ToArray();
                    }
                    else
                    {
                        HelperMethods.AddLogs("GetNetworkExpandStatisticsForUserId: (InvalidDataSet) Data received form database is invalid.");
                    }
                }
                else
                {
                    HelperMethods.AddLogs("GetNetworkExpandStatisticsForUserId: Required parameter {userId} is missing.");
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("GetNetworkExpandStatisticsForUserId: Failed to get Network Expand Statistics details from Database for userId = {0}. Exception Occured = {1}", userId, ex.Message));
            }

            HelperMethods.AddLogs("Exit from GetNetworkExpandStatisticsForUserId.");

            return networkExpandStatistics;
        }
        
        /// <summary>
        /// Method to send the Followup Network updates email.
        /// </summary>
        private void SendEmailToOFunnelUsersForFollowupNetworkUpdate()
        {
            HelperMethods.AddLogs("Enter in SendEmailToOFunnelUsersForFollowupNetworkUpdate.");

            try
            {
                OFunnelUsers oFunnelUsers = this.GetAllOFunnelUsersForFollowUpNetworkUpdates();

                if (oFunnelUsers != null && oFunnelUsers.oFunnelUser != null && oFunnelUsers.oFunnelUser.Length > 0)
                {
                    this.numberOfThreadsForFollowupNetworkUpdateEmailNotYetCompleted = oFunnelUsers.oFunnelUser.Length;

                    foreach (OFunnelUser oFunnelUser in oFunnelUsers.oFunnelUser)
                    {
                        if (oFunnelUser.userId != -1)
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail), oFunnelUser);
                        }
                        else
                        {
                            numberOfThreadsForFollowupNetworkUpdateEmailNotYetCompleted--;
                        }
                    }

                    this.doneEventForFollowUpNetworkUpdateEmail.WaitOne();
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("SendEmailToOFunnelUsersForFollowupNetworkUpdate : Failed to send network update emails for all users. \n\n", ex.Message));
            }

            HelperMethods.AddLogs("Exit from SendEmailToOFunnelUsersForFollowupNetworkUpdate.");
        }

        /// <summary>
        /// This methods gets all ofunnnel users who has any followup network update in last 1 week.
        /// </summary>
        /// <returns>OFunnelUsers</returns>
        private OFunnelUsers GetAllOFunnelUsersForFollowUpNetworkUpdates()
        {
            OFunnelUsers oFunnelUsers = null;

            try
            {
                OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                DataSet dataSet = databaseHandler.GetOFunnelUsersForFollowUpNetworkUpdate();

                if (HelperMethods.IsValidDataSet(dataSet))
                {
                    oFunnelUsers = new OFunnelUsers();

                    OFunnelUser oFunnelUser = null;

                    List<OFunnelUser> oFunnelUserList = new List<OFunnelUser>();

                    for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                    {
                        oFunnelUser = new OFunnelUser();
                        oFunnelUser.userIndex = i;
                        oFunnelUser.userId = string.IsNullOrEmpty(Convert.ToString(dataSet.Tables[0].Rows[i]["userId"])) ? -1 : Convert.ToInt32(dataSet.Tables[0].Rows[i]["userId"]); ;
                        oFunnelUser.firstName = Convert.ToString(dataSet.Tables[0].Rows[i]["firstName"]);
                        oFunnelUser.lastName = Convert.ToString(dataSet.Tables[0].Rows[i]["lastName"]);
                        oFunnelUser.email = Convert.ToString(dataSet.Tables[0].Rows[i]["email"]);
                        oFunnelUser.accountType = Convert.ToString(dataSet.Tables[0].Rows[i]["accountType"]);

                        oFunnelUserList.Add(oFunnelUser);
                    }

                    oFunnelUsers.oFunnelUser = oFunnelUserList.ToArray();
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs("GetAllOFunnelUsersForFollowUpNetworkUpdates: Failed to get all ofunnel users for target accounts from database." + ex.Message);
            }

            return oFunnelUsers;
        }


        /// <summary>
        /// Thread pool callback to send email for followup network updates.
        /// </summary>
        /// <param name="threadContext"></param>
        public void ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail(Object threadContext)
        {
            string userId = string.Empty;

            try
            {
                OFunnelUser oFunnelUser = threadContext as OFunnelUser;
                userId = Convert.ToString(oFunnelUser.userId);
                string toEmail = oFunnelUser.email;
                string userName = oFunnelUser.firstName + " " + oFunnelUser.lastName;

                HelperMethods.AddLogs(string.Format("Enter in ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail for userId = {0}.", userId));

                OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                DataSet dataSet = databaseHandler.GetRecipientsEmail("'" + userId + "'");

                List<string> toEmailsList = new List<string>();
                toEmailsList.Add(toEmail);

                // Sent mail to recipient is commented.
                List<string> recipientEmailsList = new List<string>();
                if (HelperMethods.IsValidDataSet(dataSet))
                {
                    if (dataSet.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                        {
                            string email = string.Empty;
                            email = Convert.ToString(dataSet.Tables[0].Rows[i]["email"]);
                            recipientEmailsList.Add(email);
                        }
                    }
                    else
                    {
                        HelperMethods.AddLogs("ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail: No recipient email for netwrok alerts are available for userId = " + userId);
                    }
                }
                else
                {
                    HelperMethods.AddLogs("ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail: Failed to get recipient email for netwrok alerts from database for userId = " + userId);
                }

                string[] toEmails = toEmailsList.ToArray();
                string[] recipientEmails = recipientEmailsList.ToArray();

                // Send email for all public and private request to user.
                EmailService emailService = new EmailService();

                NetworkUpdates networkUpdates = this.GetFollowUpNetworkUpdateDetailForUserId(userId);
                bool isNetworkUpdateFound = false;

                if (networkUpdates != null && networkUpdates.networkAlertsForAlertType != null && networkUpdates.networkAlertsForAlertType.Length > 0)
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail: Daily netwrok update email for (Network update count {0}) sending to {1} at EmailId: {2}.", networkUpdates.networkAlertsForAlertType.Length, userName, toEmail));

                    isNetworkUpdateFound = emailService.CreateNetworkUpdateSectionForEmailTemplate(networkUpdates.networkAlertsForAlertType);
                }
                else
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail: No network update found for userId = {0}, userName = {1}.", userId, userName));
                }

                bool isTwitterLeadFound = false;
                TwitterLeads twitterLeads = this.GetFollowupTwitterLeadDetailForUserId(userId);

                if (twitterLeads != null && twitterLeads.twitterLeadsForAlertType != null && twitterLeads.twitterLeadsForAlertType.Length > 0)
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail: Daily twitter lead followup email for (Twiter leads count {0}) sending to {1} at EmailId: {2}.", twitterLeads.twitterLeadsForAlertType.Length, userName, toEmail));

                    isTwitterLeadFound = emailService.CreateTwitterLeadSectionForEmailTemplate(twitterLeads.twitterLeadsForAlertType);
                }
                else
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail: No twitter lead found for userId = {0}, userName = {1}.", userId, userName));
                }

                // As per requested by Kushal, We are not sending NetworkExpandStatistics email to user. 24-Jun-2014
                NetworkExpandStatistics networkExpandStatistics = null; //this.GetNetworkExpandStatisticsForUserId(userId);
                bool isNetworkExpandStatisticsFound = false;

                if (networkExpandStatistics != null)
                {
                    isNetworkExpandStatisticsFound = emailService.CreateNetworkExpandStatisticsForEmailTemplate(networkExpandStatistics);
                }
                else
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail: No Network Expand Statistics found for userId = {0}, userName = {1}.", userId, userName));
                }

                if (isNetworkUpdateFound || isTwitterLeadFound || isNetworkExpandStatisticsFound)
                {
                    NameValueCollection nameValues = new NameValueCollection();
                    nameValues["userId"] = userId;

                    emailService.SetUsersDetailsToSendEmail(nameValues);

                    string netwrokUpdateFollowupAlertEmailSubject = string.Empty;

                    bool isMailSend = false;

                    netwrokUpdateFollowupAlertEmailSubject = Constants.NetwrokUpdateFollowUpAlertEmailSubject;
                    isMailSend = emailService.SendMailToAllUsers(toEmails, recipientEmails, null, netwrokUpdateFollowupAlertEmailSubject, EmailType.NetwrokUpdateFollowupAlertEmail);

                    if (isMailSend)
                    {
                        HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail: Daily network update email for network update and twitter lead followup sends sucessfully to {0} at EmailId: {1}.\n\n", userName, toEmail));

                        try
                        {
                            DataSet lastTimeDataSet = databaseHandler.SetIsMailSentForFollowupsNetworkUpdatesForUserId("'" + userId + "'");
                            if (HelperMethods.IsValidDataSet(lastTimeDataSet) && lastTimeDataSet.Tables[0].Rows.Count > 0)
                            {
                                bool isEmailSentStatusSet = Convert.ToBoolean(lastTimeDataSet.Tables[0].Rows[0]["isEmailSentStatusSet"]);
                                if (!isEmailSentStatusSet)
                                {
                                    HelperMethods.AddLogs("ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail: Failed to set isEmailSent status for follow up netwrok alert email in database for userId = " + userId);
                                }
                            }
                            else
                            {
                                HelperMethods.AddLogs("ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail: (DataSet Invalid): Failed to set isEmailSent status for follow up netwrok alert email in database for userId = " + userId);
                            }
                        }
                        catch (Exception ex)
                        {
                            HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail: Failed to set isEmailSent status for follow up netwrok alert email in database for userId = {0}. Exception = {1} \n\n", userId, ex.Message));
                        }
                    }
                    else
                    {
                        HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail: Weekly follow up network update email for network update and twitter lead followup failed to send to {0} at EmailId: {1}.\n\n", userName, toEmail));
                    }
                }
                else
                {
                    HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail: Weekly follow up Network Update Section is empty so there are no network updates to send email for userId = {0}.", userId));
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail: Failed to send follow up network update email for userId = {0}. Exception = {1} \n\n", userId, ex.Message));
            }

            if (Interlocked.Decrement(ref numberOfThreadsForFollowupNetworkUpdateEmailNotYetCompleted) == 0)
            {
                this.doneEventForFollowUpNetworkUpdateEmail.Set();
            }

            HelperMethods.AddLogs(string.Format("Exit from ThreadPoolCallbackToSendFollowUpNetworkUpdateEmail for userId = {0}.", userId));
        }

        /// <summary>
        /// This method gets follow up network updates for user.
        /// </summary>
        /// <param name="userId">userId</param>
        private NetworkUpdates GetFollowUpNetworkUpdateDetailForUserId(string userId)
        {
            NetworkUpdates networkUpdates = new NetworkUpdates();

            try
            {
                NameValueCollection nameValue = new NameValueCollection();
                nameValue["userId"] = userId;

                string message = HelperMethods.GetParametersListForLogMessage(nameValue);

                HelperMethods.AddLogs("Enter In GetFollowUpNetworkUpdateDetailForUserId. Parameters List => " + message);

                if (!string.IsNullOrEmpty(userId))
                {
                    OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                    DataSet dataSet = databaseHandler.GetFollowUpNetworkUpdateDetailForUserId("'" + userId + "'");

                    if (HelperMethods.IsValidDataSet(dataSet))
                    {
                        NetworkAlertDetails networkAlertDetails = null;
                        List<NetworkAlertDetails> networkAlertDetailsList = new List<NetworkAlertDetails>();

                        for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                        {
                            networkAlertDetails = new NetworkAlertDetails();

                            networkAlertDetails.networkUpdateId = Convert.ToString(dataSet.Tables[0].Rows[i]["id"]);

                            networkAlertDetails.yourConnectionLinkedInId = Convert.ToString(dataSet.Tables[0].Rows[i]["yourConnectionLinkedInId"]);
                            networkAlertDetails.yourConnectionFirstName = Convert.ToString(dataSet.Tables[0].Rows[i]["yourConnectionFirstName"]);
                            networkAlertDetails.yourConnectionLastName = Convert.ToString(dataSet.Tables[0].Rows[i]["yourConnectionLastName"]);
                            networkAlertDetails.yourConnectionProfileUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["yourConnectionProfileUrl"]);
                            networkAlertDetails.yourConnectionProfilePicUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["yourConnectionProfilePicUrl"]);
                            networkAlertDetails.yourConnectionHeadline = Convert.ToString(dataSet.Tables[0].Rows[i]["yourConnectionHeadline"]);
                            networkAlertDetails.yourConnectionCompany = Convert.ToString(dataSet.Tables[0].Rows[i]["yourConnectionCompany"]);

                            networkAlertDetails.connectedToLinkedInId = Convert.ToString(dataSet.Tables[0].Rows[i]["connectedToLinkedInId"]);
                            networkAlertDetails.connectedToFirstName = Convert.ToString(dataSet.Tables[0].Rows[i]["connectedToFirstName"]);
                            networkAlertDetails.connectedToLastName = Convert.ToString(dataSet.Tables[0].Rows[i]["connectedToLastName"]);
                            networkAlertDetails.connectedToProfileUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["connectedToProfileUrl"]);
                            networkAlertDetails.connectedToProfilePicUrl = Convert.ToString(dataSet.Tables[0].Rows[i]["connectedToProfilePicUrl"]);
                            networkAlertDetails.connectedToHeadline = Convert.ToString(dataSet.Tables[0].Rows[i]["connectedToHeadline"]);
                            networkAlertDetails.connectedToCompany = Convert.ToString(dataSet.Tables[0].Rows[i]["connectedToCompany"]);

                            networkAlertDetails.filterType = Convert.ToString(dataSet.Tables[0].Rows[i]["filterType"]);
                            networkAlertDetails.targetName = Convert.ToString(dataSet.Tables[0].Rows[i]["targetName"]);

                            string filterType = Convert.ToString(dataSet.Tables[0].Rows[i]["filterType"]);

                            if (string.IsNullOrEmpty(networkAlertDetails.connectedToLinkedInId) && filterType.ToUpper().Equals("COMPANY"))
                            {
                                networkAlertDetails.alertType = "POSITION";
                            }
                            else if (string.IsNullOrEmpty(networkAlertDetails.connectedToLinkedInId) && filterType.ToUpper().Equals("ROLE"))
                            {
                                networkAlertDetails.alertType = "POSITIONROLE";
                            }
                            else if (string.IsNullOrEmpty(networkAlertDetails.connectedToLinkedInId) && filterType.ToUpper().Equals("PERSON"))
                            {
                                networkAlertDetails.alertType = "POSITIONPERSON";
                            }
                            else
                            {
                                networkAlertDetails.alertType = filterType;
                            }

                            networkAlertDetailsList.Add(networkAlertDetails);
                        }

                        if (networkAlertDetailsList != null && networkAlertDetailsList.Count > 0)
                        {
                            List<NetworkAlertsForAlertType> networkAlertsForAlertTypeList = new List<NetworkAlertsForAlertType>();

                            var groupByAlertTypeNetworkAlerts = networkAlertDetailsList.GroupBy(g => g.alertType);
                            foreach (var networkAlertAlertTypeGroup in groupByAlertTypeNetworkAlerts)
                            {
                                if (networkAlertAlertTypeGroup.Count() > 0)
                                {
                                    NetworkAlertsForAlertType networkAlertsForAlertType = new NetworkAlertsForAlertType();

                                    List<NetworkAlerts> networkAlertsList = new List<NetworkAlerts>();

                                    var groupByTargetNameNetworkAlerts = networkAlertAlertTypeGroup.GroupBy(g => g.targetName);

                                    foreach (var networkAlertTargetNameGroup in groupByTargetNameNetworkAlerts)
                                    {
                                        NetworkAlerts networkAlerts = new NetworkAlerts();

                                        if (networkAlertTargetNameGroup.Count() > 0)
                                        {
                                            networkAlerts.networkAlertDetails = networkAlertTargetNameGroup.ToArray();

                                            networkAlertsForAlertType.alertType = networkAlerts.networkAlertDetails.ElementAt(0).alertType;
                                            networkAlerts.targetName = networkAlerts.networkAlertDetails.ElementAt(0).targetName;
                                        }

                                        networkAlertsList.Add(networkAlerts);
                                    }

                                    networkAlertsForAlertType.networkAlerts = networkAlertsList.ToArray();

                                    networkAlertsForAlertTypeList.Add(networkAlertsForAlertType);
                                }
                            }

                            networkUpdates.networkAlertsForAlertType = networkAlertsForAlertTypeList.ToArray();
                        }
                    }
                    else
                    {
                        HelperMethods.AddLogs("GetFollowUpNetworkUpdateDetailForUserId: (InvalidDataSet) Data received form database is invalid.");
                    }
                }
                else
                {
                    HelperMethods.AddLogs("GetFollowUpNetworkUpdateDetailForUserId: Required parameter {userId} is missing.");
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs(string.Format("GetFollowUpNetworkUpdateDetailForUserId: Failed to get network update details from Database for userId = {0}. Exception Occured = {1}", userId, ex.Message));
            }

            HelperMethods.AddLogs("Exit from GetFollowUpNetworkUpdateDetailForUserId.");

            return networkUpdates;
        }

    }
}
