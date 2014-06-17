using System.Data;

namespace OFunnelEmailScheduler.OFunnelDbLogic
{
    public class OFunnelDatabaseHandler
    {
        public OFunnelDatabaseHandler()
        {
        }

        private string sql;

        /*************** EmailScheduler ****************/

        public DataSet GetRequestsGotForUserId(string qry)
        {
            this.sql = "OFunnel_RequestMaster_GetWeeklyRequestsGotForUserId " + qry;
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet GetAllOFunnelUsers()
        {
            this.sql = "OFunnel_UserMaster_GetAllOFunnelUsers ";
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet GetLinkedInAccessTokenFromUserId(string qry)
        {
            this.sql = "OFunnel_UserMaster_GetLinkedInAccessToken " + qry;
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet GetArticaleDetails(string qry)
        {
            this.sql = "OFunnel_ArticleDetails_GetArticleDetails " + qry;
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet GetBlacklistedWords()
        {
            this.sql = "OFunnel_BlacklistedWords_GetBlacklistedWords ";
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet GetOFunnelUsersForNetworkUpdate()
        {
            this.sql = "OFunnel_UpdatedNetworkConnections_GetOFunnelUsersForNetworkUpdate ";
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet GetOFunnelUsersForFollowUpNetworkUpdate()
        {
            this.sql = "OFunnel_UpdatedNetworkConnections_GetOFunnelUsersForFollowUpNetworkUpdate ";
            return BusinessLogic.Exec_Dataset_string(sql);
        }
        
        public DataSet GetAllOFunnelUsersForAccessTokenExpired()
        {
            this.sql = "OFunnel_TargetAccount_GetAllOFunnelUserForAccessTokenExpired ";
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet GetAllUsersForTrialPeriodExpired()
        {
            this.sql = "OFunnel_SubscriptionDetails_GetOFunnelUsersForSbscriptionTrialPeriodExpiryEmailAlert ";
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet GetNetworkUpdateDetailForUserId(string qry)
        {
            this.sql = "OFunnel_UpdatedNetworkConnections_GetNetworkUpdateDetailForUserId " + qry;
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet GetNetworkExpandDetailsForLocationAndIndustryForUserId(string qry)
        {
            this.sql = "OFunnel_NetworkExpandDetails_GetNetworkExpandDetailsForLocationAndIndustryForUserId " + qry;
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet GetFollowUpNetworkUpdateDetailForUserId(string qry)
        {
            this.sql = "OFunnel_UpdatedNetworkConnections_GetFollowUpNetworkUpdateDetailForUserId " + qry;
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet GetRecipientsEmail(string qry)
        {
            this.sql = "OFunnel_RecipientEmail_GetRecipientEmail " + qry;
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet SetLastEmailSentTime(string qry)
        {
            this.sql = "OFunnel_EmailPreferences_SetLastEmailSentTime " + qry;
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet SetIsMailSentForFollowupsNetworkUpdatesForUserId(string qry)
        {
            this.sql = "OFunnel_NetworkUpdatesFollowUp_SetIsMailSentForFollowupsNetworkUpdatesForUserId " + qry;
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet GetAllOFunnelUsersForTwitterLeads()
        {
            this.sql = "OFunnel_TwitterProfilesDisplayedList_GetAllOFunnelUsersForTwitterLeads ";
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet GetTwitterLeadsDetailForUserId(string qry)
        {
            this.sql = "OFunnel_TwitterProfilesDisplayedList_GetTwitterLeadsDetailForUserId " + qry;
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet GetFollowupTwitterLeadsDetailForUserId(string qry)
        {
            this.sql = "OFunnel_TwitterProfilesDisplayedList_GetFollowUpTwitterLeadsDetailForUserId " + qry;
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet SetIsMailSentForTwitterLeadsForUserId(string qry)
        {
            this.sql = "OFunnel_TwitterProfilesDisplayedList_SetIsMailSentForTwitterLeadsForUserId " + qry;
            return BusinessLogic.Exec_Dataset_string(sql);
        }
        
        /////// Company Update //////////////////

        public DataSet GetAllActiveUsers(string qry)
        {
            this.sql = "OFunnel_AccountsLogin_GetAllTokenNotExpiredUsers " + qry;
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet GetCompaniesName(string qry)
        {
            this.sql = "OFunnel_CompanyMaster_GetAllCompanies " + qry;
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        public DataSet UpdateCompanyDetails(string qry)
        {
            this.sql = "OFunnel_CompanyMaster_UpdateCompanyDetails " + qry;
            return BusinessLogic.Exec_Dataset_string(sql);
        }

        ////////////////////////////////Send Push Notification ///////////////////////

        public DataSet GetAllUserWhoHasNotificationChannelUrl()
        {
            this.sql = "OFunnel_UserMaster_GetAllUserWhoHasNotificationChannelUrl ";
            return BusinessLogic.Exec_Dataset_string(sql);
        }
    }
}