﻿namespace EngineLayer
{
    public class FdrInfo
    {
        public int cumulativeTarget { get; set; }
        public int cumulativeDecoy { get; set; }
        public int cumulativeTargetNotch { get; set; }
        public int cumulativeDecoyNotch { get; set; }
        public double QValue { get; set; }
        public double QValueNotch { get; set; }
    }
}