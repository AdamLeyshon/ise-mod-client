#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, modsettings.cs 2021-09-07

#endregion

using UnityEngine;
using Verse;

namespace ise.settings
{
    public class ISESettings : ModSettings
    {
        public bool DebugColonyItemRemove;
        public bool DebugMaterialiser;
        public bool DebugMessages;
        public bool DebugTradeBeacons;
        public bool ShowTradeUIIcons;

        /// <summary>
        ///     The part that writes our settings to file. Note that saving is by ref.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref DebugMessages, "DebugMessages");
            Scribe_Values.Look(ref DebugTradeBeacons, "DebugTradeBeacons");
            Scribe_Values.Look(ref DebugMaterialiser, "DebugMaterialiser");
            Scribe_Values.Look(ref DebugColonyItemRemove, "DebugColonyItemRemove");
            Scribe_Values.Look(ref ShowTradeUIIcons, "ShowTradeUIIcons");
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.Saving) IseCentral.ReloadSettings();
        }
    }

    public class ISEMod : Mod
    {
        private readonly ISESettings settings;

        public ISEMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<ISESettings>();
        }


        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("ISEShowTradeUIIcons".Translate(), ref settings.ShowTradeUIIcons,
                "ISEShowTradeUIIconsTooltip".Translate());
            listingStandard.CheckboxLabeled("ISEDebugMessages".Translate(), ref settings.DebugMessages,
                "ISEDebugMessagesTooltip".Translate());
            listingStandard.CheckboxLabeled("ISEDebugTradeBeacons".Translate(), ref settings.DebugTradeBeacons,
                "ISEDebugTradeBeaconsTooltip".Translate());
            listingStandard.CheckboxLabeled("ISEDebugMaterialiser".Translate(), ref settings.DebugMaterialiser,
                "ISEDebugMaterialiserTooltip".Translate());
            listingStandard.CheckboxLabeled("ISEDebugColonyItemRemove".Translate(), ref settings.DebugMaterialiser,
                "ISEDebugColonyItemRemoveTooltip".Translate());
            // listingStandard.Label("exampleFloatExplanation");
            // settings.exampleFloat = listingStandard.Slider(settings.exampleFloat, 100f, 300f);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        ///     Override SettingsCategory to show up in the list of settings.
        ///     Using .Translate() is optional, but does allow for localisation.
        /// </summary>
        /// <returns>The (translated) mod name.</returns>
        public override string SettingsCategory()
        {
            return "ISEName".Translate();
        }
    }
}