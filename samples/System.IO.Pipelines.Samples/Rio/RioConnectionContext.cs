using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines;
using System.Net;
using System.IO.Pipelines.Networking.Windows.RIO;

namespace System.IO.Pipelines.Samples.Rio
{
    public class RioConnectionContext : IConnectionInformation
    {
        private RioTcpConnection connection;
        private readonly Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines.PipeFactory _PipeFactory;

        public RioConnectionContext(RioTcpConnection connection, Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines.PipeFactory _PipeFactory)
        {
            this.connection = connection;
            this._PipeFactory = _PipeFactory;
        }

        public IPEndPoint RemoteEndPoint { get; set; }
        public IPEndPoint LocalEndPoint { get; set; }

        public Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines.PipeFactory PipeFactory => _PipeFactory;

        public Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines.IScheduler InputWriterScheduler => Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines.InlineScheduler.Default;

        public Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines.IScheduler OutputReaderScheduler => Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines.TaskRunScheduler.Default;

    }
}
