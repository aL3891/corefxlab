using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipelines.Networking.Windows.RIO.Internal.Winsock;
using System.Threading;
using System.Net;

namespace System.IO.Pipelines.Networking.Windows.RIO
{
    public class RioTcpClientPool : RioSocketPool
    {

        public RioTcpConnection Connect(ushort port, byte address1, byte address2, byte address3, byte address4)
        {
            var socket = RioImports.WSASocket(AddressFamilies.Internet, SocketType.Stream, Protocol.IpProtocolTcp, IntPtr.Zero, 0, SocketFlags.RegisteredIO);
            if (socket == IntPtr.Zero)
            {
                var error = RioImports.WSAGetLastError();
                RioImports.WSACleanup();
                throw new Exception(string.Format("ERROR: WSASocket returned {0}", error));
            }

            Bind(socket, 0, 0, 0, 0, 0);

            var inAddress = new Ipv4InternetAddress();
            inAddress.Byte1 = address1;
            inAddress.Byte2 = address2;
            inAddress.Byte3 = address3;
            inAddress.Byte4 = address4;

            var sa = new Internal.Winsock.SocketAddress();
            sa.Family = AddressFamilies.Internet;
            sa.Port = RioImports.htons(port);
            sa.IpAddress = inAddress;

            unsafe
            {
                var res = RioImports.connect(socket, ref sa, sizeof(Internal.Winsock.SocketAddress));
                if (res == RioImports.SocketError)
                {
                    RioImports.WSACleanup();
                    throw new Exception("bind failed");
                }
            }

            var connectionId = Interlocked.Increment(ref _connectionId);
            var thread = _pool.GetThread(connectionId);

            var requestQueue = _rio.RioCreateRequestQueue(
                                        socket,
                                        maxOutstandingReceive: MaxReadsPerSocket,
                                        maxReceiveDataBuffers: 1,
                                        maxOutstandingSend: MaxWritesPerSocket,
                                        maxSendDataBuffers: 1,
                                        receiveCq: thread.ReceiveCompletionQueue,
                                        sendCq: thread.SendCompletionQueue,
                                        connectionCorrelation: connectionId);

            return new RioTcpConnection(socket, _connectionId, requestQueue, thread, _rio);
        }

        public RioTcpConnection Connect(IPEndPoint ipEndpoint)
        {
            var bytes = ipEndpoint.Address.GetAddressBytes();
            return Connect((ushort)ipEndpoint.Port, bytes[0], bytes[1], bytes[2], bytes[3]);
        }
    }
}
