using System;
using System.Collections.Generic;
using System.Text;

namespace SteamworksService
{
    /// <summary>
    /// Gets thrown when a SharingCode could not be decoded.
    /// </summary>
    class DecodeSharingCodeFailedException : FormatException
    {
        public DecodeSharingCodeFailedException() : base() { }
        public DecodeSharingCodeFailedException(string message) : base(message) { }
        public DecodeSharingCodeFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
