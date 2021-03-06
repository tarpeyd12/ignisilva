﻿using System;
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
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string xmlEncoding = "csv";
            xmlEncoding = "a64";

            string imageFolder    = @"../../../images/";
            string trainingFolder = @"../../../images/trainingdata/";
            string outputFolder   = @"../../../images/out/";

            string trainingFileSuffix = @"_trainingdata.png";

            // due to the project folders layout the 64 but executable is in another subfolder compared to the 32 bit one
            if( Environment.Is64BitProcess )
            {
                imageFolder    = @"../" + imageFolder;
                trainingFolder = @"../" + trainingFolder;
                outputFolder   = @"../" + outputFolder;
            }

            /*outputFolder = @"Z:/trees/testOutput/";

            string dataSetDir = @"numericDataSet";

            imageFolder = @"Z:/trees/" + dataSetDir + @"/";
            trainingFolder = @"Z:/trees/" + dataSetDir + @"/td/";*/

            int hogWindowSize = 17;
            int hogNormSize = 2;
            int maxImageDimension = 1024;
            bool sampleForMultipleScales = true;
            double multipleScaleSampleRatio = 0.9;
            int minImageDimention = hogWindowSize * 8 * 4;
            bool sampleMirrorHorizontal = true;

            Int32 numThreads = 8;
            Int32 treeDepth = -16;
            Int32 treesPerBlock = 64;
            bool adaptiveStrides = true;
            Func<Int32, Int32> numSamplesFunc = delegate ( Int32 s ) { return (Int32)( Math.Sqrt( s ) * 64.0 ); };
            numSamplesFunc = delegate ( Int32 s ) { return (Int32)( Math.Pow( Math.Pow( s, 1.0 / 3.0 ), 2.0 ) * 1.0 ); };


            if( !false )
            {
                bool[] toGenImages = new bool[] { false, false, false, false, !true, true, false, false };

                ImageFeatureExtraction.ExtractImageFeaturesFromDirectory( imageFolder, outputFolder, "classification_", toGenImages, maxImageDimension, hogNormSize );
            }

            Random random = new Random();

            DecisionForest forest = new DecisionForest( hogWindowSize * hogWindowSize * 9, 4 );

            string[] testFileNames = new string[] { @"final.png", @"7608091.jpg", @"SaifulHaque_GoldSphere.jpg" };


            List<FileInfo> Files = new List<FileInfo>();

            {
                string[] extensions = { @".jpg", @".jpeg", @".png", @".bmp", @".tiff"/*, @".psd"*/ };

                DirectoryInfo d = new DirectoryInfo( imageFolder );

                foreach( string ext in extensions )
                {
                    if( ext.Length < 2 ) continue;
                    string extension = ext;
                    if( '.' == extension[0] )
                    {
                        extension = "*" + extension;
                    }
                    else if( !( extension.StartsWith( "*." ) ) )
                    {
                        extension = "*." + extension;
                    }

                    Files.AddRange( d.GetFiles( extension ) );
                    //Files.AddRange( d.GetFiles( extension, SearchOption.AllDirectories ) );
                }

                Files.Sort( delegate ( FileInfo t1, FileInfo t2 ) { return ( t1.Length.CompareTo( t2.Length ) ); } );
            }

            SampleDataSet trainingData = new SampleDataSet( hogWindowSize * hogWindowSize * 9, 4 );

            int imageDimention = maxImageDimension;
            //int minImageDimention = hogWindowSize * 8 * 4;
            do
            {
                Console.WriteLine( "Extracting for max dimention {0}...", imageDimention );

                object sync = new object();

                //foreach( FileInfo fileInfo in Files )
                Parallel.ForEach( Files, new ParallelOptions { MaxDegreeOfParallelism = numThreads }, ( fileInfo ) =>
                {
                    try
                    {
                        Console.WriteLine( fileInfo );
                        SampleDataSet imageSampleData = new SampleDataSet( hogWindowSize * hogWindowSize * 9, 4 );
                        try
                        {
                            imageSampleData.AddData( ImageFeatureExtraction.ExtractHogDataFromTrainingImage( imageFolder + fileInfo.Name, trainingFolder + fileInfo.Name + trainingFileSuffix, false, false, hogWindowSize, imageDimention, 4, hogNormSize ) );
                        }
                        finally { GC.Collect(); }
                        if( sampleMirrorHorizontal )
                        {
                            try
                            {
                                imageSampleData.AddData( ImageFeatureExtraction.ExtractHogDataFromTrainingImage( imageFolder + fileInfo.Name, trainingFolder + fileInfo.Name + trainingFileSuffix, true, false, hogWindowSize, imageDimention, 4, hogNormSize ) );
                            }
                            finally { GC.Collect(); }
                        }
                        lock( sync )
                        {
                            trainingData.AddData( imageSampleData );
                        }
                    }
                    finally { GC.Collect(); }
                } );

                GC.Collect();

                imageDimention = (int)((double)imageDimention * multipleScaleSampleRatio );
            }
            while( imageDimention >= minImageDimention && sampleForMultipleScales );


            Console.WriteLine( "Saving Sample Data {0}", outputFolder + "dataset.xml" );
            XmlHelper.WriteXml( outputFolder + "dataset.xml", trainingData, xmlEncoding );

            Console.WriteLine( "Generateing Trees ..." );
            
            Int32 numSamples = numSamplesFunc( trainingData.NumSamples );

            if( numSamples > trainingData.NumSamples )
            {
                numSamples = trainingData.NumSamples;
            }

            while( forest.NumTrees < 512 )
            {
                Console.WriteLine( "Generating {0} trees for {1} samples ...", treesPerBlock, numSamples );
                forest.AddTrees( TreeGenerator.GenerateForest( treesPerBlock, trainingData, numSamples, numThreads, random, null, null, treeDepth, adaptiveStrides, numThreads == 1 ? 2 : 1 ) );
                
                {
                    //Parallel.Invoke(
                    //() =>
                    {
                        XmlHelper.WriteXml( outputFolder + string.Format( "forest{0:D5}.xml", forest.NumTrees ), forest, xmlEncoding );
                    }//,
                    //() =>
                    {
                        Parallel.ForEach( Files, new ParallelOptions { MaxDegreeOfParallelism = numThreads }, ( fileInfo ) =>
                        {
                            ClasifyAndSave( imageFolder + fileInfo.Name, outputFolder + string.Format( "classification_{1}_A_{0:D5}.png", forest.NumTrees, fileInfo.Name ), forest, hogWindowSize, maxImageDimension, false, hogNormSize );
                            ClasifyAndSave( imageFolder + fileInfo.Name, outputFolder + string.Format( "classification_{1}_V_{0:D5}.png", forest.NumTrees, fileInfo.Name ), forest, hogWindowSize, maxImageDimension, true, hogNormSize );
                        }
                        );
                    }
                    //);
                }
                Console.WriteLine( "Tree block completed. Program runtime: {0} seconds.", (double)stopwatch.ElapsedMilliseconds / 1000.0 );
            }

            GC.Collect();

            stopwatch.Stop();
            Console.WriteLine( "Completed in {0} seconds.", (double)stopwatch.ElapsedMilliseconds / 1000.0 );
            Console.WriteLine( "Press [Return] to exit ...\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a" );
            Console.ReadLine();
        }

        
        public static void ClasifyAndSave( string inputFilename, string outputFilename, DecisionForest forest, Int32 hogWindowSize = 11, Int32 maxImageDimension = 1024, bool voteBased = false, int HOG_norm_size = 2 )
        {
            {
                Bitmap inputImage = new Bitmap( Image.FromFile( inputFilename ) );
                Bitmap classifiedImage = ClasificationToBitmap( ClasifyImage( inputImage, forest, hogWindowSize, maxImageDimension, forest.NumOutputs, voteBased, HOG_norm_size ), forest.NumOutputs );
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

        public static byte[,][] ClasifyImage( Bitmap input, DecisionForest forest, Int32 featureSize = 11, Int32 maxImageDimention = 1024, Int32 bytesPerPixelOutput = 4, bool voteBased = false, int HOG_norm_size = 2 )
        {
            const int HOG_block_size = 8;
            //const int HOG_norm_size = 2;

            Size newsize = new Size( maxImageDimention, maxImageDimention );

            Bitmap image = ImageFunctions.ScaleDownImage( input, maxImageDimention, ref newsize );


            /* blur the image */
            Console.WriteLine( "blur the image" );
            Bitmap gaussian = image;
            //gaussian = ImageFunctions.ImageKernal( gaussian, ImageFunctions.GaussianBlurKernal );
            //gaussian = ImageFunctions.ImageKernal( ImageFunctions.ImageKernal( gaussian, ImageFunctions.GaussianBlurXKernal, false, false ), ImageFunctions.GaussianBlurYKernal, false, false );
            gaussian = ImageFunctions.ImageKernal( ImageFunctions.ImageKernal( gaussian, ImageFunctions.GaussianBlurFastXKernal, false, false ), ImageFunctions.GaussianBlurFastYKernal, false, false );

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

                    byte[] sampleOutput = null;
                    if( !voteBased )
                    {
                        sampleOutput = forest.Decide( sampleInput );
                    }
                    else
                    {
                        sampleOutput = forest.DecideV( sampleInput );
                    }

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
