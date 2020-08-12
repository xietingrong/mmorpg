using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utils
{
    public class MathUtil
    {
        public static int RoundToInt(float f)
        {
            return (int)Math.Round((double)f);
        }
    }
}
