using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ignisilva
{
    static class TreeGenerator
    {
        public static List<DecisionNode> Split( SampleDataSet sampleData, Random random = null, Int32[] indexList = null, Int32[] inputSignificanceList = null, Int32 maxDepth = -1, Int32 currentDepth = 0, List<DecisionNode> nodes = null, bool debug = false )
        {
            if( nodes == null )
            {
                nodes = new List<DecisionNode>();
            }

            /*if( debug ) Console.WriteLine( "Calculating Entropy across {0} Unique Outputs ...", sampleData.NumUniqueOutputs );

            float entropy = sampleData.GetEntropy();

            if( debug ) Console.WriteLine( "Entropy = {0:F4}", entropy );*/

            if( ( maxDepth > 0 && currentDepth >= maxDepth ) || sampleData.NumUniqueOutputs == 1 /*|| entropy == 0.0f*/ )
            {
                byte[] _output = sampleData.GetAverageOutput();

                if( debug ) Console.WriteLine( "New Leaf Node [{0},[{1}]]", nodes.Count, Func.ToCSV( _output ) );
                nodes.Add( new DecisionNode( nodes.Count, _output ) );
            }
            else
            {
                Int32 _splitIndex;
                byte _splitValue;


                if( debug ) Console.WriteLine( "Splitting {0} Samples to {1} Unique Outputs .... ", sampleData.NumSamples, sampleData.NumUniqueOutputs );
                sampleData.GetBestSplit( out _splitIndex, out _splitValue, random, indexList, inputSignificanceList );

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
                res = Split( splitSets[0], random, indexList, inputSignificanceList, maxDepth, currentDepth + 1, nodes, debug );

                node.Next[1] = nodes.Count;
                res = Split( splitSets[1], random, indexList, inputSignificanceList, maxDepth, currentDepth + 1, nodes, debug );

                //GC.Collect();
            }

            if( currentDepth == 0 )
            {
                nodes.Sort( delegate ( DecisionNode n1, DecisionNode n2 ) { return n1.ID.CompareTo( n2.ID ); } );
            }

            return nodes;
        }

        public static DecisionTree Generate( SampleDataSet sampleData, Random random = null, Int32[] indexList = null, Int32[] inputSignificanceList = null, Int32 maxDepth = -1, bool printDebug = false )
        {
            List<DecisionNode> nodeList = TreeGenerator.Split( sampleData, random, indexList, inputSignificanceList, maxDepth, 0, null, printDebug );

            DecisionTree tree = new DecisionTree( sampleData.NumInputs, sampleData.NumOutputs );

            foreach( DecisionNode node in nodeList )
            {
                tree.AddNode( node );
            }

            return tree;
        }


        public static DecisionTree[] GenerateForest( Int32 numTrees, SampleDataSet sampleData, Int32 subSampleSetSize, Int32 numThreads = 1, Random random = null, Int32[] indexList = null, Int32[] inputSignificanceList = null, Int32 maxDepth = -1, int printDebug = 0 )
        {
            DecisionTree[] trees = new DecisionTree[numTrees];
            Random[] randomGenerators = new Random[numTrees];

            for( int i = 0; i < randomGenerators.Length; ++i )
            {
                randomGenerators[i] = new Random( random.Next() );
            }

            object sync = new object();

            Int32 treesMade = 0;
            if( printDebug > 0 ) Console.Write( "{0,6:##0.00} %\r", (double)treesMade / (double)numTrees * 100.0 );

            Parallel.For( 0, numTrees, new ParallelOptions { MaxDegreeOfParallelism = numThreads }, ( threadID)=> 
            {
                SampleDataSet subSet = sampleData.RandomSubSet( subSampleSetSize, randomGenerators[threadID] );

                DecisionTree tree = Generate( subSet, randomGenerators[threadID], indexList, inputSignificanceList, maxDepth, printDebug > 1 ? true : false );

                lock( sync )
                {
                    trees[threadID] = tree;
                    ++treesMade;
                    if( printDebug > 0 ) Console.Write( "{0,6:##0.00} %\r", (double)treesMade / (double)numTrees * 100.0 );
                }
                
            } 
            );

            return trees;
        }

    }
}
