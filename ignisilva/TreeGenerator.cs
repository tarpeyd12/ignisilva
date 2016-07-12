using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ignisilva
{
    class TreeGenerator
    {
        public static List<DecisionNode> Split( SampleDataSet sampleData, Int32[] indexList = null, Int32 maxDepth = -1, Int32 currentDepth = 0, List<DecisionNode> nodes = null )
        {
            const bool debug = !true;
            if( nodes == null )
            {
                nodes = new List<DecisionNode>();
            }

            if( ( maxDepth > 0 && currentDepth >= maxDepth ) || sampleData.NumUniqueOutputs == 1 || sampleData.GetEntropy() == 0.0f )
            {
                byte[] _output = sampleData.GetAverageOutput();

                if( debug ) Console.WriteLine( "New Leaf Node [{0},[{1}]]", nodes.Count, Func.ToCSV( _output ) );
                nodes.Add( new DecisionNode( nodes.Count, _output ) );
            }
            else
            {
                Int32 _splitIndex;
                byte _splitValue;


                if( debug ) Console.WriteLine( "Splitting .... " );
                sampleData.GetBestSplit( out _splitIndex, out _splitValue );

                if( debug ) Console.WriteLine( "New Split Node [{0},{1},{2},[{3},{4}]]", nodes.Count, _splitIndex, _splitValue, -1, -1 );

                DecisionNode node = new DecisionNode( nodes.Count, _splitIndex, _splitValue, -1, -1 );

                nodes.Add( node );
                
                SampleDataSet[] splitSets = sampleData.SplitSet( _splitIndex, _splitValue );
                
                List<DecisionNode> res = null;
                
                node.Next[0] = nodes.Count;
                res = Split( splitSets[0], indexList, maxDepth, currentDepth + 1, nodes );

                node.Next[1] = nodes.Count;
                res = Split( splitSets[1], indexList, maxDepth, currentDepth + 1, nodes );
                
            }

            if( currentDepth == 0 )
            {
                nodes.Sort( delegate ( DecisionNode n1, DecisionNode n2 ) { return n1.ID.CompareTo( n2.ID ); } );
            }

            return nodes;
        }

        //public static DecisionTree Generate(  )
    }
}
