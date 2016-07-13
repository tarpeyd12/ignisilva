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
            string xmlEncoding = "b64";

            string folder = @"../../../images/";
            string outputFolder = @"../../../images/out/";

            if( false )
            {
                bool[] toGenImages = new bool[] { false, false, false, false, !true, true, false, false };

                ImageFeatureExtraction.ExtractImageFeaturesFromDirectory( folder, outputFolder, toGenImages );
            }

            Random random = new Random();

            DecisionForest forest = new DecisionForest( 2, 3 );
            
            for( int i = 0; i < 0; ++i  )
            {
                while( forest.NumTrees < i*i+1 )
                {
                    forest.AddTree( RandomTestTree( random ) );
                }
                Console.Write( "{0}\n", i );
                DecisionForestTestImage( random, forest ).Save( outputFolder + "____test" + i.ToString("D4") + ".png" );
            }

            TestXmlWriter( outputFolder + @"forest.xml", forest, xmlEncoding );

            SampleDataSet sampleSet = new SampleDataSet( 2, 3 );

            //sampleSet = ImageFeatureExtraction.ExtractHogDataFromTrainingImage( folder + @"7608091.jpg" );
            //sampleSet.AddData( ImageFeatureExtraction.ExtractHogDataFromTrainingImage( folder + @"final.png" ) );

            Console.WriteLine( "Generating sample Data ..." );

            Int32 _subsets = 256/4;
            for( Int32 i = 0; i < 100000; ++i )
            {
                byte[] input = new byte[2];
                byte[] output = new byte[3];

                for( int c = 0; c < input.Length; ++c )
                {
                    input[c] = (byte)random.Next( 256 );
                }
                output[2] = (byte)(( ( input[0] / _subsets ) * _subsets )); // red
                output[1] = (byte)(( ( input[1] / _subsets ) * _subsets )); // green
                output[0] = (byte)( 255 - ( (int)Func.Clamp( ImageFunctions.ColorValueDistance( new byte[] { 0, 0 }, input ), 0.0f, 255.0f ) / _subsets ) * _subsets ); // blue
                SampleData sample = new SampleData( input, output );

                sampleSet.AddData( sample );
            }

            Console.WriteLine( "Done." );

            XmlWriter xml = CreateXmlWriter( outputFolder + @"dataset.xml" );
            //sampleSet = sampleSet.RandomSubSet( (Int32)Math.Sqrt( sampleSet.NumSamples ), new Random(12345) );
            sampleSet.WriteXml( xml, xmlEncoding );
            //sampleSet.WriteXml( xml, "z64" );
            //sampleSet.SubSetByOutput( 0 ).WriteXml( xml );
            xml.Flush();
            xml.Close();

            //float entropy1 = sampleSet.GetEntropy();
            //Console.WriteLine( "SampleSetEntropy = {0:F6}.", entropy1 );

            /*SampleDataSet[] splitSubSets1 = sampleSet.SplitSet( 0, 128 );
            SampleDataSet[] splitSubSets2 = sampleSet.SplitSet( 1, 128 );

            Console.WriteLine( "SampleSetEntropy(0,128,L) = {0:F6}, {1}", splitSubSets1[0].GetEntropy(), splitSubSets1[0].NumUniqueOutputs );
            Console.WriteLine( "SampleSetEntropy(0,128,G) = {0:F6}, {1}", splitSubSets1[1].GetEntropy(), splitSubSets1[1].NumUniqueOutputs );
            Console.WriteLine( "SampleSetEntropy(1,128,L) = {0:F6}, {1}", splitSubSets2[0].GetEntropy(), splitSubSets2[0].NumUniqueOutputs );
            Console.WriteLine( "SampleSetEntropy(1,128,G) = {0:F6}, {1}", splitSubSets2[1].GetEntropy(), splitSubSets2[1].NumUniqueOutputs );*/

            /*{
                Int32[] spl0 = GetBestSplit( sampleSet );

                SampleDataSet[] splitSets0 = sampleSet.SplitSet( spl0[0], (byte)spl0[1] );

                Int32[] spl1 = GetBestSplit( splitSets0[0] );
                Int32[] spl2 = GetBestSplit( splitSets0[1] );

                SampleDataSet[] splitSets1 = splitSets0[0].SplitSet( spl1[0], (byte)spl1[1] );
                SampleDataSet[] splitSets2 = splitSets0[1].SplitSet( spl2[0], (byte)spl2[1] );

                Console.WriteLine( "[0,0]=[{0}]", Func.ToCSV( splitSets1[0].GetAverageOutput() ) );
                Console.WriteLine( "[0,1]=[{0}]", Func.ToCSV( splitSets1[1].GetAverageOutput() ) );
                Console.WriteLine( "[1,0]=[{0}]", Func.ToCSV( splitSets2[0].GetAverageOutput() ) );
                Console.WriteLine( "[1,1]=[{0}]", Func.ToCSV( splitSets2[1].GetAverageOutput() ) );
            }*/

            //Console.WriteLine( "Entropy(9,5) = {0}.", SampleDataSet._Entropy( new Int32[] { 9, 5 } ) );

            {
                DecisionForest f = new DecisionForest( 2, 3 );
                for( int a = 0; a < 200; ++a )
                {
                    Console.WriteLine( a );
                    //List<DecisionNode> nodeList = TreeGenerator.Split( sampleSet.RandomSubSet(1000,random), null, -8 );
                    List<DecisionNode> nodeList = TreeGenerator.Split( sampleSet.RandomSubSet( (int)Math.Sqrt(sampleSet.NumSamples ), random ), random, null/*Func.UniqueRandomNumberRange( 2, 0, sampleSet.NumInputs + 1, random )*/, -1 );
                    
                    DecisionTree t = new DecisionTree( 2, 3 );
                    foreach( DecisionNode node in nodeList ) { t.AddNode( node ); }

                    f.AddTree( t );
                    DecisionForestTestImage( random, f ).Save( outputFolder + "____0Foresttest__" + a.ToString( "D4" ) + ".png" );
                }
                TestXmlWriter( outputFolder + "0forest.xml", f );
                DecisionForestTestImage( random, f ).Save( outputFolder + "____0Foresttest.png" );
            }

            using( StreamWriter file = new StreamWriter( outputFolder + @"outhisto.txt" ) )
            {

                file.WriteLine( "OUTPUTS[" );
                for( Int32 i = 0; i < sampleSet.NumInputs; ++i )
                {
                    file.Write( "({0})<", i );
                    file.WriteLine( "" );
                    Int32[,] inhisto = sampleSet.GetOutputHistogram( i );

                    for( Int32 x = 0; x < inhisto.GetLength( 1 ); ++x )
                    {
                        //file.Write( "[{0}]", Convert.ToBase64String( sampleSet.GetUniqueOutput( x ) ) );
                        file.Write( "[{0}]", Func.ToCSV( sampleSet.GetUniqueOutput( x ) ) );
                        for( Int32 y = 0; y < inhisto.GetLength( 0 ); ++y )
                        {
                            //file.Write( "{0:X3},", inhisto[y, x] );
                            file.Write( "{0}", inhisto[y, x] == 0 ? "." : "#" );
                        }
                        file.WriteLine( "" );
                    }
                    file.WriteLine( ">" );
                }
                file.WriteLine( "]" );

                /*Int32[,,] outhisto = sampleSet.GetInputHistogram();

                for( Int32 i = 0; i < sampleSet.NumInputs; ++i )
                {
                    file.Write( "({0})<", i );
                    file.WriteLine( "" );
                    for( Int32 x = 0; x < outhisto.GetLength( 2 ); ++x )
                    {
                        //file.Write( "[{0}]", Convert.ToBase64String( sampleSet.GetUniqueOutput( x ) ) );
                        //file.Write( "[{0}]", Func.ToCSV( sampleSet.GetUniqueOutput( x ) ) );
                        for( Int32 y = 0; y < outhisto.GetLength( 0 ); ++y )
                        {
                            file.Write( "{0:X3},", outhisto[y, i, x] );
                            //file.Write( "{0}", outhisto[y, x] == 0 ? "." : "#" );
                        }
                        file.WriteLine( "" );
                    }
                    file.WriteLine( ">" );
                }
                file.WriteLine( ">" );*/

                /*file.WriteLine( "OUTPUTS[" );
                for( Int32 i = 0; i < sampleSet.NumInputs; ++i )
                {
                    file.Write( "({0})<", i );
                    file.WriteLine( "" );
                    Int32[,] outminmax = sampleSet.GetOutputMinMaxOfInputIndex(i);
                    for( Int32 x = 0; x < outminmax.GetLength(0); ++x )
                    {
                        file.WriteLine( "[{2}]:{0:D3},{1:D3}", outminmax[x, 0], outminmax[x, 1], Func.ToCSV( sampleSet.GetUniqueOutput( x ) ) );
                    }
                }
                file.WriteLine( "]" );*/
            }

            GC.Collect();

            Console.WriteLine( "Press [Return] to exit ...\a" );
            Console.ReadLine();
        }

        static Int32[] GetBestSplit( SampleDataSet sampleSet )
        {
            int bestIG_index = 0;
            byte bestIG_value = 0;

            sampleSet.GetBestSplit( out bestIG_index, out bestIG_value );
               
            return new Int32[] { bestIG_index, bestIG_value };
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
                    //byte[] outputs = forest.DecideR( inputs, 10, random );
                    //byte[] outputs = forest.DecideR( inputs, Math.Max( Math.Min( (int)Math.Sqrt(forest.NumTrees), forest.NumTrees ), 1 ), random );
                    //byte[] outputs = forest.DecideRN( inputs, 10, 5, random );
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

            //Console.WriteLine( forest.Decide( new byte[] { (byte)random.Next( 255 ), (byte)random.Next( 255 ) } )[0] );

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
            /*tree.AddNode( new DecisionNode( n1 ? 3 : 3, new byte[] { 255,   0,   0 } ) ); // blue
            tree.AddNode( new DecisionNode( n1 ? 5 : 4, new byte[] {   0, 255,   0 } ) ); // green
            tree.AddNode( new DecisionNode( n1 ? 4 : 5, new byte[] {   0,   0, 255 } ) ); // red
            tree.AddNode( new DecisionNode( n1 ? 6 : 6, new byte[] {   0,   0,   0 } ) ); // black*/
            tree.AddNode( new DecisionNode( n1 ? 3 : 3, new byte[] { 0, 255, 255 } ) ); // yellow
            tree.AddNode( new DecisionNode( n1 ? 5 : 4, new byte[] { 128, 0, 128 } ) ); // purple
            tree.AddNode( new DecisionNode( n1 ? 4 : 5, new byte[] { 0, 69, 255 } ) ); // orangered
            tree.AddNode( new DecisionNode( n1 ? 6 : 6, new byte[] { 0, 0, 128 } ) ); // maroon
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

        static void TestXmlWriter( string filename, DecisionForest forest, string xmlEncoding = "b64" )
        {
            XmlWriter writer = null;

            try
            {
                writer = CreateXmlWriter( filename );

                //writer.WriteStartElement( "SampleData" );

                forest.WriteXml( writer, xmlEncoding );

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
