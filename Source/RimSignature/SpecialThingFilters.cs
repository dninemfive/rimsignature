using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimSignature
{
    class SpecialThingFilterWorker_Rechargeable : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            CompGadget cg = t.TryGetComp<CompGadget>();
            return cg != null && cg.Rechargeable;
        }        
    }
    class SpecialThingFilterWorker_NotRechargeable : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            CompGadget cg = t.TryGetComp<CompGadget>();
            return cg == null || !cg.Rechargeable;
        }        
    }
}
