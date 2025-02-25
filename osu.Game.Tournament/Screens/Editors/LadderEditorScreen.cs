// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tournament.Components;
using osu.Game.Overlays;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Editors.Components;
using osu.Game.Tournament.Screens.Ladder;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Editors
{
    [Cached]
    public partial class LadderEditorScreen : LadderScreen, IHasContextMenu
    {
        public const float GRID_SPACING = 10;

        [Cached]
        private LadderEditorInfo editorInfo = new LadderEditorInfo();

        private WarningBox rightClickMessage;

        [Resolved(canBeNull: true)]
        [CanBeNull]
        private IDialogOverlay dialogOverlay { get; set; }

        protected override bool DrawLoserPaths => true;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new ControlPanel
            {
                Child = new LadderEditorSettings(),
            });

            AddInternal(rightClickMessage = new WarningBox("Right click to place and link matches"));

            ScrollContent.Add(new RectangularPositionSnapGrid(Vector2.Zero)
            {
                Spacing = new Vector2(GRID_SPACING),
                RelativeSizeAxes = Axes.Both,
                Depth = float.MaxValue
            });

            LadderInfo.Matches.CollectionChanged += (_, _) => updateMessage();
            updateMessage();
        }

        private void updateMessage()
        {
            rightClickMessage.Alpha = LadderInfo.Matches.Count > 0 ? 0 : 1;
        }

        public void BeginJoin(TournamentMatch match, bool losers)
        {
            ScrollContent.Add(new JoinVisualiser(MatchesContainer, match, losers, UpdateLayout));
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (editorInfo == null)
                    return Array.Empty<MenuItem>();

                return new MenuItem[]
                {
                    new OsuMenuItem("Create new match", MenuItemType.Highlighted, () =>
                    {
                        Vector2 pos = MatchesContainer.ToLocalSpace(GetContainingInputManager().CurrentState.Mouse.Position);
                        TournamentMatch newMatch = new TournamentMatch { Position = { Value = new Point((int)pos.X, (int)pos.Y) } };

                        LadderInfo.Matches.Add(newMatch);

                        editorInfo.Selected.Value = newMatch;
                    }),
                    new OsuMenuItem("Reset teams", MenuItemType.Destructive, () =>
                    {
                        dialogOverlay?.Push(new LadderResetTeamsDialog(() =>
                        {
                            foreach (var p in MatchesContainer)
                                p.Match.Reset();
                        }));
                    })
                };
            }
        }

        public void Remove(TournamentMatch match)
        {
            MatchesContainer.FirstOrDefault(p => p.Match == match)?.Remove();
        }

        private partial class JoinVisualiser : CompositeDrawable
        {
            private readonly Container<DrawableTournamentMatch> matchesContainer;
            public readonly TournamentMatch Source;
            private readonly bool losers;
            private readonly Action complete;

            private ProgressionPath path;

            public JoinVisualiser(Container<DrawableTournamentMatch> matchesContainer, TournamentMatch source, bool losers, Action complete)
            {
                this.matchesContainer = matchesContainer;
                RelativeSizeAxes = Axes.Both;

                Source = source;
                this.losers = losers;
                this.complete = complete;
                if (losers)
                    Source.LosersProgression.Value = null;
                else
                    Source.Progression.Value = null;
            }

            private DrawableTournamentMatch findTarget(InputState state)
            {
                return matchesContainer.FirstOrDefault(d => d.ReceivePositionalInputAt(state.Mouse.Position));
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            {
                return true;
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                var found = findTarget(e.CurrentState);

                if (found == path?.Destination)
                    return false;

                path?.Expire();
                path = null;

                if (found == null)
                    return false;

                AddInternal(path = new ProgressionPath(matchesContainer.First(c => c.Match == Source), found)
                {
                    Colour = Color4.Yellow,
                });

                return base.OnMouseMove(e);
            }

            protected override bool OnClick(ClickEvent e)
            {
                var found = findTarget(e.CurrentState);

                if (found != null)
                {
                    if (found.Match != Source)
                    {
                        if (losers)
                            Source.LosersProgression.Value = found.Match;
                        else
                            Source.Progression.Value = found.Match;
                    }

                    complete?.Invoke();
                    Expire();
                    return true;
                }

                return false;
            }
        }
    }
}
