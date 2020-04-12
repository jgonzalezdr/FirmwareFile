/**
 * @file
 * @copyright  Copyright (c) 2020 Jesús González del Río
 * @license    See LICENSE.txt
 */

using System;
using Xunit;

namespace FirmwareFile.Test
{
    public class FormatExceptionTest
    {
        [Fact]
        public void Constructor_Simple()
        {
            // Execute

            var ex = new FormatException( "Message A", 55 );

            // Check

            Assert.Equal( "[Line 55]\nMessage A", ex.Message );
        }

        [Fact]
        public void Constructor_Nested()
        {
            // Execute

            var ex = new FormatException( "Message C", 120213, new Exception( "Message B" ) );

            // Check

            Assert.Equal( "[Line 120213]\nMessage C", ex.Message );
        }
    }
}
