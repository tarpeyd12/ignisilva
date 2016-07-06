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

            DecisionForest forest = new DecisionForest( 2, 3 );
            
            for( int i = 0; i < 25; ++i  )
            {
                while( forest.NumTrees < i*i+1 )
                {
                    forest.AddTree( RandomTestTree( random ) );
                }
                DecisionForestTestImage( random, forest ).Save( outputFolder + "____test" + i.ToString("D4") + ".png" );
            }

            TestXmlWriter( outputFolder + @"forest.xml", forest );

            SampleDataSet sampleSet = new SampleDataSet( 2, 3 );

            Int32 _subsets = 128;
            for( Int32 i = 0; i < 1000000; ++i )
            {
                byte[] input = new byte[2];
                byte[] output = new byte[3];

                for( int c = 0; c < input.Length; ++c )
                {
                    input[c] = (byte)random.Next( 256 );
                }
                output[2] = (byte)( ( input[0] / _subsets ) * _subsets ); // red
                output[1] = (byte)( ( input[1] / _subsets ) * _subsets ); // green
                output[0] = (byte)( ( Func.Clamp( ImageFunctions.ColorValueDistance( new byte[] { 0, 0 }, input ) * 255.0f, 0.0f, 255.0f ) / _subsets) * _subsets); // blue
                SampleData sample = new SampleData( input, output );

                sampleSet.AddData( sample );
            }
            XmlWriter xml = CreateXmlWriter( outputFolder + @"dataset.xml" );
            sampleSet.RandomSubSet( (Int32)Math.Sqrt( sampleSet.NumSamples ) ).WriteXml( xml );
            //sampleSet.SubSetByOutput( 0 ).WriteXml( xml );
            xml.Flush();
            xml.Close();

            using( StreamWriter file = new StreamWriter( outputFolder + @"outhisto.txt" ) )
            {

                for( Int32 i = 0; i < sampleSet.NumInputs; ++i )
                {
                    file.Write( "({0})<", i );
                    file.WriteLine( "" );
                    Int32[,] outhisto = sampleSet.GetOutputHistogram( i );

                    for( Int32 x = 0; x < outhisto.GetLength( 1 ); ++x )
                    {
                        for( Int32 y = 0; y < outhisto.GetLength( 0 ); ++y )
                        {
                            file.Write( "{0:X4},", outhisto[y, x] );
                        }
                        file.WriteLine( "" );
                    }
                    file.WriteLine( ">" );
                }
            }

            Console.WriteLine( "Press [Return] to exit ..." );
            Console.ReadLine();
        }

        static Bitmap DecisionForestTestImage( Random random, DecisionForest forest )
        {
            //DecisionForest forest = RandomTestForest( random, n );

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
                    byte[] outputs = forest.DecideR( inputs, Math.Max( Math.Min( (int)Math.Sqrt(forest.NumTrees), forest.NumTrees ), 1 ), random );
                    //byte[] outputs = forest.DecideRN( inputs, 100, 5, random );
                    //byte[] outputs = forest.Decide( inputs );
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
            bool n1 = random.Next( 2 ) == 0 ? true : false;
            tree.AddNode( new DecisionNode( 0, n1 ? 1 : 0, random.Next( 256 ), 1, 2 ) );
            tree.AddNode( new DecisionNode( 1, n1 ? 0 : 1, random.Next( 256 ), 3, 4 ) );
            tree.AddNode( new DecisionNode( 2, n1 ? 0 : 1, random.Next( 256 ), 5, 6 ) );
            tree.AddNode( new DecisionNode( n1 ? 3 : 3, new byte[] { 255,   0,   0 } ) ); // blue
            tree.AddNode( new DecisionNode( n1 ? 5 : 4, new byte[] {   0, 255,   0 } ) ); // green
            tree.AddNode( new DecisionNode( n1 ? 4 : 5, new byte[] {   0,   0, 255 } ) ); // red
            tree.AddNode( new DecisionNode( n1 ? 6 : 6, new byte[] {   0,   0,   0 } ) ); // black
            return tree;
        }

        static XmlWriter CreateXmlWriter( string filename )
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "\t";
            xmlSettings.OmitXmlDeclaration = true;
            return XmlWriter.Create( filename/*Console.Out*/, xmlSettings );
        }

        static void TestXmlWriter( string filename, DecisionForest forest )
        {
            XmlWriter writer = null;

            try
            {
                writer = CreateXmlWriter( filename );

                //writer.WriteStartElement( "SampleData" );

                forest.WriteXml( writer );

                //writer.WriteEndElement();

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
