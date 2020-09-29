using EngineLayer;
using MassSpectrometry;
using MzLibUtil;
using mzPlot;
using OxyPlot;
using Proteomics.Fragmentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace MetaMorpheusGUI
{
    public class PeptideSpectrumMatchPlot : Plot
    {
        protected PsmFromTsv Psm;
        protected MsDataScan Scan;

        public PeptideSpectrumMatchPlot(OxyPlot.Wpf.PlotView plotView, PsmFromTsv psm, MsDataScan scan) : base(plotView)
        {
            Psm = psm;
            Scan = scan;

            DrawSpectrum();
            AnnotateBaseSequence(psm.BaseSeq, 0, 0);
            AnnotateSpectrum(psm.MatchedIons);
        }

        protected void DrawSpectrum()
        {
            List<Datum> spectrumData = new List<Datum>();

            for (int i = 0; i < Scan.MassSpectrum.XArray.Length; i++)
            {
                double mz = Scan.MassSpectrum.XArray[i];
                double intensity = Scan.MassSpectrum.YArray[i];

                spectrumData.Add(new Datum(mz, intensity));
            }

            AddSpectrumPlot(spectrumData);
        }

        protected void AnnotateSpectrum(List<MatchedFragmentIon> ions)
        {
            foreach (var ionSeries in ions.GroupBy(p => p.NeutralTheoreticalProduct.ProductType))
            {
                List<Datum> matchedIonData = new List<Datum>();
                Color color = MetaDrawSettings.productTypeToColor[ionSeries.Key];

                foreach (var ion in ionSeries)
                {
                    int ind = Scan.MassSpectrum.GetClosestPeakIndex(ion.Mz);
                    double intensity = Scan.MassSpectrum.YArray[ind];
                    matchedIonData.Add(new Datum(ion.Mz, intensity));
                }

                AddSpectrumPlot(matchedIonData, lineColor: OxyColor.FromRgb(color.R, color.G, color.B), lineThickness: 2);
            }
        }

        protected void AnnotateBaseSequence(string sequence, int xLoc, int yLoc)
        {
            for (int i = 0; i < sequence.Length; i++)
            {
                AddTextAnnotationToPlotArea(sequence[i].ToString(), xLoc, yLoc);

                xLoc += 20;
            }

            AnnotateModifications(sequence, xLoc, yLoc);
        }

        protected void AnnotateModifications(string sequence, int xLoc, int yLoc)
        {

        }
    }
}
