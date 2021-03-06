﻿using System;
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
        public Int32 NumNodes { get { return tree.Count; } }

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

        public bool AddNode( List<DecisionNode> nodeList )
        {
            if( nodeList == null || nodeList.Count == 0 )
            {
                return false;
            }

            foreach( DecisionNode node in nodeList )
            {
                AddNode( node );
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
                /*List<KeyValuePair<int, DecisionNode>> treeList = tree.ToList();
                foreach( KeyValuePair<int,DecisionNode> pair in treeList )
                {
                    flatTree[pair.Key] = pair.Value;
                }*/
                for( Int32 i = 0; i < flatTree.Length; ++i )
                {
                    flatTree[i] = tree[i];
                }
            }

            Int32 NextNodeID = 0;

            DecisionNode node = null;

            do
            {
                //node = tree[NextNodeID];
                node = flatTree[NextNodeID];

                // just in case, somehow this is sometimes needed :(
                if( null == node )
                {
                    node = tree[NextNodeID];
                }
            }
            while( ( NextNodeID = node.NextNodeByDecision( input ) ) >= 0 );
            
            return node.Output;
        }

        public XmlWriter WriteXml( XmlWriter xml, string fmt = "b64" )
        {
            xml.WriteStartElement( "tree" );
            xml.WriteAttributeString( "num", tree.Count.ToString() );
            xml.WriteAttributeString( "inputs", NumInputs.ToString() );
            xml.WriteAttributeString( "outputs", NumOutputs.ToString() );
            List< KeyValuePair < Int32, DecisionNode >> nodeList = tree.ToList();
            nodeList.Sort( delegate ( KeyValuePair<Int32, DecisionNode> n1, KeyValuePair<Int32, DecisionNode> n2 ) { return ( n1.Value.ID.CompareTo( n2.Value.ID ) ); } );
            foreach( KeyValuePair<Int32, DecisionNode> keyNode in nodeList ) 
            {
                keyNode.Value.WriteXml( xml, fmt );
            }
            xml.WriteEndElement();
            return xml;
        }
    }
    
}
