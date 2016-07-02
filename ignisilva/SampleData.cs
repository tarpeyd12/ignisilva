using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ignisilva
{
    class SampleData : IXmlWritable
    {

        public byte[] Input { get; }
        public byte[] Output { get; }

        public SampleData( byte[] input, byte[] output )
        {
            Input = input;
            Output = output;
        }

        public XmlWriter WriteXml( XmlWriter xml )
        {
            xml.WriteStartElement( "sample" );
            xml.WriteStartElement( "input" );   
            xml.WriteAttributeString( "format", "csv" );
            for( Int32 i = 0; i < Input.Length; ++i )
            {
                xml.WriteValue( Input[i] );
                if( i != Input.Length - 1 )
                {
                    xml.WriteString( "," );
                }
            }
            xml.WriteEndElement();
            xml.WriteStartElement( "output" );
            xml.WriteAttributeString( "format", "csv" );
            for( Int32 i = 0; i < Output.Length; ++i )
            {
                xml.WriteValue( Output[i] );
                if( i != Output.Length - 1 )
                {
                    xml.WriteString( "," );
                }
            }
            xml.WriteEndElement();
            xml.WriteEndElement();
            return xml;
        }

    }
}

