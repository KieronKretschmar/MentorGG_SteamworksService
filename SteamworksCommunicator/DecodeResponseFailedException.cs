using System;
using System.Collections.Generic;
using System.Text;

namespace SteamworksService
{
    /// <summary>
    /// Gets thrown when a response of SteamworksConnection could not be decoded.
    /// </summary>
    class DecodeResponseFailedException : FormatException
    {
        public DecodeResponseFailedException() : base() { }
        public DecodeResponseFailedException(string message) : base(message) { }
        public DecodeResponseFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
