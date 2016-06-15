﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace ignisilva
{
    class ImageProcessing
    {
        public static T Clamp<T>( T val, T min, T max ) where T : IComparable<T>
        {
            if( val.CompareTo( min ) < 0 ) return min;
            else if( val.CompareTo( max ) > 0 ) return max;
            else return val;
        }

        public static float[,] IdentityKernal
        {
            get
            {
                return new float[,] { { 1 } };
            }
        }

        public static float[,] EdgeFindKernal
        {
            get
            {
                return new float[,]{
                { -1, -1, -1},
                { -1,  8, -1},
                { -1, -1, -1} };
            }
        }

        public static float[,] SharpenKernal
        {
            get
            {
                return new float[,]{
                {  0, -1,  0},
                { -1,  5, -1},
                {  0, -1,  0} };
            }
        }

        public static float[,] GaussianBlurFastKernal
        {
            get
            {
                return new float[,]{
                {1.0f*0.0625f,2.0f*0.0625f,1.0f*0.0625f},
                {2.0f*0.0625f,4.0f*0.0625f,2.0f*0.0625f},
                {1.0f*0.0625f,2.0f*0.0625f,1.0f*0.0625f}};
            }
        }

        public static float[,] GaussianBlurFastXKernal
        {
            get
            {
                return new float[,] { { 0.27901f, 0.44198f, 0.27901f } };
            }
        }

        public static float[,] GaussianBlurFastYKernal
        {
            get
            {
                return new float[,] { { 0.27901f }, { 0.44198f }, { 0.27901f } };
            }
        }

        public static float[,] GaussianBlurXKernal
        {
            get
            {
                return new float[,] { { 0.00598f, 0.060626f, 0.241843f, 0.383103f, 0.241843f, 0.060626f, 0.00598f } };
            }
        }

        public static float[,] GaussianBlurYKernal
        {
            get
            {
                return new float[,] { { 0.00598f }, { 0.060626f }, { 0.241843f }, { 0.383103f }, { 0.241843f }, { 0.060626f }, { 0.00598f } };
            }
        }

        public static float[,] GaussianBlurKernal
        {
            get
            {
                return new float[,]{
                    { 0.00000067f,  0.00002292f,  0.00019117f,  0.00038771f,  0.00019117f,  0.00002292f,  0.00000067f },
                    { 0.00002292f,  0.00078634f,  0.00655965f,  0.01330373f,  0.00655965f,  0.00078633f,  0.00002292f },
                    { 0.00019117f,  0.00655965f,  0.05472157f,  0.11098164f,  0.05472157f,  0.00655965f,  0.00019117f },
                    { 0.00038771f,  0.01330373f,  0.11098164f,  0.22508352f,  0.11098164f,  0.01330373f,  0.00038771f },
                    { 0.00019117f,  0.00655965f,  0.05472157f,  0.11098164f,  0.05472157f,  0.00655965f,  0.00019117f },
                    { 0.00002292f,  0.00078633f,  0.00655965f,  0.01330373f,  0.00655965f,  0.00078633f,  0.00002292f },
                    { 0.00000067f,  0.00002292f,  0.00019117f,  0.00038771f,  0.00019117f,  0.00002292f,  0.00000067f } };
            }
        }

        public static float[,] SobelXKernal
        {
            get
            {
                return new float[,]{
                {1,0,-1},
                {2,0,-2},
                {1,0,-1}};
            }
        }

        public static float[,] SobelYKernal
        {
            get
            {
                return new float[,]{
                {1,2,1},
                {0,0,0},
                {-1,-2,-1}};
            }
        }

        public static float[,] SobelNXKernal
        {
            get
            {
                return new float[,]{
                {-1,0,1},
                {-2,0,2},
                {-1,0,1}};
            }
        }

        public static float[,] SobelNYKernal
        {
            get
            {
                return new float[,]{
                {-1,-2,-1},
                {0,0,0},
                {1,2,1}};
            }
        }

        public static Bitmap ImageKernal( Bitmap input, float[,] matrix, bool alpha = false, bool printPercentage = false )
        {
            // lock input buffer for read only and copy to buffer
            Rectangle rect = new Rectangle( 0, 0, input.Width, input.Height );
            BitmapData inputDataRegion = input.LockBits( rect, ImageLockMode.ReadOnly, input.PixelFormat );
            Int32 pixelDepth = Bitmap.GetPixelFormatSize( inputDataRegion.PixelFormat ) / 8; //bytes per pixel
            byte[] inputBuffer = new byte[inputDataRegion.Width * inputDataRegion.Height * pixelDepth];
            //copy pixels to buffer
            Marshal.Copy( inputDataRegion.Scan0, inputBuffer, 0, inputBuffer.Length );
            
            byte[] outputBuffer = new byte[inputBuffer.Length];
            
            for( Int32 y = 0; y < input.Size.Height; ++y )
            {
                ImgaeKernal_ProcessScanline( y, inputBuffer, outputBuffer, pixelDepth, input.Size, matrix, alpha, printPercentage );
            }

            //Parallel.For( 0, input.Size.Height, y => { ImgaeKernal_ProcessScanline( y, inputBuffer, outputBuffer, pixelDepth, input.Size, matrix, alpha, printPercentage ); } );
            
            Bitmap output = new Bitmap( input.Size.Width, input.Size.Height, input.PixelFormat );

            BitmapData outputDataRegion = output.LockBits( rect, ImageLockMode.WriteOnly, input.PixelFormat );

            //Copy the buffer back to image
            Marshal.Copy( outputBuffer, 0, outputDataRegion.Scan0, outputBuffer.Length );

            input.UnlockBits( inputDataRegion );
            output.UnlockBits( outputDataRegion );

            if( printPercentage )
            {
                Console.Write( "\rDone.                                         \n" );
            }

            return output;
        }


        private static void ImgaeKernal_ProcessScanline( Int32 y, byte[] inputBuffer, byte[] outputBuffer, Int32 pixelDepth, Size imageSize, float[,] matrix, bool alpha, bool printPercentage )
        {
            Size halfMatrixSize = new Size( matrix.GetLength( 0 ) / 2, matrix.GetLength( 1 ) / 2 );

            for( Int32 x = 0; x < imageSize.Width; ++x )
            {
                float[] value = new float[pixelDepth];
                for( Int32 color = 0; color < pixelDepth; ++color ) { value[color] = 0.0f; }

                for( Int32 _y = -halfMatrixSize.Height; _y <= halfMatrixSize.Height; ++_y )
                {
                    for( Int32 _x = -halfMatrixSize.Width; _x <= halfMatrixSize.Width; ++_x )
                    {
                        Int32 px = Clamp( x + _y, 0, imageSize.Width - 1 );
                        Int32 py = Clamp( y + _x, 0, imageSize.Height - 1 );

                        Int32 inputBufferPixelIndex = GetPixelIndex( px, py, imageSize.Width, pixelDepth );

                        Int32 matrixIndexX = _x + halfMatrixSize.Width;
                        Int32 matrixIndexY = _y + halfMatrixSize.Height;
                        for( Int32 color = 0; color < pixelDepth; ++color )
                        {
                            value[color] +=
                                matrix[matrixIndexX, matrixIndexY] *
                                (float)inputBuffer[inputBufferPixelIndex + color];
                        }
                    }
                }

                Int32 outputBufferPixelIndex = GetPixelIndex( x, y, imageSize.Width, pixelDepth );
                if( !alpha && pixelDepth >= 4 )
                {
                    value[3] = 255.0f;
                }
                for( Int32 color = 0; color < pixelDepth; ++color )
                {
                    outputBuffer[outputBufferPixelIndex + color] = (byte)Clamp( value[color], 0.0f, 255.0f );
                }

                int _c;
                if( printPercentage && ( _c = y * imageSize.Width + x ) % 1200 == 0 )
                {
                    Console.Write( "{0:F2}%\r", ( (decimal)_c / (decimal)( imageSize.Width * imageSize.Height ) * 100.0m ) );
                }
            }
            return;
        }

        // TODO: make this function select actual average colors instead of most common colors
        public static Bitmap ReduceImageColors( Bitmap input, Int32 numDesiredColors = 256, bool alpha = false, bool printPercentage = false )
        {
            // lock input buffer for read only and copy to buffer
            Rectangle rect = new Rectangle( 0, 0, input.Width, input.Height );
            BitmapData inputDataRegion = input.LockBits( rect, ImageLockMode.ReadOnly, input.PixelFormat );
            Int32 pixelDepth = Bitmap.GetPixelFormatSize( inputDataRegion.PixelFormat ) / 8; //bytes per pixel
            byte[] inputBuffer = new byte[inputDataRegion.Width * inputDataRegion.Height * pixelDepth];
            //copy pixels to buffer
            Marshal.Copy( inputDataRegion.Scan0, inputBuffer, 0, inputBuffer.Length );

            byte[] outputBuffer = new byte[inputBuffer.Length];

            // do the processing
            {

                Dictionary<Int32, Int32> histogramDictionary = new Dictionary<Int32, Int32>();
                Dictionary<Int32, byte[]> colorDictionary = new Dictionary<Int32, byte[]>();

                // get the histogram data
                for( Int32 y = 0; y < input.Size.Height; ++y )
                {
                    ReduceImageColors_ProcessScanline_GenerateHistogram( y, inputBuffer, outputBuffer, pixelDepth, input.Size, histogramDictionary, colorDictionary, alpha, printPercentage );
                }

                // TODO: fix color selection
                // here we select the colors to be used in the output
                List<KeyValuePair<Int32, Int32>> histogram = histogramDictionary.ToList();

                histogram.Sort( delegate ( KeyValuePair<Int32, Int32> t1, KeyValuePair<Int32, Int32> t2 ) { return t1.Value - t2.Value; } );

                histogram = new List<KeyValuePair<Int32, Int32>>( histogram.Take( numDesiredColors ) );  // .Take(N) if N >= .Length, returns of size .Length

                List<byte[]> topColors = new List<byte[]>();
                foreach( KeyValuePair<Int32, Int32> k in histogram )
                {
                    topColors.Add( colorDictionary[ k.Key ] );
                }
                
                // get the outputImage
                for( Int32 y = 0; y < input.Size.Height; ++y )
                {
                    ReduceImageColors_ProcessScanline_ConsolidateColors( y, inputBuffer, outputBuffer, pixelDepth, input.Size, topColors, alpha, printPercentage );
                }
            }

            Bitmap output = new Bitmap( input.Size.Width, input.Size.Height, input.PixelFormat );

            BitmapData outputDataRegion = output.LockBits( rect, ImageLockMode.WriteOnly, input.PixelFormat );

            //Copy the buffer back to image
            Marshal.Copy( outputBuffer, 0, outputDataRegion.Scan0, outputBuffer.Length );

            input.UnlockBits( inputDataRegion );
            output.UnlockBits( outputDataRegion );

            if( printPercentage )
            {
                Console.Write( "\rDone.                                         \n" );
            }

            return output;
        }

        private static void ReduceImageColors_ProcessScanline_GenerateHistogram( Int32 y, byte[] inputBuffer, byte[] outputBuffer, Int32 pixelDepth, Size imageSize, Dictionary<Int32, Int32> histogram, Dictionary<Int32, byte[]> colorSet, bool alpha, bool printPercentage )
        {

            for( Int32 x = 0; x < imageSize.Width; ++x )
            {
                Int32 pixelIndex = GetPixelIndex( x, y, imageSize.Width, pixelDepth );

                byte[] value = new byte[pixelDepth];
                for( Int32 color = 0; color < pixelDepth; ++color ) { value[color] = inputBuffer[pixelIndex + color]; }

                if( !alpha && pixelDepth >= 4 )
                {
                    value[3] = 255;
                }

                Int32 colorIndex = GetColorIndex( value );

                if( histogram.Keys.Contains( colorIndex ) )
                {
                    histogram[colorIndex]++;
                }
                else
                {
                    histogram.Add( colorIndex, 1 );
                    colorSet.Add( colorIndex, value );
                }

                int _c;
                if( printPercentage && ( _c = y * imageSize.Width + x ) % 1200 == 0 )
                {
                    Console.Write( "{0:F2}%\r", ( (decimal)_c / (decimal)( imageSize.Width * imageSize.Height ) * 50.0m ) );
                }
            }
            return;
        }


        private static void ReduceImageColors_ProcessScanline_ConsolidateColors( Int32 y, byte[] inputBuffer, byte[] outputBuffer, Int32 pixelDepth, Size imageSize, List<byte[]> colors, bool alpha, bool printPercentage )
        {

            for( Int32 x = 0; x < imageSize.Width; ++x )
            {
                Int32 pixelIndex = GetPixelIndex( x, y, imageSize.Width, pixelDepth );

                byte[] value = new byte[pixelDepth];
                for( Int32 color = 0; color < pixelDepth; ++color ) { value[color] = inputBuffer[pixelIndex + color]; }

                if( !alpha && pixelDepth >= 4 )
                {
                    value[3] = 255;
                }

                Int32 bestColorIndex = 0;
                float bestColorDist = ColorValueDistance( colors[0], value );
                for( Int32 i = 1; i < colors.Count; ++i )
                {
                    float dist = ColorValueDistance( colors[i], value );
                    if( dist < bestColorDist )
                    {
                        bestColorDist = dist;
                        bestColorIndex = i;
                    }
                }

                for( Int32 color = 0; color < pixelDepth; ++color ) { outputBuffer[pixelIndex + color] = colors[bestColorIndex][color]; }

                int _c;
                if( printPercentage && ( _c = y * imageSize.Width + x ) % 1200 == 0 )
                {
                    Console.Write( "{0:F2}%\r", ( (decimal)_c / (decimal)( imageSize.Width * imageSize.Height ) * 50.0m + 50.0m ) );
                }
            }
            return;
        }


        public static Int32 GetPixelIndex( Int32 x, Int32 y, Int32 width, Int32 depth )
        {
            return ( ( y * width ) + x ) * depth;
        }

        private static float ColorValueDistance( byte[] a, byte[] b )
        {
            float sum = 0.0f;

            for( Int32 i = 0; i < a.Length && i < b.Length; ++i )
            {
                float dif = (int)a[i] - (int)b[i];
                sum += dif * dif;
            }
            return (float)Math.Sqrt(sum);
        }

        public static Int32 GetColorIndex( byte[] color )
        {
            return BitConverter.ToInt32( color, 0 );
        }
        
        public static Int32 BytesPerPixelIn( Bitmap input )
        {
            return Bitmap.GetPixelFormatSize( input.PixelFormat ) / 8;
        }

        public static byte[] ExtractImageData( Bitmap input )
        {
            Rectangle rect = new Rectangle( 0, 0, input.Width, input.Height );
            BitmapData dataRegion = input.LockBits( rect, ImageLockMode.ReadOnly, input.PixelFormat );
            Int32 pixelDepth = Bitmap.GetPixelFormatSize( dataRegion.PixelFormat ) / 8; //bytes per pixel
            byte[] output = new byte[dataRegion.Width * dataRegion.Height * pixelDepth];
            //copy pixels to buffer
            Marshal.Copy( dataRegion.Scan0, output, 0, output.Length );

            input.UnlockBits( dataRegion );

            return output;
        }

    }
}
