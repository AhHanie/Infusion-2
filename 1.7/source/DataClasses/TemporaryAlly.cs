using Verse;

namespace Infusion
{
    public class TemporaryAlly
    {
        public Pawn pawn;
        public ThingWithComps source;

        public TemporaryAlly(Pawn pawn, ThingWithComps source)
        {
            this.pawn = pawn;
            this.source = source;
        }
    }

}