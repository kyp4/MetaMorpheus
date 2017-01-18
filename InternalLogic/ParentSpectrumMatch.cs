﻿using OldInternalLogic;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace InternalLogicEngineLayer
{
    public abstract class ParentSpectrumMatch
    {

        #region Public Fields

        public readonly string fileName;
        public readonly int scanNumber;
        public readonly double scanRetentionTime;
        public readonly int scanExperimentalPeaks;
        public readonly double totalIonCurrent;
        public readonly double scanPrecursorIntensity;
        public readonly int scanPrecursorCharge;
        public readonly double scanPrecursorMZ;
        public readonly double scanPrecursorMass;
        public readonly double score;

        #endregion Public Fields

        #region Internal Fields

        internal Dictionary<ProductType, double[]> matchedIonsList;
        internal List<double> LocalizedScores;

        #endregion Internal Fields

        #region Protected Constructors

        protected ParentSpectrumMatch(string fileName, double scanRetentionTime, double scanPrecursorIntensity, double scanPrecursorMass, int scanNumber, int scanPrecursorCharge, int scanExperimentalPeaks, double totalIonCurrent, double scanPrecursorMZ, double score)
        {
            this.fileName = fileName;
            this.scanNumber = scanNumber;
            this.scanRetentionTime = scanRetentionTime;
            this.scanExperimentalPeaks = scanExperimentalPeaks;
            this.totalIonCurrent = totalIonCurrent;
            this.scanPrecursorIntensity = scanPrecursorIntensity;
            this.scanPrecursorCharge = scanPrecursorCharge;
            this.scanPrecursorMZ = scanPrecursorMZ;
            this.scanPrecursorMass = scanPrecursorMass;
            this.score = score;
        }

        #endregion Protected Constructors

        #region Public Methods

        public abstract CompactPeptide GetCompactPeptide(List<MorpheusModification> variableModifications, List<MorpheusModification> localizeableModifications);

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(fileName + '\t');
            sb.Append(scanNumber.ToString(CultureInfo.InvariantCulture) + '\t');
            sb.Append(scanRetentionTime.ToString("F5", CultureInfo.InvariantCulture) + '\t');
            sb.Append(scanExperimentalPeaks.ToString("F5", CultureInfo.InvariantCulture) + '\t');
            sb.Append(totalIonCurrent.ToString("F5", CultureInfo.InvariantCulture) + '\t');
            sb.Append(scanPrecursorIntensity.ToString("F5", CultureInfo.InvariantCulture) + '\t');
            sb.Append(scanPrecursorCharge.ToString("F5", CultureInfo.InvariantCulture) + '\t');
            sb.Append(scanPrecursorMZ.ToString("F5", CultureInfo.InvariantCulture) + '\t');
            sb.Append(scanPrecursorMass.ToString("F5", CultureInfo.InvariantCulture) + '\t');
            sb.Append(score.ToString("F3", CultureInfo.InvariantCulture) + '\t');

            sb.Append("[");
            foreach (var kvp in matchedIonsList)
                sb.Append("[" + string.Join(",", kvp.Value.Where(b => b > 0).Select(b => b.ToString("F5", CultureInfo.InvariantCulture))) + "];");
            sb.Append("]" + '\t');

            sb.Append(string.Join(";", matchedIonsList.Select(b => b.Value.Count(c => c > 0))) + '\t');

            sb.Append("[" + string.Join(",", LocalizedScores.Select(b => b.ToString("F3", CultureInfo.InvariantCulture))) + "]" + '\t');

            sb.Append((LocalizedScores.Max() - score).ToString("F3", CultureInfo.InvariantCulture) + '\t');

            if (LocalizedScores.IndexOf(LocalizedScores.Max()) == 0)
                sb.Append("N");
            else if (LocalizedScores.IndexOf(LocalizedScores.Max()) == LocalizedScores.Count - 1)
                sb.Append("C");
            else
                sb.Append("");

            return sb.ToString();
        }

        #endregion Public Methods

        #region Internal Methods

        internal static string GetTabSeparatedHeader()
        {
            var sb = new StringBuilder();
            sb.Append("fileName" + '\t');
            sb.Append("scanNumber" + '\t');
            sb.Append("scanRetentionTime" + '\t');
            sb.Append("scanExperimentalPeaks" + '\t');
            sb.Append("totalIonCurrent" + '\t');
            sb.Append("scanPrecursorIntensity" + '\t');
            sb.Append("scanPrecursorCharge" + '\t');
            sb.Append("scanPrecursorMZ" + '\t');
            sb.Append("scanPrecursorMass" + '\t');
            sb.Append("score" + '\t');

            sb.Append("matched ions" + '\t');
            sb.Append("matched ion counts" + '\t');
            sb.Append("localized scores" + '\t');
            sb.Append("improvement" + '\t');
            sb.Append("terminal localization");
            return sb.ToString();
        }

        #endregion Internal Methods

    }
}