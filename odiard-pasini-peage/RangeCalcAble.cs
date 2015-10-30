using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace odiard_pasini_peage
{
    public class RangeCalcAble
    {
        public double PosX;
        public double PosY;

        public RangeCalcAble()
        {
            // Nothing to do
        }

        public RangeCalcAble(double paramX, double paramY)
        {
            PosX = paramX;
            PosY = paramY;
        }

        public double DistanceTo(RangeCalcAble paramObject)
        {
            return Math.Sqrt((paramObject.PosX - PosX) * (paramObject.PosX - PosX) + (paramObject.PosY - PosY) * (paramObject.PosY - PosY));
        }

        // Fast range calc thanks to V.Mathivet
        public double SqrDistanceTo(RangeCalcAble paramObject)
        {
            return (paramObject.PosX - PosX) * (paramObject.PosX - PosX) + (paramObject.PosY - PosY) * (paramObject.PosY - PosY);
        }
    }
}
