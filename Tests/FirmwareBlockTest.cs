/**
 * @file
 * @copyright  Copyright (c) 2020 Jesús González del Río
 * @license    See LICENSE.txt
 */

using System;
using Xunit;

namespace FirmwareFile.Test
{
    public class FirmwareBlockTest
    {
        [Fact]
        public void Constructor()
        {
            // Prepare

            var data = new byte[] { 1, 2, 45, 3, 255, 47, 90, 101 };
            UInt32 address = 0x8000;

            // Execute

            var fwBlock = new FirmwareBlock( address, data );

            // Check

            Assert.Equal( address, fwBlock.StartAddress );
            Assert.Equal( data, fwBlock.Data );
        }

        [Fact]
        public void SetData_NestedOverlap()
        {
            // Prepare

            var data1 = new byte[] { 1, 2, 45, 3, 255, 47, 90, 101 };
            UInt32 address1 = 0x8000;

            var fwBlock = new FirmwareBlock( address1, data1 );

            // Execute

            var data2 = new byte[] { 45, 3, 145, 32 };
            UInt32 address2 = 0x8002;

            fwBlock.SetDataAtAddress( address2, data2 );

            // Check

            Assert.Equal( address1, fwBlock.StartAddress );
            Assert.Equal( new byte[] { 1, 2, 45, 3, 145, 32, 90, 101 }, fwBlock.Data );
        }

        [Fact]
        public void SetData_HeadOverlap()
        {
            // Prepare

            var data1 = new byte[] { 1, 2, 45, 3, 255, 47, 90, 101 };
            UInt32 address1 = 0x8003;

            var fwBlock = new FirmwareBlock( address1, data1 );

            // Execute

            var data2 = new byte[] { 45, 3, 145, 32 };
            UInt32 address2 = 0x8000;

            fwBlock.SetDataAtAddress( address2, data2 );

            // Check

            Assert.Equal( address2, fwBlock.StartAddress );
            Assert.Equal( new byte[] { 45, 3, 145, 32, 2, 45, 3, 255, 47, 90, 101 }, fwBlock.Data );
        }

        [Fact]
        public void SetData_TailOverlap()
        {
            // Prepare

            var data1 = new byte[] { 1, 2, 45, 3, 255, 47, 90, 101 };
            UInt32 address1 = 0x8000;

            var fwBlock = new FirmwareBlock( address1, data1 );

            // Execute

            var data2 = new byte[] { 45, 3, 145, 32 };
            UInt32 address2 = 0x8005;

            fwBlock.SetDataAtAddress( address2, data2 );

            // Check

            Assert.Equal( address1, fwBlock.StartAddress );
            Assert.Equal( new byte[] { 1, 2, 45, 3, 255, 45, 3, 145, 32 }, fwBlock.Data );
        }

        [Fact]
        public void SetData_FullOverlap()
        {
            // Prepare

            var data2 = new byte[] { 45, 3, 145, 32 };
            UInt32 address2 = 0x8003;

            var fwBlock = new FirmwareBlock( address2, data2 );

            // Execute

            var data1 = new byte[] { 1, 2, 45, 3, 255, 47, 90, 101 };
            UInt32 address1 = 0x8000;

            fwBlock.SetDataAtAddress( address1, data1 );

            // Check

            Assert.Equal( address1, fwBlock.StartAddress );
            Assert.Equal( data1, fwBlock.Data );
        }

        [Fact]
        public void SetData_NoOverlap()
        {
            // Prepare

            var data1 = new byte[] { 1, 2, 45, 3, 255, 47, 90, 101 };
            UInt32 address1 = 0x8000;

            var fwBlock = new FirmwareBlock( address1, data1 );

            // Execute

            var data2 = new byte[] { 45, 3, 145, 32 };
            UInt32 address2 = 0x8010;

            var ex = Assert.Throws<ArgumentException>( () => fwBlock.SetDataAtAddress( address2, data2 ) );

            // Check

            Assert.Contains( "Inserted data region does not overlap the block data region", ex.Message );
        }

        [Fact]
        public void EraseDataRangeAfter_PartialOverlap()
        {
            // Prepare

            var data = new byte[] { 1, 2, 45, 3, 255, 47, 90, 101 };
            UInt32 address1 = 0x8000;

            var fwBlock = new FirmwareBlock( address1, data );

            // Execute

            UInt32 address2 = 0x8004;

            fwBlock.EraseDataRangeAfterAddress ( address2 );

            // Check

            Assert.Equal( address1, fwBlock.StartAddress );
            Assert.Equal( new byte[] { 1, 2, 45, 3 }, fwBlock.Data );
        }

        [Fact]
        public void EraseDataRangeBefore_PartialOverlap()
        {
            // Prepare

            var data = new byte[] { 1, 2, 45, 3, 255, 47, 90, 101 };
            UInt32 address1 = 0x8000;

            var fwBlock = new FirmwareBlock( address1, data );

            // Execute

            UInt32 address2 = 0x8004;

            fwBlock.EraseDataRangeBeforeAddress( address2 );

            // Check

            Assert.Equal( address2, fwBlock.StartAddress );
            Assert.Equal( new byte[] { 255, 47, 90, 101 }, fwBlock.Data );
        }

        [Fact]
        public void EraseDataRangeAfter_NoOverlap()
        {
            // Prepare

            var data = new byte[] { 1, 2, 45, 3, 255, 47, 90, 101 };
            UInt32 address1 = 0x8000;

            var fwBlock = new FirmwareBlock( address1, data );

            // Execute

            UInt32 address2 = 0x8010;

            fwBlock.EraseDataRangeAfterAddress( address2 );

            // Check

            Assert.Equal( address1, fwBlock.StartAddress );
            Assert.Equal( data, fwBlock.Data );
        }

        [Fact]
        public void EraseDataRangeBefore_NoOverlap()
        {
            // Prepare

            var data = new byte[] { 1, 2, 45, 3, 255, 47, 90, 101 };
            UInt32 address1 = 0x8000;

            var fwBlock = new FirmwareBlock( address1, data );

            // Execute

            UInt32 address2 = 0x7FFF;

            fwBlock.EraseDataRangeBeforeAddress( address2 );

            // Check

            Assert.Equal( address1, fwBlock.StartAddress );
            Assert.Equal( data, fwBlock.Data );
        }

        [Fact]
        public void EraseDataRangeAfter_FullOverlap()
        {
            // Prepare

            var data = new byte[] { 1, 2, 45, 3, 255, 47, 90, 101 };
            UInt32 address1 = 0x8000;

            var fwBlock = new FirmwareBlock( address1, data );

            // Execute

            UInt32 address2 = 0x7000;

            fwBlock.EraseDataRangeAfterAddress( address2 );

            // Check

            Assert.Equal( address1, fwBlock.StartAddress );
            Assert.Empty( fwBlock.Data );
        }

        [Fact]
        public void EraseDataRangeBefore_FullOverlap()
        {
            // Prepare

            var data = new byte[] { 1, 2, 45, 3, 255, 47, 90, 101 };
            UInt32 address1 = 0x8000;

            var fwBlock = new FirmwareBlock( address1, data );

            // Execute

            UInt32 address2 = 0x9000;

            fwBlock.EraseDataRangeBeforeAddress( address2 );

            // Check

            Assert.Equal( address1, fwBlock.StartAddress );
            Assert.Empty( fwBlock.Data );
        }

    }
}
