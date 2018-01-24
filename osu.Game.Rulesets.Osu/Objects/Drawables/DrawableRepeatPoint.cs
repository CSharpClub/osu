﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableRepeatPoint : DrawableOsuHitObject, ITrackSnaking
    {
        private readonly RepeatPoint repeatPoint;
        private readonly DrawableSlider drawableSlider;
        private bool isEndRepeat => repeatPoint.RepeatIndex % 2 == 0;

        public double FadeInTime;
        public double FadeOutTime;

        public DrawableRepeatPoint(RepeatPoint repeatPoint, DrawableSlider drawableSlider)
            : base(repeatPoint)
        {
            this.repeatPoint = repeatPoint;
            this.drawableSlider = drawableSlider;

            Size = new Vector2(45 * repeatPoint.Scale);

            Blending = BlendingMode.Additive;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                new SpriteIcon
                {
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.fa_chevron_right
                }
            };
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (repeatPoint.StartTime <= Time.Current)
                AddJudgement(new OsuJudgement { Result = drawableSlider.Tracking ? HitResult.Great : HitResult.Miss });
        }

        protected override void UpdatePreemptState()
        {
            var animIn = Math.Min(150, repeatPoint.StartTime - FadeInTime);

            this.FadeIn(animIn).ScaleTo(1.2f, animIn)
                .Then()
                .ScaleTo(1, 150, Easing.Out);
        }

        protected override void UpdateCurrentState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Idle:
                    this.Delay(FadeOutTime - repeatPoint.StartTime).FadeOut();
                    break;
                case ArmedState.Miss:
                    this.FadeOut(160);
                    break;
                case ArmedState.Hit:
                    this.FadeOut(120, Easing.OutQuint)
                        .ScaleTo(Scale * 1.5f, 120, Easing.OutQuint);
                    break;
            }
        }

        public void UpdateSnakingPosition(Vector2 start, Vector2 end)
        {
            Position = isEndRepeat ? end : start;
            var curve = drawableSlider.CurrentCurve;
            if (curve.Count < 3 || curve.All(p => p == Position))
                return;
            var referencePoint = curve[isEndRepeat ? curve.IndexOf(Position, curve.Count - 2) - 1 : curve[0] == curve[1] ? 2 : 1];
            Rotation = MathHelper.RadiansToDegrees((float)Math.Atan2(referencePoint.Y - Position.Y, referencePoint.X - Position.X));
        }
    }
}
