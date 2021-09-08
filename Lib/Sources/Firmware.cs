/**
 * @file
 * @copyright  Copyright (c) 2020 Jesús González del Río
 * @license    See LICENSE.txt
 */

using System;
using System.Collections.Generic;

#nullable enable

namespace FirmwareFile
{
    /**
     * Represent the firmware for a device a set of one or more blocks of memory.
     */
    public class Firmware
    {
        /*===========================================================================
         *                           PUBLIC PROPERTIES
         *===========================================================================*/

        /**
         * Indicates if the blocks have explicit addresses (e.g., when loaded from an HEX file),
         * or if it has implicit / unknown addresses (e.g., when loaded from a binary file).
         */
        public bool HasExplicitAddresses { get; private set; }

        /**
         * Array of FirmwareBlocks that conform the firmware.
         */
        public FirmwareBlock[] Blocks { get => m_blocks.ToArray(); }

        /*===========================================================================
         *                            PUBLIC METHODS
         *===========================================================================*/

        /**
         * Writes a data block at the given address, overwriting any previously set data if necessary.
         * 
         * FirmwareBlocks are automatically created, modified or merged as necessary as a result of inserting the data block.
         * 
         * @param [in] startAddress Starting address for the data block
         * @param [in] data Data for the data block
         */
        public void SetData( UInt32 startAddress, byte[] data )
        {
            if( data.Length == 0 )
            {
                return;
            }

            UInt32 endAddress = startAddress + (uint) data.Length;
            FirmwareBlock? startBlock = null;
            FirmwareBlock? endBlock = null;

            RemoveOverwrittenBlocks( startAddress, endAddress );

            foreach( var block in m_blocks )
            {
                if( ( block.StartAddress <= startAddress ) && ( ( block.StartAddress + block.Size ) >= startAddress ) )
                {
                    if( startBlock != null )
                    {
                        throw new Exception( "INTERNAL ERROR: Blocks are overlapping" );
                    }
                    startBlock = block;
                }

                if( ( block.StartAddress <= endAddress ) && ( ( block.StartAddress + block.Size ) >= endAddress ) )
                {
                    if( endBlock != null )
                    {
                        throw new Exception( "INTERNAL ERROR: Blocks are overlapping" );
                    }
                    endBlock = block;
                }
            }

            if( endBlock == startBlock )
            {
                endBlock = null;
            }

            if( ( startBlock != null ) && ( endBlock != null ) )
            {
                // Data overlaps partially 2 blocks => Expand start block on the tail and merge end block

                startBlock.SetDataAtOffset( (int) ( startAddress - startBlock.StartAddress ), data );

                int endBlockOffset = (int) ( endAddress - endBlock.StartAddress );
                int tailSize = (int) ( endBlock.Size - endBlockOffset );
                var tailData = new byte[tailSize];
                Array.Copy( endBlock.Data, endBlockOffset, tailData, 0, tailSize );
                startBlock.AppendData( tailData );

                m_blocks.Remove( endBlock );
            }
            else if( ( startBlock != null ) && ( endBlock == null ) )
            {
                // Data overlaps partially a block on its middle or tail => Expand start block on the middle or tail

                startBlock.SetDataAtOffset( (int) ( startAddress - startBlock.StartAddress ), data );
            }
            else if( ( startBlock == null ) && ( endBlock != null ) )
            {
                // Data overlaps partially a block on its head => Expand end block on the head

                endBlock.SetDataAtOffset( - (int) ( endBlock.StartAddress - startAddress ), data );
            }
            else
            {
                // Data does not overlap any other block => Create new block

                m_blocks.Add( new FirmwareBlock( startAddress, data ) );
            }
        }

        /**
         * Erases a data block from the given address.
         * 
         * FirmwareBlocks are automatically deleted, modified or split as necessary as a result of erasing the data block.
         * 
         * @param [in] startAddress Starting address for the data block
         * @param [in] size Size of the data block
         */
        public void EraseData( UInt32 startAddress, uint size )
        {
            if( size == 0 )
            {
                return;
            }

            UInt32 endAddress = startAddress + size;

            foreach( var block in m_blocks.ToArray() )
            {
                uint blockStartAddress = block.StartAddress;
                uint blockEndAddress = block.StartAddress + block.Size;

                if( ( blockStartAddress < startAddress ) && ( blockEndAddress > endAddress ) )
                {
                    // Region overlaps the middle of a block => Split block

                    int endBlockOffset = (int) ( endAddress - blockStartAddress );
                    int tailSize = (int) ( block.Size - endBlockOffset );
                    var tailData = new byte[tailSize];
                    Array.Copy( block.Data, endBlockOffset, tailData, 0, tailSize );

                    m_blocks.Add( new FirmwareBlock( endAddress, tailData ) );

                    uint startBlockOffset = ( startAddress - blockStartAddress );

                    block.EraseDataRangeAfterOffset( startBlockOffset );
                }
                else if( ( blockStartAddress >= startAddress ) && ( blockEndAddress <= endAddress ) )
                {
                    // Region fully overlaps a block => Remove block

                    m_blocks.Remove( block );
                }
                else if( ( blockStartAddress >= startAddress ) && ( blockStartAddress < endAddress ) )
                {
                    // Region overlaps the head of a block => Remove from head

                    uint blockOffset = ( endAddress - blockStartAddress );

                    block.EraseDataRangeBeforeOffset( blockOffset );
                }
                else if( ( blockEndAddress > startAddress ) && ( blockEndAddress <= endAddress ) )
                {
                    // Region overlaps the tail of a block => Remove from tail

                    uint blockOffset = ( startAddress - blockStartAddress );

                    block.EraseDataRangeAfterOffset( blockOffset );
                }
            }
        }

        /**
         * Gets a data block from the given address.
         * 
         * @param [in] startAddress Starting address for the data block
         * @param [in] size Size of the data block
         * 
         * @return Array filled with the contents of the firmware at the requested data block memory region,
         *         or @c null if the requested data block memory region is not completely defined (i.e., if
         *         it doesn't fully overlap a single FirmwareBlock)
         */
        public byte[]? GetData( UInt32 startAddress, uint size )
        {
            UInt32 endAddress = startAddress + size;

            foreach( var block in m_blocks )
            {
                uint blockStartAddress = block.StartAddress;
                uint blockEndAddress = block.StartAddress + block.Size;

                if( ( blockStartAddress <= startAddress ) && ( blockEndAddress >= endAddress ) )
                {
                    var returnData = new byte[size];
                    Array.Copy( block.Data, (int) ( startAddress - blockStartAddress ), returnData, 0, size );
                    return returnData;
                }
            }

            return null;
        }

        /*===========================================================================
         *                          INTERNAL CONSTRUCTORS
         *===========================================================================*/

        internal Firmware( bool hasExplicitAddresses )
        {
            HasExplicitAddresses = hasExplicitAddresses;
        }

        /*===========================================================================
         *                            PRIVATE METHODS
         *===========================================================================*/

        private void RemoveOverwrittenBlocks( UInt32 startAddress, UInt32 endAddress )
        {
            foreach( var block in m_blocks.ToArray() )
            {
                if( ( block.StartAddress >= startAddress ) && ( ( block.StartAddress + block.Size ) <= endAddress ) )
                {
                    m_blocks.Remove( block );
                }
            }
        }

        /*===========================================================================
         *                           PRIVATE ATTRIBUTES
         *===========================================================================*/

        private List<FirmwareBlock> m_blocks = new List<FirmwareBlock>();
    }
}
