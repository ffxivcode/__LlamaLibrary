﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot.Managers;
using ff14bot.RemoteAgents;
using ff14bot.RemoteWindows;
using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.RemoteWindows
{
    public class SharlayanCraftworksSupply : RemoteWindow<SharlayanCraftworksSupply>
    {
        private const string WindowName = "SharlayanCraftworksSupply";

        public SharlayanCraftworksSupply() : base(WindowName)
        {
        }

        public static readonly Dictionary<string, int> Properties = new Dictionary<string, int>
        {
            {
                "TurnInItemId",
                8
            },
            {
                "EsteemLevel",
                6
            },
            {
                "Esteem",
                7
            },
        };

        public int TurnInItemId => Elements[Properties["TurnInItemId"]].TrimmedData;

        public int EsteemLevel => Elements[Properties["EsteemLevel"]].TrimmedData;

        public int Esteem => Elements[Properties["Esteem"]].TrimmedData;

        public void Deliver()
        {
            SendAction(1, 3, 0);
        }

        public async Task HandOverItems()
        {
            var slots = InventoryManager.FilledSlots.Where(i => i.TrueItemId == TurnInItemId).OrderByDescending(i => i.Collectability).Take(6 - Esteem);

            foreach (var slot in slots)
            {
                if (slot != null)
                {
                    AgentSharlayanCraftworksSupply.Instance.HandIn(slot);
                    await Coroutine.Sleep(700);
                }
            }

            await Coroutine.Sleep(500);

            Deliver();

            await Coroutine.Wait(5000, () => Talk.DialogOpen || SelectYesno.IsOpen);

            if (SelectYesno.IsOpen)
            {
                SelectYesno.Yes();
                await Coroutine.Wait(5000, () => Talk.DialogOpen);
            }

            while (Talk.DialogOpen)
            {
                Talk.Next();
                await Coroutine.Sleep(1000);
            }

            await Coroutine.Wait(5000, () => QuestLogManager.InCutscene);

            while (QuestLogManager.InCutscene || Talk.DialogOpen)
            {
                if (QuestLogManager.InCutscene && AgentCutScene.Instance.CanSkip)
                {
                    AgentCutScene.Instance.PromptSkip();
                    await Coroutine.Wait(5000, () => SelectString.IsOpen);
                    if (SelectString.IsOpen)
                    {
                        SelectString.ClickSlot(0);
                    }
                }

                if (Talk.DialogOpen)
                {
                    Talk.Next();
                    await Coroutine.Sleep(200);
                }

                await Coroutine.Sleep(500);
            }

            await Coroutine.Wait(20000, () => JournalResult.IsOpen);
            if (JournalResult.IsOpen)
            {
                JournalAccept.Accept();
                await Coroutine.Wait(20000, () => !JournalResult.IsOpen);

                await Coroutine.Wait(5000, () => Talk.DialogOpen);
                if (Talk.DialogOpen)
                {
                    while (Talk.DialogOpen)
                    {
                        Talk.Next();
                        await Coroutine.Sleep(1000);
                    }
                }
            }
        }
    }
}