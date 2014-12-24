#define STAGING_SERVER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFunnelEmailScheduler.OFunnelUtilities
{
    class Config
    {
        // Email Timer config parameters
        public const int TimerIntervalInHours = 24; //24 hours for 1 day
        public const int EmailSendTime = 14; // At 2 PM it will send email, this value is in 24 hours format i.e. for 2PM it will be 14.

        public const int FollowupEmailTimerIntervalInHours = 24; //24 hours for 1 day
        public const int FollowupEmailSendTime = 19; // At 7 PM it will send email, this value is in 24 hours format i.e. for 7PM it will be 19.
        
        // Update Timer config parameters
        public const int UpdateNetworkTimerIntervalInHours = 24; //24 hours for 1 day
        public const int UpdateNetworkTime = 15; // At 3 PM it will update network.

        // AccessToken expired email Timer config parameters
        public const int AccessTokenExpiredTimerIntervalInHours = 24; //24 hours for 1 day
        public const int AccessTokenExpiredEmailTime = 14; // At 2 PM it will send access token expired.

        // Trial period  expired email Timer config parameters
        public const int TrialPeriodExpiredTimerIntervalInHours = 24; //24 hours for 1 day
        public const int TrialPeriodExpiredEmailTime = 13; // At 1 PM it will send access token expired.
        
        // Similar Companies Timer config parameters
        public const int SimilarCompaniesTimerIntervalInHours = 24; //24 hours for 1 day
        public const int SimilarCompaniesTime = 4; // At 4 AM it will invoke to check for similiar companies.

        // Company Update Timer config parameters
        public const int UpdateCompaniesTimerIntervalInHours = 24; //24 hours for 1 day
        public const int UpdateCompaniesTime = 1; // At 1 AM it will update company this value is in 24 hours format i.e. for 2PM it will be 14.

        // Check server status timer config parameters
        public const long CheckServerStatusTimerInterval = 5 * 60 * 1000; // 5 min in milliseconds.

        // Check server status timer config parameters
        public const long SendPushNotificationTimerInterval = 1 * 60 * 60 * 1000; // 1 hour in milliseconds.

        // Email Credentials config parameters
        public const string EmailUserName = "info@ofunnel.com";
        public const string EmailUserNameForPipelineUser = "info@pipelinedeals.com";
        public const string EmailPassword = "fe9a4371-07c2-4115-9372-62d49eebfabf";
        public const string EmailFromName = "OFunnel";
        public const string EmailFromNameForPipelineUser = "Pipeline";
        public const string EmailHost = "smtp.mandrillapp.com";
        public const int EmailPort = 587;

#if STAGING_SERVER 
        // Database config parameters for staging server
        public const string OFunnelDbConnection = "Data Source=bo1b499qc6.database.windows.net; Initial Catalog=OFunnelQA; User ID=kushal; Password=q1w2e3r4!";

#elif PRODUCTION_SERVER 
        // Database config parameters for Production server
        public const string OFunnelDbConnection = "Data Source=ec2-54-161-230-145.compute-1.amazonaws.com; Initial Catalog=OFunnel; User ID=kushal; Password=q1w2e3r4!";

#endif

#if STAGING_SERVER
        // OFunnel Staging Server related URls
        public const string SeeAllRequestUrl = "http://ofunnelfrontqa.cloudapp.net:4321/requests";
        public const string SeeParticularRequestUrl = "http://ofunnelfrontqa.cloudapp.net:4321/requests#:REQUESTID";
        public const string AddToCrmUrl = "http://ofunnelfrontqa.cloudapp.net:4321/alerts?view=desktop";
        public const string OfunnelLoginUrl = "http://ofunnelfrontqa.cloudapp.net:4321/linkedin/authorize_with_likedin";
        public const string UpgradeYourAcount = "http://ofunnelfrontqa.cloudapp.net:4321/notifications";
        public const string UnsubscribeFromAlertsUrl = "http://ofunnelfrontqa.cloudapp.net:4321/unsubscribe/:USER_ID";
        public const string GetMoreDataUrl = "http://ofunnelqa.cloudapp.net/ofunnelservice/GetMoreData.aspx?userid=:USER_IDS&yourconnection=:YOUR_CONNECTION&connectedto=:CONNECTED_TO&id=:NETWORK_UPDATE_ID";
        public const string GetMoreDataForTwitterLeadUrl = "http://ofunnelqa.cloudapp.net/ofunnelservice/GetMoreData.aspx?userid=:USER_IDS&handle=:TWITTER_HANDLE&leadid=:LEAD_ID";
#elif PRODUCTION_SERVER 
        // OFunnel Production Server related URls
        public const string SeeAllRequestUrl = "http://beta.ofunnel.com/requests";
        public const string SeeParticularRequestUrl = "http://beta.ofunnel.com/requests#:REQUESTID";
        public const string AddToCrmUrl = "http://beta.ofunnel.com/alerts?view=desktop";
        public const string OfunnelLoginUrl = "http://beta.ofunnel.com/linkedin/authorize_with_likedin";
        public const string UpgradeYourAcount = "http://beta.ofunnel.com/notifications";
        public const string UnsubscribeFromAlertsUrl = "http://beta.ofunnel.com/unsubscribe/:USER_ID";
        public const string GetMoreDataUrl = "http://ec2-54-161-230-145.compute-1.amazonaws.com/ofunnelservice/GetMoreData.aspx?userid=:USER_IDS&yourconnection=:YOUR_CONNECTION&connectedto=:CONNECTED_TO&id=:NETWORK_UPDATE_ID";
        public const string GetMoreDataForTwitterLeadUrl = "http://ec2-54-161-230-145.compute-1.amazonaws.com/ofunnelservice/GetMoreData.aspx?userid=:USER_IDS&handle=:TWITTER_HANDLE&leadid=:LEAD_ID";
#endif
        ////////OFunnel Backend Service URL /////////////////

#if STAGING_SERVER 
        // Staging Server Url.
        public const string SimilarCompaniesUrl = "http://ofunnelqa.cloudapp.net/OFunnelService/UserService.svc/CheckForSimilarCompaniesForTargetAccounts/:USER_ID";
        public const string CheckForServerStatus = "http://ofunnelqa.cloudapp.net/OFunnelService/NotificationService.svc/CheckServerStatus";
        public const string SendPushNotificationUrl = "http://ofunnelqa.cloudapp.net/OFunnelService/NotificationService.svc/postNotificationToWns?userId=:USER_ID&channelUrl=:CHANNEL_URL";

#elif PRODUCTION_SERVER 
        // Production Server Url.
        public const string SimilarCompaniesUrl = "http://ec2-54-161-230-145.compute-1.amazonaws.com/OFunnelService/UserService.svc/CheckForSimilarCompaniesForTargetAccounts/:USER_ID";
        public const string CheckForServerStatus = "http://ec2-54-161-230-145.compute-1.amazonaws.com/OFunnelService/NotificationService.svc/CheckServerStatus";
        public const string SendPushNotificationUrl = "http://ec2-54-161-230-145.compute-1.amazonaws.com/OFunnelService/NotificationService.svc/postNotificationToWns?userId=:USER_ID&channelUrl=:CHANNEL_URL";

#elif DEMO_SERVER 
        // Demo Server Url.
        public const string SimilarCompaniesUrl = "http://ec2-54-161-230-145.compute-1.amazonaws.com/OFunnelServiceDemo/UserService.svc/CheckForSimilarCompaniesForTargetAccounts/:USER_ID";
        public const string CheckForServerStatus = "http://ec2-54-161-230-145.compute-1.amazonaws.com/OFunnelServiceDemo/NotificationService.svc/CheckServerStatus";
        public const string SendPushNotificationUrl = "http://ec2-54-161-230-145.compute-1.amazonaws.com/OFunnelServiceDemo/NotificationService.svc/postNotificationToWns?userId=:USER_ID&channelUrl=:CHANNEL_URL";
#endif

#if STAGING_SERVER
        public const string OFunnelServerStatusEmailSubject = "Staging Server:: OFunnel Server is not responding.";
#elif PRODUCTION_SERVER
        public const string OFunnelServerStatusEmailSubject = "Production Server:: OFunnel Server is not responding.";
#endif



        ////////////////////////////////////////////////////

        // Email Template parameters.
        public const string BlueSectionColor = "#eef7fb";
        public const string WhiteSectionColor = "#ffffff";

        ////////////// Logs //////////////////

        public const int MaxFileSizeInBytes = 25 * 1048576; // here max file size is 25 mb = 25*1048576 bytes.

        ////////////// MaxThreadRequired //////////////////

        public const int MaxThreadRequired = 100;
        public const int MaxCompanyUpdatePerThread = 400;
        public const int MinAccessTokenPerThread = 1;
        public const int MaxAccessTokenPerThread = 2;
    }
}
