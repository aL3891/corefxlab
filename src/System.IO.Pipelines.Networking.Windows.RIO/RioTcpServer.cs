// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.IO.Pipelines.Networking.Windows.RIO.Internal;
using System.IO.Pipelines.Networking.Windows.RIO.Internal.Winsock;
using System.Net;

namespace System.IO.Pipelines.Networking.Windows.RIO
{
    public sealed class RioTcpServer : RioSocketPool
    {
        private IntPtr _listenerSocket;


        public RioTcpServer(IPEndPoint ipEndpoint)
        {
            var bytes = ipEndpoint.Address.GetAddressBytes();
            Start((ushort)ipEndpoint.Port, bytes[0], bytes[1], bytes[2], bytes[3]);
        }

        public RioTcpServer(ushort port, byte address1, byte address2, byte address3, byte address4)
        {
            Start(port, address1, address2, address3, address4);
        }

        private void Start(ushort port, byte address1, byte address2, byte address3, byte address4)
        {

            _listenerSocket = RioImports.WSASocket(AddressFamilies.Internet, SocketType.Stream, Protocol.IpProtocolTcp, IntPtr.Zero, 0, SocketFlags.RegisteredIO);
            if (_listenerSocket == IntPtr.Zero)
            {
                var error = RioImports.WSAGetLastError();
                RioImports.WSACleanup();
                throw new Exception(string.Format("ERROR: WSASocket returned {0}", error));
            }

            Bind(_listenerSocket, port, address1, address2, address3, address4);

            // LISTEN
            var result = RioImports.listen(_listenerSocket, 2048);
            if (result == RioImports.SocketError)
            {
                RioImports.WSACleanup();
                throw new Exception("listen failed");
            }
        }

        public RioTcpConnection Accept()
        {
            var accepted = RioImports.accept(_listenerSocket, IntPtr.Zero, 0);
            if (accepted == new IntPtr(-1))
            {
                var error = RioImports.WSAGetLastError();
                RioImports.WSACleanup();
                throw new Exception($"listen failed with {error}");
            }
            var connectionId = Interlocked.Increment(ref _connectionId);
            var thread = _pool.GetThread(connectionId);

            var requestQueue = _rio.RioCreateRequestQueue(
                                        accepted,
                                        maxOutstandingReceive: MaxReadsPerSocket,
                                        maxReceiveDataBuffers: 1,
                                        maxOutstandingSend: MaxWritesPerSocket,
                                        maxSendDataBuffers: 1,
                                        receiveCq: thread.ReceiveCompletionQueue,
                                        sendCq: thread.SendCompletionQueue,
                                        connectionCorrelation: connectionId);

            if (requestQueue == IntPtr.Zero)
            {
                var error = RioImports.WSAGetLastError();
                RioImports.WSACleanup();
                throw new Exception($"ERROR: RioCreateRequestQueue returned {error}");
            }

            return new RioTcpConnection(accepted, connectionId, requestQueue, thread, _rio);
        }
    }
}
