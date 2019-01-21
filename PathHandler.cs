using System.Collections.Specialized;
using System.Configuration;

namespace ExternalSQL.Net
{
    public class PathHandler : IConfigurationSectionHandler
    {
        public PathHandler()
        {    
        }
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            NameValueCollection configs;
            NameValueSectionHandler baseHandler = new NameValueSectionHandler();
            configs = (NameValueCollection)baseHandler.Create(parent, configContext, section);
            return configs;
        }


    }
}
