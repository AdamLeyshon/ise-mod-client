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
        [TweakValue("ISETradeUI", 5f, 50f)]
#endif
        private static readonly float uiTradeButtonWidth = 30f;

#if DEBUG
        [TweakValue("ISETradeUI", 5f, 50f)]
#endif
        private static readonly float uiTradeButtonPadding = 10f;

#if DEBUG
        [TweakValue("ISETradeUI", 10f, 300f)]
#endif
        private static readonly float labelStatsWidth = 150f;

#if DEBUG
        [TweakValue("ISETradeUI", 10f)]
#endif
        private static readonly float gridMargin = 16f;

#if DEBUG
        [TweakValue("ISETradeUI", 10f)]
#endif
        private static readonly float gridRowHeight = 30f;

#if DEBUG
        [TweakValue("ISETradeUI", 10f, 300f)]
#endif
        private static readonly float gridRowMargin = 40f;

#if DEBUG
        [TweakValue("ISETradeUI", 10f, 300f)]
#endif
        private static readonly float gridRowQuantityWidth = 75f;


#if DEBUG
        [TweakValue("ISETradeUI", -100f, 300f)]
#endif
        private static readonly float gridRowPriceWidth = 100f;

#if DEBUG
        [TweakValue("ISETradeUI", -100f, 300f)]
#endif
        private static readonly float gridRowQualityWidth = 100f;

#if DEBUG
        [TweakValue("ISETradeUI", -100f, 1000f)]
#endif
        private static readonly float gridRowStuffWidth = 300;

#if DEBUG
        [TweakValue("ISETradeUI", -100f, 1000f)]
#endif
        private static readonly float gridRowNameWidth = 300;

#if DEBUG
        [TweakValue("ISETradeUI", 5f)]
#endif
        private static readonly float gridRowStart = 6f;

#if DEBUG
        [TweakValue("ISETradeUI", 1, 1000)]
#endif
        private static int gridRowQty = 50;

#if DEBUG
        [TweakValue("ISETradeUI", 5f)]
#endif
        private static readonly float uiControlPadding = 6f;

#if DEBUG
        [TweakValue("ISETradeUI", 5f, 30f)]
#endif
        private static float uiControlVerticalSpacing = 5f;

#if DEBUG
        [TweakValue("ISETradeUI", 5f, 50f)]
#endif
        private static readonly float uiControlHeight = 25f;

        private static readonly ThingCategoryDef DefaultCategory = ThingCategoryDef.Named("ResourcesRaw");
        private readonly LiteDatabase db;
        private ILiteCollection<DBCachedTradable> collection;
        private List<DBCachedTradable> dataCache;
        private bool dataSourceDirty = true;
        private ThingCategoryDef filterCategory = DefaultCategory;
        private string filterText = "";
        private List<string> qualityTranslationCache;
        private Vector2 scrollPosition = Vector2.zero;
        private TradeView uiTradeMode = TradeView.Sell;
        private TradeView uiWantedTradeMode = TradeView.Buy;
        private BasketStats stats = new BasketStats();
        private DBInventory promise;

        #endregion

        private struct BasketStats
        {
            public float WeightSell { get; set; }
            public float WeightBuy { get; set; }
            public float CostSell { get; set; }
            public float CostBuy { get; set; }

            public float CostSellShipping { get; set; }

            public float CostBuyShipping { get; set; }

            public float CostTotal { get; set; }
        }

        #region ctor

        public DialogTradeUI(Pawn userPawn)
        {
            forcePause = true;
            //absorbInputAroundWindow = true;
            db = new LiteDatabase(DBLocation);
            promise = db.GetCollection<DBInventory>().FindAll().First();
            BuildQualityTranslationCache();
        }

        #endregion

        #region Properties

        public override Vector2 InitialSize => new Vector2(1500f, UI.screenHeight - 200f);

        #endregion

        #region Methods

        private void BuildQualityTranslationCache()
        {
            qualityTranslationCache = new List<string> {"N/A", "Uh-oh"};
            for (var quality = 2; quality < 7; quality++)
                qualityTranslationCache.Add(((QualityCategory) quality).GetLabel().CapitalizeFirst());
        }

        public override void DoWindowContents(Rect inRect)
        {
            UpdateDataSource();

            GUI.BeginGroup(inRect);
            inRect = inRect.AtZero();
            DrawOuterFrame(inRect);
            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        private void UpdateDataSource()
        {
            // Check if anything has changed
            if (!dataSourceDirty) return;

            Logging.WriteMessage("Updating data source");

            if (uiWantedTradeMode != uiTradeMode)
            {
                uiTradeMode = uiWantedTradeMode;

                switch (uiTradeMode)
                {
                    case TradeView.Buy:
                        collection = db.GetCollection<DBCachedTradable>("market_cache");
                        break;
                    case TradeView.Sell:
                        collection = db.GetCollection<DBCachedTradable>("colony_cache");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                RecalculateStats();
            }

            var filteredThingDefs = filterCategory.DescendantThingDefs.Select(x => x.defName).ToList();
            var items = collection.Find(x => filteredThingDefs.Contains(x.ThingDef));

            if (!filterText.NullOrEmpty())
                items = items.Where(x => x.TranslatedName
                    .ToLowerInvariant()
                    .Contains(filterText.ToLowerInvariant()));

            dataCache = items
                .OrderBy(x => x.TranslatedName)
                .ThenBy(x => x.TranslatedStuff)
                .ThenByDescending(x => x.Quality)
                .ToList();

            dataSourceDirty = false;
        }

        private void DrawOuterFrame(Rect inRect)
        {
            var addY = uiControlHeight + uiControlVerticalSpacing;

            var targetArea = new Rect(0f, 0f, inRect.width, uiControlHeight);
            DrawStatLabels(targetArea);

            targetArea = new Rect(0f, targetArea.y + addY, inRect.width, uiControlHeight);
            DrawStatDetails(targetArea);

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


        private void DrawStatLabels(Rect outerFrame)
        {
            // Subtract the grids margin to make the headers line up with grid
            var width = outerFrame.width + uiControlPadding;

            GUI.BeginGroup(outerFrame);

            // Begin group for controls in row.
            Text.Font = GameFont.Small;

            // Grid row is drawn RIGHT TO LEFT!

            // Total
            width -= labelStatsWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            var rectLabel = new Rect(width, 0f, labelStatsWidth, uiControlHeight);
            Widgets.Label(rectLabel, "Total");

            // ---------------------------
            // Delivery Costs
            // ---------------------------
            // Sell
            width -= labelStatsWidth + uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, labelStatsWidth, uiControlHeight);
            Widgets.Label(rectLabel, "Collection Cost");

            // Buy
            width -= rectLabel.width + uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, labelStatsWidth, uiControlHeight);
            Widgets.Label(rectLabel, "Delivery Cost");

            // ---------------------------
            // SELL
            // ---------------------------
            // Weight
            width -= labelStatsWidth + uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, labelStatsWidth, uiControlHeight);
            Widgets.Label(rectLabel, "Weight (Sell)");

            // Price
            width -= rectLabel.width + uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, labelStatsWidth, uiControlHeight);
            Widgets.Label(rectLabel, "Price (Sell)");

            // ---------------------------
            // BUY
            // ---------------------------
            // Weight
            width -= rectLabel.width + uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, labelStatsWidth, uiControlHeight);
            Widgets.Label(rectLabel, "Weight (Buy)");

            // Price
            width -= rectLabel.width + uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, labelStatsWidth, uiControlHeight);
            Widgets.Label(rectLabel, "Price (Buy)");

            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        private void DrawStatDetails(Rect outerFrame)
        {
            // Subtract the grids margin to make the headers line up with grid
            var width = outerFrame.width + uiControlPadding;

            GUI.BeginGroup(outerFrame);

            // Begin group for controls in row.
            Text.Font = GameFont.Small;

            // Grid row is drawn RIGHT TO LEFT!

            // Total
            width -= labelStatsWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            var rectLabel = new Rect(width, 0f, labelStatsWidth, uiControlHeight);
            Widgets.Label(rectLabel, $"{stats.CostTotal}");

            // ---------------------------
            // Delivery Costs
            // ---------------------------
            // Sell
            width -= labelStatsWidth + uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, labelStatsWidth, uiControlHeight);
            Widgets.Label(rectLabel, $"{stats.CostSellShipping}");

            // Buy
            width -= rectLabel.width + uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, labelStatsWidth, uiControlHeight);
            Widgets.Label(rectLabel, $"{stats.CostBuyShipping}");

            // ---------------------------
            // SELL
            // ---------------------------
            // Weight
            width -= labelStatsWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, labelStatsWidth, uiControlHeight);
            Widgets.Label(rectLabel, stats.WeightSell.ToStringMass());

            // Price
            width -= rectLabel.width + uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, labelStatsWidth, uiControlHeight);
            Widgets.Label(rectLabel, $"{stats.CostSell}");

            // ---------------------------
            // BUY
            // ---------------------------
            // Weight
            width -= rectLabel.width + uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, labelStatsWidth, uiControlHeight);
            Widgets.Label(rectLabel, stats.WeightBuy.ToStringMass());

            // Price
            width -= rectLabel.width + uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, labelStatsWidth, uiControlHeight);
            Widgets.Label(rectLabel, $"{stats.CostBuy}");

            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }


        private void DrawModeTabs(Rect inRect)
        {
            // Two buttons with margin in between
            var buttonWidth = (inRect.width - uiControlPadding * 3) / 2f;

            GUI.BeginGroup(inRect);
            Text.Anchor = TextAnchor.MiddleCenter;

            // Buy Button
            var buttonRect = new Rect(uiControlPadding, 0, buttonWidth, 25f);
            if (Widgets.ButtonText(buttonRect, "Buy"))
            {
                Logging.WriteMessage("Changing to Buy mode");
                uiWantedTradeMode = TradeView.Buy;
                if (filterCategory.defName == "Root") filterCategory = DefaultCategory;

                dataSourceDirty = true;
            }

            // Sell button
            buttonRect = new Rect(uiControlPadding * 2 + buttonWidth, 0, buttonWidth, 25f);
            if (Widgets.ButtonText(buttonRect, "Sell"))
            {
                Logging.WriteMessage("Changing to Sell mode");
                uiWantedTradeMode = TradeView.Sell;
                dataSourceDirty = true;
            }

            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        private void DrawFilters(Rect outerFrame)
        {
            GUI.BeginGroup(outerFrame);
            Text.Anchor = TextAnchor.MiddleLeft;

            var nextControlY = uiControlPadding;

            // Filter category
            var rectCategoryFilter = new Rect(nextControlY, 0, 300f, 25f);
            if (Widgets.ButtonText(rectCategoryFilter, $"Category: {filterCategory.LabelCap}"))
                // Open category filter drop-down 
                OpenFilterChangeFloatMenu();

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

            var wantFilterText = Widgets.TextField(filterRect, filterText);
            if (wantFilterText != filterText)
            {
                filterText = wantFilterText;
                dataSourceDirty = true;
            }

            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        private static void DrawGridHeaders(Rect outerFrame)
        {
            // Subtract the grids margin to make the headers line up with grid
            var width = outerFrame.width - gridMargin;

            GUI.BeginGroup(outerFrame);

            // Begin group for controls in row.
            Text.Font = GameFont.Small;

            // Grid row is drawn RIGHT TO LEFT!

            // Draw controls for quantity available
            width -= gridRowQuantityWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            var rectQuantityAvailable = new Rect(width, 0f, gridRowQuantityWidth, uiControlHeight);
            Widgets.Label(rectQuantityAvailable, "Available");

            // --------------------------------------------------------
            // Quantity buttons
            // --------------------------------------------------------

            // MAX
            width -= uiTradeButtonPadding + uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            var rectTradeButton = new Rect(width, 0f, uiTradeButtonWidth, uiControlHeight);
            Widgets.Label(rectTradeButton, "Mx");


            // Step UP
            width -= uiTradeButtonPadding + uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, uiTradeButtonWidth, uiControlHeight);
            Widgets.Label(rectTradeButton, "Up");

            // TEXT BOX
            width -= uiTradeButtonPadding + uiTradeButtonWidth * 3;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, uiTradeButtonWidth * 3, uiControlHeight);
            Widgets.Label(rectTradeButton, "Quantity");

            // Step DOWN
            width -= uiTradeButtonPadding + uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, uiTradeButtonWidth, uiControlHeight);
            Widgets.Label(rectTradeButton, "Dn");

            // MIN
            width -= uiTradeButtonPadding + uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, uiTradeButtonWidth, uiControlHeight);
            Widgets.Label(rectTradeButton, "Mn");

            // Price Label
            width -= gridRowPriceWidth + gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectPriceLabel = new Rect(width, 0, gridRowPriceWidth, uiControlHeight);
            Widgets.Label(rectPriceLabel, "Price");

            // Weight Label
            width -= gridRowPriceWidth + gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectWeightLabel = new Rect(width, 0, gridRowPriceWidth, uiControlHeight);
            Widgets.Label(rectWeightLabel, "Weight");

            // Quality Label
            width -= gridRowQualityWidth + gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectQuality = new Rect(width, 0f, gridRowQualityWidth, uiControlHeight);
            Widgets.Label(rectQuality, "Item Quality");

            // Stuff Label
            width -= gridRowStuffWidth + gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectStuff = new Rect(width, 0f, gridRowStuffWidth, uiControlHeight);
            Widgets.Label(rectStuff, "Material");

            // Name Label
            width -= gridRowNameWidth + gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectName = new Rect(width, 0f, gridRowNameWidth, uiControlHeight);
            Widgets.Label(rectName, "Item Name");

            GenUI.ResetLabelAlign();
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

            GenUI.ResetLabelAlign();
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
            Text.Anchor = TextAnchor.MiddleCenter;
            var rectQuantityAvailable = new Rect(width, 0f, gridRowQuantityWidth, tradeGridRow.height);
            Widgets.Label(rectQuantityAvailable, rowData.AvailableQuantity.ToString());

            // --------------------------------------------------------
            // Quantity buttons
            // --------------------------------------------------------

            width -= uiTradeButtonPadding + uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            var rectTradeButton = new Rect(width, 0f, uiTradeButtonWidth, tradeGridRow.height);

            // MAX
            if (Widgets.ButtonText(rectTradeButton, ">>"))
            {
                TradeMaxMinButtonEvent(false, false, rowData);
                collection.Upsert(rowData);
                RecalculateStats();
            }

            // Step UP
            width -= uiTradeButtonPadding + uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, uiTradeButtonWidth, tradeGridRow.height);
            if (Widgets.ButtonText(rectTradeButton, ">"))
            {
                TradeMaxMinButtonEvent(false, true, rowData);
                collection.Upsert(rowData);
                RecalculateStats();
            }

            // TEXT BOX
            width -= uiTradeButtonPadding + uiTradeButtonWidth * 3;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, uiTradeButtonWidth * 3, tradeGridRow.height);
            var qty = rowData.TradedQuantity;
            var buffer = rowData.UnityTextBuffer;
            Widgets.TextFieldNumeric(rectTradeButton, ref qty, ref buffer, 0f, rowData.AvailableQuantity);
            if (qty != rowData.TradedQuantity)
            {
                // Save changes to quantity
                ClampTradeAmount(rowData, qty);
                collection.Upsert(rowData);
                RecalculateStats();
            }

            // Step DOWN
            width -= uiTradeButtonPadding + uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, uiTradeButtonWidth, tradeGridRow.height);
            if (Widgets.ButtonText(rectTradeButton, "<"))
            {
                TradeMaxMinButtonEvent(true, true, rowData);
                collection.Upsert(rowData);
                RecalculateStats();
            }

            // MIN
            width -= uiTradeButtonPadding + uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, uiTradeButtonWidth, tradeGridRow.height);
            if (Widgets.ButtonText(rectTradeButton, "<<"))
            {
                TradeMaxMinButtonEvent(true, false, rowData);
                collection.Upsert(rowData);
                RecalculateStats();
            }

            // Price
            width -= gridRowPriceWidth + gridRowMargin;
            var priceFor = Math.Round(
                uiTradeMode == TradeView.Buy ? rowData.WeSellAt : rowData.WeBuyAt,
                2,
                MidpointRounding.AwayFromZero
            ).ToString(CultureInfo.InvariantCulture);
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectPriceLabel = new Rect(width, 0, gridRowPriceWidth, tradeGridRow.height);
            Widgets.Label(rectPriceLabel, priceFor);

            // Weight Label
            width -= gridRowPriceWidth + gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectWeightLabel = new Rect(width, 0, gridRowPriceWidth, uiControlHeight);
            Widgets.Label(rectWeightLabel, rowData.Weight.ToStringMass());

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

        private static void TradeMaxMinButtonEvent(bool decrease, bool step, DBCachedTradable tradable)
        {
            int qty;
            if (decrease)
                qty = step
                    ? tradable.TradedQuantity - 5
                    : 0;
            else
                qty = step
                    ? tradable.TradedQuantity + 5
                    : tradable.AvailableQuantity;

            ClampTradeAmount(tradable, qty);
        }

        private static void ClampTradeAmount(DBCachedTradable tradable, int quantity)
        {
            tradable.TradedQuantity = Mathf.Clamp(quantity, 0, tradable.AvailableQuantity);
        }

        private void OpenFilterChangeFloatMenu()
        {
            var list = new List<FloatMenuOption>();

            // Enumerate list of ThingCategoryDef and add to menu options.
            var allDefsListForReading = DefDatabase<ThingCategoryDef>.AllDefsListForReading;
            foreach (var def in allDefsListForReading)
                try
                {
                    if (def.LabelCap.NullOrEmpty())
                    {
                        Logging.WriteErrorMessage(
                            $"ThingCategoryDef {def.defName} has no label!, Not able to filter by this, skipping. Please report to Mod Author.");
                        continue;
                    }

                    // We'll never deal with corpses so don't show them.
                    if (def.LabelCap.ToLower().ToString().Contains("corpse")) continue;

                    // When they select a new category, update the filter and refresh the query.
                    void ChangeFilterAction()
                    {
                        if (def.defName == "Root" && uiTradeMode == TradeView.Buy)
                        {
                            PromptForRootCategory();
                        }
                        else
                        {
                            filterCategory = def;
                            dataSourceDirty = true;
                        }
                    }

                    list.Add(new FloatMenuOption(def.LabelCap, ChangeFilterAction));
                }
                catch (Exception)
                {
                    // Skip for now
                }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        public override void PreClose()
        {
            db?.Dispose();
        }

        private void PromptForRootCategory()
        {
            void Confirm()
            {
                filterCategory = ThingCategoryDef.Named("Root");
                dataSourceDirty = true;
            }

            Find.WindowStack.Add(new Dialog_MessageBox(
                "If your game crashes or the UI is slow, please don't report this as a bug,\r\n" +
                "We don't support the use of the root category due\r\nto the sheer amount of items in modded games",
                "Hurt me plenty",
                Confirm,
                "Nope",
                null,
                "That's a lot of items", false,
                Confirm
            ));
        }

        private void RecalculateStats()
        {
            var weight = collection.FindAll()
                .Where(x => x.TradedQuantity > 0)
                .Sum(x => x.Weight * x.TradedQuantity);
            var cost = collection.FindAll()
                .Where(x => x.TradedQuantity > 0)
                .Sum(x =>
                    (uiTradeMode == TradeView.Buy ? x.WeSellAt : x.WeBuyAt)
                    * x.TradedQuantity
                );

            switch (uiTradeMode)
            {
                case TradeView.Buy:
                    stats.CostBuy = (float) Math.Round(cost, 2, MidpointRounding.ToEven);
                    stats.WeightBuy = Mathf.Ceil(weight);
                    break;
                case TradeView.Sell:
                    stats.CostSell = (float) Math.Round(cost, 2, MidpointRounding.ToEven);
                    stats.WeightSell = Mathf.Ceil(weight);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            stats.CostBuyShipping = Mathf.Ceil(promise.DeliveryChargePerKG * stats.WeightBuy);
            stats.CostSellShipping = Mathf.Ceil(promise.CollectionChargePerKG * stats.WeightSell);
            stats.CostTotal = (float) Math.Round(
                stats.CostBuy +
                stats.CostBuyShipping +
                stats.CostSellShipping -
                stats.CostSell
                , 2
                , MidpointRounding.ToEven
            );
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