﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SassyStudio.Compiler.Parsing;

namespace SassyStudio.Editor.Intellisense
{
    [Export(typeof(ICompletionContextProvider))]
    class XmlDocumentationContextProvider : ICompletionContextProvider
    {
        public IEnumerable<SassCompletionContextType> GetContext(ICompletionContext context)
        {
            if (context.Current is FileReferenceTag)
            {
                var tag = context.Current as FileReferenceTag;
                if (tag.Filename == null)
                    yield return SassCompletionContextType.FileReferenceFileAttribute;
            }
            else if (context.Current is XmlDocumentationTag)
            {
                yield return SassCompletionContextType.XmlDocumentationComment;
            }
        }
    }
}
