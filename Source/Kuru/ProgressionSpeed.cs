using System;
using Verse;

namespace Kuru
{
    public static class ProgressionSpeedEnumExtensions
    {
        public static string ToStringHuman(this ProgressionSpeed mode)
        {
            switch (mode)
            {
                case ProgressionSpeed.SECCOND:
                    return "ProgressionSpeed_SECOND".Translate();
                case ProgressionSpeed.DAY:
                    return "ProgressionSpeed_DAY".Translate();
                case ProgressionSpeed.QUADRUM:
                    return "ProgressionSpeed_QUADRUM".Translate();
                case ProgressionSpeed.YEAR:
                    return "ProgressionSpeed_YEAR".Translate();
                default:
                    throw new NotImplementedException();
            }
        }

        public static int ToTicks(this ProgressionSpeed mode)
        {
            switch (mode)
            {
                case ProgressionSpeed.SECCOND:
                    return 60;
                case ProgressionSpeed.DAY:
                    return 60000;
                case ProgressionSpeed.QUADRUM:
                    return 900000;
                case ProgressionSpeed.YEAR:
                    return 3600000;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public enum ProgressionSpeed : byte
    {
        SECCOND,
        DAY,
        QUADRUM,
        YEAR
    }
    
}