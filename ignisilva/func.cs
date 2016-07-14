using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ignisilva
{
    static class Func
    {
        public static T Clamp<T>( T val, T min, T max ) where T : IComparable<T>
        {
            if     ( val.CompareTo( min ) < 0 ) { return min; }
            else if( val.CompareTo( max ) > 0 ) { return max; }
            else                                { return val; }
        }

        public static Int32[] UniqueRandomNumberRange( Int32 numberOfOutputs, Int32 Min, Int32 Max, Random random )
        {
            HashSet<Int32> check = new HashSet<Int32>();

            // TODO: this might be off-by-one
            if( Max - Min <= numberOfOutputs )
            {
                return Enumerable.Range( Min, Max - Min ).ToArray();
            }

            while( check.Count < numberOfOutputs )
            {
                while( !check.Add( random.Next( Min, Max ) ) ) ;
            }

            List<Int32> output = check.ToList();

            output.Sort();

            return output.ToArray();
        }

        public static List<Int32> UniqueRandomNumberRangeList( Int32 numberOfOutputs, Int32 Min, Int32 Max, Random random )
        {
            return new List<Int32>( UniqueRandomNumberRange( numberOfOutputs, Min, Max, random) );
        }
        
        public static string ToCSV( byte[] data )
        {
            string output = "";

            for( Int32 i = 0; i < data.Length; ++i )
            {
                output += data[i].ToString( "D3" );
                if( i < data.Length-1 )
                {
                    output += ",";
                }
            }
            return output;
        }

        public static byte[] AppendBytes( byte[] a, byte[] b )
        {
            byte[] output = new byte[ a.Length + b.Length ];

            for( Int32 i = 0; i < a.Length; ++i )
            {
                output[i] = a[i];
            }

            for( Int32 i = a.Length; i < output.Length; ++i )
            {
                output[i] = b[i - a.Length];
            }

            return output;
        }

    }
}
