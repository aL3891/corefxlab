using System.IO.Pipelines.Networking.Windows.RIO.Internal;
using System.IO.Pipelines.Networking.Windows.RIO.Internal.Winsock;
using System.Threading;

namespace System.IO.Pipelines.Networking.Windows.RIO
{
    public class RioSocketPool
    {
        protected readonly RegisteredIO _rio;
        internal readonly RioThreadPool _pool;

        protected const int MaxSocketsPerThread = 256000;
        protected const int MaxReadsPerSocket = 1;
        protected long _connectionId = 0;

        public const int MaxWritesPerSocket = 2;
        public const int MaxOutsandingCompletionsPerThread = (MaxReadsPerSocket + MaxWritesPerSocket) * MaxSocketsPerThread;


        public RioSocketPool()
        {
            var version = new Internal.Winsock.Version(2, 2);
            WindowsSocketsData wsaData;
            System.Net.Sockets.SocketError result = RioImports.WSAStartup((short)version.Raw, out wsaData);
            if (result != System.Net.Sockets.SocketError.Success)
            {
                var error = RioImports.WSAGetLastError();
                throw new Exception(string.Format("ERROR: WSAStartup returned {0}", error));
            }

            var tempSocket = RioImports.WSASocket(AddressFamilies.Internet, SocketType.Stream, Protocol.IpProtocolTcp, IntPtr.Zero, 0, SocketFlags.RegisteredIO);
            if (tempSocket == IntPtr.Zero)
            {
                var error = RioImports.WSAGetLastError();
                RioImports.WSACleanup();
                throw new Exception(string.Format("ERROR: WSASocket returned {0}", error));
            }

            _rio = RioImports.Initalize(tempSocket);
            RioImports.closesocket(tempSocket);
            _pool = new RioThreadPool(_rio, CancellationToken.None);
        }

        protected void Bind(IntPtr socket, ushort port, byte address1, byte address2, byte address3, byte address4)
        {
            // BIND
            var inAddress = new Ipv4InternetAddress();
            inAddress.Byte1 = address1;
            inAddress.Byte2 = address2;
            inAddress.Byte3 = address3;
            inAddress.Byte4 = address4;

            var sa = new SocketAddress();
            sa.Family = AddressFamilies.Internet;
            sa.Port = RioImports.htons(port);
            sa.IpAddress = inAddress;

            int result;
            unsafe
            {
                var size = sizeof(SocketAddress);
                result = RioImports.bind(socket, ref sa, size);
            }
            if (result == RioImports.SocketError)
            {
                RioImports.WSACleanup();
                throw new Exception("bind failed");
            }
        }


        public void Stop()
        {
            RioImports.WSACleanup();
        }
    }
}
