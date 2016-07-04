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

        public byte[] Decide( byte[] input )
        {
            if( forest.Count == 0 )
            {
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

            Int32[] indexes = new Int32[Num];
            for( int i = 0; i < indexes.Length; ++i )
            {
                indexes[i] = random.Next( NumTrees );
            }

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

        public XmlWriter WriteXml( XmlWriter xml )
        {
            xml.WriteStartElement( "forest" );

            xml.WriteAttributeString( "num", forest.Count.ToString() );
            xml.WriteAttributeString( "inputs", NumInputs.ToString() );
            xml.WriteAttributeString( "outputs", NumOutputs.ToString() );

            foreach( DecisionTree tree in forest )
            {
                tree.WriteXml( xml );
            }

            xml.WriteEndElement();
            return xml;
        }

    }
}
