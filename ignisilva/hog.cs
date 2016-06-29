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

        protected static float _get01AngleFromVxVy( float xG, float yG )
        {
            float angle = (float)Math.Atan2( yG, xG );
            if( angle < 0.0f )
            {
                angle += (float)Math.PI * 2.0f;
            }
            
            return angle / ( (float)Math.PI * 2.0f );
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

                    /*float angle = (float)Math.Atan2( yG, xG );
                    if( angle < 0.0f )
                    {
                        angle += (float)Math.PI * 2.0f;
                    }
                    output[x, y, 0] = angle / ( (float)Math.PI * 2.0f );*/
                    output[x, y, 0] = _get01AngleFromVxVy( xG, yG );
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
                    float mg = angleGradients[x,y,1];
                    Color c = Color.FromArgb( 0, 0, 255);
                    //if( !float.IsNaN( angleGradients[x, y, 0] ) )
                    {
                        Int32 p = ImageProcessing.Clamp( (Int32)(mg*255.0f), 0, 255 );
                        c = Color.FromArgb( p, p, p );
                    }
                    output.SetPixel( x, y, c );
                }
            }

            return output;
        }

        public static Bitmap CalculateGradiantAngleMagnitudesToBitmap( Bitmap input )
        {
            Bitmap output = new Bitmap( input.Size.Width, input.Size.Height );

            float[,,] angleGradients = CalculateGradiantAngles( input );

            for( Int32 x = 0; x < angleGradients.GetLength( 0 ); ++x )
            {
                for( Int32 y = 0; y < angleGradients.GetLength( 1 ); ++y )
                {
                    Int32 r, g, b;
                    b = 0;

                    if( float.IsNaN( angleGradients[x, y, 0] ) )
                    {
                        r = 0;
                        b = 255;
                    }
                    else
                    {
                        r = ImageProcessing.Clamp( (Int32)( angleGradients[x, y, 0] * 255.0f ), 0, 255 );
                    }
                    g = ImageProcessing.Clamp( (Int32)(angleGradients[x,y,1]*255.0f), 0, 255 );
                    
                    output.SetPixel( x, y, Color.FromArgb( r, g, b ) );
                }
            }

            return output;
        }

        protected static Int32 _get9BinValueFromn01Direction( float input )
        {
            float uangle = input * 2.0f - 1.0f; // scale form 0 to 1 to -1 to 1

            if( uangle < 0.0f ) uangle += 1.0f;

            Int32 z = (int)(uangle*9) % 9; // bin in 9's
            return z;
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

                    /*float uangle = Math.Abs(angles[x,y,0]*2.0f-1.0f); // scale form 0 to 1 to -1 to 1
                    
                    Int32 z = (int)(uangle*9) % 9; // bin in 9's
                    */
                    Int32 z = _get9BinValueFromn01Direction( angles[x,y,0] );

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
                        mag2 += output[x, y, z] * output[x, y, z];
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

        public static Bitmap BinGradientsToBitmap( float[,,] binGradients, Size binSize, Bitmap input )
        {
            return BinGradientsToBitmap( binGradients, binSize, input, Color.FromArgb(0,255,0) );
        }

        public static Bitmap BinGradientsToBitmap( float[,,] binGradients, Size binSize, Bitmap input, Color color )
        {
            Bitmap output = new Bitmap( input );
            
            float halfBinX = (float)binSize.Width / 2.0f;
            float halfBinY = (float)binSize.Height / 2.0f;
            
            for( Int32 y = 0; y < binGradients.GetLength(1); ++y )
            {
                for( Int32 x = 0; x < binGradients.GetLength(0); ++x )
                {
                    List<KeyValuePair<Int32,float>> dirs = new List<KeyValuePair<Int32,float>>();
                    for( Int32 z = 0; z < binGradients.GetLength(2); ++z )
                    {
                        dirs.Add( new KeyValuePair<Int32, float>( z, binGradients[x, y, z] ) );
                    }

                    dirs.Sort( ( a, b ) => a.Value.CompareTo( b.Value ) );

                    for( Int32 z = 0; z < dirs.Count; ++z )
                    {
                        float angle = (float)((dirs[z].Key/(float)(binGradients.GetLength(2))) * Math.PI) + (float)Math.PI*0.5f;
                        float radius = dirs[z].Value;
                        float cX = ( ((float)( x )+0.5f)*(float)(binSize.Width) );
                        float cY = ( ((float)( y )+0.5f)*(float)(binSize.Height) );

                        Int32[] lX = new Int32[] { (Int32)( (float)cX - Math.Cos( angle ) * halfBinX * radius ), (Int32)( (float)cX + Math.Cos( angle ) * halfBinX * radius ) };
                        Int32[] lY = new Int32[] { (Int32)( (float)cY - Math.Sin( angle ) * halfBinY * radius ), (Int32)( (float)cY + Math.Sin( angle ) * halfBinY * radius ) };

                        using( Graphics graphics = Graphics.FromImage( output ) )
                        {
                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            Pen pen = new Pen( Color.FromArgb( (Int32)ImageProcessing.Clamp( color.R * radius, 0.0f, 255.0f ), (Int32)ImageProcessing.Clamp( color.G * radius, 0.0f, 255.0f ), (Int32)ImageProcessing.Clamp( color.B * radius, 0.0f, 255.0f ) ), 1 );
                            graphics.DrawLine( pen, lX[0], lY[0], lX[1], lY[1] );
                            pen.Dispose();
                        }
                    }
                }
            }

            return output;

        }

        public static Bitmap BinGradientsToBitmap2( float[,,] binGradients, Size binSize, Bitmap input )
        {
            //Bitmap output = new Bitmap( binGradients.GetLength(0)*binSize.Width, binGradients.GetLength(1)*binSize.Height );
            Bitmap output = new Bitmap( input.Size.Width, input.Size.Height );

            for( Int32 y = 0; y < output.Size.Height; ++y )
            {
                for( Int32 x = 0; x < output.Size.Width; ++x )
                {
                    //Int32 inX = ImageProcessing.Clamp( x, 0, input.Size.Width-1 );
                    //Int32 inY = ImageProcessing.Clamp( y, 0, input.Size.Height-1 );

                    //Color pixel = input.GetPixel( inX, inY );
                    Color inpixel = input.GetPixel( x, y );

                    Int32 x8 = x/binSize.Width;
                    Int32 y8 = y/binSize.Height;

                    Int32 x82 = x8*binSize.Width + binSize.Width/2;
                    Int32 y82 = y8*binSize.Height + binSize.Height/2;

                    float rx = (x82-x) + (binSize.Width%2!=0?0.0f:0.5f);
                    float ry = (y82-y) + (binSize.Height%2!=0?0.0f:0.5f);

                    if( rx == ry && rx == 0.0f )
                    {
                        //output.SetPixel( x, y, Color.FromArgb( 255, 255, 255 ) );
                        continue;
                    }

                    float rmag = (float)Math.Sqrt( rx*rx + ry*ry );
                    rx /= rmag;
                    ry /= rmag;

                    float ang = _get01AngleFromVxVy( rx, ry );
                    Int32 bin = _get9BinValueFromn01Direction( ang+0.25f );

                    float bv = (binGradients[x8,y8,bin]/rmag);

                    /*int r = 0;
                    int g = 0;
                    int b = 0;*/

                    //Int32 ac = ImageProcessing.Clamp( (Int32)(ang*255.0f), 0, 255 );
                    //Int32 bc = ImageProcessing.Clamp( (Int32)( (float)(bin)*(255.0f/9.0f) ), 0, 255 );

                    Int32 pc = ImageProcessing.Clamp( (Int32)(bv*255.0f), 0, 255 );

                    /*r = 255-pc;
                    b = ac;
                    g = bc;*/

                    Int32 gs = (Int32)(inpixel.GetBrightness()*255.0f);

                    output.SetPixel( x, y, Color.FromArgb( 0, pc, 0 ) );
                }
            }

            return output;
        }


    }
}
