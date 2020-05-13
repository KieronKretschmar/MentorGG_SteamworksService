using System;
using System.Collections.Generic;
using System.Text;

namespace SteamworksService
{
    /// <summary>
    /// Gets thrown when a response of SteamworksConnection indicates that no steam user was logged in.
    /// </summary>
    class SteamNotLoggedInException : FormatException
    {
        public SteamNotLoggedInException() : base() { }
        public SteamNotLoggedInException(string message) : base(message) { }
        public SteamNotLoggedInException(string message, Exception innerException) : base(message, innerException) { }
    }
}
