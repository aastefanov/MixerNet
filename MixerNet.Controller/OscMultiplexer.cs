using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LucHeart.CoreOSC;
using Nito.AsyncEx;

namespace MixerNet.Controller
{
    public class OscMultiplexer
    {
        private readonly IOscClient client;

        private readonly Queue<Tuple<IOscPacket, AsyncManualResetEvent>> commands =
            new Queue<Tuple<IOscPacket, AsyncManualResetEvent>>();

        private readonly AsyncLock queueLock = new AsyncLock();
        private readonly Queue<IOscPacket> responses = new Queue<IOscPacket>();

        private readonly AsyncLock commandLock = new AsyncLock();

        public OscMultiplexer(IOscClient client)
        {
            this.client = client;

            client.PacketReceived += async (s, e) => await HandleResponseAsync(s, e);

            client.Open();
        }

        private async Task HandleResponseAsync(object sender, IOscPacket response)
        {
            using (await queueLock.LockAsync())
            {
                var (packet, ev) = commands.Dequeue();

                responses.Enqueue(response);
                ev.Set();
            }
        }

        public async Task<IOscPacket> CommandAsync(string address, params object?[] args)
        {
            using (await commandLock.LockAsync())
            {
                return await CommandAsync(new OscMessage(address, args));
            }
        }


        public async Task<IOscPacket> CommandAsync(IOscPacket command)
        {
            var @event = new AsyncManualResetEvent(false);
            using (await queueLock.LockAsync())
            {
                commands.Enqueue(new Tuple<IOscPacket, AsyncManualResetEvent>(command, @event));
                await client.SendAsync(command);
            }

            await @event.WaitAsync();

            using (await queueLock.LockAsync())
            {
                return responses.Dequeue();
            }
        }
    }
}