using DiGi.BDOT10k.UI.Enums;
using System.Collections.Generic;

namespace DiGi.BDOT10k.UI.Classes
{
    public class Report
    {       
        private List<ReportRecord> reportRecords = new List<ReportRecord>();

        public string Path { get; set; }

        public Report()
        {

        }

        public Report(string path)
        {
            Path = path;
        }

        public ReportRecord Add(ReportType reportType, string uniqueId, string description)
        {
            ReportRecord reportRecord = new ReportRecord(reportType, uniqueId, description);

            reportRecords.Add(new ReportRecord(reportRecord));

            return reportRecord;
        }

        public bool IsEmpty()
        {
            return reportRecords == null || reportRecords.Count == 0;
        }

        public void Clear()
        {
            reportRecords.Clear();
        }

        public bool Write(string path)
        {
            Path = path;

            return Write();
        }

        public bool Write()
        {
            if(string.IsNullOrWhiteSpace(Path))
            {
                return false;
            }

            string directory = System.IO.Path.GetDirectoryName(Path);
            if(!System.IO.Directory.Exists(directory))
            {
                return false;
            }

            List<string> lines = new List<string>();

            foreach(ReportRecord reportRecord in reportRecords)
            {
                string line = reportRecord?.ToString();
                if(line == null)
                {
                    continue;
                }


                lines.Add(line);
            }

            System.IO.File.WriteAllLines(Path, lines);
            return true;
        }
    }
}
