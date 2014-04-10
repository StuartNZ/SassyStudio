﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSassNet;
using SassyStudio.Compiler;
using SassyStudio.Options;

namespace SassyStudio.Integration.LibSass
{
    class LibSassNetDocumentCompiler : IDocumentCompiler
    {
        static readonly Encoding UTF8_ENCODING = new UTF8Encoding(true);
        readonly Lazy<ISassCompiler> _Compiler = new Lazy<ISassCompiler>(() => new SassCompiler());
        readonly ScssOptions Options;

        public LibSassNetDocumentCompiler(ScssOptions options)
        {
            Options = options;
        }

        private ISassCompiler Compiler { get { return _Compiler.Value; } }

        public FileInfo GetOutput(FileInfo source)
        {
            var filename = Path.GetFileNameWithoutExtension(source.Name);
            var directory = DetermineSaveDirectory(source);
            var target = new FileInfo(Path.Combine(directory.FullName, filename + ".css"));

            return target;
        }

        public Task CompileAsync(FileInfo source, FileInfo output)
        {
            return Task.Run(() => Compile(source, output));
        }

        public void Compile(FileInfo source, FileInfo output)
        {
            IEnumerable<string> includePaths = new[] { source.Directory.FullName };
            if (!string.IsNullOrWhiteSpace(Options.CompilationIncludePaths) && Directory.Exists(Options.CompilationIncludePaths))
                includePaths = includePaths.Concat(Options.CompilationIncludePaths.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

            var result = Compiler.CompileFile(source.FullName, sourceMapPath: output.Name + ".map", sourceComments: DetermineSourceCommentsMode(Options), additionalIncludePaths: includePaths);
            var css = result.CSS;
            var sourceMap = result.SourceMap;
            InteropHelper.CheckOut(output.FullName);

            if (Options.IncludeGeneratedCodeWarning)
                css = new StringBuilder("/* AUTOGENERATED CSS: To make changes edit ").Append(source.Name).Append(" */").AppendLine().Append(css).ToString();

            File.WriteAllText(output.FullName, css, UTF8_ENCODING);

            if (Options.GenerateSourceMaps && !string.IsNullOrEmpty(sourceMap))
            {
                InteropHelper.CheckOut(output.FullName + ".map");
                File.WriteAllText(output.FullName + ".map", sourceMap, UTF8_ENCODING);
            }
        }

        private static SourceCommentsMode DetermineSourceCommentsMode(ScssOptions options)
        {
            return options.GenerateSourceMaps 
                ? SourceCommentsMode.SourceMaps 
                : options.IncludeSourceComments 
                    ? SourceCommentsMode.Default 
                    : SourceCommentsMode.None;
        }

        private DirectoryInfo DetermineSaveDirectory(FileInfo source)
        {
            if (string.IsNullOrWhiteSpace(Options.CssGenerationOutputDirectory))
                return source.Directory;

            var path = new Stack<string>();
            var current = source.Directory;
            while (current != null && ContainsSassFiles(current.Parent))
            {
                path.Push(current.Name);
                current = current.Parent;
            }

            // eh, things aren't working out so well, just go back to default
            if (current == null || current.Parent == null)
                return source.Directory;

            // move to sibling directory
            current = new DirectoryInfo(Path.Combine(current.Parent.FullName, Options.CssGenerationOutputDirectory));
            while (path.Count > 0)
                current = new DirectoryInfo(Path.Combine(current.FullName, path.Pop()));

            EnsureDirectory(current);
            return current;
        }

        private void EnsureDirectory(DirectoryInfo current)
        {
            if (current != null && !current.Exists)
            {
                EnsureDirectory(current.Parent);
                current.Create();
            }
        }

        private bool ContainsSassFiles(DirectoryInfo directory)
        {
            return directory != null && directory.EnumerateFiles("*.scss").Any();
        }
    }
}
