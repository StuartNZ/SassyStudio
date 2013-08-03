﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassyStudio.Compiler.Parsing
{
    class UserFunctionDefinition : ComplexItem
    {
        private readonly List<FunctionArgumentDefinition> _Arguments = new List<FunctionArgumentDefinition>(1);
        public UserFunctionDefinition()
        {
            _Arguments = new List<FunctionArgumentDefinition>(1);
        }

        public AtRule Rule { get; protected set; }
        public TokenItem Name { get; protected set; }
        public TokenItem OpenBrace { get; protected set; }
        public IList<FunctionArgumentDefinition> Arguments { get { return _Arguments; } }
        public TokenItem CloseBrace { get; protected set; }

        public override bool Parse(IItemFactory itemFactory, ITextProvider text, ITokenStream stream)
        {
            if (AtRule.IsRule(text, stream, "function") && stream.Peek(2).Type == TokenType.Function)
            {
                Rule = AtRule.CreateParsed(itemFactory, text, stream);
                Children.Add(Rule);

                Name = Children.AddCurrentAndAdvance(stream, SassClassifierType.UserFunctionDefinition);
                if (stream.Current.Type == TokenType.OpenFunctionBrace)
                    Children.AddCurrentAndAdvance(stream, SassClassifierType.FunctionBrace);

                while (!IsTerminator(stream.Current.Type))
                {
                    var argument = itemFactory.CreateSpecific<FunctionArgumentDefinition>(this, text, stream);
                    if (argument == null || !argument.Parse(itemFactory, text, stream))
                        break;

                    Arguments.Add(argument);
                    Children.Add(argument);
                }

                if (stream.Current.Type == TokenType.CloseFunctionBrace)
                    Children.AddCurrentAndAdvance(stream, SassClassifierType.FunctionBrace);
            }

            return Children.Count > 0;
        }

        public override void Freeze()
        {
            base.Freeze();
            _Arguments.TrimExcess();
        }

        static bool IsTerminator(TokenType type)
        {
            switch (type)
            {
                case TokenType.EndOfFile:
                case TokenType.CloseFunctionBrace:
                case TokenType.Comma:
                case TokenType.OpenCurlyBrace:
                    return true;
            }

            return false;
        }
    }
}
