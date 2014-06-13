using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFunnelEmailScheduler.OFunnelUtilities
{
    /*************** Open Request Details **************/
    public class AllOpenRequests
    {
        public OpenRequestDetails[] openRequestDetails { get; set; }
    }


    public class OpenRequestDetails
    {
        public string querySearched { get; set; }
        public string companySearched { get; set; }
        public int fromUserId { get; set; }
        public string fromUserName { get; set; }
        public string fromUserProfilePicUrl { get; set; }
        public string fromUserProfileUrl { get; set; }
        public string fromUserCompany { get; set; }
        public string fromUserHeadline { get; set; }
        public int fromUserScore { get; set; }
        public int forUserId { get; set; }
        public string forUserName { get; set; }
        public string forUserProfilePicUrl { get; set; }
        public string forUserProfileUrl { get; set; }
        public string forUserCompany { get; set; }
        public string forUserHeadline { get; set; }
        public int forUserScore { get; set; }
        public int toUserId { get; set; }
        public string toUserName { get; set; }
        public string toUserProfilePicUrl { get; set; }
        public string toUserProfileUrl { get; set; }
        public string toUserCompany { get; set; }
        public string toUserHeadline { get; set; }
        public int toUserScore { get; set; }
        public string status { get; set; }
        public string content { get; set; }
        public string updatedAt { get; set; }
        public string createdAt { get; set; }
        public int requestId { get; set; }
        public int tagId { get; set; }
        public bool matchedFound { get; set; }
    }


    /*************** OFunnel User Details **************/
    public class OFunnelUsers
    {
        public OFunnelUser[] oFunnelUser { get; set; }
        public int count { get; set; }
    }

    public class OFunnelUser
    {
        public int userIndex { get; set; }
        public int userId { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public bool isTargetAccountSet { get; set; }
        public int daysRemainingToExpire { get; set; }
        public string channelUrl { get; set; }
        public string accountType { get; set; }
        public UserTags[] userTags { get; set; }
    }

    public class UserTags
    {
        public int tagId { get; set; }
    }

    /*************** LinkedIn Connections **************/

    public class LinkedInConnection
    {
        public values[] values { get; set; }
    }

    public class values
    {
        public int distance { get; set; }
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string headline { get; set; }
        public location location { get; set; }
        public string pictureUrl { get; set; }
        public string publicProfileUrl { get; set; }
        public positions positions { get; set; }
    }

    public class location
    {
        public string name { get; set; }
    }

    public class positions
    {
        public int _total { get; set; }
        public values1[] values { get; set; }
    }


    public class values1
    {
        public string title { get; set; }
        public company company { get; set; }
    }

    public class company
    {
        public string id { get; set; }
        public string name { get; set; }
        public string size { get; set; }
        public string type { get; set; }
        public string industry { get; set; }
    }

    public class AllArticles
    {
        public Article[] article { get; set; }
    }

    public class Article
    {
        public string headline { get; set; }
        public string summary { get; set; }
        public string articleUrl { get; set; }
    }

    /***************** Netwrok Update related data classes *****************/


    public class NetworkUpdates
    {
        public NetworkAlertsForAlertType[] networkAlertsForAlertType { get; set; }
    }

    public class NetworkAlertsForAlertType
    {
        public string alertType { get; set; }
        public NetworkAlerts[] networkAlerts { get; set; }
    }

    public class NetworkAlerts
    {
        public string targetName { get; set; }
        public NetworkAlertDetails[] networkAlertDetails { get; set; }
    }

    public class NetworkAlertDetails
    {
        public string networkUpdateId { get; set; }

        public string targetName { get; set; }
        public string filterType { get; set; }
        public string alertType { get; set; }

        public string yourConnectionLinkedInId { get; set; }
        public string yourConnectionFirstName { get; set; }
        public string yourConnectionLastName { get; set; }
        public string yourConnectionProfileUrl { get; set; }
        public string yourConnectionProfilePicUrl { get; set; }
        public string yourConnectionHeadline { get; set; }
        public string yourConnectionCompany { get; set; }

        public string connectedToLinkedInId { get; set; }
        public string connectedToFirstName { get; set; }
        public string connectedToLastName { get; set; }
        public string connectedToProfileUrl { get; set; }
        public string connectedToProfilePicUrl { get; set; }
        public string connectedToHeadline { get; set; }
        public string connectedToCompany { get; set; }
    }

    public class CompanySearchData
    {
        public string companyNameToSearch { get; set; }
        public string companyId { get; set; }
        public string processedName { get; set; }
    }

    public class CompanyUpdateData
    {
        public List<CompanySearchData> companySearchDataList { get; set; }
        public List<string> accessTokenList { get; set; }
    }







    public class TwitterLeads
    {
        public TwitterLeadsForAlertType[] twitterLeadsForAlertType { get; set; }
    }

    public class TwitterLeadsForAlertType
    {
        public string alertType { get; set; }
        public TwitterLeadAlerts[] twitterLeadAlerts { get; set; }
    }

    public class TwitterLeadAlerts
    {
        public string targetName { get; set; }
        public TwitterLeadAlertDetails[] twitterLeadAlertDetails { get; set; }
    }

    public class TwitterLeadAlertDetails
    {
        public string leadId { get; set; }

        public string targetName { get; set; }
        public string filterType { get; set; }
        public string alertType { get; set; }

        public string twitterHandle { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string profileUrl { get; set; }
        public string profilePicUrl { get; set; }
        public string twitterBio { get; set; }
    }

    public class NetworkExpandStatistics
    {
        public SubIndustryNetworkUpdates[] subIndustryNetworkUpdates { get; set; }
        public LocationNetworkUpdates[] locationNetworkUpdates { get; set; }
    }

    public class SubIndustryNetworkUpdates
    {
        public string subindustryName { get; set; }
        public string subindustryUpdateCount { get; set; }
    }

    public class LocationNetworkUpdates
    {
        public string locationName { get; set; }
        public string locationUpdateCount { get; set; }
    }
}
