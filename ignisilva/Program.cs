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
            if( false )
            {
                string folder = @"../../../images/";
                string outputFolder = @"../../../images/out/";
                bool[] toGenImages = new bool[] { false, false, false, false, true, true, false, false };

                ImageFeatureExtraction.ExtractImageFeaturesFromDirectory( folder, outputFolder, toGenImages );
            }
            DecisionForest forest = new DecisionForest( 5, 1 );
            DecisionTree tree = null;
            
            tree = new DecisionTree( 5, 1 );
            tree.AddNode( new DecisionNode( "0", "3", "5", "1,2" ) );
            tree.AddNode( new DecisionNode( "1", new byte[] { 0 } ) );
            tree.AddNode( new DecisionNode( "2", new byte[] { 255 } ) );
            forest.AddTree( tree );

            tree = new DecisionTree( 5, 1 );
            tree.AddNode( new DecisionNode( "0", "0", "2", "1,2" ) );
            tree.AddNode( new DecisionNode( "1", new byte[] { 0 } ) );
            tree.AddNode( new DecisionNode( "2", new byte[] { 255 } ) );
            forest.AddTree( tree );

            Console.WriteLine( forest.Decide( new byte[] { 1, 2, 3, 4, 5 } )[0] );
            Console.WriteLine( forest.Decide( new byte[] { 6, 7, 8, 9, 0 } )[0] );

            Console.WriteLine( "Press [Return] to exit ..." );
            Console.ReadLine();
        }
    }
}
