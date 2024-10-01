using DiGi.BDOT10k.UI.Enums;
using System;

namespace DiGi.BDOT10k.UI.Classes
{
    public class ReportRecord
    {
        private DateTime dateTime;
        private string uniqueId;
        private ReportType reportType;
        private string description;

        public ReportRecord(ReportType reportType, string uniqueId, string description)
        {
            dateTime = DateTime.Now;

            this.reportType = reportType;
            this.uniqueId = uniqueId;
            this.description = description;
        }

        public ReportRecord(DateTime dateTime, ReportType reportType, string uniqueId, string description)
        {
            this.dateTime = dateTime;
            this.reportType = reportType;
            this.uniqueId = uniqueId;
            this.description = description;
        }

        public ReportRecord(ReportRecord reportRecord)
        {
            if(reportRecord != null)
            {
                dateTime = reportRecord.dateTime;
                reportType = reportRecord.reportType;
                uniqueId = reportRecord.uniqueId;
                description = reportRecord.description;
            }
        }

        public string UniqueId
        {
            get
            {
                return uniqueId;
            }
        }

        public ReportType ReportType
        {
            get
            {
                return reportType;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
        }

        public override string ToString()
        {
            return string.Format("[DateTime: {0}]\t[UniqueId: {1}]\t[ReportType: {2}]\t[Description: \"{3}\"]", dateTime.ToString("yyyy/MM/dd HH:mm:ss"), uniqueId == null ? string.Empty : uniqueId, reportType, description == null ? string.Empty : description);
        }
    }
}
