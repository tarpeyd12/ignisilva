using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ignisilva
{
    
    class DecisionTree : IXmlWritable
    {
        public Int32 NumInputs  { get; }
        public Int32 NumOutputs { get; }

        private Dictionary<Int32,DecisionNode> tree;
        private DecisionNode[] flatTree;

        // blank tree constructor
        public DecisionTree( int _numInputs, int _numOutputs )
        {
            NumInputs = _numInputs;
            NumOutputs = _numOutputs;
            tree = new Dictionary<int, DecisionNode>();
            flatTree = null; ;
        }

        public bool AddNode( DecisionNode node )
        {
            // check for duplicate ID, check for bad output size, check for bad input size
            if( tree.ContainsKey( node.ID ) || ( node.IsLeaf && node.Output.Length != NumOutputs ) || ( !node.IsLeaf && node.SplitIndex >= NumInputs ) )
            {
                return false;
            }

            tree.Add( node.ID, node );

            if( flatTree != null )
            {
                flatTree = null;
            }

            return true;
        }

        public byte[] Decide( byte[] input )
        {
            if( input.Length != NumInputs )
            {
                // error
                return null;
            }

            if( flatTree == null )
            {
                flatTree = new DecisionNode[tree.Count];
                foreach( KeyValuePair<int,DecisionNode> pair in tree.ToList() )
                {
                    flatTree[pair.Key] = pair.Value;
                }
            }

            Int32 NextNodeID = 0;

            DecisionNode node = null;

            do
            {
                //node = tree[NextNodeID];
                node = flatTree[NextNodeID];
            }
            while( ( NextNodeID = node.NextNodeByDecision( input ) ) >= 0 );
            
            return node.Output;
        }

        public XmlWriter WriteXml( XmlWriter xml )
        {
            xml.WriteStartElement( "tree" );
            xml.WriteAttributeString( "inputs", NumInputs.ToString() );
            xml.WriteAttributeString( "outputs", NumOutputs.ToString() );
            foreach( KeyValuePair<int, DecisionNode> keyNode in tree.ToList() ) 
            {
                DecisionNode node = keyNode.Value;
                node.WriteXml( xml );
            }
            xml.WriteEndElement();
            return xml;
        }
    }
    
}
