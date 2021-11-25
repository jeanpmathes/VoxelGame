// <copyright file="Parser.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;

namespace VoxelGame.Client.Console
{
    public abstract class Parser
    {
        public abstract Type ParsedType { get; }

        public abstract bool CanParse(string input);

        public abstract object Parse(string input);

        public static Parser BuildParser<T>(Func<string, bool> check, Func<string, T> parse)
        {
            return new SimpleParser<T>(check, parse);
        }

        private class SimpleParser<T> : Parser<T>
        {
            private readonly Func<string, bool> check;
            private readonly Func<string, T> parse;

            public SimpleParser(Func<string, bool> check, Func<string, T> parse)
            {
                this.check = check;
                this.parse = parse;
            }

            public override bool CanParse(string input)
            {
                return check(input);
            }

            public override object Parse(string input)
            {
                return parse(input)!;
            }
        }
    }

    public abstract class Parser<T> : Parser
    {
        public sealed override Type ParsedType => typeof(T);
    }
}