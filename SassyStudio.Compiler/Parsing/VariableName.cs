﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SassyStudio.Compiler.Lexing;

namespace SassyStudio.Compiler.Parsing
{
    public class VariableName : ComplexItem
    {
        public VariableName(SassClassifierType classifierType)
        {
            ClassifierType = classifierType;
        }

        public TokenItem Prefix { get; protected set; }
        public TokenItem Name { get; protected set; }
        public override bool IsValid { get { return Prefix != null && Name != null && Name.Length > 0; } }

        public override bool Parse(IItemFactory itemFactory, ITextProvider text, ITokenStream stream)
        {
            if (IsVariable(text, stream))
            {
                Prefix = Children.AddCurrentAndAdvance(stream, ClassifierType);
                Name = Children.AddCurrentAndAdvance(stream, ClassifierType);
            }

            return Children.Count > 0;
        }

        public string GetName(ITextProvider text)
        {
            if (Name == null || Name.Length == 0)
                return null;

            var builder = new StringBuilder(Name.Length + 1);
            builder.Append(Prefix.SourceType == TokenType.Dollar ? '$' : '!');
            Name.WriteTo(builder, text);

            return builder.ToString();
        }

        public static bool IsVariable(ITextProvider text, ITokenStream stream)
        {
            if (stream.Current.Type == TokenType.Dollar || stream.Current.Type == TokenType.Bang)
            {
                var name = stream.Peek(1);
                if (name.Type == TokenType.Identifier && !ImportanceModifier.IsImportanceModifier(text, stream))
                    return true;
            }

            return false;
        }
    }
}
