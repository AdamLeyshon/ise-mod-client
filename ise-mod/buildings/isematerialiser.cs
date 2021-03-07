#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, isematerialiser.cs, Created 2021-03-07

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using ise.components;
using ise.lib;
using ise_core.db;
using ise_core.extend;
using LiteDB;
using RimWorld;
using Verse;

namespace ise.buildings
{
    public class ISEMaterialiser : Building
    {
        #region ProcessSpeed enum

        public enum ProcessSpeed
        {
            Stop,
            Low,
            Medium,
            High
        }

        #endregion

        #region WorkStatus enum

        public enum WorkStatus
        {
            NoPower,
            SolarFlare,
            TooCold,
            Stopped,
            NoWork,
            Working
        }

        #endregion

        #region Fields

        private const int WattsPerItem = 50;
        private const int StandbyPower = 25;
        private const int TicksBetweenProgress = 1000;

        private CompPowerTrader compPowerTrader;
        private Guid currentItemDbId;
        private int quantityToMaterialise = 0;
        private ISEGameComponent gameComponent;
        private Map currentMap;
        private int nextProgressTick = 0;
        
        #endregion

        #region Properties

        private bool Autorun { get; set; }

        private ProcessSpeed Speed { get; set; }
        private ProcessSpeed LastSpeed { get; set; }

        private WorkStatus MaterialiserStatus { get; set; }

        private int Progress { get; set; }

        private Thing CurrentItem { get; set; }

        #endregion

        #region Methods

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPowerTrader = GetComp<CompPowerTrader>();
            gameComponent = Current.Game.GetComponent<ISEGameComponent>();
            currentMap = map;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos()) yield return gizmo;

            // Add button to cycle to next speed.
            var action = new Command_Action
            {
                defaultLabel = "Next Speed", action = delegate { Speed = Speed.Next(); }
            };
            yield return action;

            // Add button to enable/disable Autorun.
            action = new Command_Action
            {
                defaultLabel = $"Autorun ({(Autorun ? "On" : "Off")})", action = delegate { Autorun = !Autorun; }
            };
            yield return action;
        }

        public override void TickRare()
        {
            base.TickRare();

            // Set initial state
            MaterialiserStatus = WorkStatus.Working;

            // Check local conditions
            if (AmbientTemperature < 5) MaterialiserStatus = WorkStatus.TooCold;
            if (!Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare))
                MaterialiserStatus = WorkStatus.SolarFlare;
            if (!compPowerTrader.PowerOn) MaterialiserStatus = WorkStatus.NoPower;

            // Check if we have work to do,
            if (CurrentItem == null)
                if (!DequeueItem())
                    MaterialiserStatus = WorkStatus.NoWork;

            // If we can't work, reset progress if any.
            if (MaterialiserStatus != WorkStatus.Working)
            {
                Progress = Progress >= 50 ? 50 : 0;
                compPowerTrader.Props.basePowerConsumption = StandbyPower;
                return;
            }

            // If we have work, determine how much power to do based on speed.
            switch (Speed)
            {
                case ProcessSpeed.Stop:
                    compPowerTrader.Props.basePowerConsumption = StandbyPower;
                    break;
                case ProcessSpeed.Low:
                    compPowerTrader.Props.basePowerConsumption = WattsPerItem;
                    break;
                case ProcessSpeed.Medium:
                    compPowerTrader.Props.basePowerConsumption = WattsPerItem * 2;
                    break;
                case ProcessSpeed.High:
                    compPowerTrader.Props.basePowerConsumption = WattsPerItem * 4;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Tick()
        {
            if(Current.Game.tickManager.TicksGame < nextProgressTick) return;
            UpdateProgress();
            nextProgressTick = Current.Game.tickManager.TicksGame + TicksBetweenProgress;
        }

        private bool DequeueItem()
        {
            var colonyId = gameComponent.GetColonyId(currentMap);
            if (colonyId == null) return false;

            var nextItem = IseCentral.DataCache.GetCollection<DBStorageItem>(Constants.Tables.Delivered)
                .FindOne(item => item.ColonyId == colonyId);

            if (nextItem == null) return false;

            currentItemDbId = nextItem.StoredItemID;
            CurrentItem = ThingMaker.MakeThing(
                DefDatabase<ThingDef>.GetNamed(nextItem.ThingDef),
                nextItem.Stuff.NullOrEmpty() ? null : DefDatabase<ThingDef>.GetNamed(nextItem.Stuff)
            );
            if (nextItem.Quality > 0)
                // Only set quality if we can.
                if (CurrentItem.def.HasComp(typeof(CompQuality)))
                {
                    // Fix some strange errors with the games tale generation
                    // by forcing these items not to have any art assigned.
                    var art = CurrentItem.TryGetComp<CompArt>();
                    if (art != null) ((ThingWithComps) CurrentItem).AllComps.Remove(art);
                    CurrentItem.TryGetComp<CompQuality>().SetQuality((QualityCategory) nextItem.Quality,
                        ArtGenerationContext.Outsider);
                }

            CurrentItem.stackCount = nextItem.Quantity;
            quantityToMaterialise = nextItem.Quantity;

            if (Autorun) Speed = LastSpeed;

            return true;
        }

        private void UpdateProgress()
        {
            if (CurrentItem == null || MaterialiserStatus != WorkStatus.Working) return;

            int amount;
            switch (Speed)
            {
                case ProcessSpeed.Stop:
                    return;
                case ProcessSpeed.Low:
                    amount = 1;
                    break;
                case ProcessSpeed.Medium:
                    amount = 2;
                    break;
                case ProcessSpeed.High:
                    amount = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Progress += amount;

            if (Progress < 100) return;

            Progress = 0;
            LastSpeed = Speed;

            MarkItemComplete();

            if (DequeueItem()) return;

            CurrentItem = null;
        }

        private void MarkItemComplete()
        {
            var collection = IseCentral.DataCache.GetCollection<DBStorageItem>(Constants.Tables.Delivered);
            var thisItem = collection.FindById(currentItemDbId);

            if (thisItem == null)
            {
                Logging.WriteErrorMessage("Item we were trying to make has been removed from the database!");
                CurrentItem = null;
                return;
                
            }

            thisItem.Quantity -= quantityToMaterialise;

            try
            {
                // Drop the items on the floor nearby
                if (!GenPlace.TryPlaceThing(CurrentItem, Position, Map, ThingPlaceMode.Near))
                {
                    throw new InvalidOperationException("TryPlaceThing failed to find a location");
                }
            }
            catch (Exception)
            {
                Logging.WriteErrorMessage($"Failed to place {CurrentItem.Label} on the map!");
                Speed = ProcessSpeed.Stop;
                Progress = 99;
            }
            
            if (thisItem.Quantity <= 0)
            {
                collection.Delete(thisItem.StoredItemID);
            }
            else
            {
                collection.Upsert(thisItem);
            }

        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0) stringBuilder.AppendLine();

            switch (MaterialiserStatus)
            {
                case WorkStatus.NoPower:
                    stringBuilder.AppendLine("NoPower".Translate());
                    break;
                case WorkStatus.SolarFlare:
                    stringBuilder.AppendLine("CannotUseSolarFlare".Translate());
                    break;
                case WorkStatus.TooCold:
                    stringBuilder.AppendLine("ISEMaterialiserTooCold".Translate(AmbientTemperature));
                    break;
                case WorkStatus.Stopped:
                    stringBuilder.AppendLine("ISEMaterialiserStopped".Translate());

                    break;
                case WorkStatus.NoWork:
                    stringBuilder.AppendLine("ISEMaterialiserNoWork".Translate());
                    break;
                case WorkStatus.Working:
                    if (CurrentItem != null)
                    {
                        stringBuilder.AppendLine("ISEMaterialiserWorking".Translate(CurrentItem.LabelCap, Progress));
                        string speed;
                        switch (Speed)
                        {
                            case ProcessSpeed.Stop:
                                speed = "ISEMaterialiserStopped".Translate();
                                break;
                            case ProcessSpeed.Low:
                                speed = "ISEMaterialiserSpeedLow".Translate();
                                break;
                            case ProcessSpeed.Medium:
                                speed = "ISEMaterialiserSpeedMedium".Translate();
                                break;
                            case ProcessSpeed.High:
                                speed = "ISEMaterialiserSpeedFast".Translate();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        stringBuilder.AppendLine("ISEMaterialiserSpeed".Translate(speed));
                    }
                    else
                    {
                        stringBuilder.AppendLine("ISEMaterialiserDownloadingData".Translate());
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            stringBuilder.AppendLine("Temperature".Translate() + ": " + AmbientTemperature.ToStringTemperature("F0"));
            return stringBuilder.ToString().TrimEndNewlines();
        }

        #endregion
    }
}