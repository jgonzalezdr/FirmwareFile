using FirmwareFile;
using System;

namespace FirmwareFiles.Example
{
    class ExampleProgram
    {
        public static void Main( string[] args )
        {
            if( args.Length > 0 )
            {
                PrintFirmwareFileInfo( args[0] );
            }
            else
            {
                var exeName = System.IO.Path.GetFileName( System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName );
                Console.WriteLine( $"USAGE: {exeName} <firmware_file_path>" );
            }
        }

        private static void PrintFirmwareFileInfo( string filepath )
        {
            try
            {
                var firmware = IntelFileLoader.Load( filepath );

                foreach( var fwBlock in firmware.Blocks )
                {
                    Console.WriteLine( $"Memory block: StartAddress=0x{fwBlock.StartAddress:X8} EndAddress=0x{fwBlock.StartAddress + fwBlock.Size:X8} Size=0x{fwBlock.Size:X} ({fwBlock.Size})" );
                }
            }
            catch( Exception e )
            {
                Console.WriteLine( $"ERROR: {e.Message}" );
            }
        }
    }
}
