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
     * Loader for binary firmware files (i.e., a block of contiguous memory without
     * an explicit address).
     */
    public class BinaryFileLoader
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
            var fwFile = new Firmware( false );

            int dataSize = (int) stream.Length;

            byte[] data = new byte[dataSize];

            if( await stream.ReadAsync( data, 0, dataSize ) != dataSize )
            {
                throw new Exception( "Couldn't read binary file contents" );
            }

            fwFile.SetData( 0, data );

            return fwFile;
        }
    }
}
