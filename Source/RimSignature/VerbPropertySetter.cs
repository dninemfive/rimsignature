using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimSignature
{
    [StaticConstructorOnStartup]
    static class VerbPropertySetter
    {
        static VerbPropertySetter()
        {
            List<ThingDef> defsWithSubvert = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.Verbs.Where(y => y.verbClass == typeof(Verb_Subvert)).Any()).ToList();
            Predicate<TargetInfo> subvertValidator = (TargetInfo t) => { return CouldEverChangeFaction(t) || HasBiocodableEqOrApparel(t); };
            foreach(ThingDef td in defsWithSubvert) foreach(VerbProperties vp in td.Verbs.Where(x => x.verbClass == typeof(Verb_Subvert)))
                {
                    if (vp.targetParams == null) vp.targetParams = new TargetingParameters();
                    vp.targetParams.validator = subvertValidator;
                }
        }
        public static bool CouldEverChangeFaction(TargetInfo x)
        {
            return (x.Thing is Building_Turret tur && tur.def.CanHaveFaction) ||
                   (x.Thing is Building_Door door && door.def.CanHaveFaction) ||
                   (x.Thing is Pawn pawn && pawn.def.race.FleshType == FleshTypeDefOf.Mechanoid) ||
                   (x.Thing.TryGetComp<CompExplosive>() != null && x.Thing.def.CanHaveFaction);
        }
        public static bool HasBiocodableEqOrApparel(TargetInfo ti)
        {
            return ti.Thing is Pawn pawn && pawn.equipment.AllEquipmentListForReading.Any(x => x.TryGetComp<CompBiocodable>() != null) && pawn.apparel.WornApparel.Any(x => x.TryGetComp<CompBiocodable>() != null);
        }
    }
}
