﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Overlays.News.Sidebar;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests.Responses;
using NUnit.Framework;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneNewsYearsPanel : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Cached]
        private readonly Bindable<APINewsSidebar> metadataBindable = new Bindable<APINewsSidebar>();

        private YearsPanel panel;

        [SetUp]
        public void SetUp() => Schedule(() => Child = panel = new YearsPanel
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre
        });

        [Test]
        public void TestVisibility()
        {
            AddStep("Change metadata to null", () => metadataBindable.Value = null);
            AddUntilStep("Panel is hidden", () => panel?.Alpha == 0);
            AddStep("Change metadata", () => metadataBindable.Value = metadata);
            AddUntilStep("Panel is visible", () => panel?.Alpha == 1);
            AddStep("Change metadata to null", () => metadataBindable.Value = null);
            AddUntilStep("Panel is hidden", () => panel?.Alpha == 0);
        }

        private static readonly APINewsSidebar metadata = new APINewsSidebar
        {
            Years = new[] { 1001, 2001, 3001, 4001, 5001, 6001, 7001, 8001, 9001 }
        };
    }
}
