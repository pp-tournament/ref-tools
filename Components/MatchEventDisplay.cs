// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Play.HUD;
using osuTK;
using PpTournamentRefTools.Configuration;

namespace PpTournamentRefTools.Components
{
    public partial class MatchEventDisplay : Container
    {
        public readonly APIMatch.MatchEvent Event;
        private readonly APIMatch.MatchMetadata metadata;

        private Container layout = null!;

        [Resolved]
        private SettingsManager configManager { get; set; } = null!;

        [Resolved]
        private Bindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private LargeTextureStore textures { get; set; } = null!;

        public MatchEventDisplay(APIMatch.MatchEvent matchEvent, APIMatch.MatchMetadata metadata)
        {
            Event = matchEvent;
            this.metadata = metadata;

            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 10;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colour)
        {
            Children = new Drawable[]
            {
                new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5
                },
                layout = new Container
                {
                    Origin = Anchor.TopRight,
                    Anchor = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(10),
                }
            };

            if (Event.Detail.Type != APIMatch.MatchEvent.MatchEventDetail.MatchEventDetailType.Other)
            {
                layout.Add(new OsuSpriteText
                {
                    Text = Event.UserId != null ? $"{Event.Detail.Type} - {UserCache.Get(Event.UserId.Value)?.Username}" : Event.Detail.Type.ToString(),
                    Font = OsuFont.Default.With(size: 14)
                });
            }

            if (Event.Game != null)
            {
                var game = Event.Game!;
                var rulesetInstance = ruleset.Value.CreateInstance();

                var gameLayout = new FillFlowContainer()
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(5),
                    Margin = new MarginPadding() { Bottom = 20 }
                };

                gameLayout.Add(new Container()
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Masking = true,
                    CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background6
                        },
                        new BufferedContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.4f,
                            Children = new Drawable[]
                            {
                                new Sprite
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Texture = textures.Get($"https://assets.ppy.sh/beatmaps/{game.Beatmap.BeatmapSet!.OnlineID}/covers/cover.jpg"),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    FillMode = FillMode.Fill
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding() { Horizontal = 10 },
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Margin = new MarginPadding() { Vertical = 20 },
                                    Text = game.Beatmap.GetDisplayTitle(),
                                    Font = OsuFont.Default.With(size: 18, weight: FontWeight.SemiBold)
                                },
                                new ModDisplay()
                                {
                                    Origin = Anchor.CentreRight,
                                    Anchor = Anchor.CentreRight,
                                    Current = { Value = rulesetInstance.AllMods.Where(x => Event.Game.Mods.Contains(x.Acronym)).Select(x => x.CreateInstance()).ToArray() }
                                }
                            }
                        }
                    }
                });

                if (game.EndTime != null)
                {
                    double blueTeamTotal = 0;
                    double redTeamTotal = 0;

                    foreach (var score in game.Scores)
                    {
                        var working = ProcessorWorkingBeatmap.FromFileOrId(game.Beatmap.OnlineID.ToString(), cachePath: configManager.GetBindable<string>(Settings.CachePath).Value);

                        var mods = rulesetInstance.AllMods.Where(x => score.Mods.Contains(x.Acronym)).Select(x => x.CreateInstance()).ToList();

                        // ALWAYS append CL since we're doing a stable tournament
                        mods.Add(new OsuModClassic());

                        var scoreInfo = score.ToScoreInfo(mods.ToArray(), working.BeatmapInfo);

                        var parsedScore = new ProcessorScoreDecoder(working).Parse(scoreInfo);

                        var difficultyCalculator = rulesetInstance.CreateDifficultyCalculator(working);
                        var difficultyAttributes = difficultyCalculator.Calculate(mods);
                        var performanceCalculator = rulesetInstance.CreatePerformanceCalculator();
                        if (performanceCalculator == null)
                            return;

                        var perfAttributes = performanceCalculator.Calculate(parsedScore.ScoreInfo, difficultyAttributes);

                        var scoreDisplay = new MatchScore(score, perfAttributes.Total);
                        gameLayout.Add(scoreDisplay);

                        if (score.Match.Team == "red")
                        {
                            redTeamTotal += perfAttributes.Total;
                        }
                        else
                        {
                            blueTeamTotal += perfAttributes.Total;
                        }
                    }

                    bool blueTeamWins = blueTeamTotal > redTeamTotal;

                    gameLayout.AddRange(new Drawable[]
                    {
                        new Container()
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding() { Horizontal = 100, Vertical = 10 },
                            Children = new Drawable[]
                            {
                                new FillFlowContainer()
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText()
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Colour = colour.Blue,
                                            Text = metadata.BlueTeam ?? "Blue",
                                            Font = OsuFont.Default.With(size: 20, weight: blueTeamWins ? FontWeight.Bold : null)
                                        },
                                        new OsuSpriteText()
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Colour = colour.Blue,
                                            Text = $"{blueTeamTotal:N2}pp",
                                            Font = OsuFont.Default.With(size: 25, weight: blueTeamWins ? FontWeight.Bold : null)
                                        },
                                    }
                                },
                                new FillFlowContainer()
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText()
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Colour = colour.Red,
                                            Text = metadata.RedTeam ?? "Red",
                                            Font = OsuFont.Default.With(size: 20, weight: !blueTeamWins ? FontWeight.Bold : null)
                                        },
                                        new OsuSpriteText()
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Colour = colour.Red,
                                            Text = $"{redTeamTotal:N2}pp",
                                            Font = OsuFont.Default.With(size: 25, weight: !blueTeamWins ? FontWeight.Bold : null)
                                        },
                                    }
                                }
                            }
                        },
                        new FormTextBox()
                        {
                            RelativeSizeAxes = Axes.X,
                            Caption = "Message",
                            Current = { Value = formatWinningTeamMessage(blueTeamWins, blueTeamTotal, redTeamTotal) }
                        }
                    });
                }
                else
                {
                    gameLayout.Add(new Container()
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding() { Top = 20 },
                        Child = new LoadingSpinner()
                        {
                            Size = new Vector2(30),
                            State = { Value = Visibility.Visible }
                        }
                    });
                }

                layout.Add(gameLayout);
            }

            layout.Add(new DrawableDate(Event.Timestamp)
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Alpha = 0.5f,
                Font = OsuFont.Default.With(size: 14)
            });
        }

        private string formatWinningTeamMessage(bool blueTeamWins, double blueTeamTotal, double redTeamTotal)
        {
            if (blueTeamWins)
                return winningTeamMessageTemplate(metadata.BlueTeam ?? "Blue", blueTeamTotal, metadata.RedTeam ?? "Red", redTeamTotal);

            return winningTeamMessageTemplate(metadata.RedTeam ?? "Red", redTeamTotal, metadata.BlueTeam ?? "Blue", blueTeamTotal);
        }

        private string winningTeamMessageTemplate(string winningTeam, double winningTotal, string losingTeam, double losingTotal) =>
            $"{winningTeam} ({winningTotal:N2}pp) : {losingTeam} ({losingTotal:N2}pp) - {winningTeam} wins!";
    }
}
