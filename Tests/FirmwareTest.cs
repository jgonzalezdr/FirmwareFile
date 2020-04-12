/**
 * @file
 * @copyright  Copyright (c) 2020 Jesús González del Río
 * @license    See LICENSE.txt
 */

using System;
using Xunit;

namespace FirmwareFile.Test
{
    public class FirmwareTest
    {
        [Fact]
        public void SetData_Empty()
        {
            // Prepare

            var firmware = new Firmware( false );
            var data = new byte[] {};
            UInt32 address = 0x1000;

            // Execute

            firmware.SetData( address, data );

            // Check

            Assert.False( firmware.HasExplicitAddresses );
            Assert.Empty( firmware.Blocks );
        }

        [Fact]
        public void SetData_Single()
        {
            // Prepare

            var firmware = new Firmware( false );
            var data = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address = 0x1000;

            // Execute

            firmware.SetData( address, data );

            // Check

            Assert.False( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address, firmware.Blocks[0].StartAddress );
            Assert.Equal( data, firmware.Blocks[0].Data );
        }

        [Fact]
        public void SetData_Double_NonOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void SetData_Double_TailOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99 };
            UInt32 address2 = 0x1002;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 1, 2, 45, 3, 145, 32, 0, 99 }, firmware.Blocks[0].Data );
        }

        [Fact]
        public void SetData_Double_TailJoin()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99 };
            UInt32 address2 = 0x1005;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 1, 2, 45, 3, 255, 45, 3, 145, 32, 0, 99 }, firmware.Blocks[0].Data );
        }

        [Fact]
        public void SetData_Double_HeadOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99 };
            UInt32 address2 = 0x0FFC;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address2, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 45, 3, 145, 32, 0, 99, 45, 3, 255 }, firmware.Blocks[0].Data );
        }

        [Fact]
        public void SetData_Double_HeadJoin()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99 };
            UInt32 address2 = 0x0FFA;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address2, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 45, 3, 145, 32, 0, 99, 1, 2, 45, 3, 255 }, firmware.Blocks[0].Data );
        }

        [Fact]
        public void SetData_Double_FullOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1002;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99, 88, 12 };
            UInt32 address2 = 0x1000;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address2, firmware.Blocks[0].StartAddress );
            Assert.Equal( data2, firmware.Blocks[0].Data );
        }

        [Fact]
        public void SetData_Double_NestedOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255, 0, 99, 88, 12 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32  };
            UInt32 address2 = 0x1003;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 1, 2, 45, 45, 3, 145, 32, 88, 12 }, firmware.Blocks[0].Data );
        }

        [Fact]
        public void SetData_Triple_Overlap()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1008;

            var data3 = new byte[] { 23, 34, 1, 44, 88, 12, 77 };
            UInt32 address3 = 0x1004;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );
            firmware.SetData( address3, data3 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 1, 2, 45, 3, 23, 34, 1, 44, 88, 12, 77, 32, 0, 99 }, firmware.Blocks[0].Data );
        }

        [Fact]
        public void EraseData_Empty()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x1002;
            uint removeSize = 0;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void EraseData_NoOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x1006;
            uint removeSize = 5;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void EraseData_TailOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x1003;
            uint removeSize = 5;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 1, 2, 45 }, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void EraseData_HeadOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x0FFE;
            uint removeSize = 5;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address3 + removeSize, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 3, 255 }, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void EraseData_DoubleOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x1002;
            uint removeSize = 0x11;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 1, 2 }, firmware.Blocks[0].Data );
            Assert.Equal( address3 + removeSize, firmware.Blocks[1].StartAddress );
            Assert.Equal( new byte[] { 32, 0, 99 }, firmware.Blocks[1].Data );
        }

        [Fact]
        public void EraseData_FullOverlap_Single()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x0FFD;
            uint removeSize = 0x10;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address2, firmware.Blocks[0].StartAddress );
            Assert.Equal( data2, firmware.Blocks[0].Data );
        }

        [Fact]
        public void EraseData_FullOverlap_Double()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x0FFD;
            uint removeSize = 0x20;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Empty( firmware.Blocks );
        }

        [Fact]
        public void EraseData_PartialAndFullOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x1004;
            uint removeSize = 0x20;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 1, 2, 45, 3 }, firmware.Blocks[0].Data );
        }

        [Fact]
        public void EraseData_MiddleOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x1012;
            uint removeSize = 2;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 3, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( new byte[] { 179, 7 }, firmware.Blocks[1].Data );
            Assert.Equal( address3 + removeSize, firmware.Blocks[2].StartAddress );
            Assert.Equal( new byte[] { 0, 99 }, firmware.Blocks[2].Data );
        }

        [Fact]
        public void GetData_MiddleOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x1001;
            uint getSize = 3;

            var data3 = firmware.GetData( address3, getSize );

            // Check

            Assert.Equal( new byte[] { 2, 45, 3 }, data3 );

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void GetData_FullOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            var data3 = firmware.GetData( address2, (uint) data2.Length );

            // Check

            Assert.Equal( data2, data3 );

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void GetData_PartialHeadOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x0FFE;
            uint getSize = 4;

            var data3 = firmware.GetData( address3, getSize );

            // Check

            Assert.Null( data3 );

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void GetData_PartialTailOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 1013;
            uint getSize = 4;

            var data3 = firmware.GetData( address3, getSize );

            // Check

            Assert.Null( data3 );

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void GetData_NoOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 1005;
            uint getSize = 11;

            var data3 = firmware.GetData( address3, getSize );

            // Check

            Assert.Null( data3 );

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }
    }
}
