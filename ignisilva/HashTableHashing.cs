using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// http://landman-code.blogspot.com/2009/02/c-superfasthash-and-murmurhash2.html

/***** BEGIN LICENSE BLOCK *****
 * Version: MPL 1.1/GPL 2.0/LGPL 2.1
 *
 * The contents of this file are subject to the Mozilla Public License Version
 * 1.1 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the
 * License.
 *
 * The Original Code is HashTableHashing.MurmurHash2.
 *
 * The Initial Developer of the Original Code is
 * Davy Landman.
 * Portions created by the Initial Developer are Copyright (C) 2009
 * the Initial Developer. All Rights Reserved.
 *
 * Contributor(s):
 *
 *
 * Alternatively, the contents of this file may be used under the terms of
 * either the GNU General Public License Version 2 or later (the "GPL"), or
 * the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
 * in which case the provisions of the GPL or the LGPL are applicable instead
 * of those above. If you wish to allow use of your version of this file only
 * under the terms of either the GPL or the LGPL, and not to allow others to
 * use your version of this file under the terms of the MPL, indicate your
 * decision by deleting the provisions above and replace them with the notice
 * and other provisions required by the GPL or the LGPL. If you do not delete
 * the provisions above, a recipient may use your version of this file under
 * the terms of any one of the MPL, the GPL or the LGPL.
 *
 * ***** END LICENSE BLOCK ***** */

using System;
using System.Runtime.InteropServices;

namespace ignisilva
{
    using System;

    namespace HashTableHashing
    {
        public interface IHashAlgorithm
        {
            UInt32 Hash( Byte[] data );
        }
        public interface ISeededHashAlgorithm : IHashAlgorithm
        {
            UInt32 Hash( Byte[] data, UInt32 seed );
        }
    }

    namespace HashTableHashing
    {
        public class SuperFastHashSimple : IHashAlgorithm
        {
            public UInt32 Hash( Byte[] dataToHash )
            {
                Int32 dataLength = dataToHash.Length;
                if( dataLength == 0 )
                    return 0;
                UInt32 hash = Convert.ToUInt32( dataLength );
                Int32 remainingBytes = dataLength & 3; // mod 4
                Int32 numberOfLoops = dataLength >> 2; // div 4
                Int32 currentIndex = 0;
                while( numberOfLoops > 0 )
                {
                    hash += BitConverter.ToUInt16( dataToHash, currentIndex );
                    UInt32 tmp = (UInt32)( BitConverter.ToUInt16( dataToHash, currentIndex + 2 ) << 11 ) ^ hash;
                    hash = ( hash << 16 ) ^ tmp;
                    hash += hash >> 11;
                    currentIndex += 4;
                    numberOfLoops--;
                }

                switch( remainingBytes )
                {
                    case 3:
                        hash += BitConverter.ToUInt16( dataToHash, currentIndex );
                        hash ^= hash << 16;
                        hash ^= ( (UInt32)dataToHash[currentIndex + 2] ) << 18;
                        hash += hash >> 11;
                        break;
                    case 2:
                        hash += BitConverter.ToUInt16( dataToHash, currentIndex );
                        hash ^= hash << 11;
                        hash += hash >> 17;
                        break;
                    case 1:
                        hash += dataToHash[currentIndex];
                        hash ^= hash << 10;
                        hash += hash >> 1;
                        break;
                    default:
                        break;
                }

                /* Force "avalanching" of final 127 bits */
                hash ^= hash << 3;
                hash += hash >> 5;
                hash ^= hash << 4;
                hash += hash >> 17;
                hash ^= hash << 25;
                hash += hash >> 6;

                return hash;
            }
        }

        public class SuperFastHashInlineBitConverter : IHashAlgorithm
        {
            public UInt32 Hash( Byte[] dataToHash )
            {
                Int32 dataLength = dataToHash.Length;
                if( dataLength == 0 )
                    return 0;
                UInt32 hash = (UInt32)dataLength;
                Int32 remainingBytes = dataLength & 3; // mod 4
                Int32 numberOfLoops = dataLength >> 2; // div 4
                Int32 currentIndex = 0;
                while( numberOfLoops > 0 )
                {
                    hash += (UInt16)( dataToHash[currentIndex++] | dataToHash[currentIndex++] << 8 );
                    UInt32 tmp = (UInt32)( (UInt32)( dataToHash[currentIndex++] | dataToHash[currentIndex++] << 8 ) << 11 ) ^ hash;
                    hash = ( hash << 16 ) ^ tmp;
                    hash += hash >> 11;
                    numberOfLoops--;
                }

                switch( remainingBytes )
                {
                    case 3:
                        hash += (UInt16)( dataToHash[currentIndex++] | dataToHash[currentIndex++] << 8 );
                        hash ^= hash << 16;
                        hash ^= ( (UInt32)dataToHash[currentIndex] ) << 18;
                        hash += hash >> 11;
                        break;
                    case 2:
                        hash += (UInt16)( dataToHash[currentIndex++] | dataToHash[currentIndex] << 8 );
                        hash ^= hash << 11;
                        hash += hash >> 17;
                        break;
                    case 1:
                        hash += dataToHash[currentIndex];
                        hash ^= hash << 10;
                        hash += hash >> 1;
                        break;
                    default:
                        break;
                }

                /* Force "avalanching" of final 127 bits */
                hash ^= hash << 3;
                hash += hash >> 5;
                hash ^= hash << 4;
                hash += hash >> 17;
                hash ^= hash << 25;
                hash += hash >> 6;

                return hash;
            }
        }

        public class SuperFastHashUInt16Hack : IHashAlgorithm
        {
            [StructLayout( LayoutKind.Explicit )]
            // no guarantee this will remain working
            struct BytetoUInt16Converter
            {
                [FieldOffset( 0 )]
                public Byte[] Bytes;

                [FieldOffset( 0 )]
                public UInt16[] UInts;
            }

            public UInt32 Hash( Byte[] dataToHash )
            {
                Int32 dataLength = dataToHash.Length;
                if( dataLength == 0 )
                    return 0;
                UInt32 hash = (UInt32)dataLength;
                Int32 remainingBytes = dataLength & 3; // mod 4
                Int32 numberOfLoops = dataLength >> 2; // div 4
                Int32 currentIndex = 0;
                UInt16[] arrayHack = new BytetoUInt16Converter { Bytes = dataToHash }.UInts;
                while( numberOfLoops > 0 )
                {
                    hash += arrayHack[currentIndex++];
                    UInt32 tmp = (UInt32)( arrayHack[currentIndex++] << 11 ) ^ hash;
                    hash = ( hash << 16 ) ^ tmp;
                    hash += hash >> 11;
                    numberOfLoops--;
                }
                currentIndex *= 2; // fix the length
                switch( remainingBytes )
                {
                    case 3:
                        hash += (UInt16)( dataToHash[currentIndex++] | dataToHash[currentIndex++] << 8 );
                        hash ^= hash << 16;
                        hash ^= ( (UInt32)dataToHash[currentIndex] ) << 18;
                        hash += hash >> 11;
                        break;
                    case 2:
                        hash += (UInt16)( dataToHash[currentIndex++] | dataToHash[currentIndex] << 8 );
                        hash ^= hash << 11;
                        hash += hash >> 17;
                        break;
                    case 1:
                        hash += dataToHash[currentIndex];
                        hash ^= hash << 10;
                        hash += hash >> 1;
                        break;
                    default:
                        break;
                }

                /* Force "avalanching" of final 127 bits */
                hash ^= hash << 3;
                hash += hash >> 5;
                hash ^= hash << 4;
                hash += hash >> 17;
                hash ^= hash << 25;
                hash += hash >> 6;

                return hash;
            }
        }

        /*public class SuperFastHashUnsafe : IHashAlgorithm
        {
            public unsafe UInt32 Hash( Byte[] dataToHash )
            {
                Int32 dataLength = dataToHash.Length;
                if( dataLength == 0 )
                    return 0;
                UInt32 hash = (UInt32)dataLength;
                Int32 remainingBytes = dataLength & 3; // mod 4
                Int32 numberOfLoops = dataLength >> 2; // div 4

                fixed ( byte* firstByte = &( dataToHash[0] ) )
                {
                    // Main loop 
                    UInt16* data = (UInt16*)firstByte;
                    for( ; numberOfLoops > 0; numberOfLoops-- )
                    {
                        hash += *data;
                        UInt32 tmp = (UInt32)( *( data + 1 ) << 11 ) ^ hash;
                        hash = ( hash << 16 ) ^ tmp;
                        data += 2;
                        hash += hash >> 11;
                    }
                    switch( remainingBytes )
                    {
                        case 3:
                            hash += *data;
                            hash ^= hash << 16;
                            hash ^= ( (UInt32)( *( ( (Byte*)( data ) ) + 2 ) ) ) << 18;
                            hash += hash >> 11;
                            break;
                        case 2:
                            hash += *data;
                            hash ^= hash << 11;
                            hash += hash >> 17;
                            break;
                        case 1:
                            hash += *( (Byte*)data );
                            hash ^= hash << 10;
                            hash += hash >> 1;
                            break;
                        default:
                            break;
                    }
                }

                // Force "avalanching" of final 127 bits 
                hash ^= hash << 3;
                hash += hash >> 5;
                hash ^= hash << 4;
                hash += hash >> 17;
                hash ^= hash << 25;
                hash += hash >> 6;

                return hash;
            }
        }*/

    }

    namespace HashTableHashing
    {
        public class MurmurHash2Simple : ISeededHashAlgorithm
        {
            public UInt32 Hash( Byte[] data )
            {
                return Hash( data, 0xc58f1a7b );
            }
            const UInt32 m = 0x5bd1e995;
            const Int32 r = 24;

            public UInt32 Hash( Byte[] data, UInt32 seed )
            {
                Int32 length = data.Length;
                if( length == 0 )
                    return 0;
                UInt32 h = seed ^ (UInt32)length;
                Int32 currentIndex = 0;
                while( length >= 4 )
                {
                    UInt32 k = BitConverter.ToUInt32( data, currentIndex );
                    k *= m;
                    k ^= k >> r;
                    k *= m;

                    h *= m;
                    h ^= k;
                    currentIndex += 4;
                    length -= 4;
                }
                switch( length )
                {
                    case 3:
                        h ^= BitConverter.ToUInt16( data, currentIndex );
                        h ^= (UInt32)data[currentIndex + 2] << 16;
                        h *= m;
                        break;
                    case 2:
                        h ^= BitConverter.ToUInt16( data, currentIndex );
                        h *= m;
                        break;
                    case 1:
                        h ^= data[currentIndex];
                        h *= m;
                        break;
                    default:
                        break;
                }

                // Do a few final mixes of the hash to ensure the last few
                // bytes are well-incorporated.

                h ^= h >> 13;
                h *= m;
                h ^= h >> 15;

                return h;
            }
        }

        public class MurmurHash2InlineBitConverter : ISeededHashAlgorithm
        {

            public UInt32 Hash( Byte[] data )
            {
                return Hash( data, 0xc58f1a7b );
            }
            const UInt32 m = 0x5bd1e995;
            const Int32 r = 24;

            public UInt32 Hash( Byte[] data, UInt32 seed )
            {
                Int32 length = data.Length;
                if( length == 0 )
                    return 0;
                UInt32 h = seed ^ (UInt32)length;
                Int32 currentIndex = 0;
                while( length >= 4 )
                {
                    UInt32 k = (UInt32)( data[currentIndex++] | data[currentIndex++] << 8 | data[currentIndex++] << 16 | data[currentIndex++] << 24 );
                    k *= m;
                    k ^= k >> r;
                    k *= m;

                    h *= m;
                    h ^= k;
                    length -= 4;
                }
                switch( length )
                {
                    case 3:
                        h ^= (UInt16)( data[currentIndex++] | data[currentIndex++] << 8 );
                        h ^= (UInt32)( data[currentIndex] << 16 );
                        h *= m;
                        break;
                    case 2:
                        h ^= (UInt16)( data[currentIndex++] | data[currentIndex] << 8 );
                        h *= m;
                        break;
                    case 1:
                        h ^= data[currentIndex];
                        h *= m;
                        break;
                    default:
                        break;
                }

                // Do a few final mixes of the hash to ensure the last few
                // bytes are well-incorporated.

                h ^= h >> 13;
                h *= m;
                h ^= h >> 15;

                return h;
            }
        }

        public class MurmurHash2UInt32Hack : ISeededHashAlgorithm
        {
            public UInt32 Hash( Byte[] data )
            {
                return Hash( data, 0xc58f1a7b );
            }
            const UInt32 m = 0x5bd1e995;
            const Int32 r = 24;

            [StructLayout( LayoutKind.Explicit )]
            struct BytetoUInt32Converter
            {
                [FieldOffset( 0 )]
                public Byte[] Bytes;

                [FieldOffset( 0 )]
                public UInt32[] UInts;
            }

            public UInt32 Hash( Byte[] data, UInt32 seed )
            {
                Int32 length = data.Length;
                if( length == 0 )
                    return 0;
                UInt32 h = seed ^ (UInt32)length;
                Int32 currentIndex = 0;
                // array will be length of Bytes but contains Uints
                // therefore the currentIndex will jump with +1 while length will jump with +4
                UInt32[] hackArray = new BytetoUInt32Converter { Bytes = data }.UInts;
                while( length >= 4 )
                {
                    UInt32 k = hackArray[currentIndex++];
                    k *= m;
                    k ^= k >> r;
                    k *= m;

                    h *= m;
                    h ^= k;
                    length -= 4;
                }
                currentIndex *= 4; // fix the length
                switch( length )
                {
                    case 3:
                        h ^= (UInt16)( data[currentIndex++] | data[currentIndex++] << 8 );
                        h ^= (UInt32)data[currentIndex] << 16;
                        h *= m;
                        break;
                    case 2:
                        h ^= (UInt16)( data[currentIndex++] | data[currentIndex] << 8 );
                        h *= m;
                        break;
                    case 1:
                        h ^= data[currentIndex];
                        h *= m;
                        break;
                    default:
                        break;
                }

                // Do a few final mixes of the hash to ensure the last few
                // bytes are well-incorporated.

                h ^= h >> 13;
                h *= m;
                h ^= h >> 15;

                return h;
            }
        }

        /*public class MurmurHash2Unsafe : ISeededHashAlgorithm
        {
            public UInt32 Hash( Byte[] data )
            {
                return Hash( data, 0xc58f1a7b );
            }
            const UInt32 m = 0x5bd1e995;
            const Int32 r = 24;

            public unsafe UInt32 Hash( Byte[] data, UInt32 seed )
            {
                Int32 length = data.Length;
                if( length == 0 )
                    return 0;
                UInt32 h = seed ^ (UInt32)length;
                Int32 remainingBytes = length & 3; // mod 4
                Int32 numberOfLoops = length >> 2; // div 4
                fixed ( byte* firstByte = &( data[0] ) )
                {
                    UInt32* realData = (UInt32*)firstByte;
                    while( numberOfLoops != 0 )
                    {
                        UInt32 k = *realData;
                        k *= m;
                        k ^= k >> r;
                        k *= m;

                        h *= m;
                        h ^= k;
                        numberOfLoops--;
                        realData++;
                    }
                    switch( remainingBytes )
                    {
                        case 3:
                            h ^= (UInt16)( *realData );
                            h ^= ( (UInt32)( *( ( (Byte*)( realData ) ) + 2 ) ) ) << 16;
                            h *= m;
                            break;
                        case 2:
                            h ^= (UInt16)( *realData );
                            h *= m;
                            break;
                        case 1:
                            h ^= *( (Byte*)realData );
                            h *= m;
                            break;
                        default:
                            break;
                    }
                }

                // Do a few final mixes of the hash to ensure the last few
                // bytes are well-incorporated.

                h ^= h >> 13;
                h *= m;
                h ^= h >> 15;

                return h;
            }
        }*/
    }
    
}
