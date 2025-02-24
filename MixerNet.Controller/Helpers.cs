using System;
using System.Linq;
using System.Text;
using LucHeart.CoreOSC;

namespace MixerNet.Controller
{
    public static class Helpers
    {
        public static IOscPacket ParseOsc(Span<byte> data)
        {
            return data.StartsWith(Encoding.ASCII.GetBytes("#bundle\0").AsSpan())
                ? OscBundle.ParseBundle(data)
                : OscMessage.ParseMessage(data) as IOscPacket;
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