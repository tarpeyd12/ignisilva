using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ignisilva
{
    interface IXmlWritable
    {
        XmlWriter WriteXml( XmlWriter xml, string fmt = "b64" );
    }
}
