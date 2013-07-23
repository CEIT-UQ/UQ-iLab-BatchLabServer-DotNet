using System;
using System.Xml.Serialization;

namespace Library.Lab.Types
{
    [Serializable]
    [XmlType(Namespace = "http://ilab.mit.edu")]
    public class ResultReport
    {
        private StatusCodes statusCode;
        private String xmlExperimentResults;
        private String xmlResultExtension;
        private String xmlBlobExtension;
        private String[] warningMessages;
        private String errorMessage;

        //-------------------------------------------------------------------------------------------------//

        [XmlIgnore]
        public static string ClassName
        {
            get { return "ResultReport"; }
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Indicates the status of this experiment.
        /// </summary>
        [XmlElement("statusCode")]
        public int IntStatusCode
        {
            get { return (int)statusCode; }
            set { statusCode = (StatusCodes)value; }
        }

        [XmlIgnore]
        public StatusCodes StatusCode
        {
            get { return statusCode; }
            set { statusCode = value; }
        }

        /// <summary>
        /// An opaque, domain-dependent set of experiment results.
        /// [REQUIRED if experimentStatus == Completed (3), OPTIONAL if experimentStatus == Failed (4)].
        /// </summary>
        [XmlElement("experimentResults")]
        public String XmlExperimentResults
        {
            get { return xmlExperimentResults; }
            set { xmlExperimentResults = value; }
        }

        /// <summary>
        /// A transparent XML string that helps to identify this experiment. Used for indexing and querying in generic
        /// components which can't understand the opaque experimentSpecification and experimentResults.
        /// [OPTIONAL, null if unused].
        /// </summary>
        [XmlElement("xmlResultExtension")]
        public String XmlResultExtension
        {
            get { return xmlResultExtension; }
            set { xmlResultExtension = value; }
        }

        /// <summary>
        /// A transparent XML string that helps to identify any blobs saved as part of this experiment's results.
        /// [OPTIONAL, null if unused].
        /// </summary>
        [XmlElement("xmlBlobExtension")]
        public String XmlBlobExtension
        {
            get { return xmlBlobExtension; }
            set { xmlBlobExtension = value; }
        }

        /// <summary>
        /// Domain-dependent human-readable text containing non-fatal warnings about the experiment including runtime warnings.
        /// </summary>
        [XmlArray("warningMessages")]
        public String[] WarningMessages
        {
            get { return warningMessages; }
            set { warningMessages = value; }
        }

        /// <summary>
        /// Domain-dependent human-readable text describing why the experiment terminated abnormally including runtime errors.
        /// [REQUIRED if experimentStatus == Failed (4)].
        /// </summary>
        [XmlElement("errorMessage")]
        public String ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; }
        }

        //-------------------------------------------------------------------------------------------------//

        public ResultReport()
        {
            this.statusCode = StatusCodes.Unknown;
        }

        public ResultReport(StatusCodes statusCode)
        {
            this.statusCode = statusCode;
        }

        public ResultReport(StatusCodes statusCode, string errorMessage)
        {
            this.statusCode = statusCode;
            this.errorMessage = errorMessage;
        }
    }
}
