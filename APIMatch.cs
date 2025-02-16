// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace PpTournamentRefTools
{
    public partial class APIMatch
    {
        public MatchMetadata Match { get; set; } = null!;
        public List<MatchEvent> Events { get; set; } = null!;

        public partial class MatchMetadata
        {
            public long Id { get; set; }
            public string Name { get; set; } = null!;
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

            [GeneratedRegex(@".+\((?'blue'.+)\) vs \((?'red'.+)\)")]
            private static partial Regex teamNameRegex();

            public string? BlueTeam => teamNameRegex().Match(Name).Groups.GetValueOrDefault("blue")?.Value;
            public string? RedTeam => teamNameRegex().Match(Name).Groups.GetValueOrDefault("red")?.Value;
        }

        public class MatchEvent
        {
            public long Id { get; set; }
            public MatchEventDetail Detail { get; set; } = null!;
            public DateTime Timestamp { get; set; }

            [JsonProperty("user_id")]
            public int? UserId { get; set; }

            public MatchGame? Game { get; set; }

            public class MatchEventDetail
            {
                public MatchEventDetailType Type { get; set; }
                public string Text { get; set; } = null!;

                public enum MatchEventDetailType
                {
                    [EnumMember(Value = "host-changed")]
                    HostChanged,

                    [EnumMember(Value = "match-created")]
                    MatchCreated,

                    [EnumMember(Value = "match-disbanded")]
                    MatchDisbanded,

                    Other,

                    [EnumMember(Value = "player-joined")]
                    PlayerJoined,

                    [EnumMember(Value = "player-kicked")]
                    PlayerKicked,

                    [EnumMember(Value = "player-left")]
                    PlayerLeft
                }
            }

            public class MatchGame
            {
                public long Id { get; set; }
                public APIBeatmap Beatmap { get; set; } = null!;
                public MatchScore[] Scores { get; set; } = Array.Empty<MatchScore>();

                [JsonProperty("end_time")]
                public DateTime? EndTime { get; set; }

                public string[] Mods { get; set; } = Array.Empty<string>();

                public class MatchScore
                {
                    [JsonProperty("passed")]
                    public bool Passed { get; set; }

                    [JsonProperty("score")]
                    public long TotalScore { get; set; }

                    [JsonProperty("accuracy")]
                    public double Accuracy { get; set; }

                    [JsonProperty("user_id")]
                    public int UserID { get; set; }

                    [JsonProperty("max_combo")]
                    public int MaxCombo { get; set; }

                    [JsonConverter(typeof(StringEnumConverter))]
                    [JsonProperty("rank", DefaultValueHandling = DefaultValueHandling.Include)]
                    public ScoreRank Rank { get; set; }

                    [JsonProperty("mods")]
                    public string[] Mods { get; set; } = Array.Empty<string>();

                    [JsonProperty("statistics")]
                    public MatchScoreStatistics Statistics { get; set; } = null!;

                    [JsonProperty("id")]
                    public long? ID { get; set; }

                    [JsonProperty("created_at")]
                    public DateTimeOffset CreatedAt { get; set; }

                    public MatchScoreData Match { get; set; } = null!;

                    public class MatchScoreStatistics
                    {
                        [JsonProperty("count_300")]
                        public int Count300 { get; set; }

                        [JsonProperty("count_100")]
                        public int Count100 { get; set; }

                        [JsonProperty("count_50")]
                        public int Count50 { get; set; }

                        [JsonProperty("count_miss")]
                        public int CountMiss { get; set; }
                    }

                    public class MatchScoreData
                    {
                        public int Slot { get; set; }
                        public string Team { get; set; } = null!;
                        public bool Pass { get; set; }
                    }

                    public ScoreInfo ToScoreInfo(Mod[] mods, BeatmapInfo beatmap)
                    {
                        var score = new ScoreInfo
                        {
                            OnlineID = ID ?? 0,
                            LegacyOnlineID = -1,
                            IsLegacyScore = true,
                            User = new APIUser { Id = UserID },
                            BeatmapInfo = beatmap,
                            Ruleset = new OsuRuleset().RulesetInfo,
                            Passed = Passed,
                            TotalScore = TotalScore,
                            TotalScoreWithoutMods = 0,
                            LegacyTotalScore = 0,
                            Accuracy = Accuracy,
                            MaxCombo = MaxCombo,
                            Rank = Rank,
                            Statistics = new Dictionary<HitResult, int>
                            {
                                { HitResult.Great, Statistics.Count300 },
                                { HitResult.Ok, Statistics.Count100 },
                                { HitResult.Meh, Statistics.Count50 },
                                { HitResult.Miss, Statistics.CountMiss },
                            },
                            MaximumStatistics = new Dictionary<HitResult, int>(),
                            Date = CreatedAt,
                            HasOnlineReplay = false,
                            Mods = mods,
                            PP = null,
                            Ranked = true,
                        };

                        score.BeatmapInfo.Ruleset.OnlineID = beatmap.Ruleset.OnlineID;
                        score.BeatmapInfo.Ruleset.Name = beatmap.Ruleset.Name;
                        score.BeatmapInfo.Ruleset.ShortName = beatmap.Ruleset.ShortName;

                        return score;
                    }
                }
            }
        }
    }
}
