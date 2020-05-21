using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimSignature
{
    public abstract class Verb_UseGadget : Verb
    {
        CompChargeable Comp => EquipmentSource?.TryGetComp<CompChargeable>() ?? null;

        protected override bool TryCastShot()
        {
            if (Comp != null) return Comp.TryUseCharge();
            return true;
        }
    }
    public class Verb_Subvert : Verb_UseGadget
    {
        protected override bool TryCastShot()
        {
            if(!base.TryCastShot()) return false;
            // do shit
            return true;
        }
    }
}
