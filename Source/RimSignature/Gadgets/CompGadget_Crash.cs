using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimSignature
{
    class CompGadget_Crash : CompGadget
    {
        public override bool CanTarget(TargetInfo target, Thing caster)
        {
            throw new NotImplementedException();
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
