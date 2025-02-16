// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Platform;

namespace PpTournamentRefTools
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            using DesktopGameHost host = Host.GetSuitableDesktopHost("PpTournamentRefTools", new HostOptions
            {
                PortableInstallation = true,
                BypassCompositor = false,
                FriendlyGameName = "PP Tournament Ref Tools"
            });

            using var game = new Game();

            host.Run(game);
        }
    }
}
