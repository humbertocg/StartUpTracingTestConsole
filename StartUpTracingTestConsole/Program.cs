using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StartUpTracingTestConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Looking for csproj file");
            var ActualDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = System.IO.Path.GetDirectoryName(ActualDirectory);
            var anyfile = Directory.GetFiles(path, "*.csproj").Any();

            if(!anyfile)
            {
                Console.WriteLine("there is no csproj file");
                return;
            }

            Console.WriteLine("Connect an Android device and press enter to continue");
            do
            {
                while (!Console.KeyAvailable)
                {
                    // Do something
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Enter);

            ExecuteCommandLine("msbuild", "/t:BuildAndStartAotProfiling");
            Console.WriteLine("Wait until the app has started...");
            Thread.Sleep(5000);
            Console.WriteLine("If the app has finished to start. Press Enter to continue");
            do
            {
                while (!Console.KeyAvailable)
                {
                    // Do something
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Enter);
            if (RuntimeOSDetection.IsWindows())
            {
                Console.WriteLine("windows");
                ExecuteCommandLine(@"set PATH=%PATH%;""C:\\Program Files(x86)\\Android\android-sdk\\platform-tools\\""", "");
            }
            else if (RuntimeOSDetection.IsMacOS())
            {
                Console.WriteLine("MacOS");
                ExecuteCommandLine(@"export PATH=$PATH:~/.android-sdk-macosx/platform-tools/", "");
            }
            ExecuteCommandLine("msbuild", "/t:FinishAotProfiling");
            Console.WriteLine("Add the following lines into csproj in Release config");
            Console.WriteLine(@"<PropertyGroup>
    < AndroidEnableProfiledAot > true </ AndroidEnableProfiledAot >
    < AndroidUseDefaultAotProfile > false </ AndroidUseDefaultAotProfile >
</ PropertyGroup >");
            Console.WriteLine("Press Enter to finish");
            do
            {
                while (!Console.KeyAvailable)
                {
                    // Do something
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Enter);
        }
        private static bool ExecuteCommandLine(string command, string arg)
        {
            var result = true;
            //Create a processstartinfo object to use the system shell to specify command and parameter settings for standard output
            var psi = new ProcessStartInfo(command, arg) { RedirectStandardOutput = true };
            try
            {
                // start up
                var proc = Process.Start(psi);
                if (proc == null)
                {
                    result = false;
                    Console.WriteLine("Can not exec.");
                }
                else
                {
                    Console.WriteLine("-------------Start read standard output--------------");
                    //Start reading
                    using (var sr = proc.StandardOutput)
                    {
                        while (!sr.EndOfStream)
                        {
                            Console.WriteLine(sr.ReadLine());
                        }

                        if (!proc.HasExited)
                        {
                            proc.Kill();
                        }
                    }
                    Console.WriteLine("---------------Read end------------------");
                    Console.WriteLine($"Total execute time :{(proc.ExitTime - proc.StartTime).TotalMilliseconds} ms");
                    Console.WriteLine($"Exited Code ： {proc.ExitCode}");
                }
            }
            catch (Exception e)
            {
                result = false;
                Console.WriteLine(e.Message);
            }
            return result;
        }
    }
}
