﻿using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using SevenZip;
using System.IO;
using DokanNet;
using DokanNet.Logging;
using System.Threading;
using System.Collections.Generic;

namespace Dokan2.Archive
{
    public class SevenZipProgram
    {
        delegate int ExtractArg(string argName, out string value);
        delegate bool ExtractIntOption(string argName, out int value);

        private static string _password = null;
        private static bool _hasReadPW = false;
        private static int _debugSelf = 0;

        public static bool VerboseOutput { get; private set; } = false;
        public static int DebugSelf { get => _debugSelf; private set => _debugSelf = value; }
        public static bool DebugDokan { get; private set; } = false;
        public static bool MixMode { get; private set; } = false;

        static int Main(string[] args)
        {
            var dashTwo = Array.FindIndex(args, i => i == "--");
            dashTwo = dashTwo >= 0 ? dashTwo : args.Length;
            ExtractArg extractArg = (string argName, out string value) =>
            {
                var index = Array.FindIndex(args, i => i.StartsWith(argName));
                value = null;
                if (index < 0 || index >= dashTwo) { }
                else if (argName.Length == 3 && args[index].Length > argName.Length)
                {
                    value = args[index].Substring(argName.Length);
                    value = value.StartsWith("=") ? value.Substring(1) : value;
                    args = args.Take(index).Concat(args.Skip(index + 1)).ToArray();
                    dashTwo--;
                }
                else if (index < args.Length - 1)
                {
                    value = args[index + 1];
                    args = args.Take(index).Concat(args.Skip(index + 2)).ToArray();
                    dashTwo -= 2;
                }
                else
                {
                    args = args.Take(index).ToArray();
                    dashTwo--;
                }
                return index;
            };
            extractArg("-l", out var labelName);
            var passwordIndex = extractArg("-p", out _password);
            string[] extractorSwitches = new string[0];
            while (extractArg("-s", out var extractorFlag) >= 0)
                extractorSwitches = extractorSwitches.Concat((extractorFlag ?? "").Split(',', ';')).ToArray();
            var parallelIndex = extractArg("-j", out var parallelNum);

            var opts = " " + string.Join(" ", args.Take(dashTwo).Where(i => i.Length > 1 && i[0] == '-')
                .Select(i => i[1] != '-' && i.Length > 2 && (i[2] < '0' || i[2] > '9')
                            ? "-" + string.Join(" -", i.Skip(1)) : i));
            args = (dashTwo < args.Length ? args.Skip(dashTwo) : args.Where(i => i.Length == 0 || i[0] != '-')).ToArray();
            if (args.Length == 0 || opts.Contains("-h"))
            {
                Console.WriteLine("Usage: Dokan2.Archive.exe [-aAdDeotv]\n" +
                    "            archive-file Drive: [root-folder]\n" +
                    "            [-l label] [-j parallel-tasks] [-s extractor-switches] [-p [password]]");
                return 0;
            }
            ExtractIntOption extractIntOption = (string argName, out int value) =>
            {
                value = 0;
                var lastIndex = opts.LastIndexOf(argName);
                if (lastIndex < 0)
                    return false;
                value = 1;
                lastIndex += argName.Length;
                var ind2 = opts.IndexOf(' ', lastIndex);
                if (ind2 >= 0 && !int.TryParse(opts.Substring(lastIndex, ind2 - lastIndex), out value))
                    value = 1;
                return true;
            };

            var file = args.FirstOrDefault();
            if (string.IsNullOrEmpty(file))
            {
                Console.WriteLine("Must specify a file.");
                return 1;
            }
            var mountPoint = args.Skip(1).FirstOrDefault();
            if (mountPoint != null && mountPoint.Length >= 2 && mountPoint.Length <= 3
                && ":\\:/".Contains(mountPoint.Substring(1)))
                mountPoint = mountPoint.Substring(0, 1);
            bool isDrive = mountPoint != null && mountPoint.Length == 1
                && 'a' <= mountPoint.ToLower()[0] && mountPoint.ToLower()[0] <= 'z';
            bool autoDrive = opts.Contains("-a") || string.IsNullOrEmpty(mountPoint);
            if (string.IsNullOrEmpty(mountPoint))
            {
                mountPoint = "X";
                isDrive = true;
            }
            if (isDrive && Directory.Exists(mountPoint + ":\\"))
            {
                var names = DriveInfo.GetDrives();
                var existing = mountPoint;
                if (autoDrive)
                {
                    var used = names.Select(i => (i.Name[0] & ~0x20) - 'A').ToDictionary(i => i);
                    var cur = (mountPoint[0] & ~0x20) - 'A';
                    int next;
                    for (next = cur; ++next < 26 && used.ContainsKey(next); ) { }
                    if (next >= 26)
                        for (next = cur; --next >= 3 && used.ContainsKey(next); ) { } // start from D:
                    if (next >= 3 && next <= 26)
                        mountPoint = ((char)(next + 'A')).ToString();
                }
                if (existing == mountPoint)
                {
                    Console.WriteLine("The drive letter has been used.");
                    return 1;
                }
            }
            var rootFolder = args.Skip(2).FirstOrDefault();
            file = file.Replace('/', '\\');
            rootFolder = !string.IsNullOrWhiteSpace(rootFolder) ? rootFolder.Replace('/', '\\')
                : opts.Contains(" -A") ? ":auto" : null;
            args = null;

            if (file.Length > 3 && file[0] == '\\' && file[1] != '\\' && !File.Exists(file))
            {
                var prefix = new[] { @"\cygdrive\", @"\mnt\", @"\" }.First(i => file.StartsWith(i));
                var file2 = file.Length > prefix.Length + 2 ? file.Substring(prefix.Length, 2).ToUpper() : "  ";
                if (file2[1] == '\\' && file2[0] >= 'A' && file2[0] <= 'Z')
                {
                    file = file2[0] + @":\" + file.Substring(prefix.Length + 2);
                    Console.WriteLine("Select an archive in {0}", file);
                }
            }
            DebugDokan = opts.Contains("-D");
            if (extractIntOption("-d", out _debugSelf) && DebugSelf == 1 && DebugDokan)
                DebugSelf = 9;
            VerboseOutput = DebugSelf > 0 || opts.Contains("-v");

            SevenZipFs fs;
            try
            {
                fs = new SevenZipFs(extractorSwitches);
                if (file != null)
                    fs.LoadOneZip(file);
            }
            catch (Exception ex)
            {
                Console.Out.Flush();
                if (ex is SevenZipException || ex is FileNotFoundException)
                    Console.Error.WriteLine("Error: {0}", ex.Message);
                else
                    Console.Error.WriteLine("Error: {0}", ex.ToString());
                return 2;
            }
            var kProcessors = Environment.ProcessorCount;
            if (!string.IsNullOrEmpty(rootFolder) && !fs.SetRoot(rootFolder))
            {
                Console.Error.WriteLine("Error: Root folder is not found in archive!");
                return 4;
            }
            var firstEncrypted = _hasReadPW ? null : fs.FindFirstEncrypted();
            if (!_hasReadPW && firstEncrypted != null && !firstEncrypted.Extractor.TryDecrypt(firstEncrypted))
            {
                    Console.Out.Flush();
                    Console.Error.WriteLine("Error: Password is wrong!");
                    return 3;
            }
            if (firstEncrypted == null && !string.IsNullOrEmpty(_password) && !_hasReadPW)
                Console.WriteLine("Warning: archive is not encrypted!");
            firstEncrypted = null;
            if (!_hasReadPW)
                fs.TryDecompress();
            Console.WriteLine("  Has loaded {0}", file);
            if (rootFolder != null && rootFolder == ":auto" && fs.RootFolder != null)
            {
                Console.WriteLine("  Auto use \"{0}\" as root", fs.RootFolder);
            }
            if (opts.Contains("-e")) // test extraction only
            {
                Console.WriteLine("  Test passed.");
                return 0;
            }

            int maxTasks = 0;
            if (string.IsNullOrEmpty(parallelNum))
                int.TryParse(parallelNum, out maxTasks);
            ThreadPool.GetMaxThreads(out int oldWorkerThreads, out int oldThreads2); ;
            ThreadPool.SetMaxThreads(maxTasks > 0 && maxTasks < kProcessors ? maxTasks
                : parallelIndex >= 0 ? kProcessors : Math.Min(4, kProcessors), oldThreads2);

            bool doesOpen = opts.Contains("-o");
            bool testOnly = opts.Contains(" -t") || opts.Contains(" --dry");
            if (!string.IsNullOrWhiteSpace(labelName))
                fs.VolumeLabel = labelName.Trim();
            else
            {
                var label = fs.VolumeLabel = Path.GetFileNameWithoutExtension(file).Trim();
                var root = fs.RootFolder;
                if (!string.IsNullOrEmpty(root) && root.StartsWith("/" + label))
                    root = "/." + root.Substring(label.Length + 1);
                label = !string.IsNullOrEmpty(label) ? !string.IsNullOrEmpty(root) ? $"{label} ({root})" : label : root;
                fs.VolumeLabel = label;
            }
            fs.OnMount = (drive) =>
            {
                if (drive != null)
                {
                    Console.WriteLine("  Has mounted as {0} .", drive.EndsWith("\\") ? drive : drive + "\\");
                    if (testOnly)
                    {
                        Console.WriteLine("  Test passed.");
                        Environment.Exit(0);
                        return;
                    }
                    if (doesOpen)
                        Process.Start(drive.EndsWith("\\") ? drive : drive + "\\");
                }
                else
                    Console.In.Close();
            };
            try
            {
                using (var dokan = new Dokan(new NullLogger()))
                {
                    var builder = new DokanInstanceBuilder(dokan);
                    builder.ConfigureOptions(options =>
                    {
                        options.MountPoint = isDrive ? mountPoint.ToUpper() + ":" : mountPoint;
                        options.Options = DokanOptions.CurrentSession | DokanOptions.WriteProtection
                            | DokanOptions.CaseSensitive
                            | DokanOptions.MountManager | DokanOptions.RemovableDrive;
                        if (DebugDokan)
                            options.Options |= DokanOptions.DebugMode | DokanOptions.StderrOutput;
                    });
                    using (var instance = builder.Build(fs))
                    {
                        builder = null;
                        opts = null;
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false, true);
                        while (Console.ReadLine() != null) { }
                        fs.OnMount = null;
                    }
                }
            }
            catch (DokanException ex)
            {
                Console.Out.Flush();
                Console.Error.Flush();
                Console.Error.WriteLine("Try to mount into {0}, but:"
                    , isDrive ? mountPoint.ToUpper() + ":\\" : mountPoint);
                Console.Error.WriteLine(ex.ToString());
                return 9;
            }
            return 0;
        }

        public static string InputPassword()
        {
            bool hasRead = _hasReadPW;
            _hasReadPW = true;
            if (hasRead || !string.IsNullOrEmpty(_password)) { return _password; }
            Console.Write("Please type archive password: ");
            Console.Out.Flush();
            _password = "";
            try
            {
                // from https://stackoverflow.com/a/3404522/5789722
                var pass = "";
                ConsoleKey key;
                do
                {
                    var keyInfo = Console.ReadKey(intercept: true);
                    key = keyInfo.Key;

                    if (key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        Console.Write("\b \b");
                        pass = pass.Substring(0, pass.Length - 1);
                    }
                    else if (!char.IsControl(keyInfo.KeyChar))
                    {
                        Console.Write("*");
                        pass += keyInfo.KeyChar;
                    }
                } while (key != ConsoleKey.Enter);
                Console.WriteLine("");
                Interlocked.MemoryBarrier();
                _password = pass;
            }
            catch (Exception)
            {
                Environment.Exit(0);
            }
            return _password;
        }
    }
}
