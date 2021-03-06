﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimSignature
{
    class CompCharger : ThingComp
    {
        Building_Storage Storage => base.parent as Building_Storage;
        IEnumerable<Thing> ThingsToCharge => Storage.slotGroup.HeldThings;
        public CompProperties_Charger Props => (CompProperties_Charger)base.props;
        CompPowerTrader Power => base.parent.TryGetComp<CompPowerTrader>();
        CompRefuelable Fuel => base.parent.TryGetComp<CompRefuelable>();

        #region cheap hash interval stuff
        private int hashOffset = 0;
        public bool IsCheapIntervalTick(int interval) => (Find.TickManager.TicksGame + hashOffset) % interval == 0;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            hashOffset = parent.thingIDNumber.HashOffset();
        }
        #endregion cheap hash interval stuff

        public override void CompTick()
        {
            base.CompTick();
            if(Storage == null)
            {
                string err = "[RimSignature] CompCharger on a Thing which is not a Building_Storage!";
                Log.ErrorOnce(err, err.GetHashCode());
                return;
            }
            if (IsCheapIntervalTick(Props.Interval) && (Fuel == null || Fuel.HasFuel) && (Power == null || Power.PowerOn))
            {
                int rechargedCount = 0;
                foreach(Thing t in ThingsToCharge)
                {
                    CompGadget cg;
                    if((cg = t.TryGetComp<CompGadget>()) != null && cg.Rechargeable)
                    {
                        cg.Charges += Props.ChargesToRestore;
                        if (Fuel != null) Fuel.ConsumeFuel(Props.FuelPerRecharge);
                    }
                }
                Power.PowerOutput = -1 * Props.PowerUsePerGadget * rechargedCount;
            }
        }
    }
    class CompProperties_Charger : CompProperties
    {
#pragma warning disable CS0649
        public int Interval = 250;
        public int ChargesToRestore = 1;
        public float PowerUsePerGadget = 500f;
        public float FuelPerRecharge = 0f;
#pragma warning restore CS0649
        public CompProperties_Charger()
        {
            base.compClass = typeof(CompCharger);
        }
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string s in base.ConfigErrors(parentDef)) yield return s;
            if (parentDef.thingClass != typeof(Building_Storage)) yield return "Parent thing isn't a Building_Storage!";
            if (parentDef.tickerType != TickerType.Normal) yield return "Parent ticker type isn't Normal!";
        }
    }
}
