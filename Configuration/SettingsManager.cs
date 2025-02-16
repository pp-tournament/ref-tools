// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Reflection;
using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace PpTournamentRefTools.Configuration
{
    public enum Settings
    {
        ClientId,
        ClientSecret,
        CachePath
    }

    public class SettingsManager : IniConfigManager<Settings>
    {
        protected override string Filename => "reftool.ini";

        public SettingsManager(Storage storage)
            : base(storage)
        {
        }

        protected override void InitialiseDefaults()
        {
            SetDefault(Settings.ClientId, string.Empty);
            SetDefault(Settings.ClientSecret, string.Empty);
            SetDefault(Settings.CachePath, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "cache"));
        }
    }
}
