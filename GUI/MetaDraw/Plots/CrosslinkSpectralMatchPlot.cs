using EngineLayer;
using MassSpectrometry;
using mzPlot;
using System;
using System.Collections.Generic;
using System.Text;

namespace MetaMorpheusGUI
{
    public class CrosslinkSpectralMatchPlot : PeptideSpectrumMatchPlot
    {
        protected PsmFromTsv Csm;

        public CrosslinkSpectralMatchPlot(OxyPlot.Wpf.PlotView plotView, PsmFromTsv csm, MsDataScan scan) : base(plotView, csm, scan)
        {
            Csm = csm;

            // annotate beta peptide base sequence
            AnnotateBaseSequence(csm.BetaPeptideBaseSequence, 0, 20);

            // annotate beta peptide matched ions
            AnnotateSpectrum(csm.BetaPeptideMatchedIons);

            // annotate crosslinker
            AnnotateCrosslinker();
        }

        protected void AnnotateCrosslinker()
        {

        }
    }
}
