using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ignisilva
{
    static class TreeGenerator
    {
        public static List<DecisionNode> Split( SampleDataSet sampleData, Random random = null, Int32[] indexList = null, Int32 maxDepth = -1, Int32 currentDepth = 0, List<DecisionNode> nodes = null )
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
                sampleData.GetBestSplit( out _splitIndex, out _splitValue, random, indexList );
                
                SampleDataSet[] splitSets = sampleData.SplitSet( _splitIndex, _splitValue );

                // do we need to be a leaf since the split makes a subset with no samples?
                if( splitSets[0].NumSamples == 0 || splitSets[1].NumSamples == 0 )
                {
                    byte[] _output = sampleData.GetAverageOutput();
                    if( debug ) Console.WriteLine( "New Leaf Node [{0},[{1}]]", nodes.Count, Func.ToCSV( _output ) );
                    nodes.Add( new DecisionNode( nodes.Count, _output ) );
                    return nodes;
                }

                DecisionNode node = new DecisionNode( nodes.Count, _splitIndex, _splitValue, -1, -1 );

                if( debug ) Console.WriteLine( "New Split Node [{0},{1},{2},[{3},{4}]]", nodes.Count, _splitIndex, _splitValue, -1, -1 );

                List<DecisionNode> res = null;

                nodes.Add( node );

                node.Next[0] = nodes.Count;
                res = Split( splitSets[0], random, indexList, maxDepth, currentDepth + 1, nodes );

                node.Next[1] = nodes.Count;
                res = Split( splitSets[1], random, indexList, maxDepth, currentDepth + 1, nodes );
                
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
