using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace ignisilva
{
    class Program
    {
        static void Main( string[] args )
        {
            //ImageFeatureExtraction.ExtractImageFeaturesFromDirectory( @"../../../images/", @"../../../images/out/", new bool[] { false, false, false, false, true, true, false, false } );
            Console.WriteLine( "1" );
            DecisionTree tree = new DecisionTree( 5, 1 );
            Console.Write( tree.AddNode( new DecisionNode( "0", "3", "5", "1,2" ) ) );
            Console.Write( tree.AddNode( new DecisionNode( "1", new byte[] { 0 } ) ) );
            Console.Write( tree.AddNode( new DecisionNode( "2", new byte[] { 1 } ) ) );

            Console.WriteLine( "\nTree Construction Complete.\nPress [Return] to Continue ..." );
            Console.ReadLine();

            Console.WriteLine( tree.Decide( new byte[] { 1, 2, 3, 4, 5 } )[0] );
            Console.WriteLine( tree.Decide( new byte[] { 6, 7, 8, 9, 0 } )[0] );

            Console.WriteLine( "Press [Return] to exit ..." );
            Console.ReadLine();
        }
    }
}
