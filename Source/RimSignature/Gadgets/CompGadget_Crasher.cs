using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimSignature
{
    class CompGadget_Crasher : CompGadget
    {
        public override bool CanTarget(TargetInfo target, Thing caster, bool sendMessage = false)
        {
            if(target.Thing.TryGetComp<CompBreakdownable>() == null && target.Thing.TryGetComp<CompExplosive>() == null)
            {
                if(sendMessage) Messages.Message("D9RS_CrashInvalidTarget".Translate(), new LookTargets(target, caster), MessageTypeDefOf.NeutralEvent, false);
            }
            return true;
        }
        public override void DoEffect(TargetInfo target, Thing caster)
        {
            CompBreakdownable breakdownable = target.Thing.TryGetComp<CompBreakdownable>();
            if (breakdownable != null) breakdownable.DoBreakdown();
            CompExplosive explosive = target.Thing.TryGetComp<CompExplosive>();
            if (explosive != null) explosive.StartWick();
        }
    }
}
