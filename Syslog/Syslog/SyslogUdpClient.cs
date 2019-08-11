using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Aonaware.Syslog
{
    public class SyslogUdpClient : SyslogClient
    {
        public SyslogUdpClient()
        {
        }

        public void Connect(IPAddress serverAddress, int port)
        {
            if (_disposed)
                throw new ObjectDisposedException("SyslogMessage");

            if (_udpClient != null)
                throw new Exception("Already connected");

            IPEndPoint sendPoint = new IPEndPoint(serverAddress, port);
            _udpClient = new UdpClient();
            _udpClient.Connect(sendPoint);
        }

        public void Connect(IPAddress serverAddress)
        {
            Connect(serverAddress, DefaultPort);
        }

        public void Close()
        {
            if (_udpClient == null)
                return;

            _udpClient.Close();
            _udpClient = null;
        }

        public bool Connected
        {
            get
            {
                return (_udpClient != null);
            }
        }

        public void Send(SyslogMessage msg)
        {
            if (_disposed)
                throw new ObjectDisposedException("SyslogMessage");

            if (_udpClient == null)
                throw new Exception("Cannot send data, connection not established");

            if (msg == null)
                throw new ArgumentNullException("msg", "SyslogMessage paramter null");

            byte[] data = _encoding.GetBytes(msg.ToString());
            _udpClient.Send(data, data.Length);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                if (disposing)
                {
                    Close();
                }
                _disposed = true;
            }
        }

        ~SyslogUdpClient()
        {
            Dispose(false);
        }

        private bool _disposed = false;
        private UdpClient _udpClient = null;
        private static ASCIIEncoding _encoding = new ASCIIEncoding();

        public const int DefaultPort = SyslogServer.DefaultPort;
    }
}
