using LucHeart.CoreOSC;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixerNet.Controller
{
    public class OscSlipClient : IOscClient, IDisposable
    {
        private static readonly byte END = (byte)'\xc0';
        private static readonly byte ESC = (byte)'\xdb';
        private static readonly byte ESC_END = (byte)'\xdc';
        private static readonly byte ESC_ESC = (byte)'\xdd';

        private readonly SerialPort port;

        public event EventHandler<IOscPacket> PacketReceived;

        public OscSlipClient(SerialPort port)
        {
            this.port = port;

            port.DataReceived += OnDataReceived;
        }

        private Span<byte> ReadToEnd()
        {
            var len = port.BytesToRead;

            // TODO: Don't read byte by byte

            int b = 0;
            byte[] buffer = new byte[len];
            for (int read = 0; read < len; read++)
            {
                byte c = (byte)port.ReadByte();

                if (c == END)
                {
                    if (b > 0) break;
                }
                else if (c == ESC)
                {
                    read++;
                    byte c2 = (byte)port.ReadByte();
                    if (c2 == ESC_END) buffer[b++] = END;
                    else if (c2 == ESC_ESC) buffer[b++] = ESC;
                }
                else
                {
                    buffer[b++] = c;
                }

            }

            return buffer;
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var data = this.ReadToEnd();

            PacketReceived?.Invoke(this, Helpers.ParseOsc(data));
        }

        public void Send(IOscPacket data)
        {

            // TODO: Don't go byte by byte

            Span<byte> bytes = data.GetBytes();

            List<byte> result = new List<byte>(bytes.Length + 10);


            result.Add(END);

            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == ESC) result.AddRange(new[] { ESC, ESC_ESC });
                else if (bytes[i] == END) result.AddRange(new[] { ESC, ESC_END });
                else result.Add(bytes[i]);
            }

            result.Add(END);

            var final = result.ToArray();
            port.Write(final, 0, final.Length);
        }

        public void Dispose()
        {
            port.Dispose();
        }

        public void Open(CancellationToken? token = null)
        {
            port.Open();
        }

        public void Close()
        {
            port.Close();
        }

        public Task SendAsync(IOscPacket data)
        {
            return Task.Run(() => this.Send(data));
        }
    }
}
