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
        private byte[,] inputMinMax; 

        public SampleDataSet( Int32 numInputs, Int32 numOutputs )
        {
            NumInputs = numInputs;
            NumOutputs = numOutputs;

            dataSet = new List<SampleData>();
            uniqueOutputSets = new Dictionary<UInt32, List<SampleData>>();

            inputMinMax = null;
        }

        private SampleDataSet( Int32 numInputs, Int32 numOutputs, List<SampleData> data )
        {
            NumInputs = numInputs;
            NumOutputs = numOutputs;

            dataSet = new List<SampleData>();
            uniqueOutputSets = new Dictionary<UInt32, List<SampleData>>();

            inputMinMax = null;

            foreach( SampleData sample in data )
            {
                AddData( sample );
            }
        }

        public bool AddData( SampleData data, bool minMax = true )
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

            if( minMax )
            {
                if( inputMinMax != null )
                {
                    for( Int32 i = 0; i < NumInputs; ++i )
                    {
                        if( data.Input[i] < inputMinMax[i, 0] ) inputMinMax[i, 0] = data.Input[i];
                        if( data.Input[i] > inputMinMax[i, 1] ) inputMinMax[i, 1] = data.Input[i];
                    }
                }
                else
                {
                    inputMinMax = new byte[NumInputs, 2];
                    for( Int32 i = 0; i < NumInputs; ++i )
                    {
                        inputMinMax[i, 0] = inputMinMax[i, 1] = data.Input[i];
                    }
                }
            }

            return true;
        }

        public bool AddData( SampleDataSet otherDataSet )
        {
            if( otherDataSet == null || otherDataSet.NumInputs != NumInputs || otherDataSet.NumOutputs != NumOutputs )
            {
                return false;
            }

            foreach( SampleData sample in otherDataSet.dataSet )
            {
                AddData( sample );
            }

            return true;
        }

        public SampleDataSet RandomSubSet( Int32 Num, Random random = null )
        {
            SampleDataSet subSampleSet = new SampleDataSet( NumInputs, NumOutputs );

            Int32[] indexes = Func.UniqueRandomNumberRange( Num, 0, NumSamples, random == null ? new Random() : random );

            foreach( Int32 index in indexes )
            {
                subSampleSet.AddData( dataSet[index] );
            }

            return subSampleSet;
        }

        public SampleDataSet[] SplitSet( Int32 splitIndex, byte splitValue, bool minMax = true )
        {
            SampleDataSet[] subSampleSet = new SampleDataSet[2];
            subSampleSet[0] = new SampleDataSet( NumInputs, NumOutputs );
            subSampleSet[1] = new SampleDataSet( NumInputs, NumOutputs );

            foreach( SampleData sample in dataSet )
            {
                subSampleSet[sample.Input[splitIndex] < splitValue ? 0 : 1].AddData( sample, minMax );
            }

            return subSampleSet;
        }

        public SampleData GetSample( Int32 index )
        {
            if( index >= NumSamples )
            {
                return null;
            }

            return dataSet[index];
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

        public Int32[,] GetOutputHistogram( Int32 InputIndex )
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

        public Int32[,,] GetInputHistogram()
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

        public byte[] GetAverageOutput()
        {
            Int32[] sums = new Int32[NumOutputs];
            
            for( Int32 i = 0; i < NumUniqueOutputs; ++i )
            {
                byte[] currentOutput = GetUniqueOutput( i );
                Int32 numCurrentOutputs = GetNumSetsByUniqueOutput( i );

                for( Int32 c = 0; c < NumOutputs; ++c )
                {
                    sums[c] += currentOutput[c] * numCurrentOutputs;
                }
            }

            byte[] avgOutput = new byte[NumOutputs];

            if( NumSamples == 0 )
            {
                Console.WriteLine( "Warning: GetAverageOutput() NO SAMPLES IN SET." );
                return avgOutput;
            }

            for( Int32 i = 0; i < NumOutputs; ++i )
            {
                avgOutput[i] = (byte)Func.Clamp( (Int32)( sums[i]/NumSamples ), 0, 255 );
            }

            return avgOutput;
        }

        public static float _Entropy( Int32[] S, Int32 total = -1 )
        {
            // this is an implementation of the Entropy function E(S) as described in:
            // http://www.saedsayad.com/decision_tree.htm

            if( total < 0 )
            {
                total = 0;
                foreach( Int32 i in S ) { total += i; }
            }

            float result = 0.0f;
            
            foreach( Int32 i in S )
            {
                float fraction = (float)i / (float)total;

                result += -( fraction * ( (float)Math.Log( fraction, 2.0 ) ) ); // log_2(x)
            }

            return result;
        }

        public float GetEntropy()
        {
            // this is a simplification of the Entropy function E(S) as described in:
            // http://www.saedsayad.com/decision_tree.htm
            // E(S) = this.GetEntropy() where this = S

            Int32[] numSamplesPerOutput = new Int32[NumUniqueOutputs];
            Int32 total = 0;

            KeyValuePair<UInt32,List<SampleData>>[] flatUniqueOutputs = uniqueOutputSets.ToArray();

            for( int i = 0; i < NumUniqueOutputs; ++i )
            {
                numSamplesPerOutput[i] = flatUniqueOutputs[i].Value.Count;
                total += numSamplesPerOutput[i];
            }
            
            return _Entropy( numSamplesPerOutput, total );
        }

        public float GetEntropy( Int32 splitIndex, byte splitValue )
        {
            // this is the Entropy function E(T,X) as described in:
            // http://www.saedsayad.com/decision_tree.htm
            // E(T,X) = this.GetEntropy( X ) where this = T and X is the split (splitIndex and the splitValue)

            float total = 0.0f;

            {
                SampleDataSet[] splitSet = SplitSet( splitIndex, splitValue, false );
                
                foreach( SampleDataSet s in splitSet )
                {
                    total += ( (float)s.NumSamples / (float)NumSamples ) * s.GetEntropy();
                }
            }
            
            return total;
        }

        public float GetInformationGainOfSplit( Int32 splitIndex, byte splitValue )
        {
            return GetEntropy() - GetEntropy( splitIndex, splitValue );
        }


        private class _SplitIGContainer //: IComparable<_SplitIGContainer>
        {
            public Int32 splitIndex { get; }
            public byte splitValue { get; }
            public float infoGain { get; }
            public Int32 byteSignificance { get; }

            public _SplitIGContainer( Int32 _splitIndex, byte _splitValue, float _infoGain, Int32 _byteSignificance = -100 )
            {
                splitIndex = _splitIndex;
                splitValue = _splitValue;
                infoGain = _infoGain;
                byteSignificance = _byteSignificance;
            }

            public override string ToString()
            {
                return "{" + string.Format( "{0,3:##0}", splitIndex ) + "," + string.Format( "{0,3:##0}", splitValue ) + "," + string.Format( "{0,7:##0.000}", infoGain ) + "," + string.Format( "{0,3:##0}", byteSignificance ) + "}";
            }

            /*public int CompareTo( _SplitIGContainer other )
            {
                //int infoComp = other.infoGain.CompareTo( infoGain );
                int infoComp = other.byteSignificance.CompareTo( byteSignificance );
                if( infoComp == 0 )
                {
                    return other.infoGain.CompareTo( infoGain );
                    //return other.byteSignificance.CompareTo( byteSignificance );
                }
                return infoComp;
            }*/
        }

        // todo Figure out how to have this function retrun SampleDataSet[] with the set presplit so that we dont have to do that again.
        public bool GetBestSplit( out Int32 splitIndex, out byte splitValue, Random random = null, Int32[] inputIndexes = null, Int32[] inputSignificance = null )
        {
            if( NumUniqueOutputs <= 1 || ( inputSignificance != null && ( inputSignificance.Length != NumInputs ) ) )
            {
                splitIndex = -1;
                splitValue = 0;
                return false;
            }

            if( random == null )
            {
                random = new Random();
            }

            // TODO: use inline functions/delegete to reduce code duplication here.

            List< _SplitIGContainer > splits = new List< _SplitIGContainer >();

            if( inputIndexes == null )
            {
                inputIndexes = Enumerable.Range( 0, NumInputs ).ToArray();
            }

            foreach( Int32 i in inputIndexes )
            {
                //for( int v = 0; v < 256; ++v )
                for( int v = inputMinMax[i, 0]; v <= inputMinMax[i, 1]; ++v )
                {
                    splits.Add( new _SplitIGContainer( i, (byte)v, GetInformationGainOfSplit( i, (byte)v ), inputSignificance == null ? -5 : inputSignificance[i] ) );
                }
            }

            List<_SplitIGContainer> bestSplits = splits;
            
            bestSplits.Sort( delegate ( _SplitIGContainer s1, _SplitIGContainer s2 ) { return s2.infoGain.CompareTo( s1.infoGain ); } );
            bestSplits = bestSplits.FindAll( delegate( _SplitIGContainer s ) { return s.infoGain == bestSplits[0].infoGain; } );

            if( inputSignificance != null )
            {
                bestSplits.Sort( delegate ( _SplitIGContainer s1, _SplitIGContainer s2 ) { return s2.byteSignificance.CompareTo( s1.byteSignificance ); } );
                bestSplits = bestSplits.FindAll( delegate ( _SplitIGContainer s ) { return s.byteSignificance == bestSplits[0].byteSignificance; } );
            }

            _SplitIGContainer bestSplit = bestSplits[random != null ? random.Next( bestSplits.Count ) : 0];
            
            splitIndex = bestSplit.splitIndex;
            splitValue = bestSplit.splitValue;

            return true;
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
