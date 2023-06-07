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

            var packageOption = new Option<IEnumerable<string>>(new string[] { "-p", "--packages" });
            latexCommand.AddOption(packageOption);
            var classOption = new Option<string>(new string[] { "-c", "--class" });
            latexCommand.AddOption(classOption);

            rootCommand.AddCommand(manimCommand);
            rootCommand.AddCommand(latexCommand);

            manimCommand.SetHandler(InitManim, nameArgument);

            latexCommand.SetHandler(InitLatex, nameArgument, packageOption, classOption);

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


            Process.Start(new ProcessStartInfo() { FileName="code", UseShellExecute=true });
        }
        #endregion

        #region latex
        public static void InitLatex(string name, IEnumerable<string> packages, string? documentClass)
        {
            Console.WriteLine($"Project Name: {name}");
            if (packages != null && packages.Any())
            {
                Console.WriteLine("Packages");
                foreach (string package in packages)
                {
                    Console.WriteLine(package);
                }
            } else
            {
                Console.WriteLine("No packages");
            }

            Console.WriteLine($"Class: {(!string.IsNullOrEmpty(documentClass) ? documentClass : "article")}");
        }

        #endregion



    }
}