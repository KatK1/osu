﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Screens.Select;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    [Cached]
    public partial class BeatmapCarousel : Carousel<BeatmapInfo>
    {
        private IBindableList<BeatmapSetInfo> detachedBeatmaps = null!;

        private readonly DrawablePool<BeatmapCarouselPanel> carouselPanelPool = new DrawablePool<BeatmapCarouselPanel>(100);

        public BeatmapCarousel()
        {
            DebounceDelay = 100;
            DistanceOffscreenToPreload = 100;

            Filters = new ICarouselFilter[]
            {
                new BeatmapCarouselFilterSorting(() => Criteria),
                new BeatmapCarouselFilterGrouping(() => Criteria),
            };

            AddInternal(carouselPanelPool);
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapStore beatmapStore, CancellationToken? cancellationToken)
        {
            detachedBeatmaps = beatmapStore.GetBeatmapSets(cancellationToken);
            detachedBeatmaps.BindCollectionChanged(beatmapSetsChanged, true);
        }

        protected override Drawable GetDrawableForDisplay(CarouselItem item)
        {
            var drawable = carouselPanelPool.Get();
            drawable.FlashColour(Color4.Red, 2000);

            return drawable;
        }

        protected override CarouselItem CreateCarouselItemForModel(BeatmapInfo model) => new BeatmapCarouselItem(model);

        private void beatmapSetsChanged(object? beatmaps, NotifyCollectionChangedEventArgs changed)
        {
            // TODO: moving management of BeatmapInfo tracking to BeatmapStore might be something we want to consider.
            // right now we are managing this locally which is a bit of added overhead.
            IEnumerable<BeatmapSetInfo>? newBeatmapSets = changed.NewItems?.Cast<BeatmapSetInfo>();
            IEnumerable<BeatmapSetInfo>? beatmapSetInfos = changed.OldItems?.Cast<BeatmapSetInfo>();

            switch (changed.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Items.AddRange(newBeatmapSets!.SelectMany(s => s.Beatmaps));
                    break;

                case NotifyCollectionChangedAction.Remove:

                    foreach (var set in beatmapSetInfos!)
                    {
                        foreach (var beatmap in set.Beatmaps)
                            Items.RemoveAll(i => i is BeatmapInfo bi && beatmap.Equals(bi));
                    }

                    break;

                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    break;
            }
        }

        public FilterCriteria Criteria { get; private set; } = new FilterCriteria();

        public void Filter(FilterCriteria criteria)
        {
            Criteria = criteria;
            QueueFilter();
        }
    }
}
