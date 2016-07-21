using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ignisilva
{
    class DecisionForest : IXmlWritable
    {
        public Int32 NumInputs { get; }
        public Int32 NumOutputs { get; }

        public Int32 NumTrees { get { return forest.Count; } }

        private List<DecisionTree> forest;

        // blank forest constructor
        public DecisionForest( int _numInputs, int _numOutputs )
        {
            NumInputs = _numInputs;
            NumOutputs = _numOutputs;
            forest = new List<DecisionTree>();
        }

        public bool AddTree( DecisionTree tree )
        {
            if( tree.NumInputs != NumInputs || tree.NumOutputs != NumOutputs )
            {
                return false;
            }

            forest.Add( tree );

            return true;
        }

        public bool AddTrees( DecisionTree[] trees )
        {
            foreach( DecisionTree tree in trees )
            {
                AddTree( tree );
            }

            return true;
        }

        public byte[] Decide( byte[] input )
        {
            if( forest.Count == 0 )
            {
                Console.Write( "NO TREES IN FOREST ..." );
                return null;
            }

            Int32[] sums = new Int32[ NumOutputs ];

            foreach( DecisionTree tree in forest )
            {
                byte[] result = tree.Decide( input );

                if( result == null )
                {
                    continue;
                }

                for( Int32 i = 0; i < NumOutputs; ++i )
                {
                    sums[i] += result[i];
                }
            }

            byte[] output = new byte[NumOutputs];

            for( Int32 i = 0; i < NumOutputs; ++i )
            {
                output[i] = (byte)Func.Clamp( ((double)sums[i] / (double)forest.Count), 0.0f, 255.0f );
            }

            return output;
        }

        public byte[] DecideR( byte[] input, Int32 Num, Random random )
        {
            if( forest.Count == 0 )
            {
                return null;
            }

            if( Num >= NumTrees  )
            {
                return Decide( input );
            }

            Int32[] sums = new Int32[NumOutputs];

            Int32[] indexes = Func.UniqueRandomNumberRange( Num, 0, NumTrees, random );

            foreach( Int32 treeIndex in indexes )
            {
                DecisionTree tree = forest[treeIndex];
                byte[] result = tree.Decide( input );

                if( result == null )
                {
                    continue;
                }

                for( Int32 i = 0; i < NumOutputs; ++i )
                {
                    sums[i] += result[i];
                }
            }

            byte[] output = new byte[NumOutputs];

            for( Int32 i = 0; i < NumOutputs; ++i )
            {
                output[i] = (byte)Func.Clamp( ( (double)sums[i] / (double)indexes.Length ), 0.0f, 255.0f );
            }

            return output;
        }

        public byte[] DecideRN( byte[] input, Int32 Num, Int32 Repetitions, Random random )
        {
            if( forest.Count == 0 )
            {
                return null;
            }

            if( Num * Repetitions >= NumTrees )
            {
                return Decide( input );
            }

            if( Repetitions <= 1 )
            {
                return DecideR( input, Num, random );
            }

            Int32[] sums = new Int32[NumOutputs];

            for( Int32 i = 0; i < Repetitions; ++i )
            {
                byte[] result = DecideR( input, Num, random );

                for( Int32 c = 0; c < NumOutputs; ++c )
                {
                    sums[c] += result[c];
                }
            }

            byte[] output = new byte[NumOutputs];

            for( Int32 i = 0; i < NumOutputs; ++i )
            {
                output[i] = (byte)Func.Clamp( ( (double)sums[i] / (double)Repetitions ), 0.0f, 255.0f );
            }

            return output;
        }

        private struct _OutputCounter
        {
            public UInt32 count;
            public byte[] output;
        }

        public byte[] DecideV( byte[] input )
        {
            if( forest.Count == 0 )
            {
                Console.Write( "NO TREES IN FOREST ..." );
                return null;
            }

            HashTableHashing.IHashAlgorithm hasher = new HashTableHashing.MurmurHash2Simple();

            Dictionary<UInt32, _OutputCounter> counts = new Dictionary<UInt32, _OutputCounter>();
            
            foreach( DecisionTree tree in forest )
            {
                byte[] result = tree.Decide( input );

                if( result == null )
                {
                    continue;
                }

                UInt32 hash = hasher.Hash( result );    

                if( !counts.ContainsKey( hash ) )
                {
                    _OutputCounter c = new _OutputCounter();
                    c.count = 0;
                    c.output = result;
                    counts.Add( hash, c );
                }

                _OutputCounter c1 = counts[hash];
                c1.count++;
                counts[hash] = c1;
            }

            List<_OutputCounter> listCounts = counts.Values.ToList();
            listCounts.Sort( delegate ( _OutputCounter c1, _OutputCounter c2 ) { return c2.count.CompareTo( c1.count ); } );

            return listCounts[0].output;
        }

        public XmlWriter WriteXml( XmlWriter xml, string fmt = "b64" )
        {
            xml.WriteStartElement( "forest" );

            xml.WriteAttributeString( "num", forest.Count.ToString() );
            xml.WriteAttributeString( "inputs", NumInputs.ToString() );
            xml.WriteAttributeString( "outputs", NumOutputs.ToString() );

            foreach( DecisionTree tree in forest )
            {
                tree.WriteXml( xml, fmt );
            }

            xml.WriteEndElement();
            return xml;
        }

    }
}
