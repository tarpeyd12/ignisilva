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
using System.Xml;

namespace ignisilva
{
    class Program
    {
        static void Main( string[] args )
        {
            string folder = @"../../../images/";
            string outputFolder = @"../../../images/out/";

            if( false )
            {
                bool[] toGenImages = new bool[] { false, false, false, false, true, true, false, false };

                ImageFeatureExtraction.ExtractImageFeaturesFromDirectory( folder, outputFolder, toGenImages );
            }

            Random random = new Random();
            

            for( int i = 0; i < 100; ++i  )
            {
                DecisionForestTesImage( random, i*i+1 ).Save( outputFolder + "____test" + i.ToString("D4") + ".png" );
            }

            Console.WriteLine( "Press [Return] to exit ..." );
            Console.ReadLine();
        }

        static Bitmap DecisionForestTesImage( Random random, int n )
        {
            DecisionForest forest = RandomTestForest( random, n );

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Console.WriteLine( "Generating Image ..." );

            Size imageTestSize = new Size( 256, 256 );

            byte[] pixelData = new byte[imageTestSize.Width * imageTestSize.Height * 3];

            byte[] inputs = new byte[2];
            int count = 0;
            for( Int32 y = 0; y < imageTestSize.Width; ++y )
            {
                inputs[1] = (byte)Func.Clamp( y, 0, 255 );
                for( Int32 x = 0; x < imageTestSize.Height; ++x )
                {
                    inputs[0] = (byte)Func.Clamp( x, 0, 255 );
                    byte[] outputs = forest.Decide( inputs );
                    int pixelIndex = ImageFunctions.GetPixelIndex( x, y, imageTestSize.Width, 3 );

                    if( count++ % 100 == 0 )
                    {
                        Console.Write( "{0:F2}\r", (float)pixelIndex / (float)pixelData.Length * 100.0f );
                    }

                    for( Int32 color = 0; color < 3; ++color )
                    {
                        pixelData[pixelIndex + color] = outputs[color];
                    }
                }
            }

            Console.WriteLine( forest.Decide( new byte[] { (byte)random.Next( 255 ), (byte)random.Next( 255 ) } )[0] );

            //TestXmlWriter( forest );

            Bitmap imageTest = ImageFunctions.GenerateImageFromData( imageTestSize, pixelData, PixelFormat.Format24bppRgb );
            //imageTest.Save( outputFolder + "____test.png" );

            stopwatch.Stop();
            Console.WriteLine( "Done, in {0} seconds.", stopwatch.ElapsedMilliseconds / 1000.0f );

            return imageTest;
        }

        static DecisionForest RandomTestForest( Random random, int num )
        {
            Console.WriteLine( "Generating Forest ..." );
            DecisionForest forest = new DecisionForest( 2, 3 );

            for( Int32 i = 0; i < num; ++i )
            {
                forest.AddTree( RandomTestTree( random ) );
            }

            Console.WriteLine( "Done." );
            return forest;
        }

        static DecisionTree RandomTestTree( Random random )
        {
            DecisionTree tree = new DecisionTree( 2, 3 );
            bool n1 = random.Next( 1 ) == 1 ? true : false;
            tree.AddNode( new DecisionNode( 0, n1 ? 1 : 0, random.Next( 256 ), 1, 2 ) );
            tree.AddNode( new DecisionNode( 1, n1 ? 0 : 1, random.Next( 256 ), 3, 4 ) );
            tree.AddNode( new DecisionNode( 2, n1 ? 0 : 1, random.Next( 256 ), 5, 6 ) );
            tree.AddNode( new DecisionNode( 3, new byte[] { 255, 0, 0 } ) );
            tree.AddNode( new DecisionNode( 4, new byte[] { 0, 255, 0 } ) );
            tree.AddNode( new DecisionNode( 5, new byte[] { 0, 0, 255 } ) );
            tree.AddNode( new DecisionNode( 6, new byte[] { 0, 0, 0 } ) );
            return tree;
        }

        static void TestXmlWriter( DecisionForest forest )
        {
            XmlWriter writer = null;

            try
            {
                XmlWriterSettings xmlSettings = new XmlWriterSettings();
                xmlSettings.Indent = true;
                xmlSettings.IndentChars = "\t";
                xmlSettings.OmitXmlDeclaration = true;
                writer = XmlWriter.Create( Console.Out, xmlSettings );

                writer.WriteStartElement( "SampleData" );

                forest.WriteXml( writer );

                writer.WriteEndElement();

                writer.Flush();
            }
            finally
            {
                if( writer != null )
                {
                    writer.Close();
                }
            }
        }

    }
}
