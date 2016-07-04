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

        public Int32 NumInputs { get { return Input.Length; } }
        public Int32 NumOutputs { get { return Output.Length; } }

        public UInt32 OutputHash { get; }
        public UInt32 InputHash { get; }

        public SampleData( byte[] input, byte[] output )
        {
            Input = input;
            Output = output;

            HashTableHashing.IHashAlgorithm hasher = new HashTableHashing.MurmurHash2Simple();

            OutputHash = hasher.Hash( Output );
            InputHash = hasher.Hash( Input );
        }

        public XmlWriter WriteXml( XmlWriter xml )
        {
            bool base64 = !false;

            xml.WriteStartElement( "sample" );
            xml.WriteStartElement( "input" ); 
            xml.WriteAttributeString( "format", base64 ? "b64" : "csv" );
            XmlHelper.WritebyteString( xml, Input, base64 );
            xml.WriteEndElement();
            xml.WriteStartElement( "output" );
            xml.WriteAttributeString( "format", base64 ? "b64" : "csv" );
            XmlHelper.WritebyteString( xml, Output, base64 );
            xml.WriteEndElement();
            xml.WriteEndElement();
            return xml;
        }

    }
}

