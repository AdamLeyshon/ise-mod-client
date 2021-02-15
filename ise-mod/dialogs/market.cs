#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, market.cs, Created 2021-02-11

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ise.lib;
using ise_core.db;
using LiteDB;
using RimWorld;
using UnityEngine;
using Verse;
using static ise.lib.Consts;

namespace ise.dialogs
{
    public class DialogTradeUI : Window
    {
        #region Fields

#if DEBUG
        [TweakValue("ISETradeUI", 10f)]
#endif
        private static float gridMargin = 16f;

#if DEBUG
        [TweakValue("ISETradeUI", 10f)]
#endif
        private static float gridRowHeight = 30f;

#if DEBUG
        [TweakValue("ISETradeUI", 10f, 300f)]
#endif
        private static float gridRowMargin = 25f;

#if DEBUG
        [TweakValue("ISETradeUI", 10f, 300f)]
#endif
        private static float gridRowQuantityWidth = 75f;

#if DEBUG
        [TweakValue("ISETradeUI", -100f, 300f)]
#endif
        private static float gridRowPriceWidth = 100f;

#if DEBUG
        [TweakValue("ISETradeUI", -100f, 300f)]
#endif
        private static float gridRowQualityWidth = 100f;

#if DEBUG
        [TweakValue("ISETradeUI", -100f, 1000f)]
#endif
        private static float gridRowStuffWidth = 300;

#if DEBUG
        [TweakValue("ISETradeUI", -100f, 1000f)]
#endif
        private static float gridRowNameWidth = 300;

#if DEBUG
        [TweakValue("ISETradeUI", 5f)]
#endif
        private static float gridRowStart = 6f;

#if DEBUG
        [TweakValue("ISETradeUI", 1, 1000)]
#endif
        private static int gridRowQty = 50;

#if DEBUG
        [TweakValue("ISETradeUI", 5f)]
#endif
        private static float uiControlPadding = 6f;

#if DEBUG
        [TweakValue("ISETradeUI", 5f, 30f)]
#endif
        private static float uiControlVerticalSpacing = 5f;

#if DEBUG
        [TweakValue("ISETradeUI", 5f, 50f)]
#endif
        private static float uiControlHeight = 25f;

        private TradeView currentMode = TradeView.Buy;
        private Vector2 scrollPosition = Vector2.zero;
        private TradeView switchToMode = TradeView.Buy;
        private readonly LiteDatabase db;
        private List<DBCachedTradable> dataCache;
        private List<string> qualityTranslationCache;
        private string filterText = "";
        private string filterTextBuffer = "";

        #endregion

        #region ctor

        public DialogTradeUI(Pawn userPawn)
        {
            forcePause = true;
            //absorbInputAroundWindow = true;
            db = new LiteDatabase(DBLocation);
            BuildQualityTranslationCache();
        }

        #endregion

        #region Properties

        private void BuildQualityTranslationCache()
        {
            qualityTranslationCache = new List<string> {"N/A", "Uh-oh"};
            for (var quality = 2; quality < 7; quality++)
            {
                qualityTranslationCache.Add(((QualityCategory) quality).GetLabel().CapitalizeFirst());
            }
        }

        public override Vector2 InitialSize => new Vector2(1500f, UI.screenHeight - 200f);

        #endregion

        #region Methods

        public override void DoWindowContents(Rect inRect)
        {
            UpdateDataSource();

            GUI.BeginGroup(inRect);
            inRect = inRect.AtZero();
            DrawOuterFrame(inRect);
            GUI.EndGroup();
        }

        private void UpdateDataSource()
        {
            if (currentMode == switchToMode && filterText == filterTextBuffer && dataCache != null) return;

            Logging.WriteMessage("Updating data source");

            currentMode = switchToMode;
            filterText = filterTextBuffer;
            IEnumerable<DBCachedTradable> collection;

            switch (currentMode)
            {
                case TradeView.Buy:
                    collection = db.GetCollection<DBCachedTradable>("market_cache").FindAll();
                    break;
                case TradeView.Sell:
                    collection = db.GetCollection<DBCachedTradable>("colony_cache").FindAll();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!filterText.NullOrEmpty())
            {
                collection = collection.Where(x => x.TranslatedName
                    .ToLowerInvariant()
                    .Contains(filterText.ToLowerInvariant()));
            }

            dataCache = collection
                .OrderBy(x => x.TranslatedName)
                .ThenBy(x => x.TranslatedStuff)
                .ThenByDescending(x => x.Quality)
                .ToList();
        }

        private void DrawOuterFrame(Rect inRect)
        {
            var addY = uiControlHeight + uiControlHeight;

            var targetArea = new Rect(0f, 0f, inRect.width, uiControlHeight);
            DrawTopLabels(targetArea);

            targetArea = new Rect(0f, targetArea.y + addY, inRect.width, uiControlHeight);
            DrawModeTabs(targetArea);

            targetArea = new Rect(0f, targetArea.y + addY, inRect.width, uiControlHeight);
            DrawFilters(targetArea);

            targetArea = new Rect(0f, targetArea.y + addY, inRect.width, uiControlHeight);
            DrawGridHeaders(targetArea);

            // Grid takes up the remaining space
            targetArea = new Rect(0f, targetArea.y + addY, inRect.width, inRect.height - targetArea.y - addY);
            DrawTradeGrid(targetArea);
        }


        private void DrawTopLabels(Rect outerFrame)
        {
        }

        private void DrawModeTabs(Rect inRect)
        {
            // Two buttons with margin in between
            var buttonWidth = (inRect.width - (uiControlPadding * 3)) / 2f;

            GUI.BeginGroup(inRect);
            Text.Anchor = TextAnchor.MiddleCenter;

            // Buy Button
            var buttonRect = new Rect(uiControlPadding, 0, buttonWidth, 25f);
            if (Widgets.ButtonText(buttonRect, "Buy"))
            {
                Logging.WriteMessage("Changing to Buy mode");
                switchToMode = TradeView.Buy;
            }

            // Sell button
            buttonRect = new Rect(uiControlPadding * 2 + buttonWidth, 0, buttonWidth, 25f);
            if (Widgets.ButtonText(buttonRect, "Sell"))
            {
                Logging.WriteMessage("Changing to Sell mode");
                switchToMode = TradeView.Sell;
            }

            GUI.EndGroup();
        }

        private void DrawFilters(Rect outerFrame)
        {
            GUI.BeginGroup(outerFrame);
            Text.Anchor = TextAnchor.MiddleLeft;

            var nextControlY = uiControlPadding;
            // Buy Button
            var rectCategoryFilter = new Rect(nextControlY, 0, 100f, 25f);
            if (Widgets.ButtonText(rectCategoryFilter, "Category: ALL"))
            {
                // Apply category filter
            }

            nextControlY += uiControlPadding + rectCategoryFilter.width;

            // Filter Box Label
            var rectFilterLabel = new Rect(nextControlY, 0f, 100f, outerFrame.height);
            Widgets.Label(rectFilterLabel, "Filter by Name: ");

            nextControlY += uiControlPadding + rectFilterLabel.width;

            // Filter Box, takes up rest of space
            var filterRect = new Rect(
                nextControlY,
                0,
                outerFrame.width - nextControlY - uiControlPadding, // Total width of controls + all padding
                outerFrame.height
            );

            filterTextBuffer = Widgets.TextField(filterRect, filterText);

            GUI.EndGroup();
        }

        private void DrawGridHeaders(Rect outerFrame)
        {
            Text.Font = GameFont.Small;
            // Subtract the grids margin to make the headers line up with grid
            var width = outerFrame.width - gridMargin;

            GUI.BeginGroup(outerFrame);

            // Grid row is drawn RIGHT TO LEFT!

            // Subtract quantity width from X
            width -= gridRowQuantityWidth;

            // Draw controls for quantity available
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectQuantityAvailable = new Rect(width, 0f, gridRowQuantityWidth, outerFrame.height);
            Widgets.Label(rectQuantityAvailable, "Quantity");

            // Subtract price width from X
            width -= gridRowPriceWidth;

            // Set up rect and Label
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectPriceLabel = new Rect(width, 0, gridRowPriceWidth, outerFrame.height);
            Widgets.Label(rectPriceLabel, "Price");

            GUI.EndGroup();
        }


        private void DrawTradeGrid(Rect outerFrame)
        {
            Text.Font = GameFont.Small;

            var gridRowCount = dataCache.Count;

            // This is the buffer that the rows are rendered in, anything outside of the viewport above is hidden
            var gridVirtualBuffer = new Rect(0f, 0f, outerFrame.width - gridMargin, gridRowHeight * gridRowCount);

            // Render gridVirtualBuffer to rectGridViewport
            Widgets.BeginScrollView(outerFrame, ref scrollPosition, gridVirtualBuffer);
            var gridRowYPos = gridRowStart;
            var gridBottom = scrollPosition.y - 30f;
            var gridTop = scrollPosition.y + outerFrame.height;
            var rowNumber = 0;
            foreach (var cachedTradable in dataCache)
            {
                if (gridRowYPos > gridBottom && gridRowYPos < gridTop)
                {
                    // Grid Row
                    var tradableRow = new Rect(0f, gridRowYPos, gridVirtualBuffer.width, gridRowHeight);
                    DrawGridRows(tradableRow, rowNumber, cachedTradable);
                }

                rowNumber++;
                gridRowYPos += gridRowHeight;
            }

            Widgets.EndScrollView();
        }

        private void DrawGridRows(Rect tradeGridRow, int index, DBCachedTradable rowData)
        {
            if (index % 2 == 1) Widgets.DrawLightHighlight(tradeGridRow);

            // Begin group for controls in row.
            Text.Font = GameFont.Small;
            GUI.BeginGroup(tradeGridRow);
            var width = tradeGridRow.width;

            // Grid row is drawn RIGHT TO LEFT!

            // Draw controls for quantity available
            width -= gridRowQuantityWidth;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectQuantityAvailable = new Rect(width, 0f, gridRowQuantityWidth, tradeGridRow.height);
            Widgets.Label(rectQuantityAvailable, rowData.AvailableQuantity.ToString());

            // Set up rect and Label
            width -= gridRowPriceWidth + gridRowMargin;
            var priceFor = Math.Round(
                currentMode == TradeView.Buy ? rowData.WeSellAt : rowData.WeBuyAt,
                2,
                MidpointRounding.AwayFromZero
            ).ToString(CultureInfo.InvariantCulture);
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectPriceLabel = new Rect(width, 0, gridRowPriceWidth, tradeGridRow.height);
            Widgets.Label(rectPriceLabel, priceFor);

            // Quality Label
            width -= gridRowQualityWidth + gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectQuality = new Rect(width, 0f, gridRowQualityWidth, tradeGridRow.height);
            Widgets.Label(rectQuality, qualityTranslationCache[rowData.Quality]);

            // Stuff Label
            width -= gridRowStuffWidth + gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectStuff = new Rect(width, 0f, gridRowStuffWidth, tradeGridRow.height);
            Widgets.Label(rectStuff, rowData.TranslatedStuff);

            // Name Label
            width -= gridRowNameWidth + gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectName = new Rect(width, 0f, gridRowNameWidth, tradeGridRow.height);
            Widgets.Label(rectName, rowData.TranslatedName);

            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        #endregion

        #region Nested type: TradeView

        private enum TradeView
        {
            Buy,
            Sell
        }

        #endregion
    }
}