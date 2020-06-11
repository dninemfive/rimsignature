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
        public Verb_Subvert() : base()
        {
            Log.Message("Be cool, honey bunny");
            TargetingParameters tp = verbProps.targetParams ?? new TargetingParameters();
            Log.Message("Be cool.");
            tp.validator = delegate (TargetInfo x)
            {
                return CouldEverChangeFaction(x) || HasBiocodableEqOrApparel(x);
            };
            Log.Message("Nothing's gonna happen.");
            VerbProperties vp = verbProps;
            Log.Message("Just put the gun down.");
            vp.targetParams = tp;
            verbProps = vp;
        }

        protected override bool TryCastShot()
        {
            if (!targetParams.validator(CurrentTarget.ToTargetInfo(CurrentTarget.Thing.Map)) || !base.TryCastShot()) return false;
            // make turrets join your team
            if (CurrentTarget.Thing is Building_Turret tur && tur.def.CanHaveFaction)
            {
                tur.SetFactionDirect(Caster.Faction);
                Log.Message("turret subverted! New faction is " + tur.Faction + ".");
            }
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
            if (explosive != null) explosive.parent.SetFactionDirect(Caster.Faction);
            return true;
        }
        public bool CouldEverChangeFaction(TargetInfo x)
        {
            return (x.Thing is Building_Turret tur && tur.def.CanHaveFaction) ||
                   (x.Thing is Building_Door door && door.def.CanHaveFaction) ||
                   (x.Thing is Pawn pawn && pawn.def.race.FleshType == FleshTypeDefOf.Mechanoid) ||
                   (x.Thing.TryGetComp<CompExplosive>() != null && x.Thing.def.CanHaveFaction);
        }
        public bool HasBiocodableEqOrApparel(TargetInfo ti)
        {
            return ti.Thing is Pawn pawn && pawn.equipment.AllEquipmentListForReading.Any(x => x.TryGetComp<CompBiocodable>() != null) && pawn.apparel.WornApparel.Any(x => x.TryGetComp<CompBiocodable>() != null);
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
