using EngineLayer;
using MassSpectrometry;
using MzLibUtil;
using mzPlot;
using OxyPlot;
using Proteomics.Fragmentation;
using Proteomics.ProteolyticDigestion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TaskLayer;
using UsefulProteomicsDatabases;

namespace MetaMorpheusGUI
{
    /// <summary>
    /// Interaction logic for MetaDraw.xaml
    /// </summary>
    public partial class MetaDraw : Window
    {
        private readonly List<string> SpectraFilePaths;
        private readonly List<string> SearchResultsFilePaths;
        private readonly List<DynamicDataConnection> MsDataFiles;
        private readonly ObservableCollection<PsmFromTsv> AllPsms; // all loaded PSMs
        private readonly ObservableCollection<PsmFromTsv> FilteredListOfPsms; // this is the filtered list of PSMs to display (after q-value filter, etc.)
        private readonly Regex IllegalInFileName = new Regex(@"[\\/:*?""<>|]"); // this is to prevent exported files from having non-allowed characters
        private Plot PsmAnnotationPlot;
        private Plot DataPlot;
        private PsmFromTsv CurrentParentPsm;

        public MetaDraw()
        {
            InitializeComponent();

            Title = "MetaDraw: version " + GlobalVariables.MetaMorpheusVersion;

            SpectraFilePaths = new List<string>();
            SearchResultsFilePaths = new List<string>();
            MsDataFiles = new List<DynamicDataConnection>();
            AllPsms = new ObservableCollection<PsmFromTsv>();
            FilteredListOfPsms = new ObservableCollection<PsmFromTsv>();

            MetaDrawSettings.SetUpDictionaries();
            PsmAnnotationPlot = new SpectrumPlot(plotView, new List<Datum>());
            DataPlot = new ScatterPlot(DataPlotView, new List<Datum>());

            dataGridScanNums.DataContext = FilteredListOfPsms;

            ParentChildScanView.Visibility = Visibility.Collapsed;
            ParentScanView.Visibility = Visibility.Collapsed;

            base.Closing += this.OnClosing;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] files = ((string[])e.Data.GetData(DataFormats.FileDrop)).OrderBy(p => p).ToArray();

            if (files != null)
            {
                foreach (var draggedFilePath in files)
                {
                    if (File.Exists(draggedFilePath))
                    {
                        AddFile(draggedFilePath);
                    }
                }
            }
        }

        private void AddFile(string filePath)
        {
            var theExtension = Path.GetExtension(filePath).ToLower();

            switch (theExtension)
            {
                case ".raw":
                case ".mzml":
                case ".mgf":
                    if (!SpectraFilePaths.Contains(filePath))
                    {
                        SpectraFilePaths.Add(filePath);

                        if (SpectraFilePaths.Count == 1)
                        {
                            spectraFileNameLabel.Text = filePath;
                        }
                        else
                        {
                            spectraFileNameLabel.Text = "[Mouse over to view files]";
                        }

                        spectraFileNameLabel.ToolTip = string.Join("\n", SpectraFilePaths);
                    }
                    break;
                case ".tsv":
                case ".psmtsv":
                    if (!SearchResultsFilePaths.Contains(filePath))
                    {
                        SearchResultsFilePaths.Add(filePath);

                        if (SearchResultsFilePaths.Count == 1)
                        {
                            psmFileNameLabel.Text = filePath;
                        }
                        else
                        {
                            psmFileNameLabel.Text = "[Mouse over to view files]";
                        }

                        psmFileNameLabel.ToolTip = string.Join("\n", SearchResultsFilePaths);
                    }
                    break;
                default:
                    MessageBox.Show("Cannot read file type: " + theExtension);
                    break;
            }
        }

        private void selectSpectraFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Spectra Files(*.raw;*.mzML)|*.raw;*.mzML",
                FilterIndex = 1,
                RestoreDirectory = true,
                Multiselect = true
            };
            if (openFileDialog1.ShowDialog() == true)
            {
                foreach (var filePath in openFileDialog1.FileNames.OrderBy(p => p))
                {
                    AddFile(filePath);
                }
            }
        }

        private void selectPsmFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Result Files(*.psmtsv)|*.psmtsv",
                FilterIndex = 1,
                RestoreDirectory = true,
                Multiselect = false
            };
            if (openFileDialog1.ShowDialog() == true)
            {
                foreach (var filePath in openFileDialog1.FileNames.OrderBy(p => p))
                {
                    AddFile(filePath);
                }
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            CleanUpDynamicConnections();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ExportToPdfButton_Click(object sender, RoutedEventArgs e)
        {
            var fileName = CurrentParentPsm.Filename;
            var path = SpectraFilePaths.First(p => Path.GetFileNameWithoutExtension(p) == fileName);
            PsmAnnotationPlot.ExportToPdf(Path.Combine(Path.GetDirectoryName(path), @"test.pdf"));
        }

        private void loadFiles_Click(object sender, RoutedEventArgs e)
        {
            AllPsms.Clear();
            FilteredListOfPsms.Clear();
            CleanUpDynamicConnections();

            // load PSMs
            HashSet<string> fileNamesWithoutExtension = new HashSet<string>(SpectraFilePaths.Select(p => Path.GetFileNameWithoutExtension(p)));

            try
            {
                // TODO: print warnings
                foreach (string path in SearchResultsFilePaths)
                {
                    foreach (PsmFromTsv psm in PsmTsvReader.ReadTsv(path, out List<string> warnings))
                    {
                        if (fileNamesWithoutExtension.Contains(psm.Filename))
                        {
                            AllPsms.Add(psm);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading PSM file:\n" + ex.Message);
            }

            foreach (PsmFromTsv psm in AllPsms)
            {
                if (MetaDrawSettings.FilterAcceptsPsm(psm))
                {
                    FilteredListOfPsms.Add(psm);
                }
            }

            // load spectra
            MyFileManager myFileManager = new MyFileManager(false);
            foreach (var path in SpectraFilePaths)
            {
                MsDataFiles.Add(myFileManager.OpenDynamicDataConnection(path));
            }
        }

        private void dataGridScanNums_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (dataGridScanNums.SelectedItem == null)
            {
                return;
            }

            if (!SpectraFilePaths.Any())
            {
                MessageBox.Show("Please add a spectra file.");
                return;
            }

            if (!MsDataFiles.Any())
            {
                MessageBox.Show("Please load spectra file.");
                return;
            }

            // draw the selected PSM
            PsmFromTsv psm = (PsmFromTsv)dataGridScanNums.SelectedItem;
            DrawSpectrumMatch(psm);
        }

        private void DrawSpectrumMatch(PsmFromTsv psm)
        {
            CurrentParentPsm = psm;

            string fileName = CurrentParentPsm.Filename;
            string path = SpectraFilePaths.First(p => Path.GetFileNameWithoutExtension(p) == fileName);
            DynamicDataConnection file = MsDataFiles.First(p => Path.GetFileNameWithoutExtension(p.FilePath) == fileName);
            int scanNumber = CurrentParentPsm.Ms2ScanNumber;
            MsDataScan scan = file.GetOneBasedScanFromDynamicConnection(scanNumber);

            if (string.IsNullOrEmpty(psm.BetaPeptideBaseSequence))
            {
                PsmAnnotationPlot = new PeptideSpectrumMatchPlot(plotView, CurrentParentPsm, scan);
            }
            else
            {
                PsmAnnotationPlot = new CrosslinkSpectralMatchPlot(plotView, CurrentParentPsm, scan);
            }
        }

        private void CleanUpDynamicConnections()
        {
            foreach (var connection in MsDataFiles)
            {
                connection.CloseDynamicConnection();
            }
            MsDataFiles.Clear();
        }

        private void AddNewTabItem_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var tabitem = new TabItem() { Header = "Test" };
            var plus = tabControl.Items[tabControl.Items.Count - 1];
            tabControl.Items.Add(tabitem);
            //tabControl.SelectedIndex = tabControl.Items.Count - 1;
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var senderType = e.OriginalSource.GetType().Name;

            if (senderType == "TabControl")
            {
                var selectedItem = (TabItem)tabControl.SelectedItem;
                var selectedItemHeader = selectedItem.Header.ToString();

                if (selectedItemHeader == "+")
                {
                    AddNewTabItem_Click(sender, null);
                }
            }
        }

        private void dataChart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
            //    Random r = new Random();
            //    int speedMode = 10000;
            //    List<Datum> data = new List<Datum>();
            //    yAxisVariableDropdownMenu.IsEnabled = true;

            //    var xAxisVariable = xAxisVariableDropdownMenu.SelectedItem;
            //    var yAxisVariable = yAxisVariableDropdownMenu.SelectedItem;
            //    var plotType = plotTypeDropdownMenu.SelectedItem;

            //    if (xAxisVariable != null && yAxisVariable != null && plotType != null)
            //    {
            //        foreach (var psm in FilteredListOfPsms)
            //        {
            //            GenerateDataPoints(psm, data, xAxisVariable.ToString(), yAxisVariable.ToString());
            //        }

            //        switch (plotType.ToString())
            //        {
            //            case "Scatter": DataPlot = new ScatterPlot(DataPlotView, data.OrderBy(p => r.Next()).Take(speedMode)); break;
            //            case "Line": DataPlot = new LinePlot(DataPlotView, data); break;
            //            case "Spectrum": DataPlot = new SpectrumPlot(DataPlotView, data); break;
            //            case "Bar": DataPlot = new BarPlot(DataPlotView, data); break;
            //            case "Histogram": DataPlot = new HistogramPlot(DataPlotView, data, numBins: 10); yAxisVariableDropdownMenu.IsEnabled = false; break;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Error creating chart: " + ex.Message);
            //}
        }

        public static void GenerateDataPoints(PsmFromTsv psm, List<Datum> data, string xAxisVariable, string yAxisVariable)
        {
            List<double> xAxisData = GenerateData(psm, xAxisVariable);
            List<double> yAxisData = GenerateData(psm, yAxisVariable);

            if ((xAxisData.Count == 1 || yAxisData.Count == 1) && (xAxisData.Count > 1 || yAxisData.Count > 1))
            {
                if (xAxisData.Count == 1)
                {
                    double value = xAxisData.First();

                    for (int i = 1; i < yAxisData.Count; i++)
                    {
                        xAxisData.Add(value);
                    }
                }
                else
                {
                    double value = yAxisData.First();

                    for (int i = 1; i < xAxisData.Count; i++)
                    {
                        yAxisData.Add(value);
                    }
                }
            }

            if (xAxisData.Count > 0 && yAxisData.Count > 0)
            {
                for (int i = 0; i < xAxisData.Count; i++)
                {
                    data.Add(new Datum(xAxisData[i], yAxisData[i]));
                }
            }
        }

        private static List<double> GenerateData(PsmFromTsv psm, string variable)
        {
            List<double> data = new List<double>();

            switch (variable)
            {
                case "Fragment mass error (ppm)":
                    data.AddRange(psm.MatchedIons.Select(p => p.MassErrorPpm));
                    break;
                case "Precursor mass error (ppm)":
                    if (double.TryParse(psm.MassDiffPpm, out double precursorMassError)) { data.Add(precursorMassError); }
                    break;
                case "MS2 Retention Time":
                    if (psm.RetentionTime.HasValue) { data.Add(psm.RetentionTime.Value); }
                    break;
                case "Fragment Intensity":
                    data.AddRange(psm.MatchedIons.Select(p => p.Intensity));
                    break;
                case "Precursor Charge":
                    data.Add(psm.PrecursorCharge);
                    break;
            }

            return data;
        }

        private void plotView_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessageBox.Show("right click");
        }

        private void addNewPlot_Click(object sender, RoutedEventArgs e)
        {
            var addPlotWindow = new AddPlotWindow(DataPlotView, FilteredListOfPsms);
            addPlotWindow.Show();
        }
    }
}