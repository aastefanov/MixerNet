using LucHeart.CoreOSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MixerNet.Controller
{
    internal static class Helpers
    {
        public static IOscPacket ParseOsc(Span<byte> data)
        {
            if (data.StartsWith(Encoding.ASCII.GetBytes("#bundle").AsSpan()))
            {
                var bundle = OscBundle.ParseBundle(data);
                return bundle;
            }
            else
            {
                var message = OscMessage.ParseMessage(data);
                return message;
            }
        }

        public static OscMessage? Find(this OscBundle bundle, string address)
        {
            return bundle.Messages.FirstOrDefault(x => x.Address.Equals(address));
        }

        public static T As<T>(this IOscPacket packet)
        {
            return (T)(packet as OscMessage)?.Arguments[0];
        }
    }
}
