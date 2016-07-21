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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string xmlEncoding = "csv";
            //xmlEncoding = "csv";

            string folder = @"../../../images/";
            string outputFolder = @"../../../images/out/";

            outputFolder = @"Z:\trees\testOutput\";

            if( false )
            {
                bool[] toGenImages = new bool[] { false, false, false, false, !true, true, false, false };

                ImageFeatureExtraction.ExtractImageFeaturesFromDirectory( folder, outputFolder, toGenImages );
            }

            Random random = new Random();

            int hogWindowSize = 11;
            int maxImageDimension = 1024;

            DecisionForest forest = new DecisionForest( hogWindowSize * hogWindowSize * 9, 4 );

            string[] testFileNames = new string[] { @"final.png", @"7608091.jpg", @"SaifulHaque_GoldSphere.jpg" };

            SampleDataSet trainingData = ImageFeatureExtraction.ExtractHogDataFromTrainingImage( folder + testFileNames[0], hogWindowSize, maxImageDimension, 4 );
            trainingData.AddData( ImageFeatureExtraction.ExtractHogDataFromTrainingImage( folder + testFileNames[1], hogWindowSize, maxImageDimension, 4 ) );

            XmlHelper.WriteXml( outputFolder + "dataset.xml", trainingData, xmlEncoding );

            Console.WriteLine( "Generateing Trees ..." );

            Int32 numThreads = 2;
            Int32 treeDepth = 6;
            Int32 treesPerBlock = 4;
            Int32 numSamples = (int)Math.Sqrt( trainingData.NumSamples ) * 64;

            while( forest.NumTrees < 12 )
            {
                Console.WriteLine( "Generating {0} trees for {1} samples ...", treesPerBlock, numSamples );
                forest.AddTrees( TreeGenerator.GenerateForest( treesPerBlock, trainingData, numSamples, numThreads, random, null, null, treeDepth, numThreads == 1 ? 2 : 1 ) );
                
                {
                    Parallel.Invoke(
                    () =>
                    {
                        XmlHelper.WriteXml( outputFolder + string.Format( "forest{0:D5}.xml", forest.NumTrees ), forest, xmlEncoding );
                    },
                    () =>
                    {
                        ClasifyAndSave( folder + testFileNames[0], outputFolder + string.Format( "classification_cube{0:D5}.png", forest.NumTrees ), forest, hogWindowSize, maxImageDimension );
                    },
                    () =>
                    {
                        ClasifyAndSave( folder + testFileNames[1], outputFolder + string.Format( "classification_sphr{0:D5}.png", forest.NumTrees ), forest, hogWindowSize, maxImageDimension );
                    },
                    () =>
                    {
                        ClasifyAndSave( folder + testFileNames[2], outputFolder + string.Format( "classification_othr{0:D5}.png", forest.NumTrees ), forest, hogWindowSize, maxImageDimension );
                    }
                    );
                }
            }

            GC.Collect();

            stopwatch.Stop();
            Console.WriteLine( "Completed in {0} seconds.", (double)stopwatch.ElapsedMilliseconds / 1000.0 );
            Console.WriteLine( "Press [Return] to exit ...\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a" );
            Console.ReadLine();
        }

        
        public static void ClasifyAndSave( string inputFilename, string outputFilename, DecisionForest forest, Int32 hogWindowSize = 11, Int32 maxImageDimension = 1024 )
        {
            {
                Bitmap inputImage = new Bitmap( Image.FromFile( inputFilename ) );
                Bitmap classifiedImage = ClasificationToBitmap( ClasifyImage( inputImage, forest, hogWindowSize, maxImageDimension, forest.NumOutputs ), forest.NumOutputs );
                Console.WriteLine( outputFilename + classifiedImage.Size );
                if( true )
                {
                    Size s = new Size( 0, 0 );
                    Bitmap greyscaleInputImage = ImageFunctions.ScaleDownImage( ImageFunctions.MakeGrayscale3( inputImage ), maxImageDimension, ref s );
                    classifiedImage = ImageFunctions.MultiplyImages( classifiedImage, greyscaleInputImage );
                }
                classifiedImage.Save( outputFilename );
            }
        }

        public static byte[,][] ClasifyImage( Bitmap input, DecisionForest forest, Int32 featureSize = 11, Int32 maxImageDimention = 1024, Int32 bytesPerPixelOutput = 4 )
        {
            const int HOG_block_size = 8;
            const int HOG_norm_size = 2;

            Size newsize = new Size( maxImageDimention, maxImageDimention );

            Bitmap image = ImageFunctions.ScaleDownImage( input, maxImageDimention, ref newsize );


            /* blur the image */
            Console.WriteLine( "blur the image" );
            Bitmap gaussian = image;
            gaussian = ImageFunctions.ImageKernal( ImageFunctions.ImageKernal( gaussian, ImageFunctions.GaussianBlurXKernal, false, false ), ImageFunctions.GaussianBlurYKernal, false, false );

            /* Extract The HOG data from the Image */
            Console.WriteLine( "Extract The HOG data from the Image" );
            Bitmap hogInput = gaussian;

            Size hogBinSize = new Size( HOG_block_size, HOG_block_size );

            float[,,] gradientAngles = Hog.CalculateGradiantAngles( hogInput );
            float[,,] binGradients = Hog.BinGradients( gradientAngles, hogBinSize );
            binGradients = Hog.NormalizeBinnedGradients2( binGradients, new Size( HOG_norm_size, HOG_norm_size ) );
            //binGradients = Hog.NormalizeBinnedGradients( binGradients );
            
            /* Extract the SampleData from the HOG */
            Console.WriteLine( "Extract the SampleData from the HOG" );

            byte[,][] output = new byte[binGradients.GetLength( 0 ), binGradients.GetLength( 1 )][];

            for( Int32 y = 0; y < binGradients.GetLength( 1 ); ++y )
            {
                for( Int32 x = 0; x < binGradients.GetLength( 0 ); ++x )
                {
                    float cX = Func.Clamp( ( ( (float)( x ) + 0.5f ) * (float)( HOG_block_size ) ), 0, image.Size.Width - 1 );
                    float cY = Func.Clamp( ( ( (float)( y ) + 0.5f ) * (float)( HOG_block_size ) ), 0, image.Size.Height - 1 );
                    
                    byte[] sampleInput = ImageFeatureExtraction.GetBytesInBoxFromHog( binGradients, x, y, featureSize );

                    byte[] sampleOutput = forest.Decide( sampleInput );
                    //byte[] sampleOutput = forest.DecideV( sampleInput );

                    output[x, y] = Func.TruncateByteSequence( sampleOutput, bytesPerPixelOutput );

                }
            }
            return output;
        }

        public static Bitmap ClasificationToBitmap( byte[,][] classification, Int32 bytesPerPixelOutput = 4 )
        {
            const int HOG_block_size = 8;

            byte[] pixelData = new byte[classification.GetLength( 0 ) * classification.GetLength( 1 ) * bytesPerPixelOutput];

            for( Int32 y = 0; y < classification.GetLength( 1 ); ++y )
            {
                for( Int32 x = 0; x < classification.GetLength( 0 ); ++x )
                {
                    Int32 pixelIndex = ImageFunctions.GetPixelIndex( x, y, classification.GetLength( 0 ), bytesPerPixelOutput );

                    for( Int32 color = 0; color < bytesPerPixelOutput; ++color )
                    {
                        pixelData[pixelIndex + color] = classification[x, y][color];
                    }
                }
            }

            Size outputSize = new Size( classification.GetLength( 0 ), classification.GetLength( 1 ) );

            return ImageFunctions.ScaleImageNearest( ImageFunctions.GenerateImageFromData( outputSize, pixelData, bytesPerPixelOutput == 3 ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppArgb ), new Size( outputSize.Width * HOG_block_size, outputSize.Height * HOG_block_size ) );
        }

    }
}
