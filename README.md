FirmwareFile
============

## About

FirmwareFile is a .NET library written in C# to load and access firmware files easily.

Supported firmware file formats:
- Binary (.bin)
- Intel HEX (.hex)
- Motorola S-Record (.srec, .s19, .s28, .s37)

## Usage

Use the `Load`/`LoadAsync` methods of the firmware file loader classes to load and create a `Firmware` instance.

Access the firmware memory blocks (`FirmwareBlock`) though the `Blocks` property of the `Firmware` instance.

#### Firmware File Loader Classes

- `BinaryFileLoader`
- `IntelFileLoader`
- `MotorolaFileLoader`

#### Example

``` CS
using FirmwareFile;
using System;

public class Example
{
  void PrintFirmwareFileInfo( string filepath )
  {
    try
    {
      var firmware = IntelFileLoader.Load( filepath );

      foreach( var fwBlock in firmware.Blocks )
      {
        Console.WriteLine( $"Memory block: Address=0x{fwBlock.StartAddress:X8} Size=0x{fwBlock.Size:X}" );
      }
    }
    catch( Exception e )
    {
      Console.WriteLine( $"ERROR: {e.Message}" );
    }
  }
}
```