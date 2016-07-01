using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ignisilva
{
    
    class DecisionTree
    {
        public int NumInputs  { get; }
        public int NumOutputs { get; }

        private Dictionary<Int32,DecisionNode> tree;

        // blank tree constructor
        public DecisionTree( int _numInputs, int _numOutputs )
        {
            NumInputs = _numInputs;
            NumOutputs = _numOutputs;
            tree = new Dictionary<int, DecisionNode>();
        }

        public bool AddNode( DecisionNode node )
        {
            // check for duplicate ID, check for bad output size, check for bad input size
            if( tree.ContainsKey( node.ID ) || ( node.IsLeaf && node.Output.Length != NumOutputs ) || ( !node.IsLeaf && node.SplitIndex >= NumInputs ) )
            {
                return false;
            }

            tree.Add( node.ID, node );

            return true;
        }

        public byte[] Decide( byte[] input )
        {
            if( input.Length != NumInputs )
            {
                // error
                return null;
            }

            Int32 NextNodeID = 0;

            DecisionNode node = null;

            do
            {
                node = tree[NextNodeID];
            }
            while( ( NextNodeID = node.NextNodeByDecision( input ) ) >= 0 );
            
            return node.Output;
        }
    }
    
}
