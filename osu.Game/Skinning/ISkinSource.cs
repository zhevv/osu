﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Types;
using OpenTK.Graphics;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Provides access to skinnable elements.
    /// </summary>
    public interface ISkinSource
    {
        event Action SourceChanged;

        Drawable GetDrawableComponent(string componentName);

        SampleChannel GetSample(string sampleName);

        Color4? GetComboColour(IHasComboIndex comboObject);
    }
}
