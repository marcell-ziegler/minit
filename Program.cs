using System.CommandLine;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace minit
{
    internal class Program
    {
        private static readonly string PYPROJECT_TOML_FILE = 
            "[tool.pylama]\n" +
            "ignore = \"W0401\"\n" +
            "[tool.pyright]\n" +
            "reportWildcardImportFromLibrary = \"none\"";

        public static int Main(string[] args)
        {
            var rootCommand = new RootCommand();

            var manimCommand = new Command("manim");

            var latexCommand = new Command("latex");

            var nameArgument = new Argument<string>("name");
            manimCommand.AddArgument(nameArgument);
            latexCommand.AddArgument(nameArgument);

            var packageOption = new Option<List<string>>(new string[] { "-p", "--packages" });
            latexCommand.AddOption(packageOption);
            var classOption = new Option<string>(new[] { "-c", "--class" });
            latexCommand.AddOption(classOption);
            var languageOption = new Option<string>(new[] { "-l", "--lang", "--language" });
            latexCommand.AddOption(languageOption);
            var bibOption = new Option<bool>(new[] { "-b", "--bib" });
            latexCommand.AddOption(bibOption);

            rootCommand.AddCommand(manimCommand);
            rootCommand.AddCommand(latexCommand);

            manimCommand.SetHandler(InitManim, nameArgument);

            latexCommand.SetHandler(InitLatex, nameArgument, packageOption, classOption, languageOption, bibOption);

            return rootCommand.Invoke(args);
        }

        public static string Prompt(string prompt, string? @default=null)
        {
            Console.Write(prompt);
            string result = Console.ReadLine();
        
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            } else
            {
                return string.IsNullOrEmpty(@default) ? "" : @default;
            }
        }

        #region manim
        internal class ManimOptions
        {
            public int fps;
            public string res;
            public int width;
            public int height;
            public float bgOpacity;
            public string bgColor;
            public string sceneName;



            private readonly Dictionary<string, int[]> RESOLUTIONS = new Dictionary<string, int[]>()
            {
                { "240p", new int[] {427, 240 } },
                { "480p", new int[] {854, 480} },
                { "720p", new int[] {1280, 720} },
                { "1080p", new int[] { 1920, 1080 } },
                { "1440p", new int[] { 2560, 1440} },
                { "2160p", new int[] { 3840, 2160 } }
            };

            public ManimOptions(string sceneName)
            {
                this.sceneName = sceneName;
                fps = int.Parse(Prompt("Frame rate [30]: ", "30"));
                res = Prompt("Resolution:\n(240p, 480p, 720p, 1080p, 1440p, 2160p) [480p]: ", "480p");
                width = RESOLUTIONS[res][0];
                height = RESOLUTIONS[res][1];
                bgColor = Prompt("Background color [BLACK]: ", "BLACK");
                bgOpacity = float.Parse(Prompt("Background opacity [1]: ", "1"));
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine("[CLI]");
                sb.AppendLine($"frame_rate = {fps}");
                sb.AppendLine($"pixel_height = {height}");
                sb.AppendLine($"pixel_width = {width}");
                sb.AppendLine($"background_color = {bgColor}");
                sb.AppendLine($"background_opacity = {bgOpacity}");
                sb.AppendLine($"scene_names = {sceneName}");

                return sb.ToString();
            }
            
        }

        static void InitManim(string name)
        {
            Directory.CreateDirectory(name);
            Directory.SetCurrentDirectory(name);


            var opts = new ManimOptions(name);

            File.WriteAllText("manim.cfg", opts.ToString());

            var mainFile = new StringBuilder();
            mainFile.AppendLine("from manim import *");
            mainFile.AppendLine("import os");
            mainFile.AppendLine();
            mainFile.AppendLine();
            mainFile.AppendLine($"class {name}(Scene):");
            mainFile.AppendLine("   def construct(self)");
            mainFile.AppendLine("       pass");
            mainFile.AppendLine();
            mainFile.AppendLine();
            mainFile.AppendLine("if __name__ == \"__main__\":");
            mainFile.AppendLine("   os.system(\"manim main.py\")");

            File.WriteAllText("main.py", mainFile.ToString());

            File.WriteAllText("pyproject.toml", PYPROJECT_TOML_FILE);


            Process.Start(new ProcessStartInfo() { FileName="code", Arguments = ".", UseShellExecute=true });
        }
        #endregion

        #region latex
        public static void InitLatex(string name, List<string> packages, string? documentClass, string? lang, bool bib)
        {
            Directory.CreateDirectory(name);
            Directory.SetCurrentDirectory(name);

            var sb = new StringBuilder();
            if (string.IsNullOrEmpty(lang)) { lang = "english"; }


            sb.AppendLine("\\documentclass{" + (!string.IsNullOrEmpty(documentClass) ? documentClass : "article") + "}");
            sb.AppendLine();

            // biblatex
            if (bib) 
            {
                sb.AppendLine("\\usepackage[backend=biber]{biblatex}");
                File.Create("main.bib");
            }
            // Default packages
            foreach (string package in new[] { "amsmath", "amssymb" })
            {
                sb.AppendLine("\\usepackage{" + package + "}");
            }
            // Language dependent packages
            foreach (string package in new[] {"babel", "varioref", "cleveref"})
            {
                sb.AppendLine($"\\usepackage[{lang}]" + "{" + package + "}");
            }



            if (packages.Any())
            {
                foreach (var package in packages)
                {
                    sb.AppendLine("\\usepackage{" + package + "}");
                }
            } else { sb.AppendLine(); }

            sb.AppendLine();

            sb.AppendLine("\\begin{document}");
            sb.AppendLine("    Hello World!");
            sb.AppendLine("\\end{document}");

            File.WriteAllText("main.tex", sb.ToString());

            Process.Start(new ProcessStartInfo() { FileName = "code", Arguments = ".", UseShellExecute = true });
        }

        #endregion



    }
}