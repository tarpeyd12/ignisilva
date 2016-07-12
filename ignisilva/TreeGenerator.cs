using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ignisilva
{
    class TreeGenerator
    {
        public static List<DecisionNode> Split( SampleDataSet sampleData, Int32[] indexList = null, Int32 maxDepth = -1, Int32 currentDepth = 0, Int32 currentIndex = 0, Int32 nextIndex = 1, List<DecisionNode> nodes = null )
        {
            const bool debug = true;
            if( nodes == null )
            {
                if( debug ) Console.WriteLine( "[{0}] nodes == null; creating new list.", currentIndex );
                nodes = new List<DecisionNode>();
            }

            if( ( maxDepth > 0 && currentDepth >= maxDepth ) || sampleData.NumUniqueOutputs == 1 || sampleData.GetEntropy() == 0.0f )
            {
                byte[] _output = sampleData.GetAverageOutput();

                if( debug ) Console.WriteLine( "New Leaf Node [{0},[{1}]]", currentIndex, Func.ToCSV( _output ) );
                nodes.Add( new DecisionNode( currentIndex, _output ) );
            }
            else
            {
                Int32 _splitIndex;
                byte _splitValue;

                sampleData.GetBestSplit( out _splitIndex, out _splitValue );

                Int32[] nextSplitIndexes = new Int32[] { nextIndex + 0, nextIndex + 1 };

                if( debug ) Console.WriteLine( "New Split Node [{0},{1},{2},[{3},{4}]]", currentIndex, _splitIndex, _splitValue, nextSplitIndexes[0], nextSplitIndexes[1] );

                DecisionNode node = new DecisionNode( currentIndex, _splitIndex, _splitValue, nextSplitIndexes[0], nextSplitIndexes[1] );

                nodes.Add( node );

                //if( debug ) Console.WriteLine( "[{0}] Splitting", currentIndex );
                SampleDataSet[] splitSets = sampleData.SplitSet( _splitIndex, _splitValue );
                
                List<DecisionNode> res = null;

                //if( debug ) Console.WriteLine( "[{0}] Repeating Less", currentIndex );
                res = Split( splitSets[0], indexList, maxDepth, currentDepth + 1, nextSplitIndexes[0], nextSplitIndexes[1] + 1, nodes );

                //if( debug ) Console.WriteLine( "[{0}] Repeating Less Adding", currentIndex );
                //foreach( DecisionNode node in res ) { if( debug ) Console.WriteLine( "[{0}]", node ); nodes.Add( node ); }

                //if( debug ) Console.WriteLine( "[{0}] Repeating Greater", currentIndex );
                res = Split( splitSets[1], indexList, maxDepth, currentDepth + 1, nextSplitIndexes[1], (nodes.Count + 1), nodes );

                //if( debug ) Console.WriteLine( "[{0}] Repeating Greater Adding", currentIndex );
                //foreach( DecisionNode node in res ) { nodes.Add( node ); }
            }

            nodes.Sort( delegate ( DecisionNode n1, DecisionNode n2 ) { return n1.ID.CompareTo( n2.ID ); } );

            return nodes;
        }

        //public static DecisionTree Generate(  )
    }
}
