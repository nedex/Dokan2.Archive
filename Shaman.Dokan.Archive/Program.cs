﻿using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using SevenZip;
using System.IO;
using DokanNet;

namespace Shaman.Dokan
{
    class SevenZipProgram
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ArchiveFs.exe <archive-file> Drive: [-p [password]]");
            }
            var passwordIndex = Array.FindIndex(args, i => i == "-p");
            string _password = null;
            if (passwordIndex >= 0)
            {
                if (passwordIndex < args.Length - 1)
                {
                    _password = args[passwordIndex + 1];
                    args = args.Take(passwordIndex).Concat(args.Skip(passwordIndex + 2)).ToArray();
                }
                else
                    _password = "";
            }
            var opts = " " + string.Join(" ", args.Where(i => i[0] == '-'));
            args = args.Where(i => i[0] != '-').ToArray();
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
            if (isDrive && Directory.Exists(mountPoint + ":\\"))
            {
                Console.WriteLine("The drive letter has been used.");
                return 1;
            }
            if (string.IsNullOrEmpty(mountPoint))
            {
                mountPoint = "X";
                isDrive = true;
            }
            args = null;

            SevenZipFs fs;
            try
            {
                fs = new SevenZipFs(file, _password);
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
            if (fs.Encrypted && _lastPass == null)
            {
                if (_password == null)
                {
                    _password = InputPassword();
                    if (_password == null) return 0;
                }
                if (!fs.extractor.TrySetPassword(_password))
                {
                    Console.Out.Flush();
                    Console.Error.WriteLine("Error: Password is wrong!");
                    return 3;
                }
            }
            else if (_lastPass == null && passwordIndex >= 0)
                Console.WriteLine("Warning: archive is not encrypted!");
            _password = null;
            Console.WriteLine("Has loaded {0}", file);

            fs.OnMount = (drive) =>
            {
                if (drive != null)
                    Console.WriteLine("Has mounted as {0} .", drive.EndsWith("\\") ? drive : drive + "\\");
                else
                    Console.In.Close();
            };
            try
            {
                using (var dokan = new DokanNet.Dokan(new DokanNet.Logging.NullLogger()))
                {
                    var builder = new DokanNet.DokanInstanceBuilder(dokan);
                    builder.ConfigureOptions(options =>
                    {
                        options.MountPoint = isDrive ? mountPoint.ToUpper() + ":" : mountPoint;
                        options.Options = DokanOptions.CurrentSession | DokanOptions.WriteProtection
                            | DokanOptions.CaseSensitive
                            | DokanOptions.MountManager | DokanOptions.RemovableDrive;
                        if (opts.Contains(" -v") || opts.Contains(" -d"))
                            options.Options |= DokanOptions.DebugMode | DokanOptions.StderrOutput;
                    });
                    using (var instance = builder.Build(fs))
                    {
                        if (opts.Contains(" -o") || opts.Contains(" --open"))
                            Process.Start(isDrive ? mountPoint.ToUpper() + ":\\" : mountPoint);
                        builder = null;
                        opts = null;
                        while (Console.ReadLine() != null) { }
                        fs.OnMount = null;
                    }
                }
            }
            catch (DokanException ex)
            {
                Console.Out.Flush();
                Console.Error.Flush();
                Console.Error.WriteLine(ex.ToString());
                return 9;
            }
            return 0;
        }

        private static string _lastPass = null;
        public static string InputPassword()
        {
            if (_lastPass != null) { return _lastPass; }
            Console.Write("Please type archive password: ");
            Console.Out.Flush();
            _lastPass = "";
            try { return _lastPass = Console.ReadLine(); }
            catch (Exception) { return null; }
        }
    }
}
