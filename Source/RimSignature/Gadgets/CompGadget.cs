using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace RimSignature
{
    public abstract class CompGadget : ThingComp
    {
        #region props/fields
        CompProperties_Gadget Props => (CompProperties_Gadget)base.props;
        private int charges;
        public int Charges {
            get => charges;
            set
            {
                charges = Mathf.Clamp(value, 0, MaxCharges);
            }
        }
        public int MaxCharges;
        public bool Rechargeable = false;
        public bool SelfCharging = false;
        public bool InChargingStation => false;
        public string CapacityStr
        {
            get
            {
                switch (MaxCharges)
                {
                    case 1:
                    case 2:
                        return "D9RS_LowCapacity".Translate();
                    case 3:
                    case 4:
                        return "D9RS_MidCapacity".Translate();
                    case 5:
                    case 6:
                    case 7:
                        return "D9RS_HighCapacity".Translate();
                    default: return null;
                }
            }
        }
        public bool DestroyOnZeroCharges => !(Rechargeable || SelfCharging);
        // adjusts width of gizmos
        public static int Modifier = 10;
        #endregion props/fields

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                QualityCategory quality = base.parent.TryGetComp<CompQuality>()?.Quality ?? QualityCategory.Normal;
                Generate(quality);
                Charges = MaxCharges;
            }
            // get first verb of type Verb_UseGadget and set its validator to use CanTarget. Hacky, but should work.
            base.parent.def.Verbs.Where(x => x.verbClass == typeof(Verb_UseGadget)).First().targetParams.validator = delegate(TargetInfo ti) { return CanTarget(ti); } ;
        }

        public void Generate(QualityCategory quality)
        {
            int qualityInt = ((int)quality)+1;
            MaxCharges = new IntRange((int)Props.minChargeCurve.Evaluate(qualityInt), (int)Props.maxChargeCurve.Evaluate(qualityInt)).RandomInRange;
            if (Rand.Value < Props.rechargeabilityChanceCurve.Evaluate(qualityInt))
            {
                if (Rand.Bool) Rechargeable = true;
                else SelfCharging = true;
            }
            if(Rand.Value < Props.rcOverlapChanceCurve.Evaluate(qualityInt) && (Rechargeable || SelfCharging))
            {
                Rechargeable = true;
                SelfCharging = true;
            }
        }
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra()) yield return g;
            yield return new Gizmo_ChargeCounter{ comp = this };
            if (Prefs.DevMode)
            {
                /*yield return new Command_Action
                {
                    action = () => TryUseCharge(null),
                    defaultLabel = "remove charge"
                };*/
                yield return new Command_Action
                {
                    action = () => Modifier++,
                    defaultLabel = "adjust modifier",
                    defaultDesc = "" + Modifier
                };
            }
        }

        public override string TransformLabel(string label)
        {
            string prefix = CapacityStr ?? "";
            if (Rechargeable && SelfCharging) {
                prefix += " " + "D9RS_SelfReCharging".Translate();
            }
            else
            {
                if (Rechargeable) prefix += " " + "D9RS_Rechargeable".Translate();
                if (SelfCharging) prefix += " " + "D9RS_SelfCharging".Translate();
            }
            string postfix = (Rechargeable || SelfCharging) ? "(" + Charges + "/" + MaxCharges + ")" : "(" + Charges + ")";
            if (prefix.Length > 0) label = prefix + " " + label;
            return label + " " + postfix;
        }

        public override string CompInspectStringExtra()
        {
            if (Rechargeable || SelfCharging) return "D9RS_ChargesRemainingChargeable".Translate(Charges, MaxCharges);
            return "D9RS_ChargesRemaining".Translate(Charges);
        }

        public bool TryUseCharge(TargetInfo target, Thing caster)
        {
            if (!CanTarget(target, caster)) return false;
            if (Charges <= 0) return false;
            DoEffect(target, caster);
            Charges--;
            if (DestroyOnZeroCharges && Charges <= 0) base.parent.SplitOff(1).Destroy();
            return true;
        }

        public abstract bool CanTarget(TargetInfo target, Thing caster = null);
        public abstract void DoEffect(TargetInfo target, Thing caster);
    }
    public class CompProperties_Gadget : CompProperties
    {
        public IntRange allowedChargeRange = new IntRange(1, 7);
        // for the following, Awful = 1 and Legendary = 7
        public SimpleCurve minChargeCurve = new SimpleCurve
            {
                new CurvePoint(1, 1),
                new CurvePoint(4, 1),
                new CurvePoint(5, 2),
                new CurvePoint(6, 3),
                new CurvePoint(7, 4)
            },
            maxChargeCurve = new SimpleCurve
            {
                new CurvePoint(1, 1),
                new CurvePoint(2, 2),
                new CurvePoint(3, 4),
                new CurvePoint(4, 4),
                new CurvePoint(5, 5),
                new CurvePoint(6, 6),
                new CurvePoint(7, 7)
            }, rechargeabilityChanceCurve = new SimpleCurve
            {
                new CurvePoint(1, 0),
                new CurvePoint(2, 0.05f),
                new CurvePoint(3, 0.1f),
                new CurvePoint(4, 0.25f),
                new CurvePoint(5, 0.33f),
                new CurvePoint(6, 0.5f),
                new CurvePoint(7, 1f)
            }, rcOverlapChanceCurve = new SimpleCurve
            {
                new CurvePoint(1, 0),
                new CurvePoint(3, 0.05f),
                new CurvePoint(7, 0.1f)
            };
    }
}
