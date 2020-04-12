/**
 * @file
 * @copyright  Copyright (c) 2020 Jesús González del Río
 * @license    See LICENSE.txt
 */

using System.IO;
using Xunit;

namespace FirmwareFile.Test
{
    public class BinaryFileLoaderTest
    {
        private Stream PrepareStream( byte[] contents )
        {
            var stream = new MemoryStream();

            stream.Write( contents );

            stream.Seek( 0, SeekOrigin.Begin );

            return stream;
        }

        [Fact]
        public void Load()
        {
            // Prepare

            var contents = new byte[]
            {
                0x21, 0x46, 0x01, 0x36, 0x01, 0x21, 0x47, 0x01, 0x36, 0x00, 0x7E, 0xFE, 0x09, 0xD2, 0x19, 0x01,
                0x21, 0x46, 0x01, 0x7E, 0x17, 0xC2, 0x00, 0x01, 0xFF, 0x5F, 0x16, 0x00, 0x21, 0x48, 0x01, 0x19,
                0x19, 0x4E, 0x79, 0x23, 0x46, 0x23, 0x96, 0x57, 0x78, 0x23, 0x9E, 0xDA, 0x3F, 0x01, 0xB2, 0xCA,
                0x3F, 0x01, 0x56, 0x70, 0x2B, 0x5E, 0x71, 0x2B, 0x72, 0x2B, 0x73, 0x21, 0x46, 0x01, 0x34, 0x21
            };

            var stream = PrepareStream( contents );

            // Execute

            var fwFile = BinaryFileLoader.Load( stream );

            // Check

            Assert.False( fwFile.HasExplicitAddresses );
            Assert.Single( fwFile.Blocks );
            Assert.Equal( 0u, fwFile.Blocks[0].StartAddress );
            Assert.Equal( contents, fwFile.Blocks[0].Data );
        }
    }
}
