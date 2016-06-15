using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ignisilva
{
    class Hog
    {
        private static Color BytesToColor( byte[] colorBytes, Int32 index, Int32 depth )
        {
            byte[] b = new byte[depth];
            for( Int32 c = 0; c < depth; ++c ) b[c] = colorBytes[c + index];
            return BytesToColor( b );
        }

        private static Color BytesToColor( byte[] b )
        {
            return Color.FromArgb( ImageProcessing.GetColorIndex( b ) );
        }

        public static float[,] CalculateGradiantAngles( Bitmap input )
        {
            float[,] output = new float[input.Size.Width,input.Size.Height];

            Int32 pixelDepth = ImageProcessing.BytesPerPixelIn( input );

            byte[] pixels = ImageProcessing.ExtractImageData( input );

            for( Int32 x = 0; x < output.GetLength( 0 ); ++x )
            {
                for( Int32 y = 0; y < output.GetLength( 1 ); ++y )
                {
                    Int32[] xIndexes = new Int32[] { ImageProcessing.GetPixelIndex( ImageProcessing.Clamp( x + 1, 0, input.Size.Width - 1 ),  y, input.Size.Width, pixelDepth ), ImageProcessing.GetPixelIndex( ImageProcessing.Clamp( x - 1, 0, input.Size.Width - 1 ),  y, input.Size.Width, pixelDepth ) };
                    Int32[] yIndexes = new Int32[] { ImageProcessing.GetPixelIndex( x, ImageProcessing.Clamp( y + 1, 0, input.Size.Height - 1 ), input.Size.Width, pixelDepth ), ImageProcessing.GetPixelIndex( x, ImageProcessing.Clamp( y - 1, 0, input.Size.Height - 1 ), input.Size.Width, pixelDepth ) };

                    Color[] xColors = new Color[] { BytesToColor( pixels, xIndexes[0], pixelDepth ), BytesToColor( pixels, xIndexes[1], pixelDepth ) };
                    Color[] yColors = new Color[] { BytesToColor( pixels, yIndexes[0], pixelDepth ), BytesToColor( pixels, yIndexes[1], pixelDepth ) };

                    float xG = xColors[0].GetBrightness() - xColors[1].GetBrightness();
                    float yG = yColors[0].GetBrightness() - yColors[1].GetBrightness();

                    output[x, y] = ( (float)Math.Atan2( xG, yG ) / (float)Math.PI + 1.0f ) / 2.0f;
                    //output[x, y] = Math.Abs( (float)Math.Atan2( xG, yG ) / (float)Math.PI );
                }
            }

            return output;
        }

        public static Bitmap CalculateGradiantAnglesToBitmap( Bitmap input )
        {
            Bitmap output = new Bitmap( input.Size.Width, input.Size.Height );

            float[,] angleGradients = CalculateGradiantAngles( input );

            for( Int32 x = 0; x < angleGradients.GetLength( 0 ); ++x )
            {
                for( Int32 y = 0; y < angleGradients.GetLength( 1 ); ++y )
                {
                    Int32 p = ImageProcessing.Clamp( (Int32)(angleGradients[x,y]*255.0f), 0, 255 );
                    Color c = Color.FromArgb( p, p, p );
                    output.SetPixel( x, y, c ); 
                }
            }

            return output;
        }
    }
}
