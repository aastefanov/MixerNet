using MixerNet.Controller;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixerNet.Cli
{
    public static class Helpers
    {
        public static T AddCommands<T>(this T root, params Command[] children) where T : Command
        {
            foreach (var child in children) root.AddCommand(child);
            return root;
        }

        public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
    this IEnumerable<TSource> source, Func<TSource, Task<TResult>> method)
        {
            return await Task.WhenAll(source.Select(async s => await method(s)));
        }

        public static void Each<T>(this IEnumerable<T> ie, Action<T, int> action)
        {
            var i = 0;
            foreach (var e in ie) action(e, i++);
        }

        public static IEnumerable<Tuple<T, int>> Enumerate<T>(this IEnumerable<T> ie)
        {
            var i = 0;
            foreach (T e in ie) yield return new Tuple<T, int>(e, i++);
        }

        public static Command AddArguments(this Command command, params Argument[] args)
        {
            foreach (var arg in args) command.AddArgument(arg);
            return command;
        }
        public static Command AddOptions(this Command command, params Option[] options)
        {
            foreach (var opt in options) command.AddOption(opt);
            return command;
        }

        internal static void Validate(this bool value, ArgumentResult result, string format = "Invalid!")
        {
            if (!value) result.ErrorMessage = string.Format(format, value);
        }

        internal static Argument<T> Validate<T>(this Argument<T> argument, Func<T, bool> predicate, string format = "Invalid!")
        {
            argument.AddValidator(a => predicate(a.GetValueForArgument(argument)).Validate(a, format));
            return argument;
        }

        public static async Task<List<T>> ToListAsync<T>(this Task<IEnumerable<T>> ie) {
            return (await ie).ToList();
        }
    }



    class Validators
    {
        private Validators()
        {

        }
        public static Validators Create(OscController mixer)
        {
            var input = new Argument<string>("input");
            input.Validate(x => mixer.Channels.Contains(x), "Invalid channel: {0}");

            var output = new Argument<string>("output");
            output.Validate(x => mixer.Buses.Contains(x), "Invalid bus: {0}");

            var gain = new Argument<float>("gain");
            gain.Validate(x => x >= 0.0f, "Gain must be non-negative, given: {0.00}");

            return new Validators { Input = input, Output = output, Gain = gain };
        }

        public Argument<string> Input { get; set; }
        public Argument<string> Output { get; set; }
        public Argument<float> Gain { get; set; }
    }

}