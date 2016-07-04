using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ignisilva
{
    class XmlHelper
    {
        public static XmlWriter WriteByteStringCSV( XmlWriter xml, byte[] byteData )
        {
            for( Int32 i = 0; i < byteData.Length; ++i )
            {
                xml.WriteValue( byteData[i] );
                if( i != byteData.Length - 1 )
                {
                    xml.WriteString( "," );
                }
            }
            return xml;
        }

        public static XmlWriter WriteByteStringBase64( XmlWriter xml, byte[] byteData )
        {
            xml.WriteBase64( byteData, 0, byteData.Length );
            return xml;
        }

        public static XmlWriter WritebyteString( XmlWriter xml, byte[] byteData, bool base64 = false )
        {
            return base64 ? WriteByteStringBase64( xml, byteData ) : WriteByteStringCSV( xml, byteData );
        }
    }
}
