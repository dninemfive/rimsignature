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
        CompGadget Comp => EquipmentSource?.TryGetComp<CompGadget>();

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
            if (!base.TryCastShot()) return false;
            // make turrets join your team
            if (CurrentTarget.Thing is Building_Turret tur && tur.def.CanHaveFaction) tur.SetFactionDirect(Caster.Faction);
            // unlock autodoors
            if (CurrentTarget.Thing is Building_Door door && door.def.CanHaveFaction && door.TryGetComp<CompPower>() != null) door.SetFactionDirect(Caster.Faction);
            if (CurrentTarget.Thing is Pawn pawn)
            {
                // code biocoded weapons and apparel to the caster
                if(CasterIsPawn) SubvertBiocodedWeapons(pawn, CasterPawn);
                // if pawn is wearing shield belt, set class to custom inverted version if possible
                // convert mechanoids to your team
                // TODO: make this a chance instead of guaranteed, based on the mechanoid's health and max HP/combat strength
                if (pawn.def.race.FleshType == FleshTypeDefOf.Mechanoid) pawn.SetFactionDirect(Caster.Faction);
            }
            CompExplosive explosive = CurrentTarget.Thing.TryGetComp<CompExplosive>();
            if (explosive != null) explosive.StartWick();
            return true;
        }
        public static void SubvertBiocodedWeapons(Pawn target, Pawn subverter)
        {
            Pawn_EquipmentTracker eqTracker = target.equipment;
            foreach(Thing eq in eqTracker.AllEquipmentListForReading)
            {
                // if biocodable and biocoded, code to subverter
                CompBiocodable biocode = eq.TryGetComp<CompBiocodable>();
                if (biocode != null && biocode.Biocoded)
                {
                    biocode.CodeFor(subverter);
                    eqTracker.TryDropEquipment(eqTracker.Primary, out ThingWithComps dontcare, target.Position);
                }
            }
            Pawn_ApparelTracker apTracker = target.apparel;
            foreach(Apparel ap in apTracker.WornApparel)
            {
                CompBiocodable biocode = ap.TryGetComp<CompBiocodable>();
                if(biocode != null && biocode.Biocoded)
                {
                    biocode.CodeFor(subverter);
                    apTracker.TryDrop(ap);
                }
            }
        }
    }
    public class Verb_Crash : Verb_UseGadget
    {
        protected override bool TryCastShot()
        {
            if (!base.TryCastShot()) return false;
            CompBreakdownable breakdownable = CurrentTarget.Thing.TryGetComp<CompBreakdownable>();
            if (breakdownable != null) breakdownable.DoBreakdown();
            return true;
        }
    }
}
