using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utils
{
    public class MathUtil
    {
        public static Random Random = new Random();
        public static int RoundToInt(float f)
        {
            return (int)Math.Round((double)f);
        }
        public static float Clamp01(float value)
        {
            if(value <0f)
            {
                return 0f;
            }
            if (value > 1f)
            {
                return 1f;
            }
            return value;
        }
        public static float Clamp(float value,float min,float max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }
    }
}
