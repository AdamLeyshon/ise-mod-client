#region license

// #region License
// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, isematerialiser.cs 2021-03-07
// #endregion

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using ise.components;
using ise.lib;
using ise_core.db;
using ise_core.extend;
using RimWorld;
using UnityEngine;
using Verse;

namespace ise.buildings
{
    [StaticConstructorOnStartup]
    public class ISEMaterialiser : Building
    {
        #region ProcessSpeed enum

        public enum ProcessSpeed
        {
            Stop,
            Low,
            Medium,
            High,
            Hyper,
            Insane
        }

        #endregion

        #region WorkStatus enum

        public enum WorkStatus
        {
            NoPower,
            CosmicEvent,
            TooCold,
            Stopped,
            NoWork,
            Working
        }

        #endregion

        #region Fields

        private const int WattsPerItemTick = 50;
        private const int StandbyPower = 25;
        private const int TicksBetweenProgress = 100;
        private static readonly Texture2D SpeedIcon = ContentFinder<Texture2D>.Get("UI/Commands/tempreset");
        private static readonly Texture2D StopIcon = ContentFinder<Texture2D>.Get("UI/Commands/Halt");
        private static readonly Texture2D GoIcon = ContentFinder<Texture2D>.Get("UI/Designators/ForbidOff");

        private CompPowerTrader _compPowerTrader;
        private string _currentItemDbId;

        private ISEGameComponent _gameComponent;
        private Map _currentMap;
        private int _nextProgressTick;
        private int _progressValue;
        private bool _autorun = true;
        private ProcessSpeed _speed = ProcessSpeed.Medium;

        #endregion

        #region Properties

        private bool Autorun
        {
            get => _autorun;
            set => _autorun = value;
        }

        private ProcessSpeed Speed
        {
            get => _speed;
            set => _speed = value;
        }

        private ProcessSpeed LastSpeed { get; set; }

        private WorkStatus MaterialiserStatus { get; set; }

        private int ProgressPercent { get; set; }

        private int ProgressValue
        {
            get => _progressValue;
            set => _progressValue = value;
        }

        private int TotalValue { get; set; }

        private Thing CurrentItem { get; set; }

        #endregion

        #region Methods

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            _compPowerTrader = GetComp<CompPowerTrader>();
            _gameComponent = Current.Game.GetComponent<ISEGameComponent>();
            _currentMap = map;
            
            if (respawningAfterLoad)
            {
                // Don't tick straight away
                _nextProgressTick = Current.Game.tickManager.TicksGame + TicksBetweenProgress;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _progressValue, "progress");
            Scribe_Values.Look(ref _currentItemDbId, "item");
            Scribe_Values.Look(ref _speed, "speed", ProcessSpeed.Medium);
            Scribe_Values.Look(ref _autorun, "autorun", true, true);

            if (Scribe.mode != LoadSaveMode.PostLoadInit) return;
            
            // Don't tick straight away
            _nextProgressTick = Current.Game.tickManager.TicksGame + TicksBetweenProgress;
            
            if (_currentItemDbId == null) return;

            var loadItem = IseCentral.DataCache.GetCollection<DBStorageItem>(Constants.Tables.Delivered)
                .FindOne(item => item.StoredItemID == _currentItemDbId);

            // It's possible that the save is referencing an item that was later on delivered,
            // But then they reloaded, if the server wasn't updated then it might start pick up the order
            // again, but it they've gone back too far, then the time warp code should roll back the order.
            if (loadItem == null) return;

            CreateItemFromDB(loadItem);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos()) yield return gizmo;

            // Add button to cycle to next speed.
            var action = new Command_Action
            {
                icon = SpeedIcon,
                defaultLabel = $"Speed\n({LookupSpeedTranslation(Speed)})", action = delegate { Speed = Speed.Next(); }
            };
            yield return action;

            // Add button to enable/disable Autorun.
            action = new Command_Action
            {
                icon = Autorun ? GoIcon : StopIcon,
                defaultLabel = $"Autorun ({(Autorun ? "On" : "Off")})", action = delegate { Autorun = !Autorun; }
            };
            yield return action;
        }

        public override void Tick()
        {
            if (Current.Game.tickManager.TicksGame < _nextProgressTick) return;
            UpdateProgress();
            _nextProgressTick = Current.Game.tickManager.TicksGame + TicksBetweenProgress;
        }

        private void CreateItemFromDB(DBStorageItem dbItem)
        {
            var thingDef = DefDatabase<ThingDef>.GetNamed(dbItem.ThingDef);
            _currentItemDbId = dbItem.StoredItemID;
            CurrentItem = ThingMaker.MakeThing(thingDef,
                dbItem.Stuff.NullOrEmpty() ? null : DefDatabase<ThingDef>.GetNamed(dbItem.Stuff)
            );

            if (dbItem.Quality > 0)
                // Only set quality if we can.
                if (CurrentItem.def.HasComp(typeof(CompQuality)))
                    // Test allowing art. Ordering too many things with Art could cause issues though.
                    // var art = CurrentItem.TryGetComp<CompArt>();
                    // if (art != null) ((ThingWithComps) CurrentItem).AllComps.Remove(art);
                    CurrentItem.TryGetComp<CompQuality>().SetQuality(
                        (QualityCategory)dbItem.Quality,
                        ArtGenerationContext.Outsider
                    );

            if (thingDef.Minifiable)
            {
                CurrentItem.stackCount = 1;
                CurrentItem = CurrentItem.MakeMinified();
                TotalValue = dbItem.Value / dbItem.Quantity;
            }
            else
            {
                CurrentItem.stackCount = Math.Min(dbItem.Quantity, CurrentItem.def.stackLimit);
                TotalValue = dbItem.Value;
            }
        }

        private bool DequeueItem()
        {
            var colonyId = _gameComponent.GetColonyId(_currentMap);
            if (colonyId == null) return false;

            var nextItem = IseCentral.DataCache.GetCollection<DBStorageItem>(Constants.Tables.Delivered)
                .FindOne(item => item.ColonyId == colonyId);

            ProgressPercent = 0;
            ProgressValue = 0;
            TotalValue = 0;

            if (nextItem == null) return false;

            CreateItemFromDB(nextItem);

            if (Autorun) Speed = LastSpeed;

            return true;
        }

        private void UpdateProgress()
        {
            // Set initial state
            MaterialiserStatus = WorkStatus.Working;

            // Check local conditions
            if (AmbientTemperature < 5) MaterialiserStatus = WorkStatus.TooCold;
            if (Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare))
                MaterialiserStatus = WorkStatus.CosmicEvent;
            if (Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.EMIField))
                MaterialiserStatus = WorkStatus.CosmicEvent;
            if (!_compPowerTrader.PowerOn) MaterialiserStatus = WorkStatus.NoPower;

            // If we can't work, reset progress if any.
            if (MaterialiserStatus != WorkStatus.Working)
            {
                ProgressPercent = ProgressPercent >= 50 ? 50 : 0;
                _compPowerTrader.PowerOutput = 0f - StandbyPower;
                return;
            }

            // If we have work, determine how much power to use based on speed.
            var consumePower = LookupPowerSpeedMap(Speed, false);

            // Check if we have work to do,
            if (CurrentItem == null && (Autorun || Speed != ProcessSpeed.Stop))
                if (!DequeueItem())
                {
                    MaterialiserStatus = WorkStatus.NoWork;
                    consumePower = StandbyPower;
                }

            // Drain the power we will consume until the next tick
            _compPowerTrader.PowerOutput = 0f - consumePower;

            if (CurrentItem == null || MaterialiserStatus != WorkStatus.Working) return;

            // Add any progress we made since the last tick, use the speed we recorded last time
            // This stops people getting large increments for free by
            // Changing the speed up/down before ticks
            ProgressValue += LookupPowerSpeedMap(LastSpeed, true);
            // Don't remove these parentheses, the compiler is lying, it will stop the percentage
            // from calculating properly
            ProgressPercent = (int)(Math.Floor(((double)ProgressValue / TotalValue) * 100));

            Logging.WriteDebugMessage($"Materialiser Progress {ProgressValue} ({ProgressValue}/{TotalValue})");

            // Store the speed after the update ready for use next time
            LastSpeed = Speed;

            if (ProgressValue < TotalValue) return;

            ProgressPercent = 0;
            ProgressValue = 0;
            TotalValue = 0;

            MarkItemComplete();

            if (DequeueItem()) return;

            CurrentItem = null;
        }

        private int LookupPowerSpeedMap(ProcessSpeed speed, bool progress)
        {
            switch (Speed)
            {
                case ProcessSpeed.Stop:
                    return progress ? 0 : StandbyPower;
                case ProcessSpeed.Low:
                    return progress ? 15 : WattsPerItemTick;
                case ProcessSpeed.Medium:
                    return progress ? 25 : WattsPerItemTick * 2;
                case ProcessSpeed.High:
                    return progress ? 50 : WattsPerItemTick * 4;
                case ProcessSpeed.Hyper:
                    return progress ? 100 : WattsPerItemTick * 60;
                case ProcessSpeed.Insane:
                    return progress ? 200 : WattsPerItemTick * 120;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string LookupSpeedTranslation(ProcessSpeed speed)
        {
            switch (Speed)
            {
                case ProcessSpeed.Stop:
                    return "ISEMaterialiserSpeedStop".Translate();
                case ProcessSpeed.Low:
                    return "ISEMaterialiserSpeedLow".Translate();
                case ProcessSpeed.Medium:
                    return "ISEMaterialiserSpeedMedium".Translate();
                case ProcessSpeed.High:
                    return "ISEMaterialiserSpeedFast".Translate();
                case ProcessSpeed.Hyper:
                    return "ISEMaterialiserSpeedHyper".Translate();
                case ProcessSpeed.Insane:
                    return "ISEMaterialiserSpeedInsane".Translate();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void MarkItemComplete()
        {
            var collection = IseCentral.DataCache.GetCollection<DBStorageItem>(Constants.Tables.Delivered);
            var thisItem = collection.FindById(_currentItemDbId);

            if (thisItem == null)
            {
                Logging.WriteErrorMessage("Item we were trying to make has been removed from the database!");
                CurrentItem = null;
                return;
            }

            thisItem.Quantity -= CurrentItem.stackCount;

            try
            {
                // Drop the items on the floor nearby
                if (!GenPlace.TryPlaceThing(CurrentItem, Position, Map, ThingPlaceMode.Near))
                    throw new InvalidOperationException("TryPlaceThing failed to find a location");
            }
            catch (Exception)
            {
                Logging.WriteErrorMessage($"Failed to place {CurrentItem.Label} on the map!");
                Speed = ProcessSpeed.Stop;
                ProgressPercent = 0;
                ProgressValue = 0;
            }

            if (thisItem.Quantity <= 0)
                collection.Delete(thisItem.StoredItemID);
            else
                collection.Upsert(thisItem);
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
                case WorkStatus.CosmicEvent:
                    stringBuilder.AppendLine("ISEMaterialiserCosmicEvent".Translate());
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
                        stringBuilder.AppendLine(
                            $"{"ISEMaterialiserWorking".Translate(CurrentItem.LabelCap)}, " +
                            $"{ProgressPercent}%");
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
                            case ProcessSpeed.Hyper:
                                speed = "ISEMaterialiserSpeedHyper".Translate();
                                break;
                            case ProcessSpeed.Insane:
                                speed = "ISEMaterialiserSpeedInsane".Translate();
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

        internal static bool HasBuilding(Map map)
        {
            // If we've got multiple buildings, only one needs to be powered to be true
            var buildingList = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("ISEMaterialiser"));
            foreach (var building in buildingList)
            {
                var powerTrader = building.TryGetComp<CompPowerTrader>();
                // Break early if we find one that's on
                if (powerTrader != null && powerTrader.PowerOn) return true;
            }

            return false;
        }

        #endregion
    }
}