#define LOGGING_ENABLED
//#define LOG_FILE_ROTATION_ENABLED

using OFunnelEmailScheduler.OFunnelDbLogic;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OFunnelEmailScheduler.OFunnelUtilities
{
    class HelperMethods
    {
        static List<string> listOfBlacklistedWords = new List<string>();
        static object _singletonLock = new object();
        static object _singletonLockForAddLog = new object();

        /// <summary>
        /// This method verify that if data set is valid and contains data.
        /// </summary>
        /// <param name="dataSet">dataSet</param>
        /// <returns>Result true/false</returns>
        public static bool IsValidDataSet(DataSet dataSet)
        {
            bool isDataSetValid = false;
            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                isDataSetValid = true;
            }
            return isDataSetValid;
        }

        /// <summary>
        /// This method validate email address.
        /// </summary>
        /// <param name="emailId">Email address to validate</param>
        /// <returns>Result</returns>
        public static bool IsValidEmailId(string emailId)
        {
            bool isValidEmail = false;

            if (!string.IsNullOrEmpty(emailId))
            {
                Regex regex = new Regex(@"^([\w\.\+\-]+)@([\w\.\-]+)((\.(\w){2,4})+)$");
                Match match = regex.Match(emailId);
                if (match.Success)
                {
                    isValidEmail = true;
                }
            }

            return isValidEmail;
        }

        /// <summary>
        /// This method gets execute directory path.
        /// </summary>
        /// <returns></returns>
        public static string GetExeDir()
        {
            System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
            string codeBase = System.IO.Path.GetDirectoryName(ass.CodeBase);
            System.Uri uri = new Uri(codeBase);
            return uri.LocalPath;
        }

        /// <summary>
        /// This method create parameters list message to add in logs. 
        /// </summary>
        /// <param name="nameValue">nameValue</param>
        /// <returns>message</returns>
        public static string GetParametersListForLogMessage(NameValueCollection nameValue)
        {
            string message = string.Empty;

            foreach (string key in nameValue.AllKeys)
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = "{ ";
                }
                else
                {
                    message += ", ";
                }

                message += key + " = " + nameValue[key];
            }

            message += " }";

            return message;
        }

        /// <summary>
        /// This method adds logs in log file.
        /// </summary>
        /// <param name="logInfo"> logInfo</param>
        /// <returns>Status true/false</returns>
        public static bool AddLogs(string logInfo)
        {
            bool status = false;

#if LOGGING_ENABLED

            try
            {
                // Acquire lock before to write log in file, as multiple thread may write logs at same time.
                lock (_singletonLockForAddLog)
                {
                    string appPath = HelperMethods.GetExeDir();

                    string logPath = appPath + "\\Logs\\";

                    bool folderExists = Directory.Exists(logPath);
                    if (!folderExists)
                    {
                        Directory.CreateDirectory(logPath);
                    }

                    string fileName = String.Format("{0:M-d-yyyy}", DateTime.Now);

                    //get the path of the server and set it to a variable
                    string filePath = string.Format(logPath + fileName + ".txt");

#if LOG_FILE_ROTATION_ENABLED 
                    // Move log file if log file size increase to MaxFileSizeInBytes.

                    FileInfo fileInfo = new FileInfo(filePath);

                    if (fileInfo.Exists && fileInfo.Length >= Config.MaxFileSizeInBytes)
                    {
                        string bakUpFileName = DateTime.Now.ToString("M-d-yyyy_hh-mm-ss");
                        string bakUpFileFilePath = string.Format(logPath + bakUpFileName + ".txt");
                        System.IO.File.Move(filePath, bakUpFileFilePath);
                    }
#endif
                    //open the file if exists or create new file with the file name above
                    using (System.IO.StreamWriter logWriter = new System.IO.StreamWriter(filePath, true))
                    {
                        //Write the log
                        logWriter.WriteLine(string.Format("{0}  --  {1}", DateTime.Now.ToString("MM/dd/yyyy   hh:mm:ss.fff tt   zzz"), logInfo));
                    }

                    status = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to write logs in file. Exception: " + ex.Message);
            }

#endif
            return status;
        }

        ///// <summary>
        ///// This method to check is two company name matched.
        ///// </summary>
        ///// <param name="firstCompanyName">firstCompanyName</param>
        ///// <param name="secondCompanyName">secondCompanyName</param>
        ///// <returns></returns>
        //public static bool CheckCompanyNameMatched(string firstCompanyName, string secondCompanyName)
        //{
        //    bool isCompanyMatched = false;

        //    if (listOfBlacklistedWords.Count == 0)
        //    {
        //        OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
        //        DataSet dataSet = databaseHandler.GetBlacklistedWords();
        //        // creating list of Blacklisted Words to compare two companies.
        //        if (HelperMethods.IsValidDataSet(dataSet) && dataSet.Tables[0].Rows.Count > 0)
        //        {
        //            for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
        //            {
        //                listOfBlacklistedWords.Add(Convert.ToString(dataSet.Tables[0].Rows[i]["words"]));
        //            }
        //        }
        //    }

        //    string firstCompanyNameAfterIgnoringWord = IgnoreUnconsideredWordFromCompany(firstCompanyName, listOfBlacklistedWords);
        //    string secondCompanyNameAfterIgnoringWord = IgnoreUnconsideredWordFromCompany(secondCompanyName, listOfBlacklistedWords);

        //    if (firstCompanyNameAfterIgnoringWord.Equals(secondCompanyNameAfterIgnoringWord))
        //    {
        //        isCompanyMatched = true;
        //    }

        //    return isCompanyMatched;
        //}

        ///// <summary>
        ///// This method removes specfic words from company name.
        ///// </summary>
        ///// <param name="companyName">companyName</param>
        ///// <returns>string with replaced words form string.</returns>
        //public static string IgnoreUnconsideredWordFromCompany(string companyName, List<string> listOfBlacklistedWordsToMatchCompany)
        //{
        //    string companyNameAfterIgnoringBlacklisteddWord = companyName;
        //    string replaceWith = string.Empty;

        //    if (!string.IsNullOrEmpty(companyNameAfterIgnoringBlacklisteddWord))
        //    {
        //        if (listOfBlacklistedWordsToMatchCompany != null)
        //        {
        //            Regex regexToRemoveSpecialChar = new Regex("(?:[^a-z0-9% ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        //            companyNameAfterIgnoringBlacklisteddWord = regexToRemoveSpecialChar.Replace(companyNameAfterIgnoringBlacklisteddWord, String.Empty);

        //            foreach (string word in listOfBlacklistedWordsToMatchCompany)
        //            {
        //                string wordToIgnore = regexToRemoveSpecialChar.Replace(word, String.Empty);
        //                string pattern = @"\b" + wordToIgnore + "\\b";
        //                string replace = string.Empty;

        //                companyNameAfterIgnoringBlacklisteddWord = Regex.Replace(companyNameAfterIgnoringBlacklisteddWord, pattern, replace, RegexOptions.IgnoreCase);
        //                companyNameAfterIgnoringBlacklisteddWord = companyNameAfterIgnoringBlacklisteddWord.Replace("  ", " ").Trim();
        //            }

        //            companyNameAfterIgnoringBlacklisteddWord = companyNameAfterIgnoringBlacklisteddWord.ToUpper();
        //        }
        //    }

        //    return companyNameAfterIgnoringBlacklisteddWord;
        //}

        /// <summary>
        /// This method to check is two company name matched.
        /// </summary>
        /// <param name="firstCompanyName">companyNameToSearch</param>
        /// <param name="secondCompanyName">targetCompanyName</param>
        /// <returns></returns>
        public static bool CheckCompanyNameMatched(string companyNameToSearch, string targetCompanyName)
        {
            bool isCompanyMatched = false;

            if (targetCompanyName.ToUpper().Contains(companyNameToSearch.ToUpper()))
            {
                isCompanyMatched = true;
            }

            return isCompanyMatched;
        }

        /// <summary>
        /// This method removes specfic words(blacklist words) from company name.
        /// </summary>
        /// <param name="companyName">companyName</param>
        /// <returns>string with replaced words form string.</returns>
        public static string IgnoreBlackListWordFromCompanyName(string companyName)
        {
            if (listOfBlacklistedWords.Count == 0)
            {
                lock (_singletonLock)
                {
                    if (listOfBlacklistedWords.Count == 0)
                    {
                        HelperMethods.AddLogs("IgnoreBlackListWordFromCompanyName: ListOfBlacklistedWords count is zero. So get List of blacklist companies from database.");

                        OFunnelDatabaseHandler databaseHandler = new OFunnelDatabaseHandler();
                        DataSet dataSet = databaseHandler.GetBlacklistedWords();
                        // creating list of Blacklisted Words to compare two companies.
                        if (HelperMethods.IsValidDataSet(dataSet) && dataSet.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                            {
                                listOfBlacklistedWords.Add(Convert.ToString(dataSet.Tables[0].Rows[i]["words"]));
                            }
                        }
                    }
                }
            }

            string companyNameAfterIgnoringBlacklisteddWord = companyName;

            if (!string.IsNullOrEmpty(companyNameAfterIgnoringBlacklisteddWord))
            {
                if (listOfBlacklistedWords != null && listOfBlacklistedWords.Count > 0)
                {
                    StringBuilder companyNameBuilder = new StringBuilder(" " + companyNameAfterIgnoringBlacklisteddWord.ToUpper() + " ");

                    string replaceWith = " ";

                    foreach (string blackListWord in listOfBlacklistedWords)
                    {
                        companyNameBuilder.Replace(" " + blackListWord.ToUpper() + " ", replaceWith);
                    }

                    companyNameAfterIgnoringBlacklisteddWord = companyNameBuilder.ToString().Trim();
                }
            }

            return companyNameAfterIgnoringBlacklisteddWord;
        }

        /// <summary>
        /// This function formats the job title string to fit in three line on email template.
        /// </summary>
        /// <param name="jobTitleString"></param>
        /// <returns>returns formated job title string</returns>
        public static string FormatYourConnectionJobTitle(string jobTitleString)
        {
            HelperMethods.AddLogs("Enter into FormatYourConnectionJobTitle");

            int noOfCharInOneLine = 27;
            string formatedString = string.Empty;
            string jobTitle = jobTitleString;
            string lastCharOfEachLine = string.Empty;

            if (jobTitleString.Length > noOfCharInOneLine)
            {
                HelperMethods.AddLogs(string.Format("FormatYourConnectionJobTitle Actual job title String-> {0}.", jobTitleString));

                for (int i = 0; i < 3; i++)
                {
                    string tempSubString = string.Empty;

                    if (jobTitle.Length > noOfCharInOneLine)
                    {
                        lastCharOfEachLine = jobTitle.Substring(noOfCharInOneLine, 1);

                        if (string.IsNullOrWhiteSpace(lastCharOfEachLine))
                        {
                            formatedString += jobTitle.Substring(0, noOfCharInOneLine) + (i == 2 ? "..." : "<br />");
                            jobTitle = jobTitle.Substring(noOfCharInOneLine);
                        }
                        else
                        {
                            tempSubString = jobTitle.Substring(0, noOfCharInOneLine);
                            if (tempSubString.LastIndexOf(" ") > 0)
                            {
                                formatedString += tempSubString.Substring(0, tempSubString.LastIndexOf(" ")) + (i == 2 ? "..." : "<br />");
                                jobTitle = jobTitle.Substring(tempSubString.LastIndexOf(" "));
                            }
                            else
                            {
                                formatedString += jobTitle.Substring(0, noOfCharInOneLine) + (i == 2 ? "..." : "<br />");
                                jobTitle = jobTitle.Substring(noOfCharInOneLine);
                            }
                        }

                        HelperMethods.AddLogs(string.Format("FormatYourConnectionJobTitle job title Substring({0})-> {1}.", i.ToString(), jobTitle));
                    }
                    else
                    {
                        formatedString += jobTitle;
                        break;
                    }
                }
            }
            else
            {
                formatedString = jobTitleString;
            }

            HelperMethods.AddLogs("Exit from FormatYourConnectionJobTitle");

            return formatedString;
        }

        /// <summary>
        /// This method retruns company size i.e  (maxValue + minValue)/2
        /// </summary>
        /// <param name="companySizeData">companySizeData</param>
        /// <returns>average company size</returns>
        public static int GetAverageCompanySize(string companySizeData)
        {
            int companySize = -1;

            string sizeString = SecurityElement.Escape(companySizeData);

            if (!string.IsNullOrEmpty(sizeString))
            {
                List<Int32> sizeList = new List<Int32>();

                sizeString = sizeString.Replace(",", string.Empty);

                Regex regex = new Regex("\\d+");
                Match match = regex.Match(sizeString);
                while (match.Success)
                {
                    sizeList.Add(Convert.ToInt32(match.Value));
                    match = match.NextMatch();
                }

                if (sizeList != null && sizeList.Count > 0)
                {
                    companySize = (sizeList.Max() + sizeList.Min()) / 2;
                }
            }

            if (companySize == -1)
            {
                companySize = 1;
            }

            return companySize;
        }
    }
}
