/**
 * @file
 * @copyright  Copyright (c) 2019 Jesús González del Río
 * @license    See LICENSE.txt
 */

using System;
using System.IO;
using System.Threading.Tasks;

namespace FirmwareFile
{
    /**
     * Loader for firmware files in Motorola (SREC/S19/S28/S37) format.
     */
    public static class MotorolaFileLoader
    {
        /*===========================================================================
         *                            PUBLIC METHODS
         *===========================================================================*/

        /**
         * Loads a firmware from the file at the given path.
         * 
         * @param [in] filePath Path to the file containing the firmware
         */
        public static Firmware Load( string filePath )
        {
            return LoadAsync( filePath ).GetAwaiter().GetResult();
        }

        /**
         * Loads a firmware from the file at the given stream.
         * 
         * @param [in] stream Stream to provide the firmware file contents
         */
        public static Firmware Load( Stream stream )
        {
            return LoadAsync( stream ).GetAwaiter().GetResult();
        }

        /**
         * Loads asynchronously a firmware from the file at the given path.
         * 
         * @param [in] filePath Path to the file containing the firmware
         */
        public static async Task<Firmware> LoadAsync( string filePath )
        {
            using var fileStream = new FileStream( filePath, FileMode.Open, FileAccess.Read );

            return await LoadAsync( fileStream );
        }

        /**
         * Loads asynchronously a firmware from the file at the given stream.
         * 
         * @param [in] stream Stream to provide the firmware file contents
         */
        public static async Task<Firmware> LoadAsync( Stream stream )
        {
            var fwFile = new Firmware( true );

            var fileReader = new StreamReader( stream );

            int lineNumber = 0;

            while( !fileReader.EndOfStream )
            {
                string line = ( await fileReader.ReadLineAsync() )?.TrimEnd() ?? "";

                lineNumber++;

                if( line.Length > 0 )
                {
                    try
                    {
                        var record = ProcessLine( line );

                        switch( record.Type )
                        {
                            case RecordType.S1:
                            case RecordType.S2:
                            case RecordType.S3:
                                ProcessDataRecord( record, fwFile );
                                break;

                            default:
                                // Ignore other supported record types
                                break;
                        }
                    }
                    catch( Exception e )
                    {
                        throw new FormatException( e.Message, lineNumber );
                    }
                }
            }

            return fwFile;
        }

        /*===========================================================================
         *                         PRIVATE NESTED CLASSES
         *===========================================================================*/

        private enum RecordType
        {
            S0,
            S1,
            S2,
            S3,
            S4,
            S5,
            S6,
            S7,
            S8,
            S9
        }

        private class Record
        {
            public RecordType Type;
            public UInt32 Address;
            public byte[] Data;

            public Record( RecordType type, UInt32 address, byte[] data )
            {
                Type = type;
                Address = address;
                Data = data;
            }
        }

        /*===========================================================================
         *                            PRIVATE METHODS
         *===========================================================================*/

        private static Record ProcessLine( string line )
        {
            if( line.Length < ( ADDRESS_INDEX + 4 + CHECKSUM_SIZE ) )
            {
                throw new Exception( "Truncated record" );
            }

            string recordTypeCode = line.Substring( RECORD_TYPE_INDEX, RECORD_TYPE_SIZE );
            RecordType recordType = ConvertToRecordType( recordTypeCode );

            int byteCount;
            UInt32 address;
            int addressSize = GetAddressSize( recordType );

            try
            {
                byteCount = Convert.ToInt32( line.Substring( BYTE_COUNT_INDEX, BYTE_COUNT_SIZE ), 16 );
                address = Convert.ToUInt32( line.Substring( ADDRESS_INDEX, addressSize ), 16 );
            }
            catch( Exception e )
            {
                throw new Exception( "Invalid hexadecimal value", e );
            }

            if( line.Length != ( ADDRESS_INDEX + ( byteCount * 2 ) ) )
            {
                throw new Exception( "Invalid record length" );
            }


            byte calculatedChecksum = (byte) byteCount;
            calculatedChecksum += (byte) ( ( address >> 0 ) & 0xFF );
            calculatedChecksum += (byte) ( ( address >> 8 ) & 0xFF );
            calculatedChecksum += (byte) ( ( address >> 16 ) & 0xFF );
            calculatedChecksum += (byte) ( ( address >> 24 ) & 0xFF );

            int dataSize = byteCount - ( ( CHECKSUM_SIZE + addressSize ) / 2 );
            int dataIndex = ADDRESS_INDEX + addressSize;

            byte[] data = new byte[dataSize];

            for( int i = 0; i < dataSize; i++ )
            {
                try
                {
                    data[i] = Convert.ToByte( line.Substring( dataIndex + ( i * 2 ), 2 ), 16 );
                    calculatedChecksum += data[i];
                }
                catch( Exception e )
                {
                    throw new Exception( "Invalid hexadecimal value", e );
                }
            }

            calculatedChecksum = (byte) ~calculatedChecksum;

            byte checksum;

            try
            {
                checksum = Convert.ToByte( line.Substring( dataIndex + ( dataSize * 2 ), CHECKSUM_SIZE ), 16 );
            }
            catch( Exception e )
            {
                throw new Exception( "Invalid hexadecimal value", e );
            }

            if( checksum != calculatedChecksum )
            {
                throw new Exception( $"Invalid checksum (expected: {calculatedChecksum:X2}h, reported: {checksum:X2}h)" );
            }

            return new Record( recordType, address, data );
        }

        private static RecordType ConvertToRecordType( string recordTypeCode )
        {
            try
            {
                return (RecordType) Enum.Parse( typeof( RecordType ), recordTypeCode );
            }
            catch( Exception )
            { 
                throw new Exception( $"Unsupported record type '{recordTypeCode}'" );
            }
        }

        private static int GetAddressSize( RecordType recordType )
        {
            switch( recordType )
            {
                case RecordType.S2:
                    return 6;

                case RecordType.S3:
                    return 8;

                default:
                    return 4;
            }
        }

        private static void ProcessDataRecord( Record record, Firmware fwFile )
        {
            fwFile.SetData( record.Address, record.Data );
        }

        /*===========================================================================
         *                           PRIVATE CONSTANTS
         *===========================================================================*/

        private const int RECORD_TYPE_INDEX = 0;
        private const int BYTE_COUNT_INDEX = 2;
        private const int ADDRESS_INDEX = 4;

        private const int RECORD_TYPE_SIZE = 2;
        private const int BYTE_COUNT_SIZE = 2;
        private const int CHECKSUM_SIZE = 2;
    }
}
