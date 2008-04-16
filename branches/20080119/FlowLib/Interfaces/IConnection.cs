
/*
 *
 * Copyright (C) 2008 Mattias Blomqvist, patr-blo at dsv dot su dot se
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *
 */

using FlowLib.Events;
using FlowLib.Containers;
using System.Net;

namespace FlowLib.Interfaces
{
    public interface IConnection
    {
        #region Events
        event FmdcEventHandler ProtocolChange;
        event FmdcEventHandler ConnectionStatusChange;
        #endregion
        #region Properties
        /// <summary>
        /// Protocol that hub should use to understand messages that is received.
        /// </summary>
        IProtocol Protocol
        {
            get;
            set;
        }
        IPEndPoint LocalAddress
        {
            get;
            set;
        }
        IPEndPoint RemoteAddress
        {
            get;
        }
        /// <summary>
        /// Sharing instance for this transfer
        /// </summary>
        Share Share
        {
            get;
            set;
        }
        #endregion
        #region Functions
        /// <summary>
        /// Make connection to server.
        /// </summary>
        void Connect();
        /// <summary>
        /// Tries to reconnect to server.
        /// This will close connection if there is any and try to connect again.
        /// </summary>
        void Reconnect();
        /// <summary>
        /// Disconnects connection.
        /// </summary>
        void Disconnect();
        /// <summary>
        /// Disconnects connection with msg as reason.
        /// </summary>
        /// <param name="msg">Reason why connection died. (For debug reasons mostly)
        /// </param>
        void Disconnect(string msg);
        /// <summary>
        /// Sending byte[] to server.
        /// </summary>
        /// <param name="raw">byte[] to send to server</param>
        void Send(byte[] raw);
        /// <summary>
        /// Sending IConMessage to server.
        /// </summary>
        /// <param name="msg">Message that will be sent to server.</param>
        void Send(IConMessage msg);
        #endregion
    }
}