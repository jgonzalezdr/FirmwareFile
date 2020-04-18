/**
 * @file
 * @copyright  Copyright (c) 2019 Jesús González del Río
 * @license    See LICENSE.txt
 */

using System.IO;
using Xunit;

namespace FirmwareFile.Test
{
    public class MotorolaFileLoaderTest
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
        public void Load_SingleBlock()
        {
            // Prepare

            string fileContents =
                "S00F000068656C6C6F202020202000003C\n" +
                "S11F00007C0802A6900100049421FFF07C6C1B787C8C23783C6000003863000026\n" +
                "S11F001C4BFFFFE5398000007D83637880010014382100107C0803A64E800020E9\n" +
                "S111003848656C6C6F20776F726C642E0A0042\n" +
                "S5030003F9\n" +
                "S9030000FC";

            var stream = PrepareStream( fileContents );

            // Execute

            var fwFile = MotorolaFileLoader.Load( stream );

            // Check

            uint expectedAddress = 0x0u;
            var expectedData = new byte[]
            {
              0x7C, 0x08, 0x02, 0xA6, 0x90, 0x01, 0x00, 0x04, 0x94, 0x21, 0xFF, 0xF0, 0x7C, 0x6C, 
              0x1B, 0x78, 0x7C, 0x8C, 0x23, 0x78, 0x3C, 0x60, 0x00, 0x00, 0x38, 0x63, 0x00, 0x00,
              0x4B, 0xFF, 0xFF, 0xE5, 0x39, 0x80, 0x00, 0x00, 0x7D, 0x83, 0x63, 0x78, 0x80, 0x01,
              0x00, 0x14, 0x38, 0x21, 0x00, 0x10, 0x7C, 0x08, 0x03, 0xA6, 0x4E, 0x80, 0x00, 0x20,
              0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x77, 0x6F, 0x72, 0x6C, 0x64, 0x2E, 0x0A, 0x00
            };

            Assert.True( fwFile.HasExplicitAddresses );
            Assert.Single( fwFile.Blocks );
            Assert.Equal( expectedAddress, fwFile.Blocks[0].StartAddress );
            Assert.Equal( expectedData, fwFile.Blocks[0].Data );
        }

        [Fact]
        public void Load_MultipleBlocks()
        {
            // Prepare

            string fileContents =
                "S11F10007C0802A6900100049421FFF07C6C1B787C8C23783C6000003863000016\n" +
                "S2200700004BFFFFE5398000007D83637880010014382100107C0803A64E800020FD\n" +
                "S3138000001048656C6C6F20776F726C642E0A00E8\n";

            var stream = PrepareStream( fileContents );

            // Execute

            var fwFile = MotorolaFileLoader.Load( stream );

            // Check

            uint expectedAddress1 = 0x1000u;
            var expectedData1 = new byte[]
            {
              0x7C, 0x08, 0x02, 0xA6, 0x90, 0x01, 0x00, 0x04, 0x94, 0x21, 0xFF, 0xF0, 0x7C, 0x6C,
              0x1B, 0x78, 0x7C, 0x8C, 0x23, 0x78, 0x3C, 0x60, 0x00, 0x00, 0x38, 0x63, 0x00, 0x00
            };

            uint expectedAddress2 = 0x070000u;
            var expectedData2 = new byte[]
            {
              0x4B, 0xFF, 0xFF, 0xE5, 0x39, 0x80, 0x00, 0x00, 0x7D, 0x83, 0x63, 0x78, 0x80, 0x01,
              0x00, 0x14, 0x38, 0x21, 0x00, 0x10, 0x7C, 0x08, 0x03, 0xA6, 0x4E, 0x80, 0x00, 0x20
            };

            uint expectedAddress3 = 0x80000010u;
            var expectedData3 = new byte[]
            {
              0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x77, 0x6F, 0x72, 0x6C, 0x64, 0x2E, 0x0A, 0x00
            };

            Assert.True( fwFile.HasExplicitAddresses );
            Assert.Equal( 3, fwFile.Blocks.Length );
            Assert.Equal( expectedAddress1, fwFile.Blocks[0].StartAddress );
            Assert.Equal( expectedData1, fwFile.Blocks[0].Data );
            Assert.Equal( expectedAddress2, fwFile.Blocks[1].StartAddress );
            Assert.Equal( expectedData2, fwFile.Blocks[1].Data );
            Assert.Equal( expectedAddress3, fwFile.Blocks[2].StartAddress );
            Assert.Equal( expectedData3, fwFile.Blocks[2].Data );
        }

        [Fact]
        public void Load_InvalidHexCode_Length()
        {
            // Prepare

            string fileContents =
                "S11l003848656C6C6F20776F726C642E0A0042";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => MotorolaFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Invalid hexadecimal value", ex.Message );
        }

        [Fact]
        public void Load_InvalidHexCode_Address()
        {
            // Prepare

            string fileContents =
                "S1110N3848656C6C6F20776F726C642E0A0042";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => MotorolaFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Invalid hexadecimal value", ex.Message );
        }

        [Fact]
        public void Load_InvalidHexCode_Data()
        {
            // Prepare

            string fileContents =
                "S111003848656C6C6F20776F72iC642E0A0042";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => MotorolaFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Invalid hexadecimal value", ex.Message );
        }

        [Fact]
        public void Load_InvalidHexCode_Checksum()
        {
            // Prepare

            string fileContents =
                "S111003848656C6C6F20776F726C642E0A004x";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => MotorolaFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Invalid hexadecimal value", ex.Message );
        }

        [Fact]
        public void Load_UnsupportedRecordType()
        {
            // Prepare

            string fileContents =
                "R111003848656C6C6F20776F726C642E0A0042";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => MotorolaFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Unsupported record type 'R1'", ex.Message );
        }

        [Fact]
        public void Load_InvalidChecksum()
        {
            // Prepare

            string fileContents =
                "S111003848656C6C6F20776F726C642E0A0043";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => MotorolaFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Invalid checksum (expected: 42h, reported: 43h)", ex.Message );
        }

        [Fact]
        public void Load_TruncatedRecord()
        {
            // Prepare

            string fileContents =
                "S111003";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => MotorolaFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Truncated record", ex.Message );
        }

        [Fact]
        public void Load_InvalidRecordLength()
        {
            // Prepare

            string fileContents =
                "S110003848656C6C6F20776F726C642E0A0042";

            var stream = PrepareStream( fileContents );

            // Execute

            var ex = Assert.Throws<FormatException>( () => MotorolaFileLoader.Load( stream ) );

            // Check

            Assert.Contains( "Invalid record length", ex.Message );
        }
    }
}
