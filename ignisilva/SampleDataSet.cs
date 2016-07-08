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

        public Int32 NumUniqueOutputs { get { return uniqueOutputSets.Count; } }

        public bool IsPureSet { get { return NumUniqueOutputs == 1; } }

        private List<SampleData> dataSet;
        private Dictionary<UInt32, List<SampleData>> uniqueOutputSets;

        public SampleDataSet( Int32 numInputs, Int32 numOutputs )
        {
            NumInputs = numInputs;
            NumOutputs = numOutputs;

            dataSet = new List<SampleData>();
            uniqueOutputSets = new Dictionary<UInt32, List<SampleData>>();
        }

        private SampleDataSet( Int32 numInputs, Int32 numOutputs, List<SampleData> data )
        {
            NumInputs = numInputs;
            NumOutputs = numOutputs;

            dataSet = new List<SampleData>();
            uniqueOutputSets = new Dictionary<UInt32, List<SampleData>>();

            foreach( SampleData sample in data )
            {
                AddData( sample );
            }
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

        public SampleDataSet RandomSubSet( Int32 Num, Random random = null )
        {
            SampleDataSet subSampleSet = new SampleDataSet( NumInputs, NumOutputs );

            List<Int32> indexes = Func.UniqueRandomNumberRange( Num, 0, NumSamples, random == null ? new Random() : random );

            foreach( Int32 index in indexes )
            {
                subSampleSet.AddData( dataSet[index] );
            }

            return subSampleSet;
        }

        public SampleDataSet[] SplitSet( Int32 splitIndex, byte splitValue )
        {
            SampleDataSet[] subSampleSet = new SampleDataSet[2];
            subSampleSet[0] = new SampleDataSet( NumInputs, NumOutputs );
            subSampleSet[1] = new SampleDataSet( NumInputs, NumOutputs );

            foreach( SampleData sample in dataSet )
            {
                subSampleSet[sample.Input[splitIndex] < splitValue ? 0 : 1].AddData( sample );
            }

            return subSampleSet;
        }

        public UInt32 GetOutputHashCodeFromFlatOutputID( Int32 OutputID  )
        {
            return uniqueOutputSets.ToList()[OutputID].Key;
        }

        // TODO: make this faster, could be extreemly slow
        public Int32 GetFlatOutputIDFromOutputHashCode( UInt32 OutputHash )
        {
            for( Int32 i = 0; i < NumUniqueOutputs; ++i )
            {
                if( OutputHash == GetOutputHashCodeFromFlatOutputID( i ) )
                {
                    return i;
                }
            }
            // error not found
            return -1;
        }

        public byte[] GetUniqueOutput( Int32 OutputID )
        {
            return uniqueOutputSets.ToList()[OutputID].Value[0].Output;
        }

        public SampleDataSet SubSetByOutput( Int32 OutputID )
        {
            if( OutputID >= NumUniqueOutputs )
            {
                return null;
            }

            // turn the sequential OutputID into the hashed code for the unique outputs key, and get that list
            // then convert to SampleDataSet
            return new SampleDataSet( NumInputs, NumOutputs, uniqueOutputSets[GetOutputHashCodeFromFlatOutputID( OutputID )] );
        }

        public Int32 GetNumSetsByUniqueOutput( Int32 OutputID )
        {
            if( OutputID >= NumUniqueOutputs )
            {
                return 0;
            }

            // turn the sequential OutputID into the hashed code for the unique outputs key, and get that list
            // then get the number of enteries
            return uniqueOutputSets[GetOutputHashCodeFromFlatOutputID( OutputID )].Count;
        }

        public Int32[,] GetInputHistogram( Int32 InputIndex )
        {
            Int32[,] output = new Int32[256,NumUniqueOutputs];

            // generate a table of the uniques hashes to the flatoutput index
            Dictionary<UInt32, Int32> _short = new Dictionary<UInt32,Int32>();
            for( Int32 i = 0; i < NumUniqueOutputs; ++i )
            {
                _short.Add( GetOutputHashCodeFromFlatOutputID( i ), i );
            }

            foreach( SampleData sample in dataSet )
            {
                byte value = sample.Input[InputIndex];

                //Int32 on = GetFlatOutputIDFromOutputHashCode( sample.OutputHash );

                output[value, _short[sample.OutputHash]]++;
            }

            return output;
        }

        public Int32[,,] GetOutputHistogram()
        {
            Int32[,,] output = new Int32[NumUniqueOutputs,NumInputs,256];

            for( Int32 i = 0; i < NumUniqueOutputs; ++i )
            {
                List<SampleData> samples = uniqueOutputSets.ToList()[i].Value;

                foreach( SampleData sample in samples )
                {
                    for( Int32 ix = 0; ix < sample.NumInputs; ++ix )
                    {
                        output[i, ix, sample.Input[ix]]++;
                    }
                }
            }

            return output;
        }

        public Int32[,] GetOutputMinMaxOfInputIndex( Int32 inputIndex )
        {
            Int32[,] output = new Int32[NumUniqueOutputs,2];

            for( Int32 i = 0; i < NumUniqueOutputs; ++i )
            {
                List<SampleData> samples = uniqueOutputSets.ToList()[i].Value;

                output[i, 0] = output[i, 1] = samples[0].Input[0];

                foreach( SampleData sample in samples )
                {
                    if( sample.Input[inputIndex] < output[i, 0] ) { output[i, 0] = sample.Input[inputIndex]; }
                    if( sample.Input[inputIndex] > output[i, 1] ) { output[i, 1] = sample.Input[inputIndex]; }
                }
            }

            return output;
        }

        public float GetEntropy()
        {
            // http://www.saedsayad.com/decision_tree.htm

            Int32[] numSamplesPerOutput = new Int32[NumUniqueOutputs];
            float a = 0.0f;

            for( int i = 0; i < NumUniqueOutputs; ++i )
            {
                numSamplesPerOutput[i] = uniqueOutputSets.ToList()[i].Value.Count;
                a += (float)numSamplesPerOutput[i] * (float)numSamplesPerOutput[i];
            }
            a = (float)Math.Sqrt( a );

            float[] normalizedSPO = new float[NumUniqueOutputs];

            for( int i = 0; i < NumUniqueOutputs; ++i )
            {
                normalizedSPO[i] = (float)numSamplesPerOutput[i] / a;
            }

            float entropyValue = 0.0f;

            // 1.0f for natural entropy bit
            //  (float)Math.Log( 2.0f ) for shannon
            // (float)Math.Log( 10.0f ) for hartley
            float divlog = NumUniqueOutputs;// (float)Math.Log(2.0f);

            for( int i = 0; i < NumUniqueOutputs; ++i )
            {
                // I think this will be correct
                //Console.WriteLine("p_i={0},E()={1}", normalizedSPO[i], -( normalizedSPO[i] * ((float)Math.Log( normalizedSPO[i] )/ divlog ) ) );
                entropyValue += -( normalizedSPO[i] * ( (float)Math.Log( normalizedSPO[i] ) / divlog ) );
            }

            return entropyValue;
        }



        public XmlWriter WriteXml( XmlWriter xml, string fmt = "b64" )
        {
            xml.WriteStartElement( "sample_set" );
            xml.WriteAttributeString( "num", NumSamples.ToString() );
            xml.WriteAttributeString( "inputs", NumInputs.ToString() );
            xml.WriteAttributeString( "outputs", NumOutputs.ToString() );

            // write similar outputs together
            foreach( KeyValuePair<UInt32, List<SampleData>> listPair in uniqueOutputSets )
            {
                foreach( SampleData sampleData in listPair.Value )
                {
                    sampleData.WriteXml( xml, fmt );
                }
            }

            xml.WriteEndElement();
            return xml;
        }

    }
}
