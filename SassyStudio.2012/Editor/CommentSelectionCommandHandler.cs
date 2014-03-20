﻿using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;

namespace SassyStudio.Editor
{
    class CommentSelectionCommandHandler : VSCommandTarget<VSConstants.VSStd2KCmdID>
    {
        readonly ITextBuffer Buffer;
        public CommentSelectionCommandHandler(IVsTextView vsTextView, IWpfTextView textView)
            : base(vsTextView, textView)
        {
            Buffer = textView.TextBuffer;
        }

        protected override bool Execute(VSConstants.VSStd2KCmdID command, uint options, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (TextView.Selection.IsEmpty) return false;


            var snapshot = Buffer.CurrentSnapshot;
            int start = TextView.Selection.Start.Position.Position;
            int end = TextView.Selection.End.Position.Position;

            // this is what we will store start offset in to get maximal indentation amount
            int? insertStartOffset = null;

            using (var edit = Buffer.CreateEdit())
            {
                while (start < end)
                {
                    var line = snapshot.GetLineFromPosition(start);
                    var text = line.GetText();
                    switch (command)
                    {
                        case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                        {
                            if (insertStartOffset == null) insertStartOffset = GetOffset(snapshot, start, end);
                            if (!string.IsNullOrEmpty(text))
                                edit.Insert(line.Start.Position + insertStartOffset.GetValueOrDefault(), "//");

                            break;
                        }
                        case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                        {
                            for (int i = 0; i < text.Length - 1; i++)
                            {
                                if (text[i] == '/' && text[i + 1] == '/')
                                {
                                    edit.Delete(line.Start.Position + i, 2);
                                    break;
                                }
                            }

                            break;
                        }
                    }

                    start = line.EndIncludingLineBreak.Position;
                }

                edit.Apply();
            }

            return true;
        }

        private int GetOffset(ITextSnapshot snapshot, int start, int end)
        {
            int offset = int.MaxValue;
            while (start < end)
            {
                var line = snapshot.GetLineFromPosition(start);
                var text = line.GetText();

                for (int i = 0; i < text.Length; i++)
                {
                    if (!char.IsWhiteSpace(text[i]))
                        offset = Math.Min(offset, i);
                }

                start = line.EndIncludingLineBreak.Position;
            }

            return offset == int.MaxValue ? 0 : offset;
        }

        protected override IEnumerable<VSConstants.VSStd2KCmdID> SupportedCommands
        {
            get
            {
                yield return VSConstants.VSStd2KCmdID.COMMENTBLOCK;
                yield return VSConstants.VSStd2KCmdID.COMMENT_BLOCK;
                yield return VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK;
                yield return VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK;
            }
        }

        protected override VSConstants.VSStd2KCmdID ConvertFromCommandId(uint id)
        {
            return (VSConstants.VSStd2KCmdID)id;
        }

        protected override uint ConvertFromCommand(VSConstants.VSStd2KCmdID command)
        {
            return (uint)command;
        }
    }
}
