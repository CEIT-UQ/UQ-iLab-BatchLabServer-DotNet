using System;
using System.Collections.Generic;
using System.Text;
using Library.LabClient.Engine;
using Library.Lab;

namespace Library.LabClient
{
    public class ExperimentSpecification : LabExperimentSpecification
    {
        #region Constants
        private const String STR_ClassName = "LabExperimentResult";
        private const Logfile.Level logLevel = Logfile.Level.Finest;
        #endregion

        #region Properties
        private String source;
        private String absorbers;
        private String distances;
        private String duration;
        private String trials;

        public String Source
        {
            get { return source; }
            set { source = value; }
        }

        public String Absorbers
        {
            get { return absorbers; }
            set { absorbers = value; }
        }

        public String Distances
        {
            get { return distances; }
            set { distances = value; }
        }

        public String Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        public String Trials
        {
            get { return trials; }
            set { trials = value; }
        }
        #endregion

        //-------------------------------------------------------------------------------------------------//

        public ExperimentSpecification(String xmlSpecification)
            : base(xmlSpecification)
        {
            const String methodName = "ExperimentSpecification";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Check that all required XML nodes exist
                 */
                XmlUtilities.GetChildNode(this.nodeSpecification, Consts.STRXML_SourceName);
                XmlUtilities.GetChildNode(this.nodeSpecification, Consts.STRXML_AbsorberName);
                XmlUtilities.GetChildNode(this.nodeSpecification, Consts.STRXML_Distance);
                XmlUtilities.GetChildNode(this.nodeSpecification, Consts.STRXML_Duration);
                XmlUtilities.GetChildNode(this.nodeSpecification, Consts.STRXML_Repeat);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw ex;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public override String ToXmlString()
        {
            const String methodName = "ToXmlString";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            String xmlString = null;

            try
            {
                /*
                 * Call super to create the XML document and add its part
                 */
                if (base.ToXmlString() != null)
                {
                    /*
                     * Add the experiment specification information to the XML document
                     */
                    XmlUtilities.SetChildValue(this.nodeSpecification, Consts.STRXML_SourceName, this.source);
                    XmlUtilities.SetChildValue(this.nodeSpecification, Consts.STRXML_AbsorberName, this.absorbers);
                    XmlUtilities.SetChildValue(this.nodeSpecification, Consts.STRXML_Distance, this.distances);
                    XmlUtilities.SetChildValue(this.nodeSpecification, Consts.STRXML_Duration, this.duration);
                    XmlUtilities.SetChildValue(this.nodeSpecification, Consts.STRXML_Repeat, this.trials);

                    /*
                     * Convert the XML document to an XML string
                     */
                    xmlString = XmlUtilities.ToXmlString(this.nodeSpecification);
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                xmlString = null;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return xmlString;
        }
    }
}
