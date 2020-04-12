/**
 * @file
 * @copyright  Copyright (c) 2020 Jesús González del Río
 * @license    See LICENSE.txt
 */

using System.IO;
using Xunit;

namespace FirmwareFile.Test
{
    public class IntelFileLoaderTest
    {
        private Stream PrepareStream( string contents )
        {
            var stream = new MemoryStream();

            var writer = new StreamWriter( stream, leaveOpen: true );
            writer.Write( contents );
            writer.Flush();

            stream.Seek( 0, SeekOrigin.Begin );

            return stream;
        }

        [Fact]
        public void Load_SingleBlock_Ordered()
        {
            // Prepare

            string fileContents =
                ":10010000214601360121470136007EFE09D2190140\n" +
                ":100110002146017E17C20001FF5F16002148011928\n" +
                ":10012000194E79234623965778239EDA3F01B2CAA7\n" +
                ":100130003F0156702B5E712B722B732146013421C7\n" +
                ":00000001FF";

            var stream = PrepareStream( fileContents );

            // Execute

            var fwFile = IntelFileLoader.Load( stream );

            // Check

            uint expectedAddress = 0x0100u;
            var expectedData = new byte[]
            {
                0x21, 0x46, 0x01, 0x36, 0x01, 0x21, 0x47, 0x01, 0x36, 0x00, 0x7E, 0xFE, 0x09, 0xD2, 0x19, 0x01,
                0x21, 0x46, 0x01, 0x7E, 0x17, 0xC2, 0x00, 0x01, 0xFF, 0x5F, 0x16, 0x00, 0x21, 0x48, 0x01, 0x19,
                0x19, 0x4E, 0x79, 0x23, 0x46, 0x23, 0x96, 0x57, 0x78, 0x23, 0x9E, 0xDA, 0x3F, 0x01, 0xB2, 0xCA,
                0x3F, 0x01, 0x56, 0x70, 0x2B, 0x5E, 0x71, 0x2B, 0x72, 0x2B, 0x73, 0x21, 0x46, 0x01, 0x34, 0x21
            };

            Assert.True( fwFile.HasExplicitAddresses );
            Assert.Single( fwFile.Blocks );
            Assert.Equal( expectedAddress, fwFile.Blocks[0].StartAddress );
            Assert.Equal( expectedData, fwFile.Blocks[0].Data );
        }

        [Fact]
        public void Load_SingleBlock_NotOrdered()
        {
            // Prepare

            string fileContents =
                ":10001300AC12AD13AE10AF1112002F8E0E8F0F2244\n" +
                ":10000300E50B250DF509E50A350CF5081200132259\n" +
                ":03000000020023D8\n" +
                ":0C002300787FE4F6D8FD7581130200031D\n" +
                ":10002F00EFF88DF0A4FFEDC5F0CEA42EFEEC88F016\n" +
                ":04003F00A42EFE22CB\n" +
                ":00000001FF";

            var stream = PrepareStream( fileContents );

            // Execute

            var fwFile = IntelFileLoader.Load( stream );

            // Check

            uint expectedAddress = 0x0000u;
            var expectedData = new byte[]
            {
                0x02, 0x00, 0x23,
                0xE5, 0x0B, 0x25, 0x0D, 0xF5, 0x09, 0xE5, 0x0A, 0x35, 0x0C, 0xF5, 0x08, 0x12, 0x00, 0x13, 0x22,
                0xAC, 0x12, 0xAD, 0x13, 0xAE, 0x10, 0xAF, 0x11, 0x12, 0x00, 0x2F, 0x8E, 0x0E, 0x8F, 0x0F, 0x22,
                0x78, 0x7F, 0xE4, 0xF6, 0xD8, 0xFD, 0x75, 0x81, 0x13, 0x02, 0x00, 0x03,
                0xEF, 0xF8, 0x8D, 0xF0, 0xA4, 0xFF, 0xED, 0xC5, 0xF0, 0xCE, 0xA4, 0x2E, 0xFE, 0xEC, 0x88, 0xF0,
                0xA4, 0x2E, 0xFE, 0x22
            };

            Assert.True( fwFile.HasExplicitAddresses );
            Assert.Single( fwFile.Blocks );
            Assert.Equal( expectedAddress, fwFile.Blocks[0].StartAddress );
            Assert.Equal( expectedData, fwFile.Blocks[0].Data );
        }

        [Fact]
        public void Load_TwoBlocks()
        {
            // Prepare

            string fileContents =
                ":10010000214601360121470136007EFE09D2190140\n" +
                ":100110002146017E17C20001FF5F16002148011928\n" +
                ":10032000194E79234623965778239EDA3F01B2CAA5\n" +
                ":100330003F0156702B5E712B722B732146013421C5\n" +
                ":00000001FF";

            var stream = PrepareStream( fileContents );

            // Execute

            var fwFile = IntelFileLoader.Load( stream );

            // Check

            uint expectedAddress1 = 0x0100u;
            var expectedData1 = new byte[]
            {
                0x21, 0x46, 0x01, 0x36, 0x01, 0x21, 0x47, 0x01, 0x36, 0x00, 0x7E, 0xFE, 0x09, 0xD2, 0x19, 0x01,
                0x21, 0x46, 0x01, 0x7E, 0x17, 0xC2, 0x00, 0x01, 0xFF, 0x5F, 0x16, 0x00, 0x21, 0x48, 0x01, 0x19
            };

            uint expectedAddress2 = 0x0320u;
            var expectedData2 = new byte[]
            {
                0x19, 0x4E, 0x79, 0x23, 0x46, 0x23, 0x96, 0x57, 0x78, 0x23, 0x9E, 0xDA, 0x3F, 0x01, 0xB2, 0xCA,
                0x3F, 0x01, 0x56, 0x70, 0x2B, 0x5E, 0x71, 0x2B, 0x72, 0x2B, 0x73, 0x21, 0x46, 0x01, 0x34, 0x21
            };

            Assert.True( fwFile.HasExplicitAddresses );
            Assert.Equal( 2, fwFile.Blocks.Length );
            Assert.Equal( expectedAddress1, fwFile.Blocks[0].StartAddress );
            Assert.Equal( expectedData1, fwFile.Blocks[0].Data );
            Assert.Equal( expectedAddress2, fwFile.Blocks[1].StartAddress );
            Assert.Equal( expectedData2, fwFile.Blocks[1].Data );
        }

        [Fact]
        public void Load_ExtendedLinearAddress_OneBlock()
        {
            // Prepare

            string fileContents =
                ":020000040800F2\n" +
                ":10010000214601360121470136007EFE09D2190140\n" +
                ":100110002146017E17C20001FF5F16002148011928\n" +
                ":10032000194E79234623965778239EDA3F01B2CAA5\n" +
                ":100330003F0156702B5E712B722B732146013421C5\n" +
                ":0400000500100008DF\n" +
                ":00000001FF";

            var stream = PrepareStream( fileContents );

            // Execute

            var fwFile = IntelFileLoader.Load( stream );

            // Check

            uint expectedAddress1 = 0x08000100u;
            var expectedData1 = new byte[]
            {
                0x21, 0x46, 0x01, 0x36, 0x01, 0x21, 0x47, 0x01, 0x36, 0x00, 0x7E, 0xFE, 0x09, 0xD2, 0x19, 0x01,
                0x21, 0x46, 0x01, 0x7E, 0x17, 0xC2, 0x00, 0x01, 0xFF, 0x5F, 0x16, 0x00, 0x21, 0x48, 0x01, 0x19
            };

            uint expectedAddress2 = 0x08000320u;
            var expectedData2 = new byte[]
            {
                0x19, 0x4E, 0x79, 0x23, 0x46, 0x23, 0x96, 0x57, 0x78, 0x23, 0x9E, 0xDA, 0x3F, 0x01, 0xB2, 0xCA,
                0x3F, 0x01, 0x56, 0x70, 0x2B, 0x5E, 0x71, 0x2B, 0x72, 0x2B, 0x73, 0x21, 0x46, 0x01, 0x34, 0x21
            };

            Assert.True( fwFile.HasExplicitAddresses );
            Assert.Equal( 2, fwFile.Blocks.Length );
            Assert.Equal( expectedAddress1, fwFile.Blocks[0].StartAddress );
            Assert.Equal( expectedData1, fwFile.Blocks[0].Data );
            Assert.Equal( expectedAddress2, fwFile.Blocks[1].StartAddress );
            Assert.Equal( expectedData2, fwFile.Blocks[1].Data );
        }

        [Fact]
        public void Load_ExtendedLinearAddress_TwoBlocks()
        {
            // Prepare

            string fileContents =
                ":020000040800F2\n" +
                ":10010000214601360121470136007EFE09D2190140\n" +
                ":100110002146017E17C20001FF5F16002148011928\n" +
                ":020000040102F7\n" +
                ":10032000194E79234623965778239EDA3F01B2CAA5\n" +
                ":100330003F0156702B5E712B722B732146013421C5\n" +
                ":00000001FF";

            var stream = PrepareStream( fileContents );

            // Execute

            var fwFile = IntelFileLoader.Load( stream );

            // Check

            uint expectedAddress1 = 0x08000100u;
            var expectedData1 = new byte[]
            {
                0x21, 0x46, 0x01, 0x36, 0x01, 0x21, 0x47, 0x01, 0x36, 0x00, 0x7E, 0xFE, 0x09, 0xD2, 0x19, 0x01,
                0x21, 0x46, 0x01, 0x7E, 0x17, 0xC2, 0x00, 0x01, 0xFF, 0x5F, 0x16, 0x00, 0x21, 0x48, 0x01, 0x19
            };

            uint expectedAddress2 = 0x01020320u;
            var expectedData2 = new byte[]
            {
                0x19, 0x4E, 0x79, 0x23, 0x46, 0x23, 0x96, 0x57, 0x78, 0x23, 0x9E, 0xDA, 0x3F, 0x01, 0xB2, 0xCA,
                0x3F, 0x01, 0x56, 0x70, 0x2B, 0x5E, 0x71, 0x2B, 0x72, 0x2B, 0x73, 0x21, 0x46, 0x01, 0x34, 0x21
            };

            Assert.True( fwFile.HasExplicitAddresses );
            Assert.Equal( 2, fwFile.Blocks.Length );
            Assert.Equal( expectedAddress1, fwFile.Blocks[0].StartAddress );
            Assert.Equal( expectedData1, fwFile.Blocks[0].Data );
            Assert.Equal( expectedAddress2, fwFile.Blocks[1].StartAddress );
            Assert.Equal( expectedData2, fwFile.Blocks[1].Data );
        }

        [Fact]
        public void Load_InvalidStartCode()
        {
            // Prepare

            string fileContents =
                ";020000040008F2\n";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => IntelFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Invalid start code ';' (3Bh)", ex.Message );
        }

        [Fact]
        public void Load_InvalidHexCode_Length()
        {
            // Prepare

            string fileContents =
                ":0x0000040008F2\n";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => IntelFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Invalid hexadecimal value", ex.Message );
        }

        [Fact]
        public void Load_InvalidHexCode_Address()
        {
            // Prepare

            string fileContents =
                ":020k00040008F2\n";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => IntelFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Invalid hexadecimal value", ex.Message );
        }

        [Fact]
        public void Load_InvalidHexCode_RecordType()
        {
            // Prepare

            string fileContents =
                ":000000l40008F2\n";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => IntelFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Invalid hexadecimal value", ex.Message );
        }

        [Fact]
        public void Load_InvalidHexCode_Data()
        {
            // Prepare

            string fileContents =
                ":0200000400j8F2\n";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => IntelFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Invalid hexadecimal value", ex.Message );
        }

        [Fact]
        public void Load_InvalidHexCode_Checksum()
        {
            // Prepare

            string fileContents =
                ":020000040008Fz\n";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => IntelFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Invalid hexadecimal value", ex.Message );
        }

        [Fact]
        public void Load_UnsupportedRecordType()
        {
            // Prepare

            string fileContents =
                ":020000020008F4\n";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => IntelFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Unsupported record type '02h'", ex.Message );
        }

        [Fact]
        public void Load_InvalidChecksum()
        {
            // Prepare

            string fileContents =
                ":020000040008F3\n";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => IntelFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Invalid checksum (expected: F2h, reported: F3h)", ex.Message );
        }

        [Fact]
        public void Load_TruncatedRecord()
        {
            // Prepare

            string fileContents =
                ":03000004\n";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => IntelFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Truncated record", ex.Message );
        }

        [Fact]
        public void Load_InvalidRecordLength()
        {
            // Prepare

            string fileContents =
                ":030000040008F2\n";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => IntelFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Invalid record length", ex.Message );
        }

        [Fact]
        public void Load_InvalidLinearAddressLength()
        {
            // Prepare

            string fileContents =
                ":0400000401020008ED\n" +
                ":10010000214601360121470136007EFE09D2190140\n" +
                ":100110002146017E17C20001FF5F16002148011928\n" +
                ":00000001FF\n" +
                ":020000040201F7\n" +
                ":10032000194E79234623965778239EDA3F01B2CAA5\n" +
                ":100330003F0156702B5E712B722B732146013421C5\n";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => IntelFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Invalid data length for 'Extended Linear Address' record", ex.Message );
        }

        [Fact]
        public void Load_RecordAfterEOF()
        {
            // Prepare

            string fileContents =
                ":020000040008F2\n" +
                ":10010000214601360121470136007EFE09D2190140\n" +
                ":100110002146017E17C20001FF5F16002148011928\n" +
                ":00000001FF\n" +
                ":020000040201F7\n" +
                ":10032000194E79234623965778239EDA3F01B2CAA5\n" +
                ":100330003F0156702B5E712B722B732146013421C5\n";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => IntelFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Record found after EOF record", ex.Message );
        }
    }
}
