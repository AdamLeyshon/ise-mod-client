#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, tradeui.cs 2021-03-07

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ise.components;
using ise.lib;
using ise_core.db;
using LiteDB;
using RimWorld;
using UnityEngine;
using Verse;
using static ise.lib.Constants;
using static ise.lib.Cache;
using static ise.lib.Tradables;

namespace ise.dialogs
{
    public class DialogTradeUI : Window
    {
        #region ctor

        public DialogTradeUI(Pawn userPawn)
        {
            var gc = Current.Game.GetComponent<ISEGameComponent>();
            forcePause = true;
            pawn = userPawn;
            //absorbInputAroundWindow = true;
            SetupData();
            promise = IseCentral.DataCache.GetCollection<DBInventoryPromise>(Tables.Promises)
                .FindById(gc.GetColonyId(userPawn.Map));
            BuildQualityTranslationCache();
        }

        #endregion

        #region Properties

        public override Vector2 InitialSize => new Vector2(1500f, UI.screenHeight - 50f);

        #endregion

        #region Nested type: BasketStats

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

        #endregion

        #region Nested type: ItemCache

        private struct ItemCache
        {
            public ILiteCollection<DBCachedTradable> Colony;
            public ILiteCollection<DBCachedTradable> Market;
            public ILiteCollection<DBCachedTradable> ColonyBasket;
            public ILiteCollection<DBCachedTradable> MarketBasket;
            public List<DBCachedTradable> CurrentItems;
        }

        #endregion

        #region Nested type: TradeView

        private enum TradeView
        {
            Buy,
            Sell
        }

        #endregion

        #region Fields

#if DEBUG
        [TweakValue("ISETradeUI", 5f, 50f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _uiTradeButtonWidth = 30f;

#if DEBUG
        [TweakValue("ISETradeUI", 5f, 200f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _uiActionButtonHeight = 50f;

#if DEBUG
        [TweakValue("ISETradeUI", 5f, 200f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _uiActionButtonWidth = 100f;

#if DEBUG
        [TweakValue("ISETradeUI", 5f, 50f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _uiTradeButtonPadding = 10f;

#if DEBUG
        [TweakValue("ISETradeUI", 10f, 300f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _labelStatsWidth = 150f;

#if DEBUG
        [TweakValue("ISETradeUI", 10f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _gridMargin = 16f;

#if DEBUG
        [TweakValue("ISETradeUI", 10f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _gridRowHeight = 30f;

#if DEBUG
        [TweakValue("ISETradeUI", 10f, 300f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _gridRowMargin = 40f;

#if DEBUG
        [TweakValue("ISETradeUI", 10f, 300f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _gridRowQuantityWidth = 75f;


#if DEBUG
        [TweakValue("ISETradeUI", -100f, 300f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _gridRowPriceWidth = 100f;

#if DEBUG
        [TweakValue("ISETradeUI", -100f, 300f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _gridRowQualityWidth = 100f;

#if DEBUG
        [TweakValue("ISETradeUI", -100f, 1000f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _gridRowStuffWidth = 300;

#if DEBUG
        [TweakValue("ISETradeUI", -100f, 1000f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _gridRowNameWidth = 300;

#if DEBUG
        [TweakValue("ISETradeUI", 5f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _gridRowStart = 6f;

#if DEBUG
        [TweakValue("ISETradeUI", 5f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _uiControlPadding = 6f;

#if DEBUG
        [TweakValue("ISETradeUI", 5f, 30f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _uiControlVerticalSpacing = 5f;

#if DEBUG
        [TweakValue("ISETradeUI", 5f, 50f)]
#endif
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private static float _uiControlHeight = 25f;

        private static readonly ThingCategoryDef DefaultCategory = ThingCategoryDef.Named("ResourcesRaw");
        private readonly Pawn pawn;
        private readonly DBInventoryPromise promise;
        private bool basketItemsOnly;
        private ItemCache cache;

        private bool dataSourceDirty = true;
        private ThingCategoryDef filterCategory = DefaultCategory;
        private string filterText = "";
        private bool ownedItemsOnly;
        private List<string> qualityTranslationCache;
        private Vector2 scrollPosition = Vector2.zero;
        private BasketStats stats;
        private TradeView uiTradeMode = TradeView.Buy;

        #endregion

        #region Methods

        private void SetupData()
        {
            var gc = Current.Game.GetComponent<ISEGameComponent>();
            var colonyId = gc.GetColonyId(pawn.Map);
            cache = new ItemCache
            {
                Colony = GetCache(colonyId, CacheType.ColonyCache),
                ColonyBasket = GetCache(colonyId, CacheType.ColonyBasket),
                Market = GetCache(colonyId, CacheType.MarketCache),
                MarketBasket = GetCache(colonyId, CacheType.MarketBasket),
                CurrentItems = null
            };
        }

        private void BuildQualityTranslationCache()
        {
            qualityTranslationCache = new List<string> { "N/A", "Uh-oh" };
            for (var quality = 2; quality < 7; quality++)
                qualityTranslationCache.Add(((QualityCategory)quality).GetLabel().CapitalizeFirst());
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

            Logging.WriteDebugMessage("Updating data source");

            ILiteCollection<DBCachedTradable> collection;

            switch (uiTradeMode)
            {
                case TradeView.Buy:
                    collection = cache.Market;
                    break;
                case TradeView.Sell:
                    collection = cache.Colony;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            RecalculateStats();

            // Category filter
            var filteredThingDefs = filterCategory.DescendantThingDefs.Select(x => x.defName).ToList();

            IEnumerable<DBCachedTradable> FullTextSearch()
            {
                var searchString = filterText.ToLowerInvariant();
                return collection.Find(x => x.IndexedName.Contains(searchString));
            }

            // Text filter if needed, do it at the DB level, hopefully faster than LINQ
            var dbItems = !filterText.NullOrEmpty()
                ? FullTextSearch()
                : collection.FindAll();

            var items = dbItems.Where(x => filteredThingDefs.Contains(x.ThingDef) && x.Quantity > 0);

            // Basket only filter
            if (basketItemsOnly)
            {
                var basketItems = (
                    uiTradeMode == TradeView.Buy ? cache.MarketBasket : cache.ColonyBasket
                ).FindAll().Select(x => x.ItemCode);
                items = items.Where(x => basketItems.Contains(x.ItemCode));
            }

            // Owned only filter
            if (ownedItemsOnly && uiTradeMode == TradeView.Buy)
            {
                var colonyItems = cache.Colony.FindAll().Select(x => x.ItemCode);
                items = items.Where(x => colonyItems.Contains(x.ItemCode));
            }

            cache.CurrentItems = items
                .OrderBy(x => x.TranslatedName)
                .ThenBy(x => x.TranslatedStuff)
                .ThenByDescending(x => x.Quality)
                .ToList();

            dataSourceDirty = false;
        }

        private void DrawOuterFrame(Rect inRect)
        {
            var addY = _uiControlHeight + _uiControlVerticalSpacing;

            var targetArea = new Rect(0f, 0f, inRect.width, _uiControlHeight);
            DrawStatLabels(targetArea);

            targetArea = new Rect(0f, targetArea.y + addY, inRect.width, _uiControlHeight);
            DrawStatDetails(targetArea);

            targetArea = new Rect(0f, targetArea.y + addY, inRect.width, _uiControlHeight);
            DrawModeTabs(targetArea);

            targetArea = new Rect(0f, targetArea.y + addY, inRect.width, _uiControlHeight);
            DrawFilters(targetArea);

            targetArea = new Rect(0f, targetArea.y + addY, inRect.width, _uiControlHeight);
            DrawGridHeaders(targetArea);

            // Anchor to bottom
            var actionButtons = new Rect(0f, inRect.height - _uiControlVerticalSpacing - _uiActionButtonHeight,
                inRect.width, _uiActionButtonHeight);
            DrawConfirmCancel(actionButtons);

            // Find top of action buttons, subtract spacing, then find distance to top of grid and subtract that
            var remaining = actionButtons.y - _uiControlVerticalSpacing * 2 - (targetArea.y + addY);

            // Grid takes up the remaining space
            targetArea = new Rect(0f, targetArea.y + addY, inRect.width, remaining);
            DrawTradeGrid(targetArea);
        }

        private void DrawStatLabels(Rect outerFrame)
        {
            // Subtract the grids margin to make the headers line up with grid
            var width = outerFrame.width + _uiControlPadding;

            GUI.BeginGroup(outerFrame);

            // Begin group for controls in row.
            Text.Font = GameFont.Small;

            // Grid row is drawn RIGHT TO LEFT!

            // Total
            width -= _labelStatsWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            var rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, "Total");

            // ---------------------------
            // Delivery Costs
            // ---------------------------
            // Sell
            width -= _labelStatsWidth + _uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, "Collection Cost");

            // Buy
            width -= rectLabel.width + _uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, "Delivery Cost");

            // ---------------------------
            // SELL
            // ---------------------------
            // Weight
            width -= _labelStatsWidth + _uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, "Weight (Sell)");

            // Price
            width -= rectLabel.width + _uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, "Price (Sell)");

            // ---------------------------
            // BUY
            // ---------------------------
            // Weight
            width -= rectLabel.width + _uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, "Weight (Buy)");

            // Price
            width -= rectLabel.width + _uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, "Price (Buy)");

            // Bank
            width -= rectLabel.width + _uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, "Bank Balance");

            // Mode
            width -= rectLabel.width + _uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, "Trading Mode");

            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        private void DrawStatDetails(Rect outerFrame)
        {
            // Subtract the grids margin to make the headers line up with grid
            var width = outerFrame.width + _uiControlPadding;

            GUI.BeginGroup(outerFrame);

            // Begin group for controls in row.
            Text.Font = GameFont.Small;

            // Grid row is drawn RIGHT TO LEFT!

            // Total
            width -= _labelStatsWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            var rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            string costText;
            if (stats.CostTotal > 0)
                costText = $"Cost: {stats.CostTotal}";
            else if (stats.CostTotal < 0)
                costText = $"Gain: {Math.Abs(stats.CostTotal)}";
            else
                costText = "Equal: 0.00";

            Widgets.Label(rectLabel, costText);

            // ---------------------------
            // Delivery Costs
            // ---------------------------
            // Sell
            width -= _labelStatsWidth + _uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, $"{stats.CostSellShipping}");

            // Buy
            width -= rectLabel.width + _uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, $"{stats.CostBuyShipping}");

            // ---------------------------
            // SELL
            // ---------------------------
            // Weight
            width -= _labelStatsWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, stats.WeightSell.ToStringMass());

            // Price
            width -= rectLabel.width + _uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, $"{stats.CostSell}");

            // ---------------------------
            // BUY
            // ---------------------------
            // Weight
            width -= rectLabel.width + _uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, stats.WeightBuy.ToStringMass());

            // Price
            width -= rectLabel.width + _uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, $"{stats.CostBuy}");

            // Price
            width -= rectLabel.width + _uiControlPadding;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, $"{promise.AccountBalance}");

            // Mode
            width -= rectLabel.width + _uiControlPadding * 2;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectLabel = new Rect(width, 0f, _labelStatsWidth, _uiControlHeight);
            Widgets.Label(rectLabel, uiTradeMode == TradeView.Buy ? "BUY" : "SELL");
            Text.Font = GameFont.Small;

            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        private void DrawModeTabs(Rect inRect)
        {
            // Two buttons with margin in between
            var buttonWidth = (inRect.width - _uiControlPadding * 3) / 2f;

            GUI.BeginGroup(inRect);
            Text.Anchor = TextAnchor.MiddleCenter;

            // Buy Button
            var buttonRect = new Rect(_uiControlPadding, 0, buttonWidth, 25f);
            if (Widgets.ButtonText(buttonRect, "Buy"))
            {
                uiTradeMode = TradeView.Buy;
                if (filterCategory.defName == "Root") filterCategory = DefaultCategory;
                dataSourceDirty = true;
            }

            // Sell button
            buttonRect = new Rect(_uiControlPadding * 2 + buttonWidth, 0, buttonWidth, 25f);
            if (Widgets.ButtonText(buttonRect, "Sell"))
            {
                uiTradeMode = TradeView.Sell;
                dataSourceDirty = true;
            }

            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        private void DrawConfirmCancel(Rect inRect)
        {
            var width = inRect.width;

            GUI.BeginGroup(inRect);
            Text.Anchor = TextAnchor.MiddleCenter;

            // Confirm button
            width -= _uiControlPadding + _uiActionButtonWidth;
            var buttonRect = new Rect(width, 0, _uiActionButtonWidth, inRect.height);
            if (Widgets.ButtonText(buttonRect, "Confirm"))
            {
                var silver = GetItemsNearBeacons(pawn.Map, ThingDefSilver);
                var fundsAvailable = silver.Sum(s => s.stackCount);
                var remaining = Mathf.Clamp((int)Math.Ceiling(Math.Round(
                    stats.CostTotal - promise.AccountBalance,
                    2,
                    MidpointRounding.ToEven
                )), 0, int.MaxValue);
                var canAfford = false;
                var noMoney = "Your colony can't afford this right now.\r\n" +
                              $"You have {fundsAvailable} Silver in stockpiles and " +
                              $"{promise.AccountBalance} in your account";

                string text;
                if (stats.CostTotal > 0)
                {
                    if (promise.AccountBalance > 0)
                    {
                        if (promise.AccountBalance > stats.CostTotal)
                        {
                            var accRemaining = (int)Math.Ceiling(Math.Round(
                                promise.AccountBalance - stats.CostTotal,
                                2,
                                MidpointRounding.ToEven
                            ));
                            text = "We will withdraw the full amount from your account,\r\n" +
                                   $"Your account balance will be {accRemaining}\r\n" +
                                   $"The total cost is: {stats.CostTotal}";
                            canAfford = true;
                        }
                        else
                        {
                            if (fundsAvailable < remaining)
                            {
                                text = noMoney;
                            }
                            else
                            {
                                text = "There are insufficient funds in your account,\r\n" +
                                       $"We will withdraw {promise.AccountBalance} of the " +
                                       $"total {stats.CostTotal} from your account" +
                                       $"\r\nThe remaining balance due is: {remaining}";
                                canAfford = true;
                            }
                        }
                    }
                    else
                    {
                        if (fundsAvailable < stats.CostTotal)
                        {
                            text = noMoney;
                        }
                        else
                        {
                            text = $"The total balance is due is: {stats.CostTotal}";
                            canAfford = true;
                        }
                    }
                }
                else if (stats.CostTotal < 0)
                {
                    var credit = Math.Abs(stats.CostTotal);
                    text = $"Your account will be credited with: {credit}\r\n" +
                           $"Your new balance will be approximately: {promise.AccountBalance + credit}";
                    canAfford = true;
                }
                else
                {
                    text = "Thank you for trading with ISE,\r\n" +
                           "We hope to hear from you again.";
                    canAfford = true;
                }

                if (canAfford)
                    Find.WindowStack.Add(new Dialog_MessageBox(
                        text,
                        "Agree, Place Order",
                        delegate { PlaceOrder(remaining); },
                        "Decline",
                        null,
                        "Terms and Conditions", false,
                        delegate { PlaceOrder(remaining); }
                    ));
                else
                    Find.WindowStack.Add(new Dialog_MessageBox(
                        text,
                        "Oh...",
                        title: "Insufficient funds"
                    ));
            }

            // Cancel Button
            width -= _uiControlPadding + _uiActionButtonWidth;
            buttonRect = new Rect(width, 0, _uiActionButtonWidth, inRect.height);
            if (Widgets.ButtonText(buttonRect, "Cancel")) Close();

            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        private void PlaceOrder(int remaining)
        {
            Find.WindowStack.Add(new DialogOrder(pawn, remaining));
            Close();
        }

        private void DrawFilters(Rect outerFrame)
        {
            GUI.BeginGroup(outerFrame);
            Text.Anchor = TextAnchor.MiddleLeft;

            var nextControlY = _uiControlPadding;

            // Filter category
            var rectCategoryFilter = new Rect(nextControlY, 0, 300f, 25f);
            if (Widgets.ButtonText(rectCategoryFilter, $"Category: {filterCategory.LabelCap}"))
                // Open category filter drop-down 
                OpenFilterChangeFloatMenu();

            // Basket filter checkbox
            nextControlY += _uiControlPadding * 2 + rectCategoryFilter.width;
            var rectBasketCheckbox = new Rect(nextControlY, 0f, 125f, outerFrame.height);
            var oldValue = basketItemsOnly;
            Widgets.CheckboxLabeled(rectBasketCheckbox, "Basket only", ref basketItemsOnly);
            if (oldValue != basketItemsOnly) dataSourceDirty = true;

            var lastControlWidth = rectBasketCheckbox.width;
            if (uiTradeMode == TradeView.Buy)
            {
                // Owned items filter checkbox
                nextControlY += _uiControlPadding * 2 + lastControlWidth;
                var rectOnlyOwnedCheckbox = new Rect(nextControlY, 0f, 125f, outerFrame.height);
                oldValue = ownedItemsOnly;
                Widgets.CheckboxLabeled(rectOnlyOwnedCheckbox, "Owned only", ref ownedItemsOnly);
                if (oldValue != ownedItemsOnly) dataSourceDirty = true;

                lastControlWidth = rectOnlyOwnedCheckbox.width;
            }

            // Filter Box Label
            nextControlY += _uiControlPadding * 2 + lastControlWidth;
            var rectFilterLabel = new Rect(nextControlY, 0f, 100f, outerFrame.height);
            Widgets.Label(rectFilterLabel, "Filter by Name: ");


            // Filter Box, takes up rest of space
            nextControlY += _uiControlPadding + rectFilterLabel.width;
            var filterRect = new Rect(
                nextControlY,
                0,
                outerFrame.width - nextControlY - _uiControlPadding, // Total width of controls + all padding
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
            var width = outerFrame.width - _gridMargin;

            GUI.BeginGroup(outerFrame);

            // Begin group for controls in row.
            Text.Font = GameFont.Small;

            // Grid row is drawn RIGHT TO LEFT!

            // Draw controls for quantity available
            width -= _gridRowQuantityWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            var rectQuantityAvailable = new Rect(width, 0f, _gridRowQuantityWidth, _uiControlHeight);
            Widgets.Label(rectQuantityAvailable, "Available");

            // --------------------------------------------------------
            // Quantity buttons
            // --------------------------------------------------------

            // MAX
            width -= _uiTradeButtonPadding + _uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            var rectTradeButton = new Rect(width, 0f, _uiTradeButtonWidth, _uiControlHeight);
            Widgets.Label(rectTradeButton, "Max");


            // Step UP
            width -= _uiTradeButtonPadding + _uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, _uiTradeButtonWidth, _uiControlHeight);
            Widgets.Label(rectTradeButton, "Inc");

            // TEXT BOX
            width -= _uiTradeButtonPadding + _uiTradeButtonWidth * 3;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, _uiTradeButtonWidth * 3, _uiControlHeight);
            Widgets.Label(rectTradeButton, "Quantity");

            // Step DOWN
            width -= _uiTradeButtonPadding + _uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, _uiTradeButtonWidth, _uiControlHeight);
            Widgets.Label(rectTradeButton, "Dec");

            // MIN
            width -= _uiTradeButtonPadding + _uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, _uiTradeButtonWidth, _uiControlHeight);
            Widgets.Label(rectTradeButton, "Min");

            // Price Label
            width -= _gridRowPriceWidth + _gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectPriceLabel = new Rect(width, 0, _gridRowPriceWidth, _uiControlHeight);
            Widgets.Label(rectPriceLabel, "Price");

            // Weight Label
            width -= _gridRowPriceWidth + _gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectWeightLabel = new Rect(width, 0, _gridRowPriceWidth, _uiControlHeight);
            Widgets.Label(rectWeightLabel, "Weight");

            // Quality Label
            width -= _gridRowQualityWidth + _gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectQuality = new Rect(width, 0f, _gridRowQualityWidth, _uiControlHeight);
            Widgets.Label(rectQuality, "Item Quality");

            // Stuff Label
            width -= _gridRowStuffWidth + _gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectStuff = new Rect(width, 0f, _gridRowStuffWidth, _uiControlHeight);
            Widgets.Label(rectStuff, "Material");

            // Name Label
            width -= _gridRowNameWidth + _gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectName = new Rect(width, 0f, _gridRowNameWidth, _uiControlHeight);
            Widgets.Label(rectName, "Item Name");

            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        private void DrawTradeGrid(Rect outerFrame)
        {
            Text.Font = GameFont.Small;

            var gridRowCount = cache.CurrentItems.Count;

            // This is the buffer that the rows are rendered in, anything outside of the viewport above is hidden
            var gridVirtualBuffer = new Rect(0f, 0f, outerFrame.width - _gridMargin, _gridRowHeight * gridRowCount);

            // Render gridVirtualBuffer to rectGridViewport
            Widgets.BeginScrollView(outerFrame, ref scrollPosition, gridVirtualBuffer);
            var gridRowYPos = _gridRowStart;
            var gridBottom = scrollPosition.y - 30f;
            var gridTop = scrollPosition.y + outerFrame.height;
            var rowNumber = 0;
            foreach (var cachedTradable in cache.CurrentItems)
            {
                if (gridRowYPos > gridBottom && gridRowYPos < gridTop)
                {
                    // Grid Row
                    var tradableRow = new Rect(0f, gridRowYPos, gridVirtualBuffer.width, _gridRowHeight);
                    DrawGridRows(tradableRow, rowNumber, cachedTradable);
                }

                rowNumber++;
                gridRowYPos += _gridRowHeight;
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
            width -= _gridRowQuantityWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            var rectQuantityAvailable = new Rect(width, 0f, _gridRowQuantityWidth, tradeGridRow.height);
            Widgets.Label(rectQuantityAvailable, rowData.Quantity.ToString());

            // --------------------------------------------------------
            // Quantity buttons
            // --------------------------------------------------------

            width -= _uiTradeButtonPadding + _uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            var rectTradeButton = new Rect(width, 0f, _uiTradeButtonWidth, tradeGridRow.height);

            // MAX
            if (Widgets.ButtonText(rectTradeButton, ">>")) TradeMaxMinButtonEvent(false, false, rowData);

            // Step UP
            width -= _uiTradeButtonPadding + _uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, _uiTradeButtonWidth, tradeGridRow.height);
            if (Widgets.ButtonText(rectTradeButton, ">")) TradeMaxMinButtonEvent(false, true, rowData);

            // TEXT BOX
            width -= _uiTradeButtonPadding + _uiTradeButtonWidth * 3;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, _uiTradeButtonWidth * 3, tradeGridRow.height);
            var qty = rowData.TradedQuantity;
            var buffer = rowData.UnityTextBuffer;
            Widgets.TextFieldNumeric(rectTradeButton, ref qty, ref buffer, 0f, rowData.Quantity);
            if (qty != rowData.TradedQuantity)
                // Save changes to quantity
                SetTradeAmount(rowData, qty);

            // Step DOWN
            width -= _uiTradeButtonPadding + _uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, _uiTradeButtonWidth, tradeGridRow.height);
            if (Widgets.ButtonText(rectTradeButton, "<")) TradeMaxMinButtonEvent(true, true, rowData);

            // MIN
            width -= _uiTradeButtonPadding + _uiTradeButtonWidth;
            Text.Anchor = TextAnchor.MiddleCenter;
            rectTradeButton = new Rect(width, 0f, _uiTradeButtonWidth, tradeGridRow.height);
            if (Widgets.ButtonText(rectTradeButton, "<<")) TradeMaxMinButtonEvent(true, false, rowData);

            // Price
            width -= _gridRowPriceWidth + _gridRowMargin;
            var priceFor = Math.Round(
                uiTradeMode == TradeView.Buy ? rowData.WeSellAt : rowData.WeBuyAt,
                2,
                MidpointRounding.AwayFromZero
            ).ToString(CultureInfo.InvariantCulture);
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectPriceLabel = new Rect(width, 0, _gridRowPriceWidth, tradeGridRow.height);
            Widgets.Label(rectPriceLabel, priceFor);

            // Weight Label
            width -= _gridRowPriceWidth + _gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectWeightLabel = new Rect(width, 0, _gridRowPriceWidth, _uiControlHeight);
            Widgets.Label(rectWeightLabel, rowData.Weight.ToStringMass());

            // Quality Label
            width -= _gridRowQualityWidth + _gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectQuality = new Rect(width, 0f, _gridRowQualityWidth, tradeGridRow.height);
            Widgets.Label(rectQuality, qualityTranslationCache[rowData.Quality]);

            // Stuff Label
            width -= _gridRowStuffWidth + _gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectStuff = new Rect(width, 0f, _gridRowStuffWidth, tradeGridRow.height);
            Widgets.Label(rectStuff, rowData.TranslatedStuff);

            // Name Label
            width -= _gridRowNameWidth + _gridRowMargin;
            Text.Anchor = TextAnchor.MiddleLeft;
            var rectName = new Rect(width, 0f, _gridRowNameWidth, tradeGridRow.height);
            Widgets.Label(rectName, rowData.TranslatedName);

            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        private void TradeMaxMinButtonEvent(bool decrease, bool step, DBCachedTradable tradable)
        {
            int qty;
            if (decrease)
                qty = step
                    ? tradable.TradedQuantity - 5
                    : 0;
            else
                qty = step
                    ? tradable.TradedQuantity + 5
                    : tradable.Quantity;

            SetTradeAmount(tradable, qty);
        }

        private void SetTradeAmount(DBCachedTradable tradable, int quantity)
        {
            tradable.TradedQuantity = Mathf.Clamp(quantity, 0, tradable.Quantity);

            var basket = uiTradeMode == TradeView.Buy ? cache.MarketBasket : cache.ColonyBasket;

            if (tradable.TradedQuantity == 0)
                basket.Delete(tradable.ItemCode);
            else
                basket.Upsert(tradable);

            (uiTradeMode == TradeView.Buy ? cache.Market : cache.Colony).Upsert(tradable);
            RecalculateStats();
        }

        private void OpenFilterChangeFloatMenu()
        {
            var list = new List<FloatMenuOption>();

            // Enumerate list of ThingCategoryDef and add to menu options.
            var allDefsListForReading = DefDatabase<ThingCategoryDef>.AllDefsListForReading;
            allDefsListForReading.RemoveAll(def => def.LabelCap.NullOrEmpty());
            allDefsListForReading.SortBy(def => def.LabelCap.ToString());

            // Ensure Root is always at the top
            var rootIndex = allDefsListForReading.FindIndex(def => def.defName == "Root");
            var root = allDefsListForReading[rootIndex];
            allDefsListForReading.RemoveAt(rootIndex);
            allDefsListForReading.Insert(0, root);
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
            var collection = uiTradeMode == TradeView.Buy ? cache.MarketBasket : cache.ColonyBasket;
            var weight = collection.FindAll()
                .Sum(x => x.Weight * x.TradedQuantity);
            var cost = collection.FindAll()
                .Sum(x =>
                    (uiTradeMode == TradeView.Buy ? x.WeSellAt : x.WeBuyAt)
                    * x.TradedQuantity
                );

            switch (uiTradeMode)
            {
                case TradeView.Buy:
                    stats.CostBuy = (float)Math.Round(cost, 2, MidpointRounding.ToEven);
                    stats.WeightBuy = Mathf.Ceil(weight);
                    break;
                case TradeView.Sell:
                    stats.CostSell = (float)Math.Round(cost, 2, MidpointRounding.ToEven);
                    stats.WeightSell = Mathf.Ceil(weight);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            stats.CostBuyShipping = Mathf.Ceil(promise.DeliveryChargePerKG * stats.WeightBuy);
            stats.CostSellShipping = Mathf.Ceil(promise.CollectionChargePerKG * stats.WeightSell);
            stats.CostTotal = (float)Math.Round(
                stats.CostBuy +
                stats.CostBuyShipping +
                stats.CostSellShipping -
                stats.CostSell
                , 2
                , MidpointRounding.ToEven
            );
        }

        #endregion
    }
}