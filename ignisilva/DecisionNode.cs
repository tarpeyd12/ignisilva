using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ignisilva
{
    class DecisionNode
    {
        public Int32   ID         { get; } // node ID
        public bool    IsLeaf     { get { return ( Output != null ); } } // do we have output???
        public int     SplitIndex { get; }
        public int     SplitValue { get; }
        public Int32[] Next       { get; }
        public byte[]  Output     { get; }

        // node constructor
        public DecisionNode( Int32 _ID, int _splitIndex, int _splitValue, Int32 _nextLess, Int32 _nextGreater )
        {
            ID = _ID;
            SplitIndex = _splitIndex;
            SplitValue = _splitValue;
            Next = new Int32[] { _nextLess, _nextGreater };
            Output = null;
        }

        // string node constructor
        public DecisionNode( string _ID, string _splitIndex, string _splitValue, string _next )
        {
            ID = Int32.Parse( _ID );
            SplitIndex = Int32.Parse( _splitIndex );
            SplitValue = Int32.Parse( _splitValue );
            string[] _nextSplit = _next.Split(',');
            Next = new Int32[] { Int32.Parse( _nextSplit[0] ), Int32.Parse( _nextSplit[1] ) };
            Output = null;
        }

        // leaf constructor
        public DecisionNode( Int32 _ID, byte[] _output )
        {
            ID = _ID;
            SplitIndex = -1; // will cause error
            SplitValue = -1; // will cause unknown behavior
            Next = new Int32[] { -1, -1 };
            Output = _output;
        }

        // string leaf constructor
        public DecisionNode( string _ID, byte[] _output )
        {
            ID = Int32.Parse( _ID );
            SplitIndex = -1; // will cause error
            SplitValue = -1; // will cause unknown behavior
            Next = new Int32[] { -1, -1 };
            Output = _output;
        }

        public Int32 NextNodeByDecision( byte[] input )
        {
            if( IsLeaf )
            {
                return -1;
            }

            return Next[ ( input[SplitIndex] < SplitValue ) ? 0 : 1 ];
        }
        
    }
}
