using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ignisilva
{
    class Func
    {
        public static T Clamp<T>( T val, T min, T max ) where T : IComparable<T>
        {
            if     ( val.CompareTo( min ) < 0 ) return min;
            else if( val.CompareTo( max ) > 0 ) return max;
            else return val;
        }

        public static List<Int32> UniqueRandomNumberRange( Int32 numberOfOutputs, Int32 Min, Int32 Max, Random random )
        {
            HashSet<Int32> check = new HashSet<Int32>();

            // TODO: this might be off-by-one
            if( Max-Min <= numberOfOutputs )
            {
                return new List<Int32>( Enumerable.Range( Min, Max-Min ).ToArray() );
            }

            Int32 value;

            while( check.Count < numberOfOutputs )
            {
                do
                {
                    value = random.Next( Min, Max );
                }
                while( check.Contains( value ) );
                
                check.Add( value );
            }

            List<Int32> output = check.ToList();

            output.Sort();

            return output;
        }
    }
}
