#region License
// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, trade_ui.cs, Created 2021-02-11
#endregion

using UnityEngine;
using Verse;

namespace ise.dialogs
{
    public class TradeUI : Window
    {
        public TradeUI(Pawn userPawn)
        {
            forcePause = true;
            absorbInputAroundWindow = true;
            // _player = userPawn;
            // _trader = traderPrime;
            // _mapComponent = Utilities.GetMapComponent(_player.Map);
            // _apiStopwatch = new Stopwatch();
            //
            // if (_mapComponent.HasSentThingListYet)
            // {
            //     _status = DownloadStatus.DoneSendingMods;
            // }
            // else
            // {
            //     // Send supported things
            //     LogWriter.WriteMessage("Starting request to server");
            //     _apiStopwatch.Start();
            //     GlitterWorldApi.PutItemListAsync(_mapComponent, PutThingsListComplete_Callback);
            // }
        }
        
        
        public override Vector2 InitialSize
        {
            get
            {
                //Text.Font = GameFont.Small;
                //var textWidth = Text.CalcSize(_downloadMessage);
                return new Vector2(200f, 200f);
            }
        }
        
        public override void DoWindowContents(Rect inRect)
        {
            throw new System.NotImplementedException();
        }
    }
}