using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFunnelEmailScheduler.OFunnelUtilities
{
    public class XmlData
    {
        /************************** Company Update Data *************************************/

        public string CompanyDetailsData = "<CompanyDetails>" +
                                                ":CompanyDetailTemplate" +
                                            "</CompanyDetails>";

        public string CompanyDetailTemplate = "<CompanyDetail>" +
                                                        "<CompanyId>:CompanyIdValue</CompanyId>" +
                                                        "<Website>:WebsiteValue</Website>" +
                                                        "<SubIndustry>:SubIndustryValue</SubIndustry>" +
                                                        "<EmployeeCount>:EmployeeCountValue</EmployeeCount>" +
                                                        "<State>:StateValue</State>" +
                                                        "<CompanyType>:CompanyTypeValue</CompanyType>" +
                                                    "</CompanyDetail>";
    }
}
