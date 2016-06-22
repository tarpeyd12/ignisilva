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

        public static float[,,] CalculateGradiantAngles( Bitmap input )
        {
            float[,,] output = new float[input.Size.Width,input.Size.Height,2];

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

                    float mag = (float)Math.Sqrt( xG*xG + yG*yG );

                    xG /= mag;
                    yG /= mag;

                    if( xG == 0.0f && yG == 0.0f )
                    {
                        output[x, y, 0] = float.NaN;
                        output[x, y, 1] = 0.0f;
                        continue;
                    }

                    float angle = (float)Math.Atan2( yG, xG );
                    if( angle < 0.0f )
                    {
                        angle += (float)Math.PI * 2.0f;
                    }
                    output[x, y, 0] = angle / ( (float)Math.PI * 2.0f );
                    output[x, y, 1] = mag;
                    
                }
            }

            return output;
        }

        public static Bitmap CalculateGradiantAnglesToBitmap( Bitmap input )
        {
            Bitmap output = new Bitmap( input.Size.Width, input.Size.Height );

            float[,,] angleGradients = CalculateGradiantAngles( input );

            for( Int32 x = 0; x < angleGradients.GetLength( 0 ); ++x )
            {
                for( Int32 y = 0; y < angleGradients.GetLength( 1 ); ++y )
                {
                    float ag = angleGradients[x,y,0];
                    Color c = Color.FromArgb( 0, 0, 255);
                    if( !float.IsNaN( ag ) )
                    {
                        Int32 p = ImageProcessing.Clamp( (Int32)(ag*255.0f), 0, 255 );
                        c = Color.FromArgb( p, p, p );
                    }
                    output.SetPixel( x, y, c ); 
                }
            }

            return output;
        }

        public static Bitmap CalculateGradiantMagnitudesToBitmap( Bitmap input )
        {
            Bitmap output = new Bitmap( input.Size.Width, input.Size.Height );

            float[,,] angleGradients = CalculateGradiantAngles( input );

            for( Int32 x = 0; x < angleGradients.GetLength( 0 ); ++x )
            {
                for( Int32 y = 0; y < angleGradients.GetLength( 1 ); ++y )
                {
                    float ag = angleGradients[x,y,1];
                    Color c = Color.FromArgb( 0, 0, 255);
                    //if( !float.IsNaN( angleGradients[x, y, 0] ) )
                    {
                        Int32 p = ImageProcessing.Clamp( (Int32)(ag*255.0f), 0, 255 );
                        c = Color.FromArgb( p, p, p );
                    }
                    output.SetPixel( x, y, c );
                }
            }

            return output;
        }

        public static float[,,] BinGradients( float[,,] angles )
        {
            return BinGradients( angles, new Size( 8, 8 ) );
        }

        public static float[,,] BinGradients( float[,,] angles, Size binSize )
        {
            float[,,] output = new float[angles.GetLength(0)/binSize.Width+1,angles.GetLength(1)/binSize.Height+1,9];

            for( Int32 x = 0; x < angles.GetLength( 0 ); ++x )
            {
                for( Int32 y = 0; y < angles.GetLength( 1 ); ++y )
                {
                    if( float.IsNaN( angles[x, y, 0] ) )
                    {
                        continue;
                    }

                    float uangle = Math.Abs(angles[x,y,0]*2.0f-1.0f); // scale form 0 to 1 to -1 to 1
                    
                    Int32 z = (int)(uangle*9) % 9; // bin in 9's

                    output[x / binSize.Width, y / binSize.Height, z] += angles[x, y, 1];
                }
            }

            // normalize the gradients
            // NOTE: I think ...
            for( Int32 x = 0; x < output.GetLength( 0 ); ++x )
            {
                for( Int32 y = 0; y < output.GetLength( 1 ); ++y )
                {
                    float mag2 = 0.0f;
                    for( Int32 z = 0; z < output.GetLength( 2 ); ++z )
                    {
                        mag2 += output[x, y, z];
                    }

                    float mag = (float)Math.Sqrt( mag2 );
                    for( Int32 z = 0; z < output.GetLength( 2 ); ++z )
                    {
                        output[x, y, z] /= mag;
                    }
                }
            }

            return output;
        }


    }
}
