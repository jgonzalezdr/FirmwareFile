/**
 * @file
 * @copyright  Copyright (c) 2020 Jesús González del Río
 * @license    See LICENSE.txt
 */

using System;

namespace FirmwareFile
{
    /**
     * Represents a format error while reading a text file.
     */
    public class FormatException : Exception
    {
        /*===========================================================================
         *                           PUBLIC PROPERTIES
         *===========================================================================*/

        public int Line { get; private set; }

        public override string Message
        {
            get => $"[Line {Line}]\n{base.Message}";
        }

        /*===========================================================================
         *                          PUBLIC CONSTRUCTORS
         *===========================================================================*/

        public FormatException( string message, int line ) : base( message )
        {
            Line = line;
        }

        public FormatException( string message, int line, Exception innerException ) : base( message, innerException )
        {
            Line = line;
        }
    }
}
