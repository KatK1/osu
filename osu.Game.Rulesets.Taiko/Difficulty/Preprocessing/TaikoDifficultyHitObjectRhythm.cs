﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    public class TaikoDifficultyHitObjectRhythm
    {
        public readonly double Difficulty;
        public readonly double Ratio;
        public readonly bool IsRepeat;

        public TaikoDifficultyHitObjectRhythm(int numerator, int denominator, double difficulty, bool isRepeat)
        {
            Ratio = numerator / (double)denominator;
            Difficulty = difficulty;
            IsRepeat = isRepeat;
        }
    }
}
