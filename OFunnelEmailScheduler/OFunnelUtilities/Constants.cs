using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFunnelEmailScheduler.OFunnelUtilities
{
    class Constants
    {
        // Linked in url to get connections from LinkedIn.
        public const string LinkedInListUrl = "https://api.linkedin.com/v1/people/~/connections:(id,first-name,last-name,headline,distance,positions:(title,company),educations,location:(name),email-address,picture-url,public-profile-url)?modified=new&oauth2_access_token=AUTH_2_TOKEN&format=json";
        public const string ConnectionLinkedInProfileUrl = "http://www.linkedin.com/vsearch/p?firstName=:FIRST_NAME&lastName=:LAST_NAME&openAdvancedForm=true&locationType=Y";
        public const string CompanySearchLinkedInUrl = "https://api.linkedin.com/v1/company-search:(companies:(id,name,universal-name,website-url,company-type,industries,employee-count-range,locations:(address:(state))))?keywords=:COMPANY_NAME&oauth2_access_token=:AUTH_2_TOKEN&count=100&format=json";

        public const string OpenRequestEmailSubject = "OFunnel Weekly Introduction Email";
        public const string OpenRequest = "Open Request";
        public const string Anyone = "Anyone";

        public const string NetwrokUpdateAlertEmailSubject = "OFunnel Connection Alerts.";
        public const string NetwrokUpdateFollowUpAlertEmailSubject = "OFunnel Connection Alerts - Weekly Follow Up";
        public const string NetwrokUpdateAlertEmailSubjectForPipelineUser = "Pipeline Connection Alerts.";
        public const string TwitterLeadAlertEmailSubject = "OFunnel Twitter Lead Alert.";

        public const string AccessTokenExpiredEmailSubject = "OFunnel Connection Alerts.";
        public const string AccessTokenExpiredEmailSubjectForPipelineUser = "Pipeline Connection Alerts.";

        public const string TrialPeriodExpiredInOneDayEmailSubject = "OFunnel Alerts trial ends in 1 days.";
        public const string TrialPeriodExpiredInSevenDayEmailSubject = "OFunnel Alerts trial ends in 7 days.";
        public const string TrialPeriodExpiredEmailSubject = "OFunnel Alerts 30 days trial ended.";

        public const string TrialPeriodExpiredInOneDayEmailHeader = "Your trial expires tomorrow!";
        public const string TrialPeriodExpiredInSevenDayEmailHeader = "Only 7 days left!";
        public const string TrialPeriodExpiredEmailHeader = "Your trial has ended!";

        public const string ServerErrorToUserEmailId = "kushal@ofunnel.com";
        public const string ServerErrorCcUserEmailId = "proj-funnelerrors@adroit-inc.com";

        public const string TrialPeriodExpiredInOneDayEmailText = "This is the last day of your trial account with OFunnel!";
        public const string TrialPeriodExpiredInSevenDayEmailText = "You have only 7 days left until you trial account ends at OFunnel!";
        public const string TrialPeriodExpiredEmailText = "Your 30 days free trial with OFunnel has ended!";

        public const string NetwrokUpdateMessageForCompany = "This means you now have a first level connection that can help you with a connections to your target account :ACCOUNT_NAME.";
        public const string NetwrokUpdateMessageForRole = "This means you now have a first level connection that can help you with a connection to someone with role of interest.";
        public const string NetwrokUpdateMessageForPerson = "";

        public const string TargetCompany = "Target Company: ";
        public const string TargetRole = "Target Role: ";
        public const string TargetPerson = "Target Person: ";
        public const string AlertEmailType = "ALERT";

        public const string OpenRequestSectionForUserNotAvailable = "<tr bgcolor=':SECTION_BACKGROUND_COLOR'>" +
	        "<td colspan='2'>" +
                "<table width='100%' align='center' cellpadding='0' cellspacing='0'>" +
			        "<tbody>" +
				        "<tr>" +
					        "<td width='50%' valign='top'>" +
                                "<table cellpadding='5' cellspacing='0'>" +
                                    "<tbody>" +
                                        "<tr>" +
                                            "<td valign='top'>" +
						                        "<img width='30' height='30' src='cid:fromUserProfilePicUrl' style='float:left; margin-right:10px;' alt='' />" +
                                            "</td>" +
                                            "<td style='font-size:13px;'>" +
                                                "<span style='width:220px; float:left; font-family:Arial, Helvetica, sans-serif;'>" +
                                                    "<strong style='font-size:14px; color: #0068a1; font-family:Calibri; text-decoration: none;'>:FROM_USER_NAME</strong>&nbsp;" +
							                        "<span style='background:#ff5d00; color:#fff; font-family:Calibri; font-weight:bold; padding:1px 4px;'>:FROM_USER_SCORE</span>" +
                                                    "<br />" +
                                                    "<span style='font-size:13px; width:220px; text-overflow: ellipsis; white-space: nowrap; overflow: hidden; float: left;'>:FROM_USER_HEADLINE</span>" +
                                                    "<br />" +
                                                    "<span style='font-size:13px; width:220px; text-overflow: ellipsis; white-space: nowrap; overflow: hidden; float: left;'>:FROM_USER_COMPANY</span>" +
						                        "</span>" +
					                        "</td>" +
                                        "</tr>" +
                                    "</tbody>" +
                                "</table>" +
                            "</td>" +
                            "<td width='50%' valign='top'>" +
                                "<table cellpadding='5' cellspacing='0'>" +
                                    "<tbody>" +
                                        "<tr>" +
                                            "<td>" +
                                                "<img src='cid:forUserProfilePicUrl' style='float:left;' alt='' />" +
                                            "</td>" +
                                            "<td style='width:220px; text-overflow: ellipsis; white-space: nowrap; overflow: hidden; float:left; line-height: 15px;' valign='top'>" +
                                                "<strong style='color:#0068a1; font-size:14px; font-family:Calibri;'>:ANYONE at :SEARCH_QUERY</strong>" +
                                            "</td>" +
                                        "</tr>" +
                                    "</tbody>" +
                                "</table>" +
                            "</td>" +
				        "</tr>" +
				        "<tr>" +
					        "<td colspan='2'>" +
                                "<table width='100%' align='center' cellpadding='5' cellspacing='0'>" +
                                    //"<tr>" +
                                    //    "<td style='font-style: italic; font-family:Arial, Helvetica, sans-serif; text-align:justify; font-size:13px; padding-top:10px;'>" +
                                    //        "<strong style='font-style:normal;'>Request for:</strong> <span style='color:#0068a1;'>\":SEARCH_QUERY\"</span>" +
                                    //    "</td>" +
                                    //"</tr>" +
                                    "<tr>" +
                                        "<td style='font-style: italic; font-family:Arial, Helvetica, sans-serif; text-align:justify; font-size:13px; padding:10px 10px 0 0;'>" +
						                    "<strong style='font-style:normal;'>Message:</strong> :MESSAGE_CONTENT" +
                                            "&nbsp;<a href=':SEE_PARTICULAR_REQUEST_LINK' style='font-size:14px; color: #0068a1; font-family:Calibri; text-decoration: none;'>View this request</a>" +
                                        "</td>" +
                                    "</tr>" +
                                "</table>" +
					        "</td>" +
				        "</tr>" +
			        "</tbody>" +
		        "</table>" +
	        "</td>" +
        "</tr>" ;

        public const string OpenRequestSectionForUserAvailable = "<tr bgcolor=':SECTION_BACKGROUND_COLOR'>" +
            "<td colspan='2'>" +
                "<table width='100%' align='center' cellpadding='0' cellspacing='0'>" +
                    "<tbody>" +
                        "<tr>" +
                            "<td width='50%' valign='top'>" +
                                "<table cellpadding='5' cellspacing='0'>" +
                                    "<tbody>" +
                                        "<tr>" +
                                            "<td valign='top'>" +
                                                "<img width='30' height='30' src='cid:fromUserProfilePicUrl' style='float:left; margin-right:10px;' alt='' />" +
                                            "</td>" +
                                            "<td style='font-size:13px;'>" +
                                                "<span style='width:220px; float:left; font-family:Arial, Helvetica, sans-serif;'>" +
                                                    "<strong style='font-size:14px; color: #0068a1; font-family:Calibri; text-decoration: none;'>:FROM_USER_NAME</strong>&nbsp;" +
                                                    ":FROM_USER_SCORE_TEMPLATE" +
                                                    "<br />" +
                                                    "<span style='font-size:13px; width:220px; text-overflow: ellipsis; white-space: nowrap; overflow: hidden; float: left;'>:FROM_USER_HEADLINE</span>" +
                                                    "<br />" +
                                                    "<span style='font-size:13px; width:220px; text-overflow: ellipsis; white-space: nowrap; overflow: hidden; float: left;'>:FROM_USER_COMPANY</span>" +
                                                "</span>" +
                                            "</td>" +
                                        "</tr>" +
                                    "</tbody>" +
                                "</table>" +
                            "</td>" +
                            "<td width='50%' valign='top'>" +
                                "<table cellpadding='5' cellspacing='0'>" +
                                    "<tbody>" +
                                        "<tr>" +
                                            "<td valign='top'>" +
                                                "<img width='30' height='30' src='cid:forUserProfilePicUrl' style='float:left; margin-right:10px;' alt='' />" +
                                            "</td>" +
                                            "<td style='font-size:13px;'>" +
                                                "<span style='width:220px; float:left; font-family:Arial, Helvetica, sans-serif;'>" +
                                                    "<strong style='color:#0068a1; font-size:14px; font-family:Calibri;'>:FOR_USER_NAME</strong>&nbsp;" +
                                                    ":FOR_USER_SCORE_TEMPLATE" +
                                                    "<br />" +
                                                    "<span style='font-size:13px; width:220px; text-overflow: ellipsis; white-space: nowrap; overflow: hidden; float: left;'>:FOR_USER_HEADLINE</span>" +
                                                    "<br />" +
                                                    "<span style='font-size:13px; width:220px; text-overflow: ellipsis; white-space: nowrap; overflow: hidden; float: left;'>:FOR_USER_COMPANY</span>" +
                                                "</span>" +
                                            "</td>" +
                                        "</tr>" +
                                    "</tbody>" +
                                "</table>" +
                            "</td>" +
                        "</tr>" +
                        "<tr>" +
                            "<td colspan='2'>" +
                                "<table width='100%' align='center' cellpadding='5' cellspacing='0'>" +
                                    "<tr>" +
                                        "<td style='font-style: italic; font-family:Arial, Helvetica, sans-serif; text-align:justify; font-size:13px; padding-top:10px;'>" +
                                            "<strong style='font-style:normal;'>Request for:</strong> <span style='color:#0068a1;'>\":SEARCH_QUERY\"</span>" +
                                        "</td>" +
                                    "</tr>" +
                                    "<tr>" +
                                        "<td style='font-style: italic; font-family:Arial, Helvetica, sans-serif; text-align:justify; font-size:13px;  padding:10px 10px 0 0;'>" +
                                            "<strong style='font-style:normal;'>Message:</strong> :MESSAGE_CONTENT" +
                                            "&nbsp;<a href=':SEE_PARTICULAR_REQUEST_LINK' style='font-size:14px; color: #0068a1; font-family:Calibri; text-decoration: none;'>View this request</a>" +
                                        "</td>" +
                                    "</tr>" +
                                "</table>" +
                            "</td>" +
                        "</tr>" +
                    "</tbody>" +
                "</table>" +
            "</td>" +
        "</tr>";

        public const string ForUserScoreTemplate = "<span style='background:#ff5d00; color:#fff; font-family:Calibri; font-weight:bold; padding:1px 4px;'>:FOR_USER_SCORE</span>";
        public const string FromUserScoreTemplate = "<span style='background:#ff5d00; color:#fff; font-family:Calibri; font-weight:bold; padding:1px 4px;'>:FROM_USER_SCORE</span>";


        public const string ArticalDetails = "<tr>" +
                                "<td style='font-size:15px; font-weight: bold; line-height:20px; color:#0066A4;'>" +
                                    ":ARTICLE_HEADLINE" +
                                "</td>" +
                            "</tr>" +
                            "<tr>" +
                                "<td style='font-size:11px; color:#252525;'>" +
                                    "<span style='width:220px; word-wrap:break-word; white-space:normal; overflow: hidden; float: left;'>:ARTICLE_SUMMARY</span>" +
                                "</td>" +
                            "</tr>" +
                            "<tr>" +
                                "<td style='border-bottom:1px solid #FFFFFF; font-size:11px; font-weight: bold; line-height:20px; color:#0066A4; padding-bottom:10px'>" +
                                    "[<a href=':FULL_ARTICLE_LINK' style='font-size:11px; color: #0066A4; text-decoration: none;'>Full Article</a>]" +
                                "</td>" +
                            "</tr>";

        //public const string ForUserNotAvailableSection = "<td width='50%' valign='top'>" +
        //                                                    "<table cellpadding='5' cellspacing='0'>" +
        //                                                        "<tbody>" +
        //                                                            "<tr>" +
        //                                                                "<td>" +
        //                                                                    "<img src='cid:forUserProfilePicUrl' style='float:left;' alt='' />" +
        //                                                                "</td>" +
        //                                                                "<td style='width:220px; float:left; line-height: 15px;' valign='top'>" +
        //                                                                    "<strong style='color:#0068a1; font-size:14px; font-family:Calibri;'>:ANYONE_REQUEST_MESSAGE</strong>" +
        //                                                                "</td>" +
        //                                                            "</tr>" +
        //                                                        "</tbody>" +
        //                                                    "</table>" +
        //                                                "</td>";

        //public const string ForUserAvailableSection = "<td width='50%' valign='top'>" +
        //                                                    "<table cellpadding='5' cellspacing='0'>" +
        //                                                        "<tbody>" +
        //                                                            "<tr>" +
        //                                                                "<td valign='top'>" +
        //                                                                    "<img width='30' height='30' src='cid:forUserProfilePicUrl' style='float:left; margin-right:10px;' alt='' />" +
        //                                                                "</td>" +
        //                                                                "<td style='font-size:13px;'>" +
        //                                                                    "<span style='width:220px; float:left; font-family:Arial, Helvetica, sans-serif;'>" +
        //                                                                        "<strong style='color:#0068a1; font-size:14px; font-family:Calibri;'>:FOR_USER_NAME</strong>&nbsp;" +
        //                                                                        "<span style='background:#ff5d00; color:#fff; font-family:Calibri; font-weight:bold; padding:1px 4px;'>:FOR_USER_SCORE</span>" +
        //                                                                        "<br />:FOR_USER_HEADLINE," +
        //                                                                        "<br />:FOR_USER_COMPANY" +
        //                                                                    "</span>" +
        //                                                                "</td>" +
        //                                                            "</tr>" +
        //                                                        "</tbody>" +
        //                                                    "</table>" +
        //                                                "</td>";



        //public const string netwrokUpdateDetails = "<tr>" +
        //                                                "<td colspan='2' style='border-bottom:1px solid #cdcdcd; padding-bottom:20px;'>" +
        //                                                    "<table align='left' cellpadding='5' cellspacing='0'>" +
        //                                                        "<tbody>" +
        //                                                            "<tr>" +
        //                                                                "<td style='width:50%'>" +
        //                                                                    "<table align='left' cellpadding='0' cellspacing='0'>"+
        //                                                                        "<tr>"+
        //                                                                            "<td style='vertical-align:top;'>" +
        //                                                                                "<img style='border: 1px solid #d2d3d3' width='38' height='38' title='' alt='' src='cid:yourConnectionProfilePicUrl' />" +
        //                                                                            "</td>" +
        //                                                                            "<td style='vertical-align:top; padding-left:10px;'>" +
        //                                                                                "<span style='width:100%; font-size: 16px; margin-top:-5px; margin-right:5px; vertical-align:text-top; color: #0068a1; font-family: Calibri, arial, sans-serif;'>" +
        //                                                                                    "<a href=':YOUR_CONNECTION_PROFILE_URL' style='color:#0068a1; text-decoration: none;'><strong>:YOUR_CONNECTION_FULL_NAME</strong></a></span>" +
        //                                                                                "<span style='background:#d5efff; font-size:14px; border-radius:8px 8px 8px 8px; display:inline-block; padding:2px 10px; color:#0068a1; border:1px solid #0068a1'>1st</span>" +
        //                                                                                "<br />" +
        //                                                                                "<span style='width:100%; float: left; font-size: 13px; height:30px; overflow:hidden; font-family: Arial, Helvetica, sans-serif;'>:YOUR_CONNECTION_JOB_TITLE</span><br />" +
        //                                                                            "</td>" +
        //                                                                        "</tr>" +
        //                                                                    "</table>" +
        //                                                                "</td>" +

        //                                                                "<td style='width:50%'>" +
        //                                                                    "<table align='left' cellpadding='0' cellspacing='0'>" +
        //                                                                        "<tr>" +
        //                                                                            "<td style='vertical-align:top;'>" +
        //                                                                                "<img style='border: 1px solid #d2d3d3' width='38' height='38' title='' alt='' src='cid:connectedToProfilePicUrl' />" +
        //                                                                            "</td>" +
        //                                                                            "<td style='vertical-align:top; padding-left:10px;'>" +
        //                                                                                "<span style='width:100%; font-size: 16px; margin-top:-5px; vertical-align:text-top; color: #0068a1; font-family: Calibri, arial, sans-serif;'>" +
        //                                                                                    "<a href=':CONNECTED_TO_PROFILE_URL' style='color:#0068a1; text-decoration: none;'><strong>:CONNECTED_TO_FULL_NAME</strong></a></span> " +
        //                                                                                "<br />" +
        //                                                                                "<span style='width:100%; float: left; font-size: 13px; height:30px; overflow:hidden; font-family: Arial, Helvetica, sans-serif;'>:CONNECTED_TO_JOB_TITLE</span><br />" +
        //                                                                            "</td>" +
        //                                                                         "</tr>" +
        //                                                                    "</table>" +
        //                                                                "</td>" +
        //                                                            "</tr>" +
        //                                                            "<tr><td colspan='4' style='font-size:16px;'>:MESSAGE_FOR_NETWORK_UPDATE</td></tr>" +
        //                                                        "</tbody>" +
        //                                                    "</table>" +
        //                                                "</td>" +
        //                                            "</tr>";


        public const string NetworkUpdateDetailsHeader = "<tr>" +
            "<td colspan='2' style='padding-bottom:0px;'>" +
                "<table align='left' cellpadding='5' cellspacing='0' style='width:100%'>" +
                    "<tbody>" +
                        "<tr>" +
                            "<td  style='width:50%; color:#ff5d00; border-bottom: 1px solid #d2d3d3; padding-bottom:10px; font-size:14px; font-family:Arial, Helvetica, sans-serif;'><strong>Your current connection:</strong></td>" +
                            "<td  style='width:50%; color:#ff5d00; border-bottom: 1px solid #d2d3d3; padding-bottom:10px; font-size:14px; font-family:Arial, Helvetica, sans-serif;'><strong>Is now connected to:</strong></td>" +
                        "</tr>" +
                        ":NETWORK_UPDATE_SECTION" +
                    "</tbody>" +
                "</table>" +
            "</td>" +
        "</tr>";

        public const string NetworkUpdateSection = "<tr>" +
                            "<td colspan='2' style='font-size:14px; font-weight:600; padding-top:10px; color: #000000; background:#D2D2D2; font-family:Arial, Helvetica, sans-serif;'>:NETWORK_UPDATE_TYPE_AND_TARGET_NAME</td>" +
                            "</tr>:NETWROK_UPDATE_DETAILS";

        public const string NetwrokUpdateDetails = "<tr>" +
                                "<td style='width:50%; padding-top:10px; vertical-align:top'>" +
                                    "<table align='left' cellpadding='0' cellspacing='0'>" +
                                        "<tr>" +
                                            "<td style='vertical-align:top;'> " +
                                                "<img style='border: 1px solid #d2d3d3; margin-top:4px;' width='38' height='38' title='' alt='' src='cid:yourConnectionProfilePicUrl' />" +
                                            "</td>" +
                                            "<td style='vertical-align:top; padding-left:10px;'>" +
                                                "<span style='width:100%; font-size: 15px; margin-top:-5px; margin-right:5px; vertical-align:text-top; color: #0068a1; font-family: Calibri, arial, sans-serif;'>" +
                                                "<a href=':YOUR_CONNECTION_PROFILE_URL' style='color:#0068a1; text-decoration: none;'><strong>:YOUR_CONNECTION_FULL_NAME</strong></a></span>" +
                                                "<br />" +
                                                "<span style='width:100%; float: left; font-size: 13px; height:auto; overflow:hidden; font-family: Arial, Helvetica, sans-serif;'>:YOUR_CONNECTION_JOB_TITLE</span><br />" +
                                            "</td>" +
                                        "</tr>" +
                                    "</table>" +
                                "</td>" +
                                "<td style='width:50%; padding-top:10px; vertical-align:top'>" +
                                    "<table align='left' cellpadding='0' cellspacing='0'>" +
                                        "<tr>" +
                                            "<td style='vertical-align:top;'>" +
                                                "<img style='border: 1px solid #d2d3d3; margin-top:4px;' width='38' height='38' title='' alt='' src='cid:connectedToProfilePicUrl' />" +
                                            "</td>" +
                                            "<td style='vertical-align:top; padding-left:10px;'>" +
                                                "<span style='width:100%; font-size: 15px; margin-top:-5px; vertical-align:text-top; color: #0068a1; font-family: Calibri, arial, sans-serif;'>" +
                                                    "<a href=':CONNECTED_TO_PROFILE_URL' style='color:#0068a1; text-decoration: none;'><strong>:CONNECTED_TO_FULL_NAME</strong></a></span> " +
                                                "<br />" +
                                                "<span style='width:100%; float: left; font-size: 13px; height:auto; overflow:hidden; font-family: Arial, Helvetica, sans-serif;'>:CONNECTED_TO_JOB_TITLE</span><br />" +
                                            "</td>" +
                                            "</tr>" +
                                    "</table>" +
                                "</td>" +
                            "</tr>" +
                            "<tr><td colspan='2'><span style='font-size:14px; font-style:italic; color: #ff5d00; text-align: justify; float:left; font-family: Arial, Helvetica, sans-serif;'>Want to convert :NEW_CONNECTION into a prospect? Ask us how by clicking <a href=':GET_MORE_DATA_LINK' style='font-size: 14px; text-decoration: none; color: #1369a2;'>here.</a></span></td></tr>" +
                            "<tr><td colspan='2' style='border-bottom: 1px solid #d2d3d3; padding-bottom:5px;'></td></tr>";
                            
        public const string MoreGoodNewsDetails = "<tr>" +
                "<td colspan='2'>" +
                    "<span style='font-size: 15px; line-height:20px; color: #ff5d00; text-align: justify; font-family:Arial, Helvetica, sans-serif;'><strong>More&nbsp;good&nbsp;News!</strong></span><br />" +
                    "<span style='font-size: 15px; color: #252525; text-align: justify; font-family: Arial, Helvetica, sans-serif;'>The following person in your network now works at a target company you are interested in:</span>" +
                "</td>" +
            "</tr>" +
            "<tr>" +
                "<td colspan='2'>" +
                    "<table align='left' cellpadding='5' cellspacing='0' style='width:100%'>" +
                        "<tbody>" +
                            ":POSITION_UPDATE_SECTION" +
                        "</tbody>" +
                    "</table>" +
                "</td>" +
            "</tr>";

        public const string PositionUpdateSection =
                        "<tr>" +
                            "<td colspan='2' style='font-size:14px; font-weight:600; padding-top:10px; color: #000000; font-family:Arial, Helvetica, sans-serif;'>:POSITION_UPDATE_TYPE_AND_TARGET_NAME</td>" +
                        "</tr>:POSITION_UPDATE_DETAILS";

        public const string PositionUpdateDetails = "<tr>" +
                            "<td style='width:50%;'>" +
                                "<table align='left' cellpadding='0' cellspacing='0'>" +
                                    "<tr>" +
                                        "<td style='vertical-align:top;'>" +
                                            "<img style='border: 1px solid #d2d3d3; margin-top:4px;' width='38' height='38' title='' alt='' src='cid:connectionProfilePicUrl' />" +
                                        "</td>" +
                                        "<td style='vertical-align:top; padding-left:10px;'>" +
                                            "<span style='width:100%; font-size: 15px; margin-top:-5px; vertical-align:text-top; color: #0068a1; font-family: Calibri, arial, sans-serif;'>" +
                                                "<a href=':CONNECTION_PROFILE_URL' style='color:#0068a1; text-decoration: none;'><strong>:CONNECTION_FULL_NAME</strong></a></span>" +
                                            "<br />" +
                                            "<span style='width:100%; float: left; font-size: 13px; height:auto; overflow:hidden; font-family: Arial, Helvetica, sans-serif;'>:CONNECTION_JOB_TITLE</span><br />" +
                                        "</td>" +
                                    "</tr>" +
                                "</table>" +
                            "</td>" +
                            "<td style='width:50%;'>" +
                            "</td>" +
                        "</tr>" +
                        "<tr><td colspan='2' style='border-bottom: 1px solid #d2d3d3; padding-bottom:5px;'></td></tr>";

        public const string TwitterLeadTypeSection = "<tr>"+
                            "<td colspan='2' style='padding-bottom: 0px;'>"+
                                "<table align='left' cellpadding='5' cellspacing='0' style='width: 100%'>"+
                                    "<tbody>"+
                                        "<tr>"+
                                            "<td colspan='2' style='font-size: 14px; font-weight: 600; padding-top: 10px; color: #000000; background: #D2D2D2; font-family: Arial, Helvetica, sans-serif;'>:TARGET_NAME</td>"+
                                        "</tr>"+
                                        ":TWITTER_LEADS_SECTION"+
                                    "</tbody>"+
                                "</table>"+
                            "</td>"+
                        "</tr>";

        public const string TwitterLeadsSection = "<tr>"+
                                            "<td colspan='2' style='width: 100%; padding-top: 10px; vertical-align: top'>"+
                                                "<table align='left' cellpadding='0' cellspacing='0'>"+
                                                    "<tr>"+
                                                        "<td style='vertical-align: top;'>"+
                                                            "<img style='border: 1px solid #d2d3d3; margin-top: 4px;' width='38' height='38' title='' alt='' src=':PROFILE_PIC_URL' /></td>"+
                                                        "<td style='vertical-align: top; padding-left: 10px;'><span style='width: 100%; font-size: 15px; margin-top: -5px; margin-right: 5px; vertical-align: text-top; color: #0068a1; font-family: Calibri, arial, sans-serif;'><a href=':PROFILE_URL' target='_blank' style='color: #0068a1; text-decoration: none;'><strong>:USER_NAME</strong></a></span><br />" +
                                                            "<span style='width: 100%; float: left; font-size: 13px; height: auto; overflow: hidden; font-family: Arial, Helvetica, sans-serif;'>:TWITTER_HANDLE</span>"+
                                                            "<br />"+
                                                            "<span style='width: 100%; float: left; font-size: 13px; height: auto; overflow: hidden; font-family: Arial, Helvetica, sans-serif;'>:TWITTER_BIO</span>"+
                                                        "</td>"+
                                                    "</tr>"+
                                                    "<tr>"+
                                                        "<td colspan='2' style='padding-bottom: 5px; height: 5px;'></td>"+
                                                    "</tr>" +
                                                    "<tr>" +
                                                        "<td colspan='2'><span style='font-size: 14px; font-style: italic; color: #ff5d00; text-align: justify; float: left; font-family: Arial, Helvetica, sans-serif;'>Want to convert Tim into a prospect? Ask us how by clicking <a href=':GET_MORE_DATA_LINK' style='font-size: 14px; text-decoration: none; color: #1369a2;'>here.</a></span></td>" +
                                                    "</tr>" +
                                                "</table>"+
                                            "</td>" +
                                        "</tr>"+
                                        "<tr>" +
                                            "<td colspan='2' style='border-bottom: 1px solid #d2d3d3; padding-bottom: 5px;'></td>" +
                                        "</tr>";

        public const string LeadFromTwitterHeading = "<tr>"+
                                                        "<td colspan='2'>"+
                                                            "<span style='font-size: 15px; line-height:20px; color: #ff5d00; text-align: justify; font-family:Arial, Helvetica, sans-serif;'><strong>Leads&nbsp;from&nbsp;Twitter!</strong></span><br />" +
                                                        "</td>"+
                                                    "</tr>";

    }
}
