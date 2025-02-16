// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using PpTournamentRefTools.Components;
using PpTournamentRefTools.Components.TextBoxes;

namespace PpTournamentRefTools
{
    public partial class MainScreen : CompositeDrawable
    {
        private RoundedButton calculationButton = null!;
        private VerboseLoadingLayer loadingLayer = null!;

        private Container matchTitleCardContainer = null!;
        private FillFlowContainer<MatchEventDisplay> events = null!;

        private LabelledTextBox matchLinkTextBox = null!;

        private int? currentMatch;

        private CancellationTokenSource? calculationCancellatonToken;

        [Resolved]
        private NotificationDisplay notificationDisplay { get; set; } = null!;

        [Resolved]
        private APIManager apiManager { get; set; } = null!;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Red);

        private const float top_bar_height = 40;

        public MainScreen()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new ScalingContainer(ScalingMode.Everything)
                    {
                        Depth = 1,
                        Children = new Drawable[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ColumnDimensions = new[] { new Dimension() },
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.Absolute, top_bar_height),
                                    new Dimension(GridSizeMode.AutoSize),
                                    new Dimension()
                                },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        new GridContainer
                                        {
                                            Name = "Settings",
                                            Height = top_bar_height,
                                            RelativeSizeAxes = Axes.X,
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(),
                                                new Dimension(GridSizeMode.AutoSize),
                                                new Dimension(GridSizeMode.AutoSize)
                                            },
                                            RowDimensions = new[]
                                            {
                                                new Dimension(GridSizeMode.AutoSize)
                                            },
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    matchLinkTextBox = new ExtendedLabelledTextBox
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        Anchor = Anchor.TopLeft,
                                                        Label = "Match",
                                                        PlaceholderText = "https://osu.ppy.sh/community/matches/123727",
                                                        CommitOnFocusLoss = false
                                                    },
                                                    calculationButton = new RoundedButton()
                                                    {
                                                        Width = 100,
                                                        Height = top_bar_height,
                                                        Text = "Calculate",
                                                        Action = calculateOrUpdateMatch
                                                    },
                                                    new SettingsButton()
                                                }
                                            }
                                        },
                                    },
                                    new Drawable[]
                                    {
                                        matchTitleCardContainer = new Container()
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y
                                        }
                                    },
                                    new Drawable[]
                                    {
                                        new OsuScrollContainer(Direction.Vertical)
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Child = events = new FillFlowContainer<MatchEventDisplay>
                                            {
                                                Padding = new MarginPadding() { Horizontal = 15, Vertical = 5 },
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Direction = FillDirection.Vertical,
                                                Spacing = new Vector2(0, 5),
                                            }
                                        }
                                    },
                                }
                            },
                            loadingLayer = new VerboseLoadingLayer(true)
                            {
                                RelativeSizeAxes = Axes.Both
                            }
                        }
                    }
                }
            };

            matchLinkTextBox.OnCommit += (_, _) => { calculateMatch(matchLinkTextBox.Current.Value); };
            matchLinkTextBox.Current.ValueChanged += _ => { calculationButton.Text = "Calculate"; };
        }

        private void calculateOrUpdateMatch()
        {
            int? textboxMatchId = parseMatchId(matchLinkTextBox.Current.Value);

            if (textboxMatchId == currentMatch)
            {
                loadingLayer.Show();

                var match = apiManager.GetJsonFromApi<APIMatch>($"matches/{currentMatch}").GetResultSafely();

                foreach (var potentiallyUpdatedMatch in match.Events.Where(x => events.Any(e => e.Event.Id == x.Id)))
                {
                    var existingMatch = events.First(x => x.Event.Id == potentiallyUpdatedMatch.Id);
                    if (existingMatch.Event.Game?.EndTime != potentiallyUpdatedMatch.Game?.EndTime)
                        events.Remove(existingMatch, true);
                }

                foreach (var newMatchEvent in match.Events.Where(x => !events.Any(e => e.Event.Id == x.Id)))
                {
                    if (newMatchEvent.UserId != null && !UserCache.Contains(newMatchEvent.UserId.Value))
                    {
                        var user = apiManager.GetJsonFromApi<APIUser>($"users/{newMatchEvent.UserId}").GetResultSafely();

                        UserCache.Add(user);
                    }

                    events.Add(new MatchEventDisplay(newMatchEvent, match.Match));
                }

                loadingLayer.Hide();
            }
            else
            {
                calculateMatch(matchLinkTextBox.Current.Value);
            }
        }

        private void calculateMatch(string? matchLink)
        {
            if (string.IsNullOrEmpty(matchLink))
            {
                matchLinkTextBox.FlashColour(Color4.Red, 1);
                return;
            }

            currentMatch = parseMatchId(matchLink);

            if (currentMatch == null)
            {
                matchLinkTextBox.FlashColour(Color4.Red, 1);
                return;
            }

            calculationCancellatonToken?.Cancel();
            calculationCancellatonToken?.Dispose();

            loadingLayer.Show();

            matchTitleCardContainer.Clear();
            events.Clear();

            calculationCancellatonToken = new CancellationTokenSource();
            var token = calculationCancellatonToken.Token;

            Task.Run(async () =>
            {
                Schedule(() => loadingLayer.Text.Value = "Getting match data...");

                var match = await apiManager.GetJsonFromApi<APIMatch>($"matches/{currentMatch}").ConfigureAwait(false);

                Schedule(() => matchTitleCardContainer.Add(new MatchTitleCard(match)));

                if (token.IsCancellationRequested)
                    return;

                Schedule(() => loadingLayer.Text.Value = "Processing events...");

                foreach (var matchEvent in match.Events)
                {
                    if (matchEvent.UserId != null && !UserCache.Contains(matchEvent.UserId.Value))
                    {
                        var user = await apiManager.GetJsonFromApi<APIUser>($"users/{matchEvent.UserId}").ConfigureAwait(false);

                        UserCache.Add(user);
                    }

                    Schedule(() => events.Add(new MatchEventDisplay(matchEvent, match.Match)));

                    if (token.IsCancellationRequested)
                        return;
                }
            }, token).ContinueWith(t =>
            {
                Logger.Log(t.Exception?.ToString(), level: LogLevel.Error);
                notificationDisplay.Display(new Notification(t.Exception?.Flatten().Message ?? "Unknown Error"));
            }, TaskContinuationOptions.OnlyOnFaulted).ContinueWith(t =>
            {
                Schedule(() =>
                {
                    calculationButton.Text = "Update";
                    loadingLayer.Hide();
                });
            }, TaskContinuationOptions.None);
        }

        private int? parseMatchId(string matchLink)
        {
            if (!int.TryParse(matchLink.Split('/').Last(), out int matchId))
            {
                return null;
            }

            return matchId;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            calculationCancellatonToken?.Cancel();
            calculationCancellatonToken?.Dispose();
            calculationCancellatonToken = null;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Key == Key.Escape && calculationCancellatonToken != null && !calculationCancellatonToken.IsCancellationRequested)
            {
                calculationCancellatonToken?.Cancel();
            }

            return base.OnKeyDown(e);
        }
    }
}
