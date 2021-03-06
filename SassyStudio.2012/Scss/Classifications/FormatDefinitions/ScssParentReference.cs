﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Language.StandardClassification;
using System.Windows.Media;

namespace SassyStudio.Scss.Classifications
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ScssClassificationTypes.ParentReference)]
    [Name(ScssClassificationTypes.ParentReference)]
    [Order(Before = Priority.Default)]
    [UserVisible(true)]
    sealed class ScssParentReference : ClassificationFormatDefinition
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public ScssParentReference()
        {
            DisplayName = "SCSS Parent Reference";
            ForegroundCustomizable = true;
            ForegroundColor = Color.FromRgb(0x75, 0x75, 0x75);
        }

        //protected override FormatColorStorage Light { get { return new FormatColorStorage { Foreground = Color.FromRgb(0, 0, 0) }; } }
        //protected override FormatColorStorage Dark { get { return new FormatColorStorage { Foreground = Color.FromRgb(0, 0, 0) }; } }
    }
}
