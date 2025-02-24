using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace ACO2_App._0.INIT
{
    public class XmlHelper<T> where T : class, new()
    {

        public static string SerializeToString(T TObj)
        {
            try
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    OmitXmlDeclaration = true

                };
                //Create our own namespaces for the output
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
                //Add an empty namespace and empty value
                //ns.Add("", "");
                //XmlWriterSettings set = new XmlWriterSettings();
                //   set.Indent = true;
                StringWriter sw = new StringWriter();

                using (XmlWriter writer = XmlWriter.Create(sw, settings))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(writer, TObj, ns);
                }
                return sw.GetStringBuilder().ToString();
            }
            catch (Exception ex)
            {
                var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", MethodBase.GetCurrentMethod().DeclaringType.Name.ToString(), MethodBase.GetCurrentMethod().Name, ex.Message);
                LogTxt.Add(LogTxt.Type.Exception, debug);
                return string.Empty;
            }
        }
        public static T DeserializeFromString(string str)
        {
            try
            {
                StringReader sr = new StringReader(str);
                using (XmlReader reader = XmlReader.Create(sr))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    return serializer.Deserialize(reader) as T;
                }
            }
            catch (Exception ex)
            {
                var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", MethodBase.GetCurrentMethod().DeclaringType.Name.ToString(), MethodBase.GetCurrentMethod().Name, ex.Message);
                LogTxt.Add(LogTxt.Type.Exception, debug);
                return null;
            }
        }
       
    }
}
