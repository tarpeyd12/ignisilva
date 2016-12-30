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

        public XmlWriter WriteXml( XmlWriter xml, string fmt = "b64" )
        {
            xml.WriteStartElement( "sample" );
            xml.WriteStartElement( "input" );
            string inputFormat = XmlHelper.RecommendBinaryEncodingFormat( fmt, Input );
            xml.WriteAttributeString( "format", inputFormat );
            XmlHelper.WriteByteString( xml, Input, inputFormat );
            xml.WriteEndElement();
            xml.WriteStartElement( "output" );
            string outputFormat = XmlHelper.RecommendBinaryEncodingFormat( fmt, Output );
            xml.WriteAttributeString( "format", outputFormat );
            XmlHelper.WriteByteString( xml, Output, outputFormat );
            xml.WriteEndElement();
            xml.WriteEndElement();
            return xml;
        }

    }
}

