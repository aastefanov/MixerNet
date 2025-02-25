using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LucHeart.CoreOSC;
using Nito.AsyncEx;

namespace MixerNet.Controller
{
    public class OscSlipClient : IOscClient, IDisposable
    {
        private static readonly byte END = (byte)'\xc0';
        private static readonly byte ESC = (byte)'\xdb';
        private static readonly byte ESC_END = (byte)'\xdc';
        private static readonly byte ESC_ESC = (byte)'\xdd';

        private readonly RingBuffer<byte> buffer = new RingBuffer<byte>(1024);
        private readonly byte[] scratch = new byte[1024];


        private readonly AsyncLock serialLock = new AsyncLock();

        private readonly SerialPort port;
        private event EventHandler<int> SerialPacketReceived;


        public OscSlipClient(SerialPort port)
        {
            this.port = port;

            port.DataReceived += async (s, e) => await ReadToEnd();

            SerialPacketReceived += async (s, e) => await OnPacketComplete(s, e);
        }

        public void Dispose()
        {
            Close();
            port.Dispose();
        }

        public event EventHandler<IOscPacket>? PacketReceived;

        public void Send(IOscPacket data)
        {
            var bytes = data.GetBytes();
            var final = CreatePacket(bytes).ToArray();
            port.Write(final, 0, final.Length);
        }

        public void Open(CancellationToken? token = null)
        {
            // port.Close();
            // Task.Delay(1000).Wait();
            port.Open();
            port.DiscardInBuffer();
        }

        public Task SendAsync(IOscPacket data)
        {
            return Task.Run(() => Send(data));
        }


        private async Task OnPacketComplete(object sender, int e)
        {
            using (await serialLock.LockAsync())
            {
                var data = buffer.Read(e).ToArray();
                buffer.DecrementCount(e);

                var packet = Helpers.ParseOsc(data);
                PacketReceived?.Invoke(this, packet);
            }
        }

        private async Task ReadToEnd()
        {
            using (await serialLock.LockAsync())
            {
                var initialCount = buffer.Count;

                while (port.BytesToRead > 0)
                {
                    var len = Math.Min(port.BytesToRead, scratch.Length);

                    port.Read(scratch, 0, len);
                    // var buf = Encoding.UTF8.GetBytes(port.ReadExisting());

                    for (var i = 0; i < len; i++)
                    {
                        if (scratch[i] == END)
                        {
                            // this is the end, not the beginning of a packet
                            if (buffer.Count - initialCount > 0)
                            {
                                SerialPacketReceived?.Invoke(this, (buffer.Count - initialCount));
                                return;
                            }
                        }
                        else if (scratch[i] == ESC)
                        {
                            // TODO: make sure this doesn't break
                            var c2 = scratch[++i];
                            if (c2 == ESC_END) buffer.Add(END);
                            else if (c2 == ESC_ESC) buffer.Add(ESC);
                        }
                        else
                        {
                            buffer.Add(scratch[i]);
                        }
                    }
                }
            }
        }


        private IEnumerable<byte> CreatePacket(byte[] source)
        {
            yield return END;
            foreach (var t in source)
                if (t == ESC)
                {
                    yield return ESC;
                    yield return ESC_ESC;
                }
                else if (t == END)
                {
                    yield return ESC;
                    yield return ESC_END;
                }
                else
                {
                    yield return t;
                }

            yield return END;
        }

        public void Close()
        {
            port.Close();
        }
        
        
    }
}