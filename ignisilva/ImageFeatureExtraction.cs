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
    class ImageFeatureExtraction
    {
        public static void ExtractImageFeaturesFromDirectory( string folder, string outputFolder, bool[] imageTypesToSave = null )
        {
            Stopwatch totalStopwatch = Stopwatch.StartNew();

            //string folder = @"../../../images/";
            //string outputFolder = @"../../../images/out/";

            string[] extensions = { @".jpg", @".jpeg", @".png", @".bmp", @".tiff"/*, @".psd"*/ };

            int maxImageDimention = 1024;
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

            if( imageTypesToSave[itts++] || true )
            {
                ImageFunctions.MakeGrayscale3( image ).Save( outputFolder + fileinfo.Name + "_" + itts + "03_gray.jpg", jpgCodec, jpgQuality );
            }

            if( imageTypesToSave[itts++] || true )
            {
                //Hog.CalculateGradiantAnglesToBitmap( gaussian ).Save( outputFolder + file + "_"+itts+"01_hogA.jpg", jpgCodec, jpgQuality );
                //Hog.CalculateGradiantMagnitudesToBitmap( gaussian ).Save( outputFolder + file + "_"+itts+"02_hogM.jpg", jpgCodec, jpgQuality );
                //Hog.CalculateGradiantAngleMagnitudesToBitmap( gaussian ).Save( outputFolder + file + "_"+itts+"03_hogAM.jpg", jpgCodec, jpgQuality );

                Bitmap hogInput = gaussian;

                Size hogBinSize = new Size( HOG_block_size, HOG_block_size );

                float[,,] gradientAngles = Hog.CalculateGradiantAngles( hogInput );
                float[,,] binGradients = Hog.BinGradients( gradientAngles, hogBinSize );
                binGradients = Hog.NormalizeBinnedGradients2( binGradients, new Size( HOG_norm_size, HOG_norm_size ) );

                Bitmap backdropImage = ImageFunctions.MakeGrayscale3( image );
                backdropImage = ImageFunctions.ImageKernal( backdropImage, new float[,] { { 0.0f } } );

                Bitmap hogOutput = Hog.BinGradientsToBitmap( binGradients, hogBinSize, backdropImage );
                //hogOutput.Save( outputFolder + file + "_104_hogV.jpg", jpgCodec, jpgQuality );
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


    }
}
