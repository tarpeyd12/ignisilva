using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ignisilva
{
    class ColorStore : IXmlWritable
    {
        private Int32 _maxNumColors;
        private Int32 _colorlength;

        private object _sync;

        private Dictionary<UInt32, Tuple<Int32, byte[]>> hashToColor;
        private Dictionary<Int32, byte[]> idToColor;

        private HashTableHashing.IHashAlgorithm _hasher;

        public Int32 MaxNumColors { get { return _maxNumColors; } }
        public Int32 HistogramLength { get { return _maxNumColors; } }
        public Int32 NumColors { get { return hashToColor.Count; } }
        public Int32 ColorLength { get { return _colorlength; } }


        public ColorStore( Int32 cololength, Int32 maxnumcolors)
        {
            _sync = new object();

            _maxNumColors = maxnumcolors;
            if( _maxNumColors <= 0 )
            {
                _maxNumColors = 1;
            }
            _colorlength = cololength;
            if( _colorlength <= 0 )
            {
                _colorlength = 1;
            }

            hashToColor = new Dictionary<UInt32, Tuple<Int32, byte[]>>();

            _hasher = new HashTableHashing.MurmurHash2Simple();
        }

        public byte[] ColorToHistogram( byte[] color )
        {
            UInt32 hash = _hasher.Hash( color );

            // if the color is not in the database
            if( !hashToColor.ContainsKey( hash ) )
            {
                lock( _sync )
                {
                    if( NumColors >= MaxNumColors )
                    {
                        return null;
                    }
                    
                    idToColor = null;

                    // add it
                    hashToColor.Add( hash, Tuple.Create( NumColors, color ) );
                }
                
            }

            byte[] histogram = new byte[HistogramLength];

            /*for( UInt32 i = 0; i < histogram.Length; ++i )
            {
                histogram[i] = 0;
            }*/

            // set the index of the color in the histogram to max value
            histogram[hashToColor[hash].Item1] = 255;

            return histogram;
        }


        public byte[] HistogramToColor( byte[] histogram )
        {
            if( histogram.Length != HistogramLength )
            {
                return null;
            }

            if( null == idToColor )
            {
                lock( _sync )
                {
                    idToColor = new Dictionary<Int32, byte[]>();

                    foreach( KeyValuePair<UInt32, Tuple<Int32, byte[]>> hc in hashToColor )
                    {
                        idToColor.Add( hc.Value.Item1, hc.Value.Item2 );
                    }
                }
            }
            
            double[] floatColor = new double[ColorLength];
            double total = 0.0;

            for( Int32 index = 0; index < NumColors; ++index )
            {
                double value = (double)histogram[index] / 255.0;
                total += value;
                byte[] colorAtIndex = idToColor[index];

                for( UInt32 i = 0; i < ColorLength; ++i )
                {
                    floatColor[i] += ((double)colorAtIndex[i]/255.0) * value;
                }
            }

            //floatColor = Func.NormalizeDoubleArray( floatColor );

            double numColors = (double)NumColors;
            byte[] color = new byte[ColorLength];

            for( UInt32 i = 0; i < color.Length; ++i )
            {
                color[i] = (byte)( Func.Clamp( floatColor[i] / total * 255.0, 0.0, 255.0 ) );
            }
            
            return color;
        }

        public XmlWriter WriteXml( XmlWriter xml, string fmt = "b64" )
        {
            xml.WriteStartElement( "color_store" );
            xml.WriteAttributeString( "color_len", ColorLength.ToString() );
            xml.WriteAttributeString( "hist_len", HistogramLength.ToString() );
            xml.WriteAttributeString( "num_colors", NumColors.ToString() );
            foreach( KeyValuePair<UInt32, Tuple<Int32, byte[]>> hc in hashToColor )
            { 
                xml.WriteStartElement( "color" );
                xml.WriteAttributeString( "id", hc.Value.Item1.ToString() );
                string format = XmlHelper.RecommendBinaryEncodingFormat( fmt, hc.Value.Item2 );
                xml.WriteAttributeString( "format", format );
                XmlHelper.WriteByteString( xml, hc.Value.Item2, format );
                xml.WriteEndElement();
            }
            xml.WriteEndElement();
            return xml;
        }

    }
}
