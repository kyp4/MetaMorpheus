using EngineLayer;
using MzLibUtil;
using mzPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MetaMorpheusGUI
{
    /// <summary>
    /// Interaction logic for AddPlotWindow.xaml
    /// </summary>
    public partial class AddPlotWindow : Window
    {
        private OxyPlot.Wpf.PlotView PlotView;
        ObservableCollection<DataPlotPreset> Presets;
        ObservableCollection<PsmFromTsv> Psms;
        Plot plot;

        public AddPlotWindow(OxyPlot.Wpf.PlotView plotView, ObservableCollection<PsmFromTsv> psms)
        {
            InitializeComponent();

            PlotView = plotView;
            Presets = new ObservableCollection<DataPlotPreset>();
            presetPlotsDropdownMenu.ItemsSource = Presets;
            plot = new ScatterPlot(PlotView, new List<Datum>());
            Psms = psms;

            LoadPresets();
            AddAxisOptions();
        }

        private void AddAxisOptions()
        {
            xAxisVariableDropdownMenu.Items.Add("Fragment mass error (ppm)");
            xAxisVariableDropdownMenu.Items.Add("Precursor mass error (ppm)");
            xAxisVariableDropdownMenu.Items.Add("MS2 Retention Time");
            xAxisVariableDropdownMenu.Items.Add("Fragment Intensity");
            xAxisVariableDropdownMenu.Items.Add("Precursor Charge");

            yAxisVariableDropdownMenu.Items.Add("Fragment mass error (ppm)");
            yAxisVariableDropdownMenu.Items.Add("Precursor mass error (ppm)");
            yAxisVariableDropdownMenu.Items.Add("MS2 Retention Time");
            yAxisVariableDropdownMenu.Items.Add("Fragment Intensity");
            yAxisVariableDropdownMenu.Items.Add("Precursor Charge");

            plotTypeDropdownMenu.Items.Add("Scatter");
            plotTypeDropdownMenu.Items.Add("Line");
            plotTypeDropdownMenu.Items.Add("Spectrum");
            plotTypeDropdownMenu.Items.Add("Bar");
            plotTypeDropdownMenu.Items.Add("Histogram");
        }

        private void SavePresets()
        {
            var dataFolder = GlobalVariables.DataDir;
            var presetsFile = Path.Combine(dataFolder, @"MetaDrawPresetPlots.toml");
            List<string> output = new List<string>();

            foreach (var preset in Presets)
            {
                output.Add(preset.ToToml());
            }

            File.WriteAllLines(presetsFile, output);
            LoadPresets();
        }

        private void LoadPresets()
        {
            var dataFolder = GlobalVariables.DataDir;
            var presetsFile = Path.Combine(dataFolder, @"MetaDrawPresetPlots.toml");

            if (!File.Exists(presetsFile))
            {
                // add presets like RT vs precursor PPM error, etc.
                //Presets.Add(new DataPlotPreset("Scatter", new ScatterPlot(PlotView, new List<Datum>())));
                //Presets.Add(new DataPlotPreset("Line", new LinePlot(PlotView, new List<Datum>())));
                //Presets.Add(new DataPlotPreset("Bar", new BarPlot(PlotView, new List<Datum>())));
                //Presets.Add(new DataPlotPreset("Spectrum", new SpectrumPlot(PlotView, new List<Datum>())));
                //Presets.Add(new DataPlotPreset("Histogram", new HistogramPlot(PlotView, new List<Datum>(), 10)));

                //SavePresets();
            }

            Presets.Clear();

            if (File.Exists(presetsFile))
            {
                foreach (var line in File.ReadAllLines(presetsFile))
                {
                    var preset = new DataPlotPreset(line);

                    if (preset != null)
                    {
                        Presets.Add(preset);
                    }
                }
            }
        }

        private void saveToPresetsButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(presetNameTextBox.Text))
            {
                MessageBox.Show("The preset must be given a name.");
                return;
            }

            Presets.Add(new DataPlotPreset(presetNameTextBox.Text, xAxisVariableDropdownMenu.Text, yAxisVariableDropdownMenu.Text, plotTypeDropdownMenu.Text));
            SavePresets();
        }

        private void dataChart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Random r = new Random();
                int speedMode = 5000;
                List<Datum> data = new List<Datum>();
                yAxisVariableDropdownMenu.IsEnabled = true;
                saveToPresetsButton.IsEnabled = false;

                var xAxisVariable = xAxisVariableDropdownMenu.SelectedItem;
                var yAxisVariable = yAxisVariableDropdownMenu.SelectedItem;
                var plotType = plotTypeDropdownMenu.SelectedItem;

                if (xAxisVariable != null && yAxisVariable != null && plotType != null)
                {
                    foreach (var psm in Psms)
                    {
                        MetaDraw.GenerateDataPoints(psm, data, xAxisVariable.ToString(), yAxisVariable.ToString());
                    }

                    switch (plotType.ToString())
                    {
                        case "Scatter": plot = new ScatterPlot(PlotView, data.OrderBy(p => r.Next()).Take(speedMode)); break;
                        case "Line": plot = new LinePlot(PlotView, data); break;
                        case "Spectrum": plot = new SpectrumPlot(PlotView, data); break;
                        case "Bar": plot = new BarPlot(PlotView, data); break;
                        case "Histogram": plot = new HistogramPlot(PlotView, data, numBins: 10); yAxisVariableDropdownMenu.IsEnabled = false; break;
                    }

                    saveToPresetsButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating chart: " + ex.Message);
            }
        }

        private void presetPlotsDropdownMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var thePreset = (DataPlotPreset)presetPlotsDropdownMenu.SelectedItem;

            if (thePreset != null)
            {
                List<Datum> data = new List<Datum>();

                foreach (var psm in Psms)
                {
                    MetaDraw.GenerateDataPoints(psm, data, thePreset.XAxisVariable, thePreset.YAxisVariable);
                }

                plot = thePreset.ToMzPlot(PlotView, data);
            }
        }
    }
}
