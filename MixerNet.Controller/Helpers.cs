using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LucHeart.CoreOSC;

namespace MixerNet.Controller
{
    public static class Helpers
    {
        public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, Task<TResult>> func)
        {
            return await Task.WhenAll(source.Select(async s => await func(s)));
        }

        public static void Each<T>(this IEnumerable<T> ie, Action<T, int> action)
        {
            var i = 0;
            foreach (var e in ie) action(e, i++);
        }

        public static IEnumerable<Tuple<T, int>> Enumerate<T>(this IEnumerable<T> ie)
        {
            var i = 0;
            foreach (var e in ie) yield return new Tuple<T, int>(e, i++);
        }

        public static IEnumerable<TResult> SelectEnum<TSource, TResult>(this IEnumerable<TSource> ie,
            Func<TSource, int, TResult> func)
        {
            foreach (var (e, i) in ie.Enumerate()) yield return func(e, i);
        }


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