using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ignisilva
{
    static class ImageFeatureExtraction
    {
        public static void ExtractImageFeaturesFromDirectory( string folder, string outputFolder, bool[] imageTypesToSave = null )
        {
            Stopwatch totalStopwatch = Stopwatch.StartNew();

            //string folder = @"../../../images/";
            //string outputFolder = @"../../../images/out/";

            string[] extensions = { @".jpg", @".jpeg", @".png", @".bmp", @".tiff"/*, @".psd"*/ };

            int maxImageDimention = -1024;
            int HOG_block_size = 8;
            int HOG_norm_size = 2;

            if( imageTypesToSave == null )
            {
                imageTypesToSave = new bool[100];
                for( int i = 0; i < imageTypesToSave.Length; ++i )
                {
                    imageTypesToSave[i] = true;
                }
            }

            DirectoryInfo d = new DirectoryInfo( folder );
            List<FileInfo> Files = new List<FileInfo>();

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

            //foreach( FileInfo fileinfo in Files )
            Parallel.ForEach( Files, ( fileinfo ) =>
            {
                ExtractImageFeatures( fileinfo, maxImageDimention, outputFolder, HOG_block_size, HOG_norm_size, imageTypesToSave );
            }
            );

            GC.Collect();

            totalStopwatch.Stop();

            Console.WriteLine( "Completed Processing {0} images in {1} seconds.", Files.Count, (float)totalStopwatch.ElapsedMilliseconds / 1000.0f );

        }

        public static void ExtractImageFeatures( FileInfo fileinfo, int maxImageDimention, string outputFolder, int HOG_block_size, int HOG_norm_size, bool[] imageTypesToSave )
        {
            int itts = 0;

            bool progressPrint = false;

            ImageCodecInfo jpgCodec = ImageExtensions.GetEncoderInfoOrDefault("image/jpeg");
            EncoderParameters jpgQuality = new EncoderParameters( 1 );
            jpgQuality.Param[0] = new EncoderParameter( System.Drawing.Imaging.Encoder.Quality, 100L );

            Stopwatch perStopwatch = Stopwatch.StartNew();

            //string file = fileinfo.Name;
            string directory = fileinfo.DirectoryName;

            Console.WriteLine( "Loading {0} ({1} KBytes).", fileinfo.Name, fileinfo.Length / 1024 );
            Bitmap image = new Bitmap( Image.FromFile( fileinfo.FullName ) );

            double maxDimention = maxImageDimention;
            double scale = Math.Min( maxDimention / image.Width, maxDimention / image.Height );

            if( maxDimention > 0 && scale < 1.0 )
            {
                image = new Bitmap( image, new Size( (int)( image.Width * scale ), (int)( image.Height * scale ) ) );
            }

            if( imageTypesToSave[itts++] || !true )
            {
                image.Save( outputFolder + fileinfo.Name + "_" + itts + "00_scaled.jpg", jpgCodec, jpgQuality );
            }


            Bitmap gaussian = image;
            //gaussian = ImageProcessing.ImageKernal( gaussian, ImageProcessing.GaussianBlurKernal );
            gaussian = ImageFunctions.ImageKernal( ImageFunctions.ImageKernal( gaussian, ImageFunctions.GaussianBlurXKernal, false, progressPrint ), ImageFunctions.GaussianBlurYKernal, false, progressPrint );
            //gaussian = ImageProcessing.ImageKernal( ImageProcessing.ImageKernal( gaussian, ImageProcessing.GaussianBlurFastXKernal, false, progressPrint ), ImageProcessing.GaussianBlurFastYKernal, false, progressPrint );
            if( imageTypesToSave[itts++] || !true )
            {
                gaussian.Save( outputFolder + fileinfo.Name + "_" + itts + "01_gaussian.jpg", jpgCodec, jpgQuality );
            }

            if( imageTypesToSave[itts++] || !true )
            {
                Bitmap[] sobel = new Bitmap[4];

                ( sobel[0] = ImageFunctions.ImageKernal( gaussian, ImageFunctions.SobelXKernal,  false, progressPrint ) ).Save( outputFolder + fileinfo.Name + "_" + itts + "01_sobel_x.jpg",  jpgCodec, jpgQuality );
                ( sobel[1] = ImageFunctions.ImageKernal( gaussian, ImageFunctions.SobelYKernal,  false, progressPrint ) ).Save( outputFolder + fileinfo.Name + "_" + itts + "02_sobel_y.jpg",  jpgCodec, jpgQuality );
                ( sobel[2] = ImageFunctions.ImageKernal( gaussian, ImageFunctions.SobelNXKernal, false, progressPrint ) ).Save( outputFolder + fileinfo.Name + "_" + itts + "03_sobel_xn.jpg", jpgCodec, jpgQuality );
                ( sobel[3] = ImageFunctions.ImageKernal( gaussian, ImageFunctions.SobelNYKernal, false, progressPrint ) ).Save( outputFolder + fileinfo.Name + "_" + itts + "04_sobel_yn.jpg", jpgCodec, jpgQuality );

            }

            if( imageTypesToSave[itts++] || !true )
            {
                Bitmap sharp = image;
                sharp = ImageFunctions.ImageKernal( sharp, ImageFunctions.SharpenKernal, false, progressPrint );
                //sharp = ImageProcessing.ImageKernal( sharp, ImageProcessing.IdentityKernal );
                sharp.Save( outputFolder + fileinfo.Name + "_" + itts + "02_sharp.jpg", jpgCodec, jpgQuality );
            }

            if( imageTypesToSave[itts++] || !true )
            {
                ImageFunctions.MakeGrayscale3( image ).Save( outputFolder + fileinfo.Name + "_" + itts + "03_gray.jpg", jpgCodec, jpgQuality );
            }

            if( imageTypesToSave[itts++] || true )
            {
                //Hog.CalculateGradiantAnglesToBitmap( gaussian ).Save( outputFolder + fileinfo.Name + "_"+itts+"01_hogA.jpg", jpgCodec, jpgQuality );
                //Hog.CalculateGradiantMagnitudesToBitmap( gaussian ).Save( outputFolder + fileinfo.Name + "_"+itts+"02_hogM.jpg", jpgCodec, jpgQuality );
                //Hog.CalculateGradiantAngleMagnitudesToBitmap( gaussian ).Save( outputFolder + fileinfo.Name + "_"+itts+"03_hogAM.jpg", jpgCodec, jpgQuality );

                Bitmap hogInput = gaussian;

                Size hogBinSize = new Size( HOG_block_size, HOG_block_size );

                float[,,] gradientAngles = Hog.CalculateGradiantAngles( hogInput );
                float[,,] binGradients = Hog.BinGradients( gradientAngles, hogBinSize );
                binGradients = Hog.NormalizeBinnedGradients2( binGradients, new Size( HOG_norm_size, HOG_norm_size ) );

                Bitmap backdropImage = ImageFunctions.MakeGrayscale3( image );
                Bitmap darkenedBackdropImage = ImageFunctions.ImageKernal( backdropImage, new float[,] { { 0.0f } } );

                Bitmap hogOutput = Hog.BinGradientsToBitmap( binGradients, hogBinSize, darkenedBackdropImage );

                hogOutput = ImageFunctions.AddImages( hogOutput, ImageFunctions.ImageKernal( backdropImage, new float[,] { { 1.0f/16.0f } } ) );

                hogOutput.Save( outputFolder + fileinfo.Name + "_" + itts + "04_hogV.png" );
            }

            if( imageTypesToSave[itts++] || !true )
            {
                ImageFunctions.ReduceImageColors( gaussian, 2,   false, progressPrint ).Save( outputFolder + fileinfo.Name + "_" + itts + "06_reduced_2.jpg",   jpgCodec, jpgQuality );
                ImageFunctions.ReduceImageColors( gaussian, 4,   false, progressPrint ).Save( outputFolder + fileinfo.Name + "_" + itts + "07_reduced_4.jpg",   jpgCodec, jpgQuality );
                ImageFunctions.ReduceImageColors( gaussian, 8,   false, progressPrint ).Save( outputFolder + fileinfo.Name + "_" + itts + "08_reduced_8.jpg",   jpgCodec, jpgQuality );
                ImageFunctions.ReduceImageColors( gaussian, 16,  false, progressPrint ).Save( outputFolder + fileinfo.Name + "_" + itts + "09_reduced_16.jpg",  jpgCodec, jpgQuality );
                ImageFunctions.ReduceImageColors( gaussian, 32,  false, progressPrint ).Save( outputFolder + fileinfo.Name + "_" + itts + "10_reduced_32.jpg",  jpgCodec, jpgQuality );
                ImageFunctions.ReduceImageColors( gaussian, 64,  false, progressPrint ).Save( outputFolder + fileinfo.Name + "_" + itts + "11_reduced_64.jpg",  jpgCodec, jpgQuality );
                ImageFunctions.ReduceImageColors( gaussian, 128, false, progressPrint ).Save( outputFolder + fileinfo.Name + "_" + itts + "12_reduced_128.jpg", jpgCodec, jpgQuality );
            }

            if( imageTypesToSave[itts++] || !true )
            {
                Bitmap final = gaussian;
                final = ImageFunctions.ImageKernal( final, ImageFunctions.EdgeFindKernal, false, progressPrint );
                final.Save( outputFolder + fileinfo.Name + "_" + itts + "99_edge.jpg", jpgCodec, jpgQuality );
            }

            perStopwatch.Stop();
            Console.WriteLine( "Completed processing {0} in {1} seconds.", fileinfo.Name, (float)perStopwatch.ElapsedMilliseconds / 1000.0f );
            //GC.Collect();
        }


        /*public static SampleDataSet ExtractHogDataFromTrainingImage( string filename )
        {
            return ExtractHogDataFromTrainingImage( filename, 11 );
        }*/

        public static SampleDataSet ExtractHogDataFromTrainingImage( string imageFilename, string trainingFilename, Int32 featureSize = 11, Int32 maxImageDimention = 1024, Int32 bytesPerPixelOutput = 3 )
        {
            //const int maxImageDimention = -1024;
            const int HOG_block_size = 8;
            const int HOG_norm_size = 2;

            SampleDataSet output = null;
            try
            {
                FileInfo inputFileInfo = new FileInfo( imageFilename );
                FileInfo trainingFileInfo = new FileInfo( trainingFilename );

                /* Load The Images */
                Console.WriteLine( "Load The Images {0} {1}", inputFileInfo, trainingFileInfo );
                Bitmap image = new Bitmap( Image.FromFile( inputFileInfo.FullName ) );
                Bitmap trainingImage = new Bitmap( Image.FromFile( trainingFileInfo.FullName ) );

                if( image.Size != trainingImage.Size )
                {
                    return null;
                }

                double scale = Math.Min( (double)maxImageDimention / image.Width, (double)maxImageDimention / image.Height );

                /* Scale the Image */
                Console.WriteLine( "Scale the Image {0}", scale );
                if( maxImageDimention > 0 && scale < 1.0 && scale > 0.0 )
                {
                    //trainingImage = new Bitmap( trainingImage,  (int)(trainingImage.Width * scale ), (int)( trainingImage.Height * scale )  );
                    trainingImage = ImageFunctions.ScaleImageNearest( trainingImage, new Size( (int)( trainingImage.Width * scale ), (int)( trainingImage.Height * scale ) ) );

                    //trainingImage.Save( @"../../../images/out/" + trainingFileInfo.Name + @".png" );
                    //ImageFunctions.MultiplyImages( ImageFunctions.MakeGrayscale3(image), new Bitmap( trainingImage , image.Size) ).Save( @"../../../images/out/" + trainingFileInfo.Name + @"2.png" );

                    image = new Bitmap( image,  (int)( image.Width * scale ), (int)( image.Height * scale ) );
                    
                    //GC.Collect();
                }

                /* blur the image */
                Console.WriteLine( "blur the image" );
                Bitmap gaussian = image;
                gaussian = ImageFunctions.ImageKernal( ImageFunctions.ImageKernal( gaussian, ImageFunctions.GaussianBlurXKernal, false, false ), ImageFunctions.GaussianBlurYKernal, false, false );

                /* Extract The HOG data from the Image */
                Console.WriteLine( "Extract The HOG data from the Image");
                Bitmap hogInput = gaussian;

                Size hogBinSize = new Size( HOG_block_size, HOG_block_size );

                float[,,] gradientAngles = Hog.CalculateGradiantAngles( hogInput );
                float[,,] binGradients = Hog.BinGradients( gradientAngles, hogBinSize );
                binGradients = Hog.NormalizeBinnedGradients2( binGradients, new Size( HOG_norm_size, HOG_norm_size ) );
                //binGradients = Hog.NormalizeBinnedGradients( binGradients );

                byte[] trainingImageData = ImageFunctions.ExtractImageData( trainingImage );
                Int32 trainingImageColorDepth = ImageFunctions.BytesPerPixelIn( trainingImage );

                /* Extract the SampleData from the HOG */
                Console.WriteLine( "Extract the SampleData from the HOG" );
                output = new SampleDataSet( featureSize * featureSize * binGradients.GetLength( 2 ), trainingImageColorDepth );

                for( Int32 y = 0; y < binGradients.GetLength( 1 ); ++y )
                {
                    for( Int32 x = 0; x < binGradients.GetLength( 0 ); ++x )
                    {
                        float cX = Func.Clamp( ( ( (float)( x ) + 0.5f ) * (float)( HOG_block_size ) ), 0, trainingImage.Size.Width  - 1 );
                        float cY = Func.Clamp( ( ( (float)( y ) + 0.5f ) * (float)( HOG_block_size ) ), 0, trainingImage.Size.Height - 1 );

                        byte[] sampleOutput = Func.TruncateByteSequence( ImageFunctions.GetRawPixelFromImageData( trainingImageData, (Int32)cX, (Int32)cY, trainingImage.Size.Width, trainingImageColorDepth ), bytesPerPixelOutput );
                        byte[] sampleInput = GetBytesInBoxFromHog( binGradients, x, y, featureSize );
                        
                        output.AddData( new SampleData( sampleInput, sampleOutput ) );
                    }
                }

                //return output;
            }
            catch( Exception e )
            {
                Console.WriteLine( "Exception In ImageFeatureExtraction.ExtractHogDataFromTrainingImage()\nMessage: " + e.Message + "\nStackTrace: " + e.StackTrace );
            }
            finally
            {

            }
            return output;
        }
        
        public static byte[] GetBytesInBoxFromHog( float[,,] binGradients, Int32 in_x, Int32 in_y, Int32 featureSize )
        {
            byte[] output = new byte[featureSize * featureSize * binGradients.GetLength( 2 )];

            for( Int32 _y = 0; _y < featureSize; ++_y )
            {
                for( Int32 _x = 0; _x < featureSize; ++_x )
                {
                    Int32 x = Func.Clamp( ( in_x + _x - ( featureSize - 1 ) / 2 ), 0, binGradients.GetLength( 0 ) - 1 );
                    Int32 y = Func.Clamp( ( in_y + _y - ( featureSize - 1 ) / 2 ), 0, binGradients.GetLength( 1 ) - 1 );

                    Int32 outputIndex = ImageFunctions.GetPixelIndex( _x, _y, featureSize, binGradients.GetLength( 2 ) );

                    for( Int32 z = 0; z < binGradients.GetLength( 2 ); ++z )
                    {
                        output[outputIndex + z] = (byte)Func.Clamp( binGradients[x, y, z]*255.0f, 0.0f, 255.0f );
                    }
                }
            }

            return output;
        }


        public static SampleDataSet ExtractPixelColorPositionData( string filename, ref Size imageSize, Int32 maxImageDimention = 1024, Int32 maxColorDepth = 3 )
        {
            FileInfo inputFileInfo = new FileInfo( filename );
            FileInfo trainingFileInfo = new FileInfo( filename + "_trainingdata.png" );

            /* Load The Images */
            Console.WriteLine( "Load The Images" );
            Bitmap image = new Bitmap( Image.FromFile( inputFileInfo.FullName ) );

            Console.WriteLine( "Image Size: {0}", image.Size );

            double scale = Math.Min( (double)maxImageDimention / (double)image.Width, (double)maxImageDimention / (double)image.Height );

            /* Scale the Image */
            Console.WriteLine( "Scale the Image {0}", scale );
            if( maxImageDimention > 0 && scale < 1.0 && scale > 0.0 )
            {
                image = new Bitmap( image, (int)( image.Width * scale ), (int)( image.Height * scale ) );
            }

            if( imageSize != null ) imageSize = image.Size;
            
            Console.WriteLine( "Image Size: {0}", image.Size );

            /* extract image data */
            Int32 _trueBytesPerPixel = ImageFunctions.BytesPerPixelIn( image );
            Int32 bytesPerPixel = Math.Min( _trueBytesPerPixel, maxColorDepth );
            byte[] imageData = ImageFunctions.ExtractImageData( image );
            
            SampleDataSet output = new SampleDataSet( sizeof( Int32 ) * 2, bytesPerPixel );

            for( Int32 y = 0; y < image.Size.Height;  ++y )
            {
                for( Int32 x = 0; x < image.Size.Width; ++x )
                {
                    Int32 pixelIndex = ImageFunctions.GetPixelIndex( x, y, image.Size.Width, _trueBytesPerPixel );

                    byte[] inputData = Func.AppendBytes( BitConverter.GetBytes( x ), BitConverter.GetBytes( y ) );
                    byte[] outputData = new byte[bytesPerPixel];
                    for( int color = 0; color < bytesPerPixel; ++color )
                    {
                        outputData[color] = imageData[pixelIndex + color];
                    }

                    output.AddData( new SampleData( inputData, outputData ) );
                }
            }

            return output;
        }


    }
}
