using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;


namespace OFunnelEmailScheduler.OFunnelUtilities
{
    /// <summary>
    /// Enum for email type.
    /// </summary>
    public enum EmailType
    {
        OpenRequestEmail,
        NetwrokUpdateAlertEmail,
        NetwrokUpdateFollowupAlertEmail,
        NetwrokUpdateAlertEmailForPipelineUser,
        AccessTokenExpiredEmail,
        AccessTokenExpiredEmailForPipelineUser,
        TrialPeriodExpiredEmail,
        OFunnelServerError
    }

    /// <summary>
    /// This class send email to specified user.
    /// </summary>
    public class EmailService
    {
        private NameValueCollection nameValues = null;
        private string requestSection = string.Empty;
        private string articleSection = string.Empty;
        private string networkUpdateSection = string.Empty;
        private string positionUpdateSection = string.Empty;
        private StringBuilder twitterAllLeadSection = new StringBuilder();
        StringBuilder allOtherTypeNetworkUpdatesSection = new StringBuilder(string.Empty);

        private string networkExpandForLocationSection = string.Empty;
        private string networkExpandForSubIndustrySection = string.Empty;

        /// <summary>
        /// This method sent email to all Ofunnel users.
        /// </summary>
        /// <param name="data">Stream which contains infomration over which need to send email. </param>
        /// <returns>Result true/false</returns>
        public bool SendMailToAllUsers(string toEmailId, string ccEmailId, string subject, EmailType emailType)
        {
            bool isMailSentSuccessfully = false;
            try
            {
                if (!string.IsNullOrEmpty(toEmailId))
                {
                    string emailUserName = string.Empty;
                    string emailPassword = Config.EmailPassword;
                    string emailFromName = Config.EmailFromName;
                    string emailHost = Config.EmailHost;
                    int emailPort = Config.EmailPort;

                    AlternateView htmlView = null;

                    switch (emailType)
                    {
                        case EmailType.OpenRequestEmail:
                            emailUserName = Config.EmailUserName;

                            htmlView = this.CreateAlternateViewForRequestStatusEmail();
                            break;

                        case EmailType.NetwrokUpdateAlertEmail:
                            emailUserName = Config.EmailUserName;

                            htmlView = this.CreateAlternateViewForNetworkUpdateAlertEmail();
                            break;

                        case EmailType.NetwrokUpdateFollowupAlertEmail:
                            emailUserName = Config.EmailUserName;

                            htmlView = this.CreateAlternateViewForNetworkUpdateFollowupAlertEmail();
                            break;

                        case EmailType.NetwrokUpdateAlertEmailForPipelineUser:
                            emailUserName = Config.EmailUserName;

                            emailFromName = Config.EmailFromNameForPipelineUser;

                            htmlView = this.CreateAlternateViewForNetworkUpdateAlertEmailForPipelineUser();
                            break;

                        case EmailType.AccessTokenExpiredEmail:
                            emailUserName = Config.EmailUserName;

                            htmlView = this.CreateAlternateViewForAccessTokenExpiredEmail();
                            break;

                        case EmailType.AccessTokenExpiredEmailForPipelineUser:
                            emailUserName = Config.EmailUserNameForPipelineUser;

                            emailFromName = Config.EmailFromNameForPipelineUser;

                            htmlView = this.CreateAlternateViewForAccessTokenExpiredEmailForPipelineUser();
                            break;

                        case EmailType.TrialPeriodExpiredEmail:
                            emailUserName = Config.EmailUserName;

                            htmlView = this.CreateAlternateViewForTrialPeriodExpiredEmail();
                            break;

                        case EmailType.OFunnelServerError:
                            emailUserName = Config.EmailUserName;

                            htmlView = this.CreateAlternateViewForOFunnelServerErrorEmail();
                            break;
                    }

                    MailMessage message = new MailMessage();
                    SmtpClient smtpClient = new SmtpClient();
                    MailAddress fromAddress = new MailAddress(emailUserName, emailFromName);
                    message.From = fromAddress;

 
                    if (HelperMethods.IsValidEmailId(toEmailId))
                    {
                        message.To.Add(toEmailId.Trim());

                        if (!string.IsNullOrEmpty(subject))
                        {
                            message.Subject = subject;
                        }

                        message.IsBodyHtml = true;

                        if (HelperMethods.IsValidEmailId(ccEmailId))
                        {
                            message.CC.Add(ccEmailId.Trim());
                        }

                        message.AlternateViews.Add(htmlView);

                        smtpClient.Host = emailHost;
                        smtpClient.Port = emailPort;
                        smtpClient.UseDefaultCredentials = true;
                        smtpClient.Credentials = new System.Net.NetworkCredential(emailUserName, emailPassword);
                        smtpClient.Send(message);
                        isMailSentSuccessfully = true;  
                    }
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs("SendMailToAllUsers: Send Email to user failed. Exception: " + ex.Message);
            }

            return isMailSentSuccessfully;
        }

        /// <summary>
        /// This method send emails to multiple to or cc users.
        /// </summary>
        /// <param name="toEmailIds">toEmailIds</param>
        /// <param name="ccEmailIds">ccEmailIds</param>
        /// <param name="subject">subject</param>
        /// <param name="emailType">emailType</param>
        /// <returns>Email send status true/false</returns>
        public bool SendMailToAllUsers(string[] toEmailIds, string[] ccEmailIds, string[] bccEmailIds, string subject, EmailType emailType)
        {
            bool isMailSentSuccessfully = false;
            try
            {
                HelperMethods.AddLogs("Enters in SendMailToAllUsers.");

                if (toEmailIds != null && toEmailIds.Length > 0)
                {
                    string emailUserName = string.Empty;
                    string emailPassword = Config.EmailPassword;
                    string emailFromName = Config.EmailFromName;
                    string emailHost = Config.EmailHost;
                    int emailPort = Config.EmailPort;

                    AlternateView htmlView = null;

                    switch (emailType)
                    {
                        case EmailType.OpenRequestEmail:
                            emailUserName = Config.EmailUserName;

                            htmlView = this.CreateAlternateViewForRequestStatusEmail();
                            break;

                        case EmailType.NetwrokUpdateAlertEmail:
                            emailUserName = Config.EmailUserName;

                            htmlView = this.CreateAlternateViewForNetworkUpdateAlertEmail();
                            break;

                        case EmailType.NetwrokUpdateFollowupAlertEmail:
                            emailUserName = Config.EmailUserName;

                            htmlView = this.CreateAlternateViewForNetworkUpdateFollowupAlertEmail();
                            break;

                        case EmailType.NetwrokUpdateAlertEmailForPipelineUser:
                            emailUserName = Config.EmailUserNameForPipelineUser;

                            emailFromName = Config.EmailFromNameForPipelineUser;

                            htmlView = this.CreateAlternateViewForNetworkUpdateAlertEmailForPipelineUser();
                            break;

                        case EmailType.AccessTokenExpiredEmail:
                            emailUserName = Config.EmailUserName;

                            htmlView = this.CreateAlternateViewForAccessTokenExpiredEmail();
                            break;

                        case EmailType.AccessTokenExpiredEmailForPipelineUser:
                            emailUserName = Config.EmailUserNameForPipelineUser;

                            emailFromName = Config.EmailFromNameForPipelineUser;

                            htmlView = this.CreateAlternateViewForAccessTokenExpiredEmailForPipelineUser();
                            break;

                        case EmailType.TrialPeriodExpiredEmail:
                            emailUserName = Config.EmailUserName;

                            htmlView = this.CreateAlternateViewForTrialPeriodExpiredEmail();
                            break;

                        case EmailType.OFunnelServerError:
                            emailUserName = Config.EmailUserName;

                            htmlView = this.CreateAlternateViewForOFunnelServerErrorEmail();
                            break;
                    }

                    MailMessage message = new MailMessage();
                    SmtpClient smtpClient = new SmtpClient();
                    MailAddress fromAddress = new MailAddress(emailUserName, emailFromName);
                    message.From = fromAddress;

                    foreach (string toEmailId in toEmailIds)
                    {
                        if (HelperMethods.IsValidEmailId(toEmailId))
                        {
                            message.To.Add(toEmailId.Trim());
                        }
                    }

                    if (!string.IsNullOrEmpty(subject))
                    {
                        message.Subject = subject;
                    }

                    message.IsBodyHtml = true;

                    if (ccEmailIds != null && ccEmailIds.Length > 0)
                    {
                        foreach (string ccEmailId in ccEmailIds)
                        {
                            if (HelperMethods.IsValidEmailId(ccEmailId))
                            {
                                message.CC.Add(ccEmailId.Trim());
                            }
                        }
                    }

                    if (bccEmailIds != null && bccEmailIds.Length > 0)
                    {
                        foreach (string bccEmailId in bccEmailIds)
                        {
                            if (HelperMethods.IsValidEmailId(bccEmailId))
                            {
                                message.Bcc.Add(bccEmailId.Trim());
                            }
                        }
                    }

                    if (message.To.Count > 0)
                    {
                        message.AlternateViews.Add(htmlView);

                        smtpClient.Host = emailHost;
                        smtpClient.Port = emailPort;
                        smtpClient.UseDefaultCredentials = true;
                        smtpClient.Credentials = new System.Net.NetworkCredential(emailUserName, emailPassword);
                        smtpClient.Send(message);
                        isMailSentSuccessfully = true;
                    }
                }
            }
            catch (Exception ex)
            {
                HelperMethods.AddLogs("Failed to send mail to user. Exception: " + ex.Message);
            }

            HelperMethods.AddLogs("Exist from SendMailToAllUsers.");

            return isMailSentSuccessfully;
        }

        /// <summary>
        /// This method sets user details which is required to send request status email.
        /// </summary>
        /// <param name="nameValuesCollection">nameValuesCollection</param>
        public void SetUsersDetailsToSendEmail(NameValueCollection nameValuesCollection)
        {
            this.nameValues = nameValuesCollection;
        }

        # region AlternetView for request status email

        /// <summary>
        /// This method creates alternate view for request status email.
        /// </summary>
        /// <returns>Alternate view for request status email.</returns>
        private AlternateView CreateAlternateViewForRequestStatusEmail()
        {
            //Get applicaiton path

            //CommandLine without the first and last two characters
            //Path.GetDirectory seems to have some difficulties with these (special chars maybe?)

            //string cmdLine = Environment.CommandLine.Remove(Environment.CommandLine.Length - 2, 2).Remove(0, 1);
            
            //string workDir = Path.GetDirectoryName(cmdLine);
            
            //string loc = System.Reflection.Assembly.GetEntryAssembly().Location;

            //string path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            var appPath = HelperMethods.GetExeDir();

            //Get the html file for request address
            var bodyFile = Path.Combine(appPath, @"EmailTemplates\OpenRequestEmailTemplate.html");
            //Read contents of HTML file
            StreamReader dataStreamReader = File.OpenText(bodyFile);

            string mailBody = dataStreamReader.ReadToEnd();

            dataStreamReader.Close();

            // To User Data
            mailBody = mailBody.Replace(":USER_NAME", nameValues["userName"]);

            mailBody = mailBody.Replace(":SEE_ALL_REQUESTS_LINK", Config.SeeAllRequestUrl);

            mailBody = mailBody.Replace(":USER_DETAILS", requestSection);
            mailBody = mailBody.Replace(":ARTICLE_DATA", this.articleSection);

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailBody, null, "text/html");

            LinkedResource fromUserProfilePicUrl = new LinkedResource(appPath + @"\Assets\user-photo.jpg", "image/jpg");
            fromUserProfilePicUrl.ContentId = "fromUserProfilePicUrl";
            fromUserProfilePicUrl.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(fromUserProfilePicUrl);

            LinkedResource forUserImage = new LinkedResource(appPath + @"\Assets\user-photo.jpg", "image/jpg");
            forUserImage.ContentId = "forUserImage";
            forUserImage.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(forUserImage);

            LinkedResource forUserProfilePicUrl = new LinkedResource(appPath + @"\Assets\open-request.png", "image/png");
            forUserProfilePicUrl.ContentId = "forUserProfilePicUrl";
            forUserProfilePicUrl.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(forUserProfilePicUrl);

            LinkedResource emailLogoImagelink = new LinkedResource(appPath + @"\Assets\logo_email.jpg", "image/jpg");
            emailLogoImagelink.ContentId = "emailLogo";
            emailLogoImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(emailLogoImagelink);

            LinkedResource faceBookiconImagelink = new LinkedResource(appPath + @"\Assets\f-icon.jpg", "image/jpg");
            faceBookiconImagelink.ContentId = "faceBookicon";
            faceBookiconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(faceBookiconImagelink);

            LinkedResource twitterIconImagelink = new LinkedResource(appPath + @"\Assets\t-icon.jpg", "image/jpg");
            twitterIconImagelink.ContentId = "twitterIcon";
            twitterIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(twitterIconImagelink);

            LinkedResource linkedInIconImagelink = new LinkedResource(appPath + @"\Assets\in-icon.jpg", "image/jpg");
            linkedInIconImagelink.ContentId = "linkedInIcon";
            linkedInIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(linkedInIconImagelink);

            return htmlView;
        }

        /// <summary>
        /// This method creates open request section for email Templates.
        /// </summary>
        /// <param name="openRequestDetails"></param>
        public void CreateOpenRequestSectionForEmailTemplate(OpenRequestDetails[] openRequestDetails)
        {
            if (openRequestDetails != null && openRequestDetails.Length > 0)
            {
                bool colorStatus = true;

                foreach (OpenRequestDetails requestDetails in openRequestDetails)
                {
                    string requestSectionTemplate = string.Empty;
                    if (requestDetails.forUserId != -1)
                    {
                        requestSectionTemplate = Constants.OpenRequestSectionForUserAvailable;
                        requestSectionTemplate = requestSectionTemplate.Replace(":FOR_USER_NAME", requestDetails.forUserName);
                        requestSectionTemplate = requestSectionTemplate.Replace(":FOR_USER_HEADLINE", requestDetails.forUserHeadline);
                        requestSectionTemplate = requestSectionTemplate.Replace(":FOR_USER_COMPANY", requestDetails.forUserCompany);

                        if (requestDetails.forUserScore != -1)
                        {
                            requestSectionTemplate = requestSectionTemplate.Replace(":FOR_USER_SCORE_TEMPLATE", Constants.ForUserScoreTemplate);
                            requestSectionTemplate = requestSectionTemplate.Replace(":FOR_USER_SCORE", Convert.ToString(requestDetails.forUserScore));
                        }
                        else
                        {
                            requestSectionTemplate = requestSectionTemplate.Replace(":FOR_USER_SCORE_TEMPLATE", string.Empty);
                        }

                        if (!string.IsNullOrEmpty(requestDetails.forUserProfilePicUrl))
                        {
                            requestSectionTemplate = requestSectionTemplate.Replace("cid:forUserProfilePicUrl", requestDetails.forUserProfilePicUrl);
                        }
                        else
                        {
                            requestSectionTemplate = requestSectionTemplate.Replace("cid:forUserProfilePicUrl", "cid:forUserImage");
                        }
                    }
                    else
                    { 
                        requestSectionTemplate = Constants.OpenRequestSectionForUserNotAvailable;
                        requestSectionTemplate = requestSectionTemplate.Replace(":ANYONE", Constants.Anyone);
                    }

                    if (colorStatus)
                    {
                        requestSectionTemplate = requestSectionTemplate.Replace(":SECTION_BACKGROUND_COLOR", Config.WhiteSectionColor);
                        colorStatus = false;
                    }
                    else
                    {
                        requestSectionTemplate = requestSectionTemplate.Replace(":SECTION_BACKGROUND_COLOR", Config.BlueSectionColor);
                        colorStatus = true;
                    }

                    string particularRequestUrl = Config.SeeParticularRequestUrl.Replace(":REQUESTID", Convert.ToString(requestDetails.requestId));
                    requestSectionTemplate = requestSectionTemplate.Replace(":SEE_PARTICULAR_REQUEST_LINK", particularRequestUrl);

                    requestSectionTemplate = requestSectionTemplate.Replace(":FROM_USER_NAME", requestDetails.fromUserName);
                    requestSectionTemplate = requestSectionTemplate.Replace(":FROM_USER_HEADLINE", requestDetails.fromUserHeadline);
                    requestSectionTemplate = requestSectionTemplate.Replace(":FROM_USER_COMPANY", requestDetails.fromUserCompany);

                    if (requestDetails.fromUserScore != -1)
                    {
                        requestSectionTemplate = requestSectionTemplate.Replace(":FROM_USER_SCORE_TEMPLATE", Constants.FromUserScoreTemplate);
                        requestSectionTemplate = requestSectionTemplate.Replace(":FROM_USER_SCORE", Convert.ToString(requestDetails.fromUserScore));
                    }
                    else
                    {
                        requestSectionTemplate = requestSectionTemplate.Replace(":FROM_USER_SCORE_TEMPLATE", string.Empty);
                    }

                    if (!string.IsNullOrEmpty(requestDetails.fromUserProfilePicUrl))
                    {
                        requestSectionTemplate = requestSectionTemplate.Replace("cid:fromUserProfilePicUrl", requestDetails.fromUserProfilePicUrl);
                    }

                    requestSectionTemplate = requestSectionTemplate.Replace(":SEARCH_QUERY", requestDetails.querySearched);
                    requestSectionTemplate = requestSectionTemplate.Replace(":MESSAGE_CONTENT", requestDetails.content);

                    requestSection += requestSectionTemplate;
                }
            }
        }

        /// <summary>
        /// This method creates article section for email Templates.
        /// </summary>
        /// <param name="articleDetails">articleDetails</param>
        public void CreateArticleSectionForEmailTemplate(AllArticles allArticles)
        {
            if (allArticles != null && allArticles.article != null && allArticles.article.Length > 0)
            {
                foreach (Article article in allArticles.article)
                {
                    string articleSectionTemplate = Constants.ArticalDetails;

                    articleSectionTemplate = articleSectionTemplate.Replace(":ARTICLE_HEADLINE", article.headline);
                    articleSectionTemplate = articleSectionTemplate.Replace(":ARTICLE_SUMMARY", article.summary);
                    articleSectionTemplate = articleSectionTemplate.Replace(":FULL_ARTICLE_LINK", article.articleUrl);

                    this.articleSection += articleSectionTemplate;
                }
            }
        }

        #endregion


        # region Create alternate view for network update email.

        /// <summary>
        /// This method creates alternate view for netwrok update Alert Email.
        /// </summary>
        /// <returns>Alternate view for Connection Alert Email.</returns>
        private AlternateView CreateAlternateViewForNetworkUpdateAlertEmail()
        {
            var appPath = HelperMethods.GetExeDir();

            //Get the html file for request address
            var bodyFile = Path.Combine(appPath, @"EmailTemplates\ConnectionAlertEmailTemplate.html");
            //Read contents of HTML file
            StreamReader dataStreamReader = File.OpenText(bodyFile);

            string mailBody = dataStreamReader.ReadToEnd();

            dataStreamReader.Close();

            if (!string.IsNullOrEmpty(this.networkUpdateSection))
            {
                mailBody = mailBody.Replace(":NETWORK_UPDATE_DETAILS", Constants.NetworkUpdateDetailsHeader);
                mailBody = mailBody.Replace(":NETWORK_UPDATE_SECTION", this.networkUpdateSection);
            }
            else
            {
                mailBody = mailBody.Replace(":NETWORK_UPDATE_DETAILS", string.Empty);
            }

            if (!string.IsNullOrEmpty(this.positionUpdateSection))
            {
                mailBody = mailBody.Replace(":MORE_GOOD_NEWS_DETAILS", Constants.MoreGoodNewsDetails);
                mailBody = mailBody.Replace(":POSITION_UPDATE_SECTION", this.positionUpdateSection);
            }
            else
            {
                mailBody = mailBody.Replace(":MORE_GOOD_NEWS_DETAILS", string.Empty);
            }

            string allOtherTypeNetworkUpdateDetails = Convert.ToString(allOtherTypeNetworkUpdatesSection);

            string allOtherNetworkUupdateSection = Constants.AllOtherNetworkUupdateSection;

            if (!string.IsNullOrEmpty(allOtherTypeNetworkUpdateDetails))
            {
                allOtherNetworkUupdateSection = allOtherNetworkUupdateSection.Replace(":ALL_SECTION_UPDATES", allOtherTypeNetworkUpdateDetails);
                mailBody = mailBody.Replace(":ALL_OTHER_NETWORK_UPDATE_SECTION", allOtherNetworkUupdateSection);
            }
            else
            {
                mailBody = mailBody.Replace(":ALL_OTHER_NETWORK_UPDATE_SECTION", string.Empty);
            }

            HelperMethods.AddLogs(string.Format("CreateAlternateViewForNetworkUpdateAlertEmail: Twitter lead data for userId = {0}, Lead Data = {1}.", nameValues["userId"], Convert.ToString(twitterAllLeadSection)));

            string twitterLeadDetails = Convert.ToString(twitterAllLeadSection);
            mailBody = mailBody.Replace(":TWITTER_LEAD_DETAILS", twitterLeadDetails);

            //Kushal Additions
            mailBody = mailBody.Replace(":USER_IDS", nameValues["userId"]);
            //This is for A/B testing
            if (Convert.ToInt32(nameValues["userId"]) % 2 == 0) 
            {
                mailBody = mailBody.Replace("Want to know more about this connection? Ask us", "Is this connection of Interest? Tell us");
            }

            string unsubscribeFromAlertsUrl = Config.UnsubscribeFromAlertsUrl;

            unsubscribeFromAlertsUrl = unsubscribeFromAlertsUrl.Replace(":USER_ID", nameValues["userId"]);

            mailBody = mailBody.Replace(":UNSUBSCRIBE_LINK", unsubscribeFromAlertsUrl);

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailBody, null, "text/html");

            LinkedResource voteNowLogoImagelink = new LinkedResource(appPath + @"\Assets\votenow-icon.png", "image/png");
            voteNowLogoImagelink.ContentId = "voteNowLogo";
            voteNowLogoImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(voteNowLogoImagelink);

            LinkedResource emailLogoImagelink = new LinkedResource(appPath + @"\Assets\logo_email.jpg", "image/jpg");
            emailLogoImagelink.ContentId = "emailLogo";
            emailLogoImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(emailLogoImagelink);


            LinkedResource userImageForYourConnection = new LinkedResource(appPath + @"\Assets\user-photo.jpg", "image/jpg");
            userImageForYourConnection.ContentId = "yourConnectionProfilePicUrl";
            userImageForYourConnection.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(userImageForYourConnection);
            

            LinkedResource userImageForConnectedTo = new LinkedResource(appPath + @"\Assets\user-photo.jpg", "image/jpg");
            userImageForConnectedTo.ContentId = "connectedToProfilePicUrl";
            userImageForConnectedTo.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(userImageForConnectedTo);
            

            LinkedResource faceBookiconImagelink = new LinkedResource(appPath + @"\Assets\f-icon.jpg", "image/jpg");
            faceBookiconImagelink.ContentId = "faceBookicon";
            faceBookiconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(faceBookiconImagelink);

            LinkedResource twitterIconImagelink = new LinkedResource(appPath + @"\Assets\t-icon.jpg", "image/jpg");
            twitterIconImagelink.ContentId = "twitterIcon";
            twitterIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(twitterIconImagelink);

            LinkedResource linkedInIconImagelink = new LinkedResource(appPath + @"\Assets\in-icon.jpg", "image/jpg");
            linkedInIconImagelink.ContentId = "linkedInIcon";
            linkedInIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(linkedInIconImagelink);

            return htmlView;
        }

        /// <summary>
        /// This method creates alternate view for netwrok update followup Alert Email.
        /// </summary>
        /// <returns>Alternate view for Connection Alert Email.</returns>
        private AlternateView CreateAlternateViewForNetworkUpdateFollowupAlertEmail()
        {
            var appPath = HelperMethods.GetExeDir();

            //Get the html file for request address
            var bodyFile = Path.Combine(appPath, @"EmailTemplates\ConnectionFollowupAlertEmailTemplate.html");
            //Read contents of HTML file
            StreamReader dataStreamReader = File.OpenText(bodyFile);

            string mailBody = dataStreamReader.ReadToEnd();

            dataStreamReader.Close();

            if (!string.IsNullOrEmpty(this.networkUpdateSection))
            {
                mailBody = mailBody.Replace(":NETWORK_UPDATE_DETAILS", Constants.NetworkUpdateDetailsHeader);
                mailBody = mailBody.Replace(":NETWORK_UPDATE_SECTION", this.networkUpdateSection);
            }
            else
            {
                mailBody = mailBody.Replace(":NETWORK_UPDATE_DETAILS", string.Empty);
            }

            if (!string.IsNullOrEmpty(this.positionUpdateSection))
            {
                mailBody = mailBody.Replace(":MORE_GOOD_NEWS_DETAILS", Constants.MoreGoodNewsDetails);
                mailBody = mailBody.Replace(":POSITION_UPDATE_SECTION", this.positionUpdateSection);
            }
            else
            {
                mailBody = mailBody.Replace(":MORE_GOOD_NEWS_DETAILS", string.Empty);
            }

            if (string.IsNullOrEmpty(this.networkUpdateSection) && string.IsNullOrEmpty(this.positionUpdateSection))
            {
                mailBody = mailBody.Replace(":FOLLOWUP_HEADING", string.Empty);
            }
            else
            {
                mailBody = mailBody.Replace(":FOLLOWUP_HEADING", Constants.FollowUpHeader);
            }

            string twitterLeadDetails = Convert.ToString(twitterAllLeadSection);
            mailBody = mailBody.Replace(":TWITTER_LEAD_DETAILS", twitterLeadDetails);

            mailBody = mailBody.Replace(":USER_IDS", nameValues["userId"]);

            mailBody = mailBody.Replace(":LOCATION_NETWORK_EXPAND_STATISTICS", this.networkExpandForLocationSection);
            mailBody = mailBody.Replace(":SUBINDUSTRY_NETWORK_EXPAND_STATISTICS", this.networkExpandForSubIndustrySection);

            string unsubscribeFromAlertsUrl = Config.UnsubscribeFromAlertsUrl;

            unsubscribeFromAlertsUrl = unsubscribeFromAlertsUrl.Replace(":USER_ID", nameValues["userId"]);

            mailBody = mailBody.Replace(":UNSUBSCRIBE_LINK", unsubscribeFromAlertsUrl);

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailBody, null, "text/html");

            LinkedResource voteNowLogoImagelink = new LinkedResource(appPath + @"\Assets\votenow-icon.png", "image/png");
            voteNowLogoImagelink.ContentId = "voteNowLogo";
            voteNowLogoImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(voteNowLogoImagelink);

            LinkedResource emailLogoImagelink = new LinkedResource(appPath + @"\Assets\logo_email.jpg", "image/jpg");
            emailLogoImagelink.ContentId = "emailLogo";
            emailLogoImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(emailLogoImagelink);


            LinkedResource userImageForYourConnection = new LinkedResource(appPath + @"\Assets\user-photo.jpg", "image/jpg");
            userImageForYourConnection.ContentId = "yourConnectionProfilePicUrl";
            userImageForYourConnection.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(userImageForYourConnection);


            LinkedResource userImageForConnectedTo = new LinkedResource(appPath + @"\Assets\user-photo.jpg", "image/jpg");
            userImageForConnectedTo.ContentId = "connectedToProfilePicUrl";
            userImageForConnectedTo.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(userImageForConnectedTo);


            LinkedResource faceBookiconImagelink = new LinkedResource(appPath + @"\Assets\f-icon.jpg", "image/jpg");
            faceBookiconImagelink.ContentId = "faceBookicon";
            faceBookiconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(faceBookiconImagelink);

            LinkedResource twitterIconImagelink = new LinkedResource(appPath + @"\Assets\t-icon.jpg", "image/jpg");
            twitterIconImagelink.ContentId = "twitterIcon";
            twitterIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(twitterIconImagelink);

            LinkedResource linkedInIconImagelink = new LinkedResource(appPath + @"\Assets\in-icon.jpg", "image/jpg");
            linkedInIconImagelink.ContentId = "linkedInIcon";
            linkedInIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(linkedInIconImagelink);

            return htmlView;
        }

        /// <summary>
        /// This method creates alternate view for netwrok update Alert Email for pipeline users.
        /// </summary>
        /// <returns>Alternate view for Connection Alert Email.</returns>
        private AlternateView CreateAlternateViewForNetworkUpdateAlertEmailForPipelineUser()
        {
            var appPath = HelperMethods.GetExeDir();

            //Get the html file for request address
            var bodyFile = Path.Combine(appPath, @"EmailTemplates\ConnectionAlertEmailForPipelineUserTemplate.html");
            //Read contents of HTML file
            StreamReader dataStreamReader = File.OpenText(bodyFile);

            string mailBody = dataStreamReader.ReadToEnd();

            dataStreamReader.Close();

            if (!string.IsNullOrEmpty(this.networkUpdateSection))
            {
                mailBody = mailBody.Replace(":NETWORK_UPDATE_DETAILS", Constants.NetworkUpdateDetailsHeader);
                mailBody = mailBody.Replace(":NETWORK_UPDATE_SECTION", this.networkUpdateSection);
            }
            else
            {
                mailBody = mailBody.Replace(":NETWORK_UPDATE_DETAILS", string.Empty);
            }

            if (!string.IsNullOrEmpty(this.positionUpdateSection))
            {
                mailBody = mailBody.Replace(":MORE_GOOD_NEWS_DETAILS", Constants.MoreGoodNewsDetails);
                mailBody = mailBody.Replace(":POSITION_UPDATE_SECTION", this.positionUpdateSection);
            }
            else
            {
                mailBody = mailBody.Replace(":MORE_GOOD_NEWS_DETAILS", string.Empty);
            }

            //Kushal Additions
            mailBody = mailBody.Replace(":USER_IDS", nameValues["userId"]);
            //This is for A/B testing
            if (Convert.ToInt32(nameValues["userId"]) % 2 == 0) 
            {
                mailBody = mailBody.Replace("Want to know more about this connection? Ask us", "Is this connection of Interest? Tell us");
            }

            string unsubscribeFromAlertsUrl = Config.UnsubscribeFromAlertsUrl;

            unsubscribeFromAlertsUrl = unsubscribeFromAlertsUrl.Replace(":USER_ID", nameValues["userId"]);

            mailBody = mailBody.Replace(":UNSUBSCRIBE_LINK", unsubscribeFromAlertsUrl);

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailBody, null, "text/html");

            LinkedResource voteNowLogoImagelink = new LinkedResource(appPath + @"\Assets\votenow-icon.png", "image/png");
            voteNowLogoImagelink.ContentId = "voteNowLogo";
            voteNowLogoImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(voteNowLogoImagelink);

            LinkedResource emailLogoImagelink = new LinkedResource(appPath + @"\Assets\pipelinedeal_logo.jpg", "image/jpg");
            emailLogoImagelink.ContentId = "emailLogo";
            emailLogoImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(emailLogoImagelink);


            LinkedResource userImageForYourConnection = new LinkedResource(appPath + @"\Assets\user-photo.jpg", "image/jpg");
            userImageForYourConnection.ContentId = "yourConnectionProfilePicUrl";
            userImageForYourConnection.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(userImageForYourConnection);
            

            LinkedResource userImageForConnectedTo = new LinkedResource(appPath + @"\Assets\user-photo.jpg", "image/jpg");
            userImageForConnectedTo.ContentId = "connectedToProfilePicUrl";
            userImageForConnectedTo.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(userImageForConnectedTo);
            

            LinkedResource faceBookiconImagelink = new LinkedResource(appPath + @"\Assets\f-icon.jpg", "image/jpg");
            faceBookiconImagelink.ContentId = "faceBookicon";
            faceBookiconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(faceBookiconImagelink);

            LinkedResource twitterIconImagelink = new LinkedResource(appPath + @"\Assets\t-icon.jpg", "image/jpg");
            twitterIconImagelink.ContentId = "twitterIcon";
            twitterIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(twitterIconImagelink);

            LinkedResource linkedInIconImagelink = new LinkedResource(appPath + @"\Assets\in-icon.jpg", "image/jpg");
            linkedInIconImagelink.ContentId = "linkedInIcon";
            linkedInIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(linkedInIconImagelink);

            return htmlView;
        }
        
        /// <summary>
        /// This method creates network update section for network updxate email Templates.
        /// </summary>
        /// <param name="openRequestDetails"></param>
        public bool CreateNetworkUpdateSectionForEmailTemplate(NetworkAlertsForAlertType[] networkAlertsForAlertTypeArray)
        {
            bool isUpdatedConnectionFound = false;

            if (networkAlertsForAlertTypeArray != null && networkAlertsForAlertTypeArray.Length > 0)
            {
                foreach (NetworkAlertsForAlertType networkAlertsForAlertType in networkAlertsForAlertTypeArray)
                {
                    string alertType = networkAlertsForAlertType.alertType;
                    string alertTypeForNetworkUpdate = string.Empty;

                    switch (alertType.ToUpper())
                    { 
                        case "COMPANY":
                            alertTypeForNetworkUpdate = Constants.TargetCompany;
                            break;
                        case "ROLE":
                            alertTypeForNetworkUpdate = Constants.TargetRole;
                            break;
                        case "PERSON":
                            alertTypeForNetworkUpdate = Constants.TargetPerson;
                            break;
                        case "POSITION":
                            alertTypeForNetworkUpdate = Constants.TargetCompany;
                            break;
                        case "POSITIONROLE":
                            alertTypeForNetworkUpdate = Constants.TargetRole;
                            break;
                        case "POSITIONPERSON":
                            alertTypeForNetworkUpdate = Constants.TargetPerson;
                            break;
                        case "KEYWORD":
                            alertTypeForNetworkUpdate = Constants.TargetKeyword;
                            break;
                    }

                    if (networkAlertsForAlertType.networkAlerts != null && networkAlertsForAlertType.networkAlerts.Length > 0)
                    {
                        foreach (NetworkAlerts networkAlert in networkAlertsForAlertType.networkAlerts)
                        {
                            string targetName = networkAlert.targetName;
                            string alertTypeAndTargetForNetworkUpdate = alertTypeForNetworkUpdate + targetName;

                            string networkUpdateSectionTemplate = string.Empty;
                            string positionUpdateSectionTemplate = string.Empty;

                            if (!alertType.ToUpper().Equals("POSITION") && !alertType.ToUpper().Equals("POSITIONROLE") && !alertType.ToUpper().Equals("POSITIONPERSON"))
                            {
                                networkUpdateSectionTemplate = Constants.NetworkUpdateSection;
                                networkUpdateSectionTemplate = networkUpdateSectionTemplate.Replace(":NETWORK_UPDATE_TYPE_AND_TARGET_NAME", alertTypeAndTargetForNetworkUpdate);
                            }
                            else if (alertType.ToUpper().Equals("POSITION"))
                            {
                                positionUpdateSectionTemplate = Constants.PositionUpdateSection;
                                positionUpdateSectionTemplate = positionUpdateSectionTemplate.Replace(":POSITION_UPDATE_TYPE_AND_TARGET_NAME", alertTypeAndTargetForNetworkUpdate);
                            }
                            else if (alertType.ToUpper().Equals("POSITIONROLE"))
                            {
                                positionUpdateSectionTemplate = Constants.PositionUpdateSection;
                                positionUpdateSectionTemplate = positionUpdateSectionTemplate.Replace(":POSITION_UPDATE_TYPE_AND_TARGET_NAME", alertTypeAndTargetForNetworkUpdate);
                            }
                            else if (alertType.ToUpper().Equals("POSITIONPERSON"))
                            {
                                positionUpdateSectionTemplate = Constants.PositionUpdateSection;
                                positionUpdateSectionTemplate = positionUpdateSectionTemplate.Replace(":POSITION_UPDATE_TYPE_AND_TARGET_NAME", alertTypeAndTargetForNetworkUpdate);
                            }

                            if (networkAlert.networkAlertDetails != null && networkAlert.networkAlertDetails.Length > 0)
                            {
                                string netwrokUpdateDetails = string.Empty;
                                string positionUpdateDetails = string.Empty;

                                foreach (NetworkAlertDetails networkAlertDetail in networkAlert.networkAlertDetails)
                                {
                                    if (!string.IsNullOrEmpty(networkAlertDetail.yourConnectionLinkedInId))
                                    {
                                        isUpdatedConnectionFound = true;

                                        string yourConnectionJobTitle = networkAlertDetail.yourConnectionHeadline;
                                        if (!string.IsNullOrEmpty(yourConnectionJobTitle) && !string.IsNullOrEmpty(networkAlertDetail.yourConnectionCompany))
                                        {
                                            yourConnectionJobTitle += ", " + networkAlertDetail.yourConnectionCompany;
                                        }
                                        else
                                        {
                                            yourConnectionJobTitle += networkAlertDetail.yourConnectionCompany;
                                        }

                                        yourConnectionJobTitle = HelperMethods.FormatYourConnectionJobTitle(yourConnectionJobTitle);

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

                                        if (!alertType.ToUpper().Equals("POSITION") && !alertType.ToUpper().Equals("POSITIONROLE") && !alertType.ToUpper().Equals("POSITIONPERSON"))
                                        {
                                            string tempNetwrokUpdateDetails = Constants.NetwrokUpdateDetails;

                                            tempNetwrokUpdateDetails = tempNetwrokUpdateDetails.Replace(":YOUR_CONNECTION_FULL_NAME", networkAlertDetail.yourConnectionFirstName + " " + networkAlertDetail.yourConnectionLastName);

                                            tempNetwrokUpdateDetails = tempNetwrokUpdateDetails.Replace(":YOUR_CONNECTION_JOB_TITLE", yourConnectionJobTitle);

                                            tempNetwrokUpdateDetails = tempNetwrokUpdateDetails.Replace(":CONNECTED_TO_FULL_NAME", networkAlertDetail.connectedToFirstName + " " + networkAlertDetail.connectedToLastName);

                                            string connectedToJobTitle = networkAlertDetail.connectedToHeadline;

                                            if (!string.IsNullOrEmpty(connectedToJobTitle) && !string.IsNullOrEmpty(networkAlertDetail.connectedToCompany))
                                            {
                                                connectedToJobTitle += ", " + networkAlertDetail.connectedToCompany;
                                            }
                                            else
                                            {
                                                connectedToJobTitle += networkAlertDetail.connectedToCompany;
                                            }

                                            connectedToJobTitle = HelperMethods.FormatYourConnectionJobTitle(connectedToJobTitle);
                                            tempNetwrokUpdateDetails = tempNetwrokUpdateDetails.Replace(":CONNECTED_TO_JOB_TITLE", connectedToJobTitle);

                                            if (!string.IsNullOrEmpty(yourConnectionProfileUrl))
                                            {
                                                tempNetwrokUpdateDetails = tempNetwrokUpdateDetails.Replace(":YOUR_CONNECTION_PROFILE_URL", yourConnectionProfileUrl);
                                            }
                                            else
                                            {
                                                tempNetwrokUpdateDetails = tempNetwrokUpdateDetails.Replace(":YOUR_CONNECTION_PROFILE_URL", "#");
                                            }

                                            if (!string.IsNullOrEmpty(connectedToProfileUrl))
                                            {
                                                tempNetwrokUpdateDetails = tempNetwrokUpdateDetails.Replace(":CONNECTED_TO_PROFILE_URL", connectedToProfileUrl);
                                            }
                                            else
                                            {
                                                tempNetwrokUpdateDetails = tempNetwrokUpdateDetails.Replace(":CONNECTED_TO_PROFILE_URL", "#");
                                            }

                                            if (!string.IsNullOrEmpty(yourConnectionProfilePicUrl))
                                            {
                                                tempNetwrokUpdateDetails = tempNetwrokUpdateDetails.Replace("cid:yourConnectionProfilePicUrl", yourConnectionProfilePicUrl);
                                            }

                                            string connectedToProfilePicUrl = networkAlertDetail.connectedToProfilePicUrl;

                                            if (!string.IsNullOrEmpty(connectedToProfilePicUrl))
                                            {
                                                tempNetwrokUpdateDetails = tempNetwrokUpdateDetails.Replace("cid:connectedToProfilePicUrl", connectedToProfilePicUrl);
                                            }

                                            netwrokUpdateDetails += tempNetwrokUpdateDetails;

                                            //Kushal additions
                                            string getMoreDataLink = Config.GetMoreDataUrl;
                                            getMoreDataLink = getMoreDataLink.Replace(":CONNECTED_TO", networkAlertDetail.connectedToLinkedInId);
                                            getMoreDataLink = getMoreDataLink.Replace(":YOUR_CONNECTION", networkAlertDetail.yourConnectionLinkedInId);
                                            getMoreDataLink = getMoreDataLink.Replace(":NETWORK_UPDATE_ID", networkAlertDetail.networkUpdateId);
                                            
                                            netwrokUpdateDetails = netwrokUpdateDetails.Replace(":GET_MORE_DATA_LINK", getMoreDataLink);
                                            netwrokUpdateDetails = netwrokUpdateDetails.Replace(":NEW_CONNECTION", networkAlertDetail.connectedToFirstName);

                                        }

                                        if (alertType.ToUpper().Equals("POSITION") || alertType.ToUpper().Equals("POSITIONROLE") || alertType.ToUpper().Equals("POSITIONPERSON"))
                                        {
                                            string tempPositionUpdateDetails = Constants.PositionUpdateDetails;

                                            if (!string.IsNullOrEmpty(yourConnectionProfilePicUrl))
                                            {
                                                tempPositionUpdateDetails = tempPositionUpdateDetails.Replace("cid:connectionProfilePicUrl", yourConnectionProfilePicUrl);
                                            }

                                            if (!string.IsNullOrEmpty(yourConnectionProfileUrl))
                                            {
                                                tempPositionUpdateDetails = tempPositionUpdateDetails.Replace(":CONNECTION_PROFILE_URL", yourConnectionProfileUrl);
                                            }
                                            else
                                            {
                                                tempPositionUpdateDetails = tempPositionUpdateDetails.Replace(":CONNECTION_PROFILE_URL", "#");
                                            }

                                            tempPositionUpdateDetails = tempPositionUpdateDetails.Replace(":CONNECTION_FULL_NAME", networkAlertDetail.yourConnectionFirstName + " " + networkAlertDetail.yourConnectionLastName);

                                            tempPositionUpdateDetails = tempPositionUpdateDetails.Replace(":CONNECTION_JOB_TITLE", yourConnectionJobTitle);

                                            positionUpdateDetails += tempPositionUpdateDetails;
                                        }
                                    }
                                }

                                networkUpdateSectionTemplate = networkUpdateSectionTemplate.Replace(":NETWROK_UPDATE_DETAILS", netwrokUpdateDetails);
                                positionUpdateSectionTemplate = positionUpdateSectionTemplate.Replace(":POSITION_UPDATE_DETAILS", positionUpdateDetails);
                            }

                            this.networkUpdateSection += networkUpdateSectionTemplate;
                            this.positionUpdateSection += positionUpdateSectionTemplate;
                        }
                    }
                }
            }

            return isUpdatedConnectionFound;
        }


        /// <summary>
        /// This method creates network update section for network update type CMPY, JGRP, SHAR, PICU, PROF, PREC, VIRL.
        /// </summary>
        /// <param name="openRequestDetails"></param>
        public bool CreateAllOtherTypeNetworkUpdateSectionForEmailTemplate(NetworkAlertsForOtherUpdateType[] networkAlertsForOtherUpdateType)
        {
            bool isUpdatedNetworkFound = false;

            if (networkAlertsForOtherUpdateType != null && networkAlertsForOtherUpdateType.Length > 0)
            {
                foreach (NetworkAlertsForOtherUpdateType networkAlertsForAlertType in networkAlertsForOtherUpdateType)
                {
                    string alertType = networkAlertsForAlertType.alertType;
                    string alertTypeForNetworkUpdate = string.Empty;

                    alertTypeForNetworkUpdate = Constants.TargetKeyword;

                    string networkUpdateHeading = string.Empty;

                    if (networkAlertsForAlertType.networkAlerts != null && networkAlertsForAlertType.networkAlerts.Length > 0)
                    {
                        foreach (NetworkAlerts networkAlert in networkAlertsForAlertType.networkAlerts)
                        {
                            networkUpdateHeading = networkAlert.targetName;
                            if (alertType.ToUpper() == "PERSON")
                            {
                                networkUpdateHeading = Constants.TargetPerson + networkUpdateHeading;
                            }
                            else
                            {
                                networkUpdateHeading = Constants.TargetKeyword + networkUpdateHeading;
                            }

                            string headingSection = Constants.TargetHeadingName;

                            headingSection = headingSection.Replace(":KEWORD_TARGET_NAME", networkUpdateHeading);

                            string cmpyStatusUpdateSection = string.Empty;
                            string cmpyJobChangeSection = string.Empty;
                            string picuUpdateSection = string.Empty;
                            string profUpdateSection = string.Empty;
                            string prfxUpdateSection = string.Empty;
                            string precUpdateSection = string.Empty;
                            string sharUpdateSection = string.Empty;
                            string virlUpdateSection = string.Empty;
                            string jgrpUpdateSection = string.Empty;
                            

                            foreach (NetworkAlertDetails networkAlertDetails in networkAlert.networkAlertDetails)
                            {
                                string cmpyStatusUpdateTemplate = string.Empty;
                                string cmpyJobChangeTemplate = string.Empty;
                                string picuUpdateTemplate = string.Empty;
                                string profUpdateTemplate = string.Empty;
                                string prfxUpdateTemplate = string.Empty;
                                string precUpdateTemplate = string.Empty;
                                string sharUpdateTemplate = string.Empty;
                                string virlUpdateTemplate = string.Empty;
                                string jgrpUpdateTemplate = string.Empty;
                                

                                if (!string.IsNullOrEmpty(networkAlertDetails.updateType))
                                {
                                    string connectionName = string.Empty;
                                    string connectionProfileUrl = string.Empty;
                                    string connectionJobTitle = string.Empty;
                                    string connectionProfilePicUrl = string.Empty;

                                    string connectionToName = string.Empty;
                                    string connectionToProfileUrl = string.Empty;
                                    string connectionToJobTitle = string.Empty;
                                    string connectionToProfilePicUrl = string.Empty;

                                    switch (networkAlertDetails.updateType.ToUpper())
                                    {
                                        case "CMPY":
                                            if (string.IsNullOrEmpty(networkAlertDetails.jobTitle))
                                            {
                                                cmpyStatusUpdateTemplate = Constants.CmpyStatusUpdate;
                                                cmpyStatusUpdateTemplate = cmpyStatusUpdateTemplate.Replace(":COMPANY_NAME_URL", networkAlertDetails.shortenedUrl);
                                                cmpyStatusUpdateTemplate = cmpyStatusUpdateTemplate.Replace(":COMPANY_NAME", networkAlertDetails.companyName);

                                                cmpyStatusUpdateSection += cmpyStatusUpdateTemplate;
                                            }
                                            else
                                            {
                                                cmpyJobChangeTemplate = Constants.CmpyJobChange;
                                                cmpyJobChangeTemplate = cmpyJobChangeTemplate.Replace(":COMPANY_NAME", networkAlertDetails.companyName);
                                                cmpyJobChangeTemplate = cmpyJobChangeTemplate.Replace(":JOB_POSITION", networkAlertDetails.jobTitle);
                                                cmpyJobChangeTemplate = cmpyJobChangeTemplate.Replace(":POSITION_URL", networkAlertDetails.shortenedUrl);

                                                cmpyJobChangeSection += cmpyJobChangeTemplate;
                                            }
                                            break;

                                        case "PICU":
                                            picuUpdateTemplate = Constants.PicuProfilePicChange;
                                            connectionName = networkAlertDetails.yourConnectionFirstName + " " + networkAlertDetails.yourConnectionLastName;
                                            picuUpdateTemplate = picuUpdateTemplate.Replace(":YOUR_CONNECTION_FULL_NAME", connectionName);

                                            connectionProfileUrl = networkAlertDetails.yourConnectionProfileUrl;

                                            if (string.IsNullOrEmpty(connectionProfileUrl))
                                            {
                                                connectionProfileUrl = Constants.ConnectionLinkedInProfileUrl;

                                                connectionProfileUrl = connectionProfileUrl.Replace(":FIRST_NAME", networkAlertDetails.yourConnectionFirstName);
                                                connectionProfileUrl = connectionProfileUrl.Replace(":LAST_NAME", networkAlertDetails.yourConnectionLastName);
                                            }
                                            picuUpdateTemplate = picuUpdateTemplate.Replace(":YOUR_CONNECTION_PROFILE_URL", connectionProfileUrl);
                                            
                                            connectionJobTitle = networkAlertDetails.yourConnectionHeadline;
                                            if (!string.IsNullOrEmpty(connectionJobTitle) && !string.IsNullOrEmpty(networkAlertDetails.yourConnectionCompany))
                                            {
                                                connectionJobTitle += ", " + networkAlertDetails.yourConnectionCompany;
                                            }
                                            picuUpdateTemplate = picuUpdateTemplate.Replace(":YOUR_CONNECTION_JOB_TITLE", connectionJobTitle);
                                            picuUpdateTemplate = picuUpdateTemplate.Replace(":YOUR_CONNECTION_JOB_TITLE", connectionJobTitle);

                                            connectionProfilePicUrl = networkAlertDetails.yourConnectionProfilePicUrl;

                                            if (!string.IsNullOrEmpty(connectionProfilePicUrl))
                                            {
                                                picuUpdateTemplate = picuUpdateTemplate.Replace("cid:yourConnectionProfilePicUrl", connectionProfilePicUrl);
                                            }

                                            picuUpdateSection += picuUpdateTemplate;

                                            break;

                                        case "PROF":
                                            profUpdateTemplate = Constants.ProfProfileChange;
                                            connectionName = networkAlertDetails.yourConnectionFirstName + " " + networkAlertDetails.yourConnectionLastName;
                                            profUpdateTemplate = profUpdateTemplate.Replace(":YOUR_CONNECTION_FULL_NAME", connectionName);

                                            connectionProfileUrl = networkAlertDetails.yourConnectionProfileUrl;

                                            if (string.IsNullOrEmpty(connectionProfileUrl))
                                            {
                                                connectionProfileUrl = Constants.ConnectionLinkedInProfileUrl;

                                                connectionProfileUrl = connectionProfileUrl.Replace(":FIRST_NAME", networkAlertDetails.yourConnectionFirstName);
                                                connectionProfileUrl = connectionProfileUrl.Replace(":LAST_NAME", networkAlertDetails.yourConnectionLastName);
                                            }
                                            profUpdateTemplate = profUpdateTemplate.Replace(":YOUR_CONNECTION_PROFILE_URL", connectionProfileUrl);
                                            
                                            connectionJobTitle = networkAlertDetails.yourConnectionHeadline;
                                            if (!string.IsNullOrEmpty(connectionJobTitle) && !string.IsNullOrEmpty(networkAlertDetails.yourConnectionCompany))
                                            {
                                                connectionJobTitle += ", " + networkAlertDetails.yourConnectionCompany;
                                            }
                                            profUpdateTemplate = profUpdateTemplate.Replace(":YOUR_CONNECTION_JOB_TITLE", connectionJobTitle);

                                            connectionProfilePicUrl = networkAlertDetails.yourConnectionProfilePicUrl;
                                            if (!string.IsNullOrEmpty(connectionProfilePicUrl))
                                            {
                                                profUpdateTemplate = profUpdateTemplate.Replace("cid:yourConnectionProfilePicUrl", connectionProfilePicUrl);
                                            }

                                            profUpdateSection += profUpdateTemplate;
                                            break;

                                        case "PRFX":
                                            prfxUpdateTemplate = Constants.PrfxExtendedProfileChange;
                                            connectionName = networkAlertDetails.yourConnectionFirstName + " " + networkAlertDetails.yourConnectionLastName;
                                            prfxUpdateTemplate = prfxUpdateTemplate.Replace(":YOUR_CONNECTION_FULL_NAME", connectionName);

                                            connectionProfileUrl = networkAlertDetails.yourConnectionProfileUrl;

                                            if (string.IsNullOrEmpty(connectionProfileUrl))
                                            {
                                                connectionProfileUrl = Constants.ConnectionLinkedInProfileUrl;

                                                connectionProfileUrl = connectionProfileUrl.Replace(":FIRST_NAME", networkAlertDetails.yourConnectionFirstName);
                                                connectionProfileUrl = connectionProfileUrl.Replace(":LAST_NAME", networkAlertDetails.yourConnectionLastName);
                                            }
                                            prfxUpdateTemplate = prfxUpdateTemplate.Replace(":YOUR_CONNECTION_PROFILE_URL", connectionProfileUrl);
                                            
                                            connectionJobTitle = networkAlertDetails.yourConnectionHeadline;
                                            if (!string.IsNullOrEmpty(connectionJobTitle) && !string.IsNullOrEmpty(networkAlertDetails.yourConnectionCompany))
                                            {
                                                connectionJobTitle += ", " + networkAlertDetails.yourConnectionCompany;
                                            }
                                            prfxUpdateTemplate = prfxUpdateTemplate.Replace(":YOUR_CONNECTION_JOB_TITLE", connectionJobTitle);

                                            connectionProfilePicUrl = networkAlertDetails.yourConnectionProfilePicUrl;
                                            if (!string.IsNullOrEmpty(connectionProfilePicUrl))
                                            {
                                                prfxUpdateTemplate = prfxUpdateTemplate.Replace("cid:yourConnectionProfilePicUrl", connectionProfilePicUrl);
                                            }

                                            prfxUpdateSection += prfxUpdateTemplate;
                                            break;

                                        case "PREC":
                                            
                                            precUpdateTemplate = Constants.PrecRecommendation;

                                            connectionName = networkAlertDetails.yourConnectionFirstName + " " + networkAlertDetails.yourConnectionLastName;
                                            precUpdateTemplate = precUpdateTemplate.Replace(":YOUR_CONNECTION_FULL_NAME", connectionName);

                                            connectionProfileUrl = networkAlertDetails.yourConnectionProfileUrl;

                                            if (string.IsNullOrEmpty(connectionProfileUrl))
                                            {
                                                connectionProfileUrl = Constants.ConnectionLinkedInProfileUrl;

                                                connectionProfileUrl = connectionProfileUrl.Replace(":FIRST_NAME", networkAlertDetails.yourConnectionFirstName);
                                                connectionProfileUrl = connectionProfileUrl.Replace(":LAST_NAME", networkAlertDetails.yourConnectionLastName);
                                            }
                                            precUpdateTemplate = precUpdateTemplate.Replace(":YOUR_CONNECTION_PROFILE_URL", connectionProfileUrl);
                                            
                                            connectionJobTitle = networkAlertDetails.yourConnectionHeadline;
                                            if (!string.IsNullOrEmpty(connectionJobTitle) && !string.IsNullOrEmpty(networkAlertDetails.yourConnectionCompany))
                                            {
                                                connectionJobTitle += ", " + networkAlertDetails.yourConnectionCompany;
                                            }
                                            precUpdateTemplate = precUpdateTemplate.Replace(":YOUR_CONNECTION_JOB_TITLE", connectionJobTitle);

                                            connectionProfilePicUrl = networkAlertDetails.yourConnectionProfilePicUrl;
                                            if (!string.IsNullOrEmpty(connectionProfilePicUrl))
                                            {
                                                precUpdateTemplate = precUpdateTemplate.Replace("cid:yourConnectionProfilePicUrl", connectionProfilePicUrl);
                                            }


                                            connectionToName = networkAlertDetails.connectedToFirstName +" "+ networkAlertDetails.connectedToLastName;
                                            precUpdateTemplate = precUpdateTemplate.Replace(":RECOMMENDEE_FULL_NAME", connectionToName);

                                            connectionToProfileUrl = networkAlertDetails.yourConnectionProfileUrl;

                                            if (string.IsNullOrEmpty(connectionToProfileUrl))
                                            {
                                                connectionToProfileUrl = Constants.ConnectionLinkedInProfileUrl;

                                                connectionToProfileUrl = connectionToProfileUrl.Replace(":FIRST_NAME", networkAlertDetails.yourConnectionFirstName);
                                                connectionToProfileUrl = connectionToProfileUrl.Replace(":LAST_NAME", networkAlertDetails.yourConnectionLastName);
                                            }
                                            precUpdateTemplate = precUpdateTemplate.Replace(":RECOMMENDEE_PROFILE_URL", connectionToProfileUrl);
                                            
                                            connectionToJobTitle = networkAlertDetails.connectedToHeadline;
                                            if (!string.IsNullOrEmpty(connectionToJobTitle) && !string.IsNullOrEmpty(networkAlertDetails.connectedToCompany))
                                            {
                                                connectionToJobTitle += ", " + networkAlertDetails.connectedToCompany;
                                            }
                                            precUpdateTemplate = precUpdateTemplate.Replace(":RECOMMENDEE_JOB_TITLE", connectionToJobTitle);

                                            connectionToProfilePicUrl = networkAlertDetails.connectedToProfilePicUrl;
                                            if (!string.IsNullOrEmpty(connectionToProfilePicUrl))
                                            {
                                                precUpdateTemplate = precUpdateTemplate.Replace("cid:connectedToProfilePicUrl", connectionToProfilePicUrl);
                                            }

                                            precUpdateSection += precUpdateTemplate;
                                            break;
                                            
                                        case "SHAR":
                                            sharUpdateTemplate = Constants.SharConnectionSharedLink;
                                            connectionName = networkAlertDetails.yourConnectionFirstName + " " + networkAlertDetails.yourConnectionLastName;
                                            sharUpdateTemplate = sharUpdateTemplate.Replace(":YOUR_CONNECTION_FULL_NAME", connectionName);

                                            connectionProfileUrl = networkAlertDetails.yourConnectionProfileUrl;

                                            if (string.IsNullOrEmpty(connectionProfileUrl))
                                            {
                                                connectionProfileUrl = Constants.ConnectionLinkedInProfileUrl;

                                                connectionProfileUrl = connectionProfileUrl.Replace(":FIRST_NAME", networkAlertDetails.yourConnectionFirstName);
                                                connectionProfileUrl = connectionProfileUrl.Replace(":LAST_NAME", networkAlertDetails.yourConnectionLastName);
                                            }
                                            sharUpdateTemplate = sharUpdateTemplate.Replace(":YOUR_CONNECTION_PROFILE_URL", connectionProfileUrl);
                                            
                                            connectionJobTitle = networkAlertDetails.yourConnectionHeadline;
                                            if (!string.IsNullOrEmpty(connectionJobTitle) && !string.IsNullOrEmpty(networkAlertDetails.yourConnectionCompany))
                                            {
                                                connectionJobTitle += ", " + networkAlertDetails.yourConnectionCompany;
                                            }
                                            sharUpdateTemplate = sharUpdateTemplate.Replace(":YOUR_CONNECTION_JOB_TITLE", connectionJobTitle);

                                            connectionProfilePicUrl = networkAlertDetails.yourConnectionProfilePicUrl;
                                            if (!string.IsNullOrEmpty(connectionProfilePicUrl))
                                            {
                                                sharUpdateTemplate = sharUpdateTemplate.Replace("cid:yourConnectionProfilePicUrl", connectionProfilePicUrl);
                                            }

                                            sharUpdateTemplate = sharUpdateTemplate.Replace(":LINK_URL", networkAlertDetails.shortenedUrl);
                                            sharUpdateTemplate = sharUpdateTemplate.Replace(":LINK_NAME", networkAlertDetails.shortenedUrl);

                                            sharUpdateSection += sharUpdateTemplate;
                                            break;

                                        case "VIRL":
                                            virlUpdateTemplate = Constants.VirlCommentedOrLiked;

                                            connectionName = networkAlertDetails.yourConnectionFirstName + " " + networkAlertDetails.yourConnectionLastName;
                                            virlUpdateTemplate = virlUpdateTemplate.Replace(":YOUR_CONNECTION_FULL_NAME", connectionName);

                                            connectionProfileUrl = networkAlertDetails.yourConnectionProfileUrl;

                                            if (string.IsNullOrEmpty(connectionProfileUrl))
                                            {
                                                connectionProfileUrl = Constants.ConnectionLinkedInProfileUrl;

                                                connectionProfileUrl = connectionProfileUrl.Replace(":FIRST_NAME", networkAlertDetails.yourConnectionFirstName);
                                                connectionProfileUrl = connectionProfileUrl.Replace(":LAST_NAME", networkAlertDetails.yourConnectionLastName);
                                            }
                                            virlUpdateTemplate = virlUpdateTemplate.Replace(":YOUR_CONNECTION_PROFILE_URL", connectionProfileUrl);
                                            
                                            connectionJobTitle = networkAlertDetails.yourConnectionHeadline;
                                            if (!string.IsNullOrEmpty(connectionJobTitle) && !string.IsNullOrEmpty(networkAlertDetails.yourConnectionCompany))
                                            {
                                                connectionJobTitle += ", " + networkAlertDetails.yourConnectionCompany;
                                            }
                                            virlUpdateTemplate = virlUpdateTemplate.Replace(":YOUR_CONNECTION_JOB_TITLE", connectionJobTitle);

                                            connectionProfilePicUrl = networkAlertDetails.yourConnectionProfilePicUrl;
                                            if (!string.IsNullOrEmpty(connectionProfilePicUrl))
                                            {
                                                virlUpdateTemplate = virlUpdateTemplate.Replace("cid:yourConnectionProfilePicUrl", connectionProfilePicUrl);
                                            }


                                            connectionToName = networkAlertDetails.connectedToFirstName +" "+ networkAlertDetails.connectedToLastName;
                                            virlUpdateTemplate = virlUpdateTemplate.Replace(":UPDATED_BY_FULL_NAME", connectionToName);

                                            connectionToProfileUrl = networkAlertDetails.yourConnectionProfileUrl;

                                            if (string.IsNullOrEmpty(connectionToProfileUrl))
                                            {
                                                connectionToProfileUrl = Constants.ConnectionLinkedInProfileUrl;

                                                connectionToProfileUrl = connectionToProfileUrl.Replace(":FIRST_NAME", networkAlertDetails.yourConnectionFirstName);
                                                connectionToProfileUrl = connectionToProfileUrl.Replace(":LAST_NAME", networkAlertDetails.yourConnectionLastName);
                                            }
                                            virlUpdateTemplate = virlUpdateTemplate.Replace(":UPDATED_BY_PROFILE_URL", connectionToProfileUrl);
                                            
                                            connectionToJobTitle = networkAlertDetails.connectedToHeadline;
                                            if (!string.IsNullOrEmpty(connectionToJobTitle) && !string.IsNullOrEmpty(networkAlertDetails.connectedToCompany))
                                            {
                                                connectionToJobTitle += ", " + networkAlertDetails.connectedToCompany;
                                            }
                                            virlUpdateTemplate = virlUpdateTemplate.Replace(":UPDATED_BY_JOB_TITLE", connectionToJobTitle);

                                            connectionToProfilePicUrl = networkAlertDetails.connectedToProfilePicUrl;
                                            if (!string.IsNullOrEmpty(connectionToProfilePicUrl))
                                            {
                                                virlUpdateTemplate = virlUpdateTemplate.Replace("cid:connectedToProfilePicUrl", connectionToProfilePicUrl);
                                            }

                                            virlUpdateTemplate = virlUpdateTemplate.Replace(":COMMNT_OR_LIKED_URL", networkAlertDetails.shortenedUrl);
                                            virlUpdateTemplate = virlUpdateTemplate.Replace(":COMMNT_OR_LIKED_TEXT", networkAlertDetails.comment);

                                            virlUpdateSection += virlUpdateTemplate;

                                            break;

                                        case "JGRP":
                                            jgrpUpdateTemplate = Constants.JgrpJoinedGroup;
                                            connectionName = networkAlertDetails.yourConnectionFirstName + " " + networkAlertDetails.yourConnectionLastName;
                                            jgrpUpdateTemplate = jgrpUpdateTemplate.Replace(":YOUR_CONNECTION_FULL_NAME", connectionName);

                                            connectionProfileUrl = networkAlertDetails.yourConnectionProfileUrl;

                                            if (string.IsNullOrEmpty(connectionProfileUrl))
                                            {
                                                connectionProfileUrl = Constants.ConnectionLinkedInProfileUrl;

                                                connectionProfileUrl = connectionProfileUrl.Replace(":FIRST_NAME", networkAlertDetails.yourConnectionFirstName);
                                                connectionProfileUrl = connectionProfileUrl.Replace(":LAST_NAME", networkAlertDetails.yourConnectionLastName);
                                            }
                                            jgrpUpdateTemplate = jgrpUpdateTemplate.Replace(":YOUR_CONNECTION_PROFILE_URL", connectionProfileUrl);
                                            
                                            connectionJobTitle = networkAlertDetails.yourConnectionHeadline;
                                            if (!string.IsNullOrEmpty(connectionJobTitle) && !string.IsNullOrEmpty(networkAlertDetails.yourConnectionCompany))
                                            {
                                                connectionJobTitle += ", " + networkAlertDetails.yourConnectionCompany;
                                            }
                                            jgrpUpdateTemplate = jgrpUpdateTemplate.Replace(":YOUR_CONNECTION_JOB_TITLE", connectionJobTitle);

                                            connectionProfilePicUrl = networkAlertDetails.yourConnectionProfilePicUrl;
                                            if (!string.IsNullOrEmpty(connectionProfilePicUrl))
                                            {
                                                jgrpUpdateTemplate = jgrpUpdateTemplate.Replace("cid:yourConnectionProfilePicUrl", connectionProfilePicUrl);
                                            }

                                            jgrpUpdateTemplate = jgrpUpdateTemplate.Replace(":GROUP_URL", networkAlertDetails.shortenedUrl);
                                            jgrpUpdateTemplate = jgrpUpdateTemplate.Replace(":GROUP_NAME", networkAlertDetails.groupName);

                                            jgrpUpdateSection += jgrpUpdateTemplate;
                                            break;
                                        
                                    }
                                }
                            }

                            this.allOtherTypeNetworkUpdatesSection.Append(headingSection);
                            this.allOtherTypeNetworkUpdatesSection.Append(cmpyStatusUpdateSection);
                            this.allOtherTypeNetworkUpdatesSection.Append(cmpyJobChangeSection);
                            this.allOtherTypeNetworkUpdatesSection.Append(picuUpdateSection);
                            this.allOtherTypeNetworkUpdatesSection.Append(profUpdateSection);
                            this.allOtherTypeNetworkUpdatesSection.Append(prfxUpdateSection);
                            this.allOtherTypeNetworkUpdatesSection.Append(precUpdateSection);
                            this.allOtherTypeNetworkUpdatesSection.Append(sharUpdateSection);
                            this.allOtherTypeNetworkUpdatesSection.Append(virlUpdateSection);
                            this.allOtherTypeNetworkUpdatesSection.Append(jgrpUpdateSection); 

                        }
                    }

                    isUpdatedNetworkFound = true;
                }
            }

            return isUpdatedNetworkFound;
        }

        /// <summary>
        /// This method creates Twitter lead section for Twitter Lead email Templates.
        /// </summary>
        /// <param name="openRequestDetails"></param>
        public bool CreateTwitterLeadSectionForEmailTemplate(TwitterLeadsForAlertType[] twitterLeadsForAlertTypeArray, string userId)
        {
            bool isTwitterLeadFound = false;

            HelperMethods.AddLogs("Enter in CreateTwitterLeadSectionForEmailTemplate. for userId=" + userId);

            if (twitterLeadsForAlertTypeArray != null && twitterLeadsForAlertTypeArray.Length > 0)
            {
                this.twitterAllLeadSection.Append(Constants.LeadFromTwitterHeading);

                foreach (TwitterLeadsForAlertType twitterLeadsForAlertType in twitterLeadsForAlertTypeArray)
                {
                    string alertType = twitterLeadsForAlertType.alertType;
                    string alertTypeForNetworkUpdate = "Leads from Twitter for ";
                    
                    if (twitterLeadsForAlertType.twitterLeadAlerts != null && twitterLeadsForAlertType.twitterLeadAlerts.Length > 0)
                    {
                        foreach (TwitterLeadAlerts twitterLeadAlerts in twitterLeadsForAlertType.twitterLeadAlerts)
                        {
                            string targetName = twitterLeadAlerts.targetName;
                            string alertTypeAndTargetForNetworkUpdate = alertTypeForNetworkUpdate + targetName;

                            string twitterLeadTypeSection = Constants.TwitterLeadTypeSection;

                            twitterLeadTypeSection = twitterLeadTypeSection.Replace(":TARGET_NAME",alertTypeAndTargetForNetworkUpdate);

                            if (twitterLeadAlerts.twitterLeadAlertDetails != null && twitterLeadAlerts.twitterLeadAlertDetails.Length > 0)
                            {
                                string netwrokUpdateDetails = string.Empty;
                                string positionUpdateDetails = string.Empty;

                                
                                StringBuilder twitterLeadsSectionBuilder =new StringBuilder(string.Empty);
                                
                                foreach (TwitterLeadAlertDetails twitterLeadAlertDetails in twitterLeadAlerts.twitterLeadAlertDetails)
                                {
                                    if (!string.IsNullOrEmpty(twitterLeadAlertDetails.twitterHandle))
                                    {
                                        string twitterLeadSection = Constants.TwitterLeadsSection;

                                        isTwitterLeadFound = true;

                                        string twitterHandle = twitterLeadAlertDetails.twitterHandle;
                                        string profilePicUrl = twitterLeadAlertDetails.profilePicUrl;
                                        string profileUrl = twitterLeadAlertDetails.profileUrl;
                                        string twitterBio = twitterLeadAlertDetails.twitterBio;
                                        string userName = twitterLeadAlertDetails.firstName + " " + (string.IsNullOrEmpty(twitterLeadAlertDetails.lastName) ? string.Empty : twitterLeadAlertDetails.lastName);

                                        twitterLeadSection = twitterLeadSection.Replace(":USER_NAME", userName);
                                        twitterLeadSection = twitterLeadSection.Replace(":PROFILE_PIC_URL", profilePicUrl);
                                        twitterLeadSection = twitterLeadSection.Replace(":PROFILE_URL", profileUrl);
                                        twitterLeadSection = twitterLeadSection.Replace(":TWITTER_HANDLE", twitterHandle);
                                        twitterLeadSection = twitterLeadSection.Replace(":TWITTER_BIO", twitterBio);

                                        string getMoreDataLink = Config.GetMoreDataForTwitterLeadUrl;
                                        getMoreDataLink = getMoreDataLink.Replace(":TWITTER_HANDLE", twitterHandle);
                                        getMoreDataLink = getMoreDataLink.Replace(":LEAD_ID", twitterLeadAlertDetails.leadId);

                                        twitterLeadSection = twitterLeadSection.Replace(":GET_MORE_DATA_LINK", getMoreDataLink);

                                        twitterLeadsSectionBuilder.Append(twitterLeadSection);
                                    }
                                }

                                twitterLeadTypeSection = twitterLeadTypeSection.Replace(":TWITTER_LEADS_SECTION", twitterLeadsSectionBuilder.ToString());
                            }

                            this.twitterAllLeadSection.Append(twitterLeadTypeSection);
                        }
                    }
                }

                HelperMethods.AddLogs(string.Format("CreateTwitterLeadSectionForEmailTemplate. Twitter lead HTML template data for userId- {0}, twitterAllLeadSection- {1}", userId, Convert.ToString(this.twitterAllLeadSection)));
            }

            HelperMethods.AddLogs("Exit from CreateTwitterLeadSectionForEmailTemplate. for userId=" + userId);

            return isTwitterLeadFound;
        }

        /// <summary>
        /// This method creates Network Expand Statistics Email Templates.
        /// </summary>
        /// <param name="openRequestDetails"></param>
        public bool CreateNetworkExpandStatisticsForEmailTemplate(NetworkExpandStatistics networkExpandStatistics)
        {
            bool isNetworkExpandStatisticsFound = false;

            string networkExpandForLocationSectionTemplate = string.Empty;
            string networkExpandForSubIndustrySectionTemplate = string.Empty;

            if (networkExpandStatistics.locationNetworkUpdates != null && networkExpandStatistics.locationNetworkUpdates.Length > 0)
            {
                foreach (LocationNetworkUpdates locationNetworkUpdates in networkExpandStatistics.locationNetworkUpdates)
                {
                    networkExpandForLocationSectionTemplate = Constants.NetworkExpandForLocation;

                    string locationName = locationNetworkUpdates.locationName;
                    string locationCount = locationNetworkUpdates.locationUpdateCount;

                    networkExpandForLocationSectionTemplate = networkExpandForLocationSectionTemplate.Replace(":LOCATION_NAME", locationName);
                    networkExpandForLocationSectionTemplate = networkExpandForLocationSectionTemplate.Replace(":LOCATION_COUNT", locationCount);

                    this.networkExpandForLocationSection += networkExpandForLocationSectionTemplate;
                }
                isNetworkExpandStatisticsFound = true;
            }
            else
            {
                networkExpandForLocationSectionTemplate = Constants.NoNetworkExpandForLocation;

                networkExpandForLocationSectionTemplate = networkExpandForLocationSectionTemplate.Replace(":NO_LOCATION_NETWORKEXPAND", "No Location Updates in last 7 days.");

                this.networkExpandForLocationSection += networkExpandForLocationSectionTemplate;
            }



            if (networkExpandStatistics.subIndustryNetworkUpdates != null && networkExpandStatistics.subIndustryNetworkUpdates.Length > 0)
            {
                foreach (SubIndustryNetworkUpdates subIndustryNetworkUpdates in networkExpandStatistics.subIndustryNetworkUpdates)
                {
                    networkExpandForSubIndustrySectionTemplate = Constants.NetworkExpandForSubIndustry;

                    string subindustryName = subIndustryNetworkUpdates.subindustryName;
                    string subindustryUpdateCount = subIndustryNetworkUpdates.subindustryUpdateCount;

                    networkExpandForSubIndustrySectionTemplate = networkExpandForSubIndustrySectionTemplate.Replace(":SUBINDUSTRY_NAME", subindustryName);
                    networkExpandForSubIndustrySectionTemplate = networkExpandForSubIndustrySectionTemplate.Replace(":SUBINDUSTRY_COUNT", subindustryUpdateCount);

                    this.networkExpandForSubIndustrySection += networkExpandForSubIndustrySectionTemplate;
                }

                isNetworkExpandStatisticsFound = true;
            }
            else
            {
                networkExpandForSubIndustrySectionTemplate = Constants.NetworkExpandForSubIndustry;

                networkExpandForSubIndustrySectionTemplate = networkExpandForSubIndustrySectionTemplate.Replace(":NO_SUBINDUSTRY_NETWORKEXPAND", "No Sub Industry Updates in last 7 days.");

                this.networkExpandForSubIndustrySection += networkExpandForSubIndustrySectionTemplate;
            }

            return isNetworkExpandStatisticsFound;
        }

        /// <summary>
        /// This method creates alternate view for network expand statistics Email.
        /// </summary>
        /// <returns>Alternate view for network expand statistics Email.</returns>
        private AlternateView CreateAlternateViewFornetworkExpandStatisticsEmail()
        {
            var appPath = HelperMethods.GetExeDir();

            //Get the html file for request address
            var bodyFile = Path.Combine(appPath, @"EmailTemplates\NetworkExpandStatisticsEmailTemplate.html");
            //Read contents of HTML file
            StreamReader dataStreamReader = File.OpenText(bodyFile);

            string mailBody = dataStreamReader.ReadToEnd();

            dataStreamReader.Close();

            mailBody = mailBody.Replace(":LOCATION_NETWORK_EXPAND_STATISTICS", this.networkExpandForLocationSection);
            mailBody = mailBody.Replace(":SUBINDUSTRY_NETWORK_EXPAND_STATISTICS", this.networkExpandForSubIndustrySection);


            string unsubscribeFromAlertsUrl = Config.UnsubscribeFromAlertsUrl;

            unsubscribeFromAlertsUrl = unsubscribeFromAlertsUrl.Replace(":USER_ID", nameValues["userId"]);

            mailBody = mailBody.Replace(":UNSUBSCRIBE_LINK", unsubscribeFromAlertsUrl);

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailBody, null, "text/html");

            LinkedResource voteNowLogoImagelink = new LinkedResource(appPath + @"\Assets\votenow-icon.png", "image/png");
            voteNowLogoImagelink.ContentId = "voteNowLogo";
            voteNowLogoImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(voteNowLogoImagelink);

            LinkedResource emailLogoImagelink = new LinkedResource(appPath + @"\Assets\logo_email.jpg", "image/jpg");
            emailLogoImagelink.ContentId = "emailLogo";
            emailLogoImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(emailLogoImagelink);


            LinkedResource userImageForYourConnection = new LinkedResource(appPath + @"\Assets\user-photo.jpg", "image/jpg");
            userImageForYourConnection.ContentId = "yourConnectionProfilePicUrl";
            userImageForYourConnection.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(userImageForYourConnection);


            LinkedResource userImageForConnectedTo = new LinkedResource(appPath + @"\Assets\user-photo.jpg", "image/jpg");
            userImageForConnectedTo.ContentId = "connectedToProfilePicUrl";
            userImageForConnectedTo.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(userImageForConnectedTo);


            LinkedResource faceBookiconImagelink = new LinkedResource(appPath + @"\Assets\f-icon.jpg", "image/jpg");
            faceBookiconImagelink.ContentId = "faceBookicon";
            faceBookiconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(faceBookiconImagelink);

            LinkedResource twitterIconImagelink = new LinkedResource(appPath + @"\Assets\t-icon.jpg", "image/jpg");
            twitterIconImagelink.ContentId = "twitterIcon";
            twitterIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(twitterIconImagelink);

            LinkedResource linkedInIconImagelink = new LinkedResource(appPath + @"\Assets\in-icon.jpg", "image/jpg");
            linkedInIconImagelink.ContentId = "linkedInIcon";
            linkedInIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(linkedInIconImagelink);

            return htmlView;
        }
        #endregion

        # region AlternetView for access token expired email

        /// <summary>
        /// This method creates alternate view for access token expired email.
        /// </summary>
        /// <returns>Alternate view for access token expired email.</returns>
        private AlternateView CreateAlternateViewForAccessTokenExpiredEmail()
        {
            //Get applicaiton path

            var appPath = HelperMethods.GetExeDir();

            //Get the html file for request address
            var bodyFile = Path.Combine(appPath, @"EmailTemplates\ReconnectToLinkedInEmailTemplate.html");
            //Read contents of HTML file
            StreamReader dataStreamReader = File.OpenText(bodyFile);

            string mailBody = dataStreamReader.ReadToEnd();

            dataStreamReader.Close();

            mailBody = mailBody.Replace(":RECONNECT_YOUR_ACCOUNT_LINK", Config.OfunnelLoginUrl);

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailBody, null, "text/html");

            LinkedResource emailLogoImagelink = new LinkedResource(appPath + @"\Assets\logo_email.jpg", "image/jpg");
            emailLogoImagelink.ContentId = "emailLogo";
            emailLogoImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(emailLogoImagelink);

            LinkedResource faceBookiconImagelink = new LinkedResource(appPath + @"\Assets\f-icon.jpg", "image/jpg");
            faceBookiconImagelink.ContentId = "faceBookicon";
            faceBookiconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(faceBookiconImagelink);

            LinkedResource twitterIconImagelink = new LinkedResource(appPath + @"\Assets\t-icon.jpg", "image/jpg");
            twitterIconImagelink.ContentId = "twitterIcon";
            twitterIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(twitterIconImagelink);

            LinkedResource linkedInIconImagelink = new LinkedResource(appPath + @"\Assets\in-icon.jpg", "image/jpg");
            linkedInIconImagelink.ContentId = "linkedInIcon";
            linkedInIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(linkedInIconImagelink);

            return htmlView;
        }


        /// <summary>
        /// This method creates alternate view for access token expired email for pipeline users.
        /// </summary>
        /// <returns>Alternate view for access token expired email.</returns>
        private AlternateView CreateAlternateViewForAccessTokenExpiredEmailForPipelineUser()
        {
            //Get applicaiton path

            var appPath = HelperMethods.GetExeDir();

            //Get the html file for request address
            var bodyFile = Path.Combine(appPath, @"EmailTemplates\ReconnectToLinkedInEmailForPipelineUserTemplate.html");
            //Read contents of HTML file
            StreamReader dataStreamReader = File.OpenText(bodyFile);

            string mailBody = dataStreamReader.ReadToEnd();

            dataStreamReader.Close();

            mailBody = mailBody.Replace(":RECONNECT_YOUR_ACCOUNT_LINK", Config.OfunnelLoginUrl);

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailBody, null, "text/html");

            LinkedResource emailLogoImagelink = new LinkedResource(appPath + @"\Assets\pipelinedeal_logo.jpg", "image/jpg");
            emailLogoImagelink.ContentId = "emailLogo";
            emailLogoImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(emailLogoImagelink);

            LinkedResource faceBookiconImagelink = new LinkedResource(appPath + @"\Assets\f-icon.jpg", "image/jpg");
            faceBookiconImagelink.ContentId = "faceBookicon";
            faceBookiconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(faceBookiconImagelink);

            LinkedResource twitterIconImagelink = new LinkedResource(appPath + @"\Assets\t-icon.jpg", "image/jpg");
            twitterIconImagelink.ContentId = "twitterIcon";
            twitterIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(twitterIconImagelink);

            LinkedResource linkedInIconImagelink = new LinkedResource(appPath + @"\Assets\in-icon.jpg", "image/jpg");
            linkedInIconImagelink.ContentId = "linkedInIcon";
            linkedInIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(linkedInIconImagelink);

            return htmlView;
        }
        
        #endregion


        # region AlternetView for trial period expired email

        /// <summary>
        /// This method creates alternate view for trial period expired email.
        /// </summary>
        /// <returns>Alternate view for trial period expired email.</returns>
        private AlternateView CreateAlternateViewForTrialPeriodExpiredEmail()
        {
            //Get applicaiton path

            var appPath = HelperMethods.GetExeDir();

            //Get the html file for request address
            var bodyFile = Path.Combine(appPath, @"EmailTemplates\TrailAlertEmailTemplate.html");
            //Read contents of HTML file
            StreamReader dataStreamReader = File.OpenText(bodyFile);

            string mailBody = dataStreamReader.ReadToEnd();

            dataStreamReader.Close();

            mailBody = mailBody.Replace(":UPGRADE_YOUR_ACCOUNT_NOW_URL", Config.UpgradeYourAcount);

            int daysRemainingToExpire = Convert.ToInt32(this.nameValues["daysRemainingToExpire"]);
            switch (daysRemainingToExpire)
            {
                case 0:
                    mailBody = mailBody.Replace(":TRIAL_HEADING", Constants.TrialPeriodExpiredEmailHeader);
                    mailBody = mailBody.Replace(":TRIAL_TEXT", Constants.TrialPeriodExpiredEmailText);
                    break;
                case 1:
                    mailBody = mailBody.Replace(":TRIAL_HEADING", Constants.TrialPeriodExpiredInOneDayEmailHeader);
                    mailBody = mailBody.Replace(":TRIAL_TEXT", Constants.TrialPeriodExpiredInOneDayEmailText);
                    break;
                case 7:
                    mailBody = mailBody.Replace(":TRIAL_HEADING", Constants.TrialPeriodExpiredInSevenDayEmailHeader);
                    mailBody = mailBody.Replace(":TRIAL_TEXT", Constants.TrialPeriodExpiredInSevenDayEmailText);
                    break;
            }

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailBody, null, "text/html");

            LinkedResource emailLogoImagelink = new LinkedResource(appPath + @"\Assets\logo_email.jpg", "image/jpg");
            emailLogoImagelink.ContentId = "emailLogo";
            emailLogoImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(emailLogoImagelink);

            LinkedResource faceBookiconImagelink = new LinkedResource(appPath + @"\Assets\f-icon.jpg", "image/jpg");
            faceBookiconImagelink.ContentId = "faceBookicon";
            faceBookiconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(faceBookiconImagelink);

            LinkedResource twitterIconImagelink = new LinkedResource(appPath + @"\Assets\t-icon.jpg", "image/jpg");
            twitterIconImagelink.ContentId = "twitterIcon";
            twitterIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(twitterIconImagelink);

            LinkedResource linkedInIconImagelink = new LinkedResource(appPath + @"\Assets\in-icon.jpg", "image/jpg");
            linkedInIconImagelink.ContentId = "linkedInIcon";
            linkedInIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(linkedInIconImagelink);

            return htmlView;
        }
        #endregion

        # region AlternetView for Ofunnel Server Error email

        /// <summary>
        /// This method creates alternate view for Ofunnel Server Error email.
        /// </summary>
        /// <returns>Alternate view for Ofunnel Server Error email.</returns>
        private AlternateView CreateAlternateViewForOFunnelServerErrorEmail()
        {
            //Get applicaiton path

            var appPath = HelperMethods.GetExeDir();

            //Get the html file for request address
            var bodyFile = Path.Combine(appPath, @"EmailTemplates\ServerProblemEmailTemplate.html");
            //Read contents of HTML file
            StreamReader dataStreamReader = File.OpenText(bodyFile);

            string mailBody = dataStreamReader.ReadToEnd();

            dataStreamReader.Close();

            mailBody = mailBody.Replace(":STATUS_CODE", this.nameValues["httpStatusCode"]);
            mailBody = mailBody.Replace(":DESCRIPTION", this.nameValues["description"]);
            mailBody = mailBody.Replace(":ERROR_MESSAGE", this.nameValues["errorMessage"]);

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailBody, null, "text/html");

            LinkedResource emailLogoImagelink = new LinkedResource(appPath + @"\Assets\logo_email.jpg", "image/jpg");
            emailLogoImagelink.ContentId = "emailLogo";
            emailLogoImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(emailLogoImagelink);

            LinkedResource faceBookiconImagelink = new LinkedResource(appPath + @"\Assets\f-icon.jpg", "image/jpg");
            faceBookiconImagelink.ContentId = "faceBookicon";
            faceBookiconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(faceBookiconImagelink);

            LinkedResource twitterIconImagelink = new LinkedResource(appPath + @"\Assets\t-icon.jpg", "image/jpg");
            twitterIconImagelink.ContentId = "twitterIcon";
            twitterIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(twitterIconImagelink);

            LinkedResource linkedInIconImagelink = new LinkedResource(appPath + @"\Assets\in-icon.jpg", "image/jpg");
            linkedInIconImagelink.ContentId = "linkedInIcon";
            linkedInIconImagelink.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlView.LinkedResources.Add(linkedInIconImagelink);

            return htmlView;
        }

        #endregion
    }
}