using MzLibUtil;
using mzPlot;
using System;
using System.Collections.Generic;
using System.Text;

namespace MetaMorpheusGUI
{
    public class DataPlotPreset
    {
        public string Name { get; private set; }
        public string PlotType { get; private set; }
        public string XAxisVariable { get; private set; }
        public string YAxisVariable { get; private set; }

        //TODO: settings for plot type

        public DataPlotPreset(string name, string xAxisVariable, string yAxisVariable, string plotType)
        {
            this.Name = name;
            this.XAxisVariable = xAxisVariable;
            this.YAxisVariable = yAxisVariable;
            this.PlotType = plotType;
        }

        public DataPlotPreset(string line)
        {
            var split = line.Split(new char[] { ',' });
            Name = split[0];
            XAxisVariable = split[1];
            YAxisVariable = split[2];
            PlotType = split[3];
        }

        public Plot ToMzPlot(OxyPlot.Wpf.PlotView plotView, List<Datum> data)
        {
            if (PlotType == "Scatter")
            {
                return new ScatterPlot(plotView, data);
            }
            else if (PlotType == "Line")
            {
                return new LinePlot(plotView, data);
            }
            else if (PlotType == "Histogram")
            {
                return new HistogramPlot(plotView, data, 10);
            }
            else if (PlotType == "Bar")
            {
                return new BarPlot(plotView, data);
            }
            else if (PlotType == "Spectrum")
            {
                return new SpectrumPlot(plotView, data);
            }

            return null;
        }

        public string ToToml()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Name);
            sb.Append(",");

            sb.Append(XAxisVariable);
            sb.Append(",");

            sb.Append(YAxisVariable);
            sb.Append(",");

            sb.Append(PlotType);

            return sb.ToString();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
