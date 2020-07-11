using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimSignature
{
    public class Verb_UseGadget : Verb
    {
        CompGadget Comp => EquipmentSource?.TryGetComp<CompGadget>();

        protected override bool TryCastShot()
        {
            if (Comp != null) return Comp.TryUseCharge(CurrentTarget.ToTargetInfo(CurrentTarget.Thing.Map), Caster);
            return false;
        }
    }
}
