using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        private static byte[] GZipByteArray( byte[] byteData )
        {
            using( MemoryStream msIn = new MemoryStream( byteData ) )
            {
                using( MemoryStream msOut = new MemoryStream() )
                {
                    using( GZipStream gs = new GZipStream( msOut, CompressionMode.Compress ) )
                    {
                        msIn.CopyTo( gs );
                        //CopyTo( msIn, gs );
                    }

                    return msOut.ToArray();
                }
            }
        }

        public static XmlWriter WriteByteStringGZip64( XmlWriter xml, byte[] byteData )
        {
            return WriteByteStringBase64( xml, GZipByteArray( byteData ) );
        }

        public static XmlWriter WriteByteString( XmlWriter xml, byte[] byteData, string fmt = "b64" )
        {
            ;

            switch( fmt )
            {
                case "b64":
                    return WriteByteStringBase64( xml, byteData );
                case "z64":
                    return WriteByteStringGZip64( xml, byteData );
                case "csv":
                default:
                    return WriteByteStringCSV( xml, byteData );
            }
        }
    }
}
