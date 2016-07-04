using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ignisilva
{
    class SampleDataSet : IXmlWritable
    {
        public Int32 NumInputs { get; }
        public Int32 NumOutputs { get; }

        public Int32 NumSamples { get { return dataSet.Count; } }

        private List<SampleData> dataSet;
        private Dictionary<UInt32, List<SampleData>> uniqueOutputSets;

        SampleDataSet( Int32 numInputs, Int32 numOutputs )
        {
            NumInputs = numInputs;
            NumOutputs = numOutputs;

            dataSet = new List<SampleData>();
            uniqueOutputSets = new Dictionary<UInt32, List<SampleData>>();
        }

        public bool AddData( SampleData data )
        {
            if( data.NumInputs != NumInputs || data.NumOutputs != NumOutputs )
            {
                return false;
            }

            dataSet.Add( data );

            UInt32 outputHash = data.OutputHash;

            if( uniqueOutputSets.ContainsKey( outputHash ) )
            {
                uniqueOutputSets[outputHash].Add( data );
            }
            else
            {
                List<SampleData> newList = new List<SampleData>();
                newList.Add( data );
                uniqueOutputSets.Add( outputHash, newList );
            }

            return true;
        }

        public XmlWriter WriteXml( XmlWriter xml )
        {
            xml.WriteStartElement( "sample_set" );
            xml.WriteAttributeString( "num", NumSamples.ToString() );
            xml.WriteAttributeString( "inputs", NumInputs.ToString() );
            xml.WriteAttributeString( "outputs", NumOutputs.ToString() );

            // write similar outputs together
            /*foreach( KeyValuePair<UInt32, List<SampleData>> listPair in uniqueOutputSets )
            {
                foreach( SampleData sampleData in listPair.Value )
                {
                    sampleData.WriteXml( xml );
                }
            }*/

            foreach( SampleData sampleData in dataSet )
            {
                sampleData.WriteXml( xml );
            }

            xml.WriteEndElement();
            return xml;
        }

    }
}
