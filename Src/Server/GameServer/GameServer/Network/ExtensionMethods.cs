using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

namespace GameServer.Network
{
    /// <summary>
    /// Represents one of the new Socket xxxAsync methods in .NET 3.5.
    /// </summary>
    /// <param name="args">The SocketAsyncEventArgs for use with the method.</param>
    /// <returns>Returns true if the operation completed asynchronously, false otherwise.</returns>
    public delegate Boolean SocketAsyncMethod(SocketAsyncEventArgs args);

    /// <summary>
    /// Holds helper methods for working with the new Socket xxxAsync methods in .NET 3.5.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Extension method to simplyfiy the pattern required by the new Socket xxxAsync methods in .NET 3.5.
        /// See http://www.flawlesscode.com/post/2007/12/Extension-Methods-and-SocketAsyncEventArgs.aspx
        /// </summary>
        /// <param name="socket">The socket this method acts on.</param>
        /// <param name="method">The xxxAsync method to be invoked.</param>
        /// <param name="callback">The callback for the method. Note: The Completed event must already have been attached to the same.</param>
        /// <param name="args">The SocketAsyncEventArgs to be used with this call.</param>
        public static void InvokeAsyncMethod(this Socket socket, SocketAsyncMethod method, EventHandler<SocketAsyncEventArgs> callback, SocketAsyncEventArgs args)
        {
            if (!method(args))
            {
                callback(socket, args);
            }
        }
    }
}