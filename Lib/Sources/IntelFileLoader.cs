/**
 * @file
 * @copyright  Copyright (c) 2020 Jesús González del Río
 * @license    See LICENSE.txt
 */

using System;
using System.IO;
using System.Threading.Tasks;

namespace FirmwareFile
{
    /**
     * Loader for firmware files in Intel (HEX) format.
     */
    public static class IntelFileLoader
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
        public static Task<Firmware> LoadAsync( string filePath )
        {
            var fileStream = new FileStream( filePath, FileMode.Open, FileAccess.Read );

            return LoadAsync( fileStream );
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
            UInt32 extendedLinearAddress = 0;
            bool eofExpected = false;

            while( !fileReader.EndOfStream )
            {
                string line = ( await fileReader.ReadLineAsync() )?.TrimEnd() ?? "";

                lineNumber++;

                if( eofExpected )
                {
                    throw new FormatException( "Record found after EOF record", lineNumber );
                }

                if( line.Length > 0 )
                {
                    try
                    {
                        var record = ProcessLine( line );

                        record.Address += extendedLinearAddress;

                        switch( record.Type )
                        {
                            case RecordType.DATA:
                                ProcessDataRecord( record, fwFile );
                                break;

                            case RecordType.EOF:
                                eofExpected = true;
                                break;

                            case RecordType.EXTENDED_LINEAR_ADDRESS:
                                extendedLinearAddress = GetExtendedLinearAddress( record );
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
            DATA,
            EOF,
            EXTENDED_LINEAR_ADDRESS,
            START_LINEAR_ADDRESS
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
            if( line.Length < ( DATA_INDEX + CHECKSUM_SIZE ) )
            {
                throw new Exception( "Truncated record" );
            }

            if( line[START_CODE_INDEX] != START_CODE )
            {
                throw new Exception( $"Invalid start code '{line[0]}' ({(int)line[0]:X2}h)" );
            }

            int byteCount;
            UInt32 address;
            int recordTypeCode;

            try
            {
                byteCount = Convert.ToInt32( line.Substring( BYTE_COUNT_INDEX, BYTE_COUNT_SIZE ), 16 );
                address = Convert.ToUInt32( line.Substring( ADDRESS_INDEX, ADDRESS_SIZE ), 16 );
                recordTypeCode = Convert.ToInt32( line.Substring( RECORD_TYPE_INDEX, RECORD_TYPE_SIZE ), 16 );
            }
            catch( Exception e )
            {
                throw new Exception( "Invalid hexadecimal value", e );
            }

            if( line.Length != ( DATA_INDEX + CHECKSUM_SIZE + ( byteCount * 2 ) ) )
            {
                throw new Exception( "Invalid record length" );
            }

            RecordType recordType = ConvertToRecordType( recordTypeCode );

            byte calculatedChecksum = (byte) byteCount;
            calculatedChecksum += (byte) ( ( address >> 0 ) & 0xFF );
            calculatedChecksum += (byte) ( ( address >> 8 ) & 0xFF );
            calculatedChecksum += (byte) recordTypeCode;

            byte[] data = new byte[ byteCount ];

            for( int i = 0; i < byteCount; i++ )
            {
                try
                {
                    data[i] = Convert.ToByte( line.Substring( DATA_INDEX + ( i * 2 ), 2 ), 16 );
                    calculatedChecksum += data[i];
                }
                catch( Exception e )
                {
                    throw new Exception( "Invalid hexadecimal value", e );
                }
            }

            calculatedChecksum = (byte) - (int) calculatedChecksum;

            byte checksum;

            try
            {
                checksum = Convert.ToByte( line.Substring( DATA_INDEX + ( byteCount * 2 ), CHECKSUM_SIZE ), 16 );
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

        private static RecordType ConvertToRecordType( int recordTypeCode )
        {
            switch( recordTypeCode )
            {
                case RECORD_TYPE_DATA_CODE:
                    return RecordType.DATA;

                case RECORD_TYPE_EOF_CODE:
                    return RecordType.EOF;

                case RECORD_TYPE_ELA_CODE:
                    return RecordType.EXTENDED_LINEAR_ADDRESS;

                case RECORD_TYPE_SLA_CODE:
                    return RecordType.START_LINEAR_ADDRESS;

                default:
                    throw new Exception( $"Unsupported record type '{recordTypeCode:X2}h'" );
            }
        }

        private static UInt32 GetExtendedLinearAddress( Record record )
        {
            if( record.Data.Length != 2 )
            {
                throw new Exception( "Invalid data length for 'Extended Linear Address' record" );
            }

            return ( ( (UInt32) record.Data[0] ) << 24 ) + ( ( (UInt32) record.Data[1] ) << 16 );
        }

        private static void ProcessDataRecord( Record record, Firmware fwFile )
        {
            fwFile.SetData( record.Address, record.Data );
        }

        /*===========================================================================
         *                           PRIVATE CONSTANTS
         *===========================================================================*/

        private const char START_CODE = ':';

        private const int RECORD_TYPE_DATA_CODE = 0x00;
        private const int RECORD_TYPE_EOF_CODE = 0x01;
        private const int RECORD_TYPE_ELA_CODE = 0x04;
        private const int RECORD_TYPE_SLA_CODE = 0x05;

        private const int START_CODE_INDEX = 0;
        private const int BYTE_COUNT_INDEX = 1;
        private const int ADDRESS_INDEX = 3;
        private const int RECORD_TYPE_INDEX = 7;
        private const int DATA_INDEX = 9;

        private const int BYTE_COUNT_SIZE = 2;
        private const int ADDRESS_SIZE = 4;
        private const int RECORD_TYPE_SIZE = 2;
        private const int CHECKSUM_SIZE = 2;
    }
}
