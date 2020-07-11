using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimSignature
{
    class CompGadget_Subverter : CompGadget
    {
        public override bool CanTarget(TargetInfo target, Thing caster)
        {
            if (target.Thing?.Faction == caster.Faction)
            {
                Messages.Message("D9RS_TargetSameFaction".Translate(), new LookTargets(target, caster), MessageTypeDefOf.NeutralEvent, false);
                return false;
            }
            if (!(target.Thing is Building_Turret || 
                (target.Thing is Building_Door && target.Thing.TryGetComp<CompPower>() != null) || 
                target.Thing is Pawn || 
                target.Thing.TryGetComp<CompExplosive>() != null))
            {
                Messages.Message("D9RS_SubvertInvalidTarget".Translate(), new LookTargets(target, caster), MessageTypeDefOf.NeutralEvent, false);
                return false;
            }
            return true;
        }
        // TODO: sound effect when successfully subverting
        public override void DoEffect(TargetInfo target, Thing caster)
        {
            // make turrets join your team
            if (target.Thing is Building_Turret tur && tur.def.CanHaveFaction)
            {
                tur.SetFactionDirect(caster.Faction);
                Log.Message("turret subverted! New faction is " + tur.Faction + ".");
            }
            // unlock autodoors
            if (target.Thing is Building_Door door && door.def.CanHaveFaction && door.TryGetComp<CompPower>() != null) door.SetFactionDirect(caster.Faction);
            if (target.Thing is Pawn targetPawn)
            {
                // code biocoded weapons and apparel to the caster
                // TODO: check to make sure pawn has biocoded gear
                if (caster is Pawn casterPawn) SubvertBiocodedGear(targetPawn, casterPawn);
                // if pawn is wearing shield belt, set class to custom inverted version if possible
                // convert mechanoids to your team
                // TODO: make this a chance instead of guaranteed, based on the mechanoid's health and max HP/combat strength
                if (targetPawn.def.race.FleshType == FleshTypeDefOf.Mechanoid) targetPawn.SetFactionDirect(caster.Faction);
            }
            CompExplosive explosive = target.Thing.TryGetComp<CompExplosive>();
            if (explosive != null) explosive.parent.SetFactionDirect(caster.Faction);
        }
        public static void SubvertBiocodedGear(Pawn target, Pawn subverter)
        {
            Pawn_EquipmentTracker eqTracker = target.equipment;
            foreach (Thing eq in eqTracker.AllEquipmentListForReading)
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
            foreach (Apparel ap in apTracker.WornApparel)
            {
                CompBiocodable biocode = ap.TryGetComp<CompBiocodable>();
                if (biocode != null && biocode.Biocoded)
                {
                    biocode.CodeFor(subverter);
                    apTracker.TryDrop(ap);
                }
            }
        }
    }
}
