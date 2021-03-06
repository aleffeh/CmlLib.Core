﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CmlLib.Core;
using CmlLib.Utils;

namespace CmlLibCoreSample
{
    class Program
    {
        static void Main()
        {
            var p = new Program();

            var session = p.Login();
            p.Start(session);
        }

        MSession Login()
        {
            bool premiumMode = false;
            MSession session;

            if (premiumMode)
            {
                Console.WriteLine("Try Auto login");

                var login = new MLogin();

                session = login.TryAutoLogin();
                if (session.Result != MLoginResult.Success)
                {
                    Console.WriteLine("Auto login failed : {0}", session.Result.ToString());

                    Console.WriteLine("Input mojang email : ");
                    var email = Console.ReadLine();
                    Console.WriteLine("Input mojang password : ");
                    var pw = Console.ReadLine();

                    session = login.Authenticate(email, pw);

                    if (session.Result != MLoginResult.Success)
                    {
                        Console.WriteLine("failed to login. {0} : {1}", session.Result.ToString(), session.Message);
                        Console.ReadLine();
                        return null;
                    }
                }
            }
            else
                session = MSession.GetOfflineSession("tester123");

            Console.WriteLine("Success to login : {0} / {1} / {2}", session.Username, session.UUID, session.AccessToken);
            return session;
        }

        void Start(MSession session)
        {
            // initializing launcher
            var path = Minecraft.GetOSDefaultPath(); // mc directory
            //var path = @"./mctestdir";

            var launcher = new CmlLib.CMLauncher(path);
            launcher.ProgressChanged += Downloader_ChangeProgress;
            launcher.FileChanged += Downloader_ChangeFile;

            Console.WriteLine($"Initialized in {launcher.Minecraft.path}");

            launcher.UpdateProfileInfos();

            foreach (var item in launcher.ProfileInfos)
            {
                Console.WriteLine(item.Name);
            }

            var launchOption = new MLaunchOption
            {
                MaximumRamMb = 1024,
                Session = session,
            };

            // launch forge
            // var process = launcher.CreateProcess("1.12.2", "14.23.5.2768", launchOption);

            // launch vanila
            // var process = launcher.CreateProcess("1.15.2", launchOption);

            // launch by user input
            Console.WriteLine("input version : ");
            var process = launcher.CreateProcess(Console.ReadLine(), launchOption);

            Console.WriteLine(process.StartInfo.Arguments);
            process.Start();

            Console.WriteLine("Started");
            Console.ReadLine();

            return;
        }

        // Event

        int nextline = -1;

        private void Downloader_ChangeProgress(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if (nextline < 0)
                return;

            Console.SetCursorPosition(0, nextline);
            Console.WriteLine("{0}%", e.ProgressPercentage);
        }

        private void Downloader_ChangeFile(DownloadFileChangedEventArgs e)
        {
            if (e.FileKind == MFile.Resource)
            {
                Console.WriteLine("[Resource] {0} - {1} / {2}", e.FileName, e.ProgressedFileCount, e.TotalFileCount);

                if (e.ProgressedFileCount < e.TotalFileCount)
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
            else
            {
                Console.WriteLine("[{0}] {1} - {2}/{3}", e.FileKind.ToString(), e.FileName, e.ProgressedFileCount, e.TotalFileCount);
                nextline = Console.CursorTop;
            }
        }
    }
}
