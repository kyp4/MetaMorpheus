using EngineLayer;
using EngineLayer.GlycoSearch;
using EngineLayer.FdrAnalysis;
using Proteomics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TaskLayer
{
    public class PostGlycoSearchAnalysisTask : MetaMorpheusTask
    {
        public PostGlycoSearchAnalysisTask() : base(MyTask.Search)
        {
        }

        protected override MyTaskResults RunSpecific(string OutputFolder, List<DbForTask> dbFilenameList, List<string> currentRawFileList, string taskId, FileSpecificParameters[] fileSettingsList)
        {
            return null;
        }

        public MyTaskResults Run(string OutputFolder, List<DbForTask> dbFilenameList, List<string> currentRawFileList, string taskId, FileSpecificParameters[] fileSettingsList, List<GlycoSpectralMatch> allPsms, CommonParameters commonParameters, GlycoSearchParameters glycoSearchParameters, List<Protein> proteinList, List<Modification> variableModifications, List<Modification> fixedModifications, List<string> localizeableModificationTypes, MyTaskResults MyTaskResults)
        {
            if (glycoSearchParameters.GlycoSearchType == GlycoSearchType.NGlycanSearch)
            {
                var allPsmsSingle = allPsms.Where(p => p.NGlycan == null ).OrderByDescending(p => p.Score).ToList();
                SingleFDRAnalysis(allPsmsSingle, commonParameters, new List<string> { taskId });

                var writtenFileSingle = Path.Combine(OutputFolder, "single" + ".psmtsv");
                WriteFile.WritePsmGlycoToTsv(allPsmsSingle, writtenFileSingle, 1);
                FinishedWritingFile(writtenFileSingle, new List<string> { taskId });

                var allPsmsGly = allPsms.Where(p => p.NGlycan != null ).OrderByDescending(p => p.Score).ToList();
                SingleFDRAnalysis(allPsmsGly, commonParameters, new List<string> { taskId });

                var writtenFileNGlyco = Path.Combine(OutputFolder, "nglyco" + ".psmtsv");
                WriteFile.WritePsmGlycoToTsv(allPsmsGly, writtenFileNGlyco, 3);
                FinishedWritingFile(writtenFileNGlyco, new List<string> { taskId });

                //new
                var nglycoMass = allPsms.Where(p => p.NGlycan != null).OrderByDescending(p => p.ScanPrecursorMass).ToList();
                SingleFDRAnalysis(nglycoMass, commonParameters, new List<string> { taskId });

                var nglycoMass_write = Path.Combine(OutputFolder, "nglyco_mass_file" + ".psmtsv");
                WriteFile.WritePsmGlycoToTsv(nglycoMass, nglycoMass_write, 3);
                FinishedWritingFile(nglycoMass_write, new List<string> { taskId });

         

                return MyTaskResults;
            }
            else if (glycoSearchParameters.GlycoSearchType == GlycoSearchType.OGlycanSearch)
            {
                var allPsmsSingle = allPsms.Where(p => p.Routes == null ).OrderByDescending(p => p.Score).ToList();
                SingleFDRAnalysis(allPsmsSingle, commonParameters, new List<string> { taskId });

                var writtenFileSingle = Path.Combine(OutputFolder, "single" + ".psmtsv");
                WriteFile.WritePsmGlycoToTsv(allPsmsSingle, writtenFileSingle, 1);
                FinishedWritingFile(writtenFileSingle, new List<string> { taskId });

                var allPsmsGly = allPsms.Where(p => p.Routes != null ).OrderByDescending(p => p.Score).ToList();
                SingleFDRAnalysis(allPsmsGly, commonParameters, new List<string> { taskId });

                var writtenFileOGlyco = Path.Combine(OutputFolder, "oglyco" + ".psmtsv");
                WriteFile.WritePsmGlycoToTsv(allPsmsGly, writtenFileOGlyco, 2);
                FinishedWritingFile(writtenFileOGlyco, new List<string> { taskId });

                var ProteinLevelLocalization = GlycoProteinParsimony.ProteinLevelGlycoParsimony(allPsmsGly.Where(p=>p.ProteinAccession!=null && p.OneBasedStartResidueInProtein.HasValue).ToList());

                var seen_oglyco_localization_file = Path.Combine(OutputFolder, "seen_oglyco_localization" + ".tsv");
                WriteFile.WriteSeenProteinGlycoLocalization(ProteinLevelLocalization, seen_oglyco_localization_file);
                FinishedWritingFile(seen_oglyco_localization_file, new List<string> { taskId });

                var protein_oglyco_localization_file = Path.Combine(OutputFolder, "protein_oglyco_localization" + ".tsv");
                WriteFile.WriteProteinGlycoLocalization(ProteinLevelLocalization, protein_oglyco_localization_file);
                FinishedWritingFile(protein_oglyco_localization_file, new List<string> { taskId });

                //new
                /*
                var oglycoMass = allPsms.Where(p => p.Routes != null).OrderByDescending(p => p.ScanPrecursorMass).ToList();
                SingleFDRAnalysis(oglycoMass, commonParameters, new List<string> { taskId });

                var oglycoMass_write = Path.Combine(OutputFolder, "oglyco_mass_file" + ".psmtsv");
                WriteFile.WritePsmGlycoToTsv(oglycoMass, oglycoMass_write, 3);
                FinishedWritingFile(oglycoMass_write, new List<string> { taskId });
                */

                var uniqueOGlycan = allPsms.Where(p => p.Routes != null).GroupBy(p => p.LocalizedGlycan).ToList();
                //SingleFDRAnalysis(uniqueOGlycan, commonParameters, new List<string> { taskId });

                //var uniqueOGlycan_write = Path.Combine(OutputFolder, "oglyco_unique_file" + ".psmtsv");
                //WriteFile.WriteSeenProteinGlycoLocalizationUnique(uniqueOGlycan, uniqueOGlycan_write);
                //FinishedWritingFile(uniqueOGlycan_write, new List<string> { taskId });

                //new outputs

                //var testOutput = allPsms.OrderByDescending(p => p.ScanPrecursorMass).ToList();
                // SingleFDRAnalysis(testOutput, commonParameters, new List<string> { taskId });

                // var writtenFileSingle1 = Path.Combine(OutputFolder, "new" + ".psmtsv");
                // WriteFile.WritePsmGlycoToTsv(testOutput, writtenFileSingle1, 1);
                //FinishedWritingFile(writtenFileSingle1, new List<string> { taskId });

                //GlycoPepMix_35trig_EThcD35_Glycans - glycan, mass, site type, localized, #unique seq localized, localized uniprot IDs, #proteinds localized, #unique seq all, all uniprot IDs, #proteins all
                //var testOutput = allPsms.Where(p => p.Routes != null).OrderByDescending(p => p.ScanPrecursorMass)
                // .Select(p => new { p.LocalizedGlycan, p.ScanPrecursorMass, p.LocalizationLevel, p.BaseSequence, p.LocalizedScores}).ToList();
                //SingleFDRAnalysis(testOutput, commonParameters, new List<string> { taskId });

                //var writtenFileSingle1 = Path.Combine(OutputFolder, "new" + ".psmtsv");
                //WriteFile.WritePsmGlycoToTsv(testOutput, writtenFileSingle1, 1);
                //FinishedWritingFile(writtenFileSingle1, new List<string> { taskId });

                var glyco_mass_file = Path.Combine(OutputFolder, "glyco_mass_file" + ".tsv");
                WriteFile.GlycoMassSummaryTSV(ProteinLevelLocalization, glyco_mass_file);
                FinishedWritingFile(glyco_mass_file, new List<string> { taskId });








                return MyTaskResults;
            }
            else
            {
                var allPsmsSingle = allPsms.Where(p => p.NGlycan == null && p.Routes == null).OrderByDescending(p => p.Score).ToList();
                SingleFDRAnalysis(allPsmsSingle, commonParameters, new List<string> { taskId });

                var writtenFileSingle = Path.Combine(OutputFolder, "single" + ".psmtsv");
                WriteFile.WritePsmGlycoToTsv(allPsmsSingle, writtenFileSingle, 1);
                FinishedWritingFile(writtenFileSingle, new List<string> { taskId });

                var allPsmsNGly = allPsms.Where(p => p.NGlycan != null).OrderByDescending(p => p.Score).ToList();
                SingleFDRAnalysis(allPsmsNGly, commonParameters, new List<string> { taskId });

                var writtenFileNGlyco = Path.Combine(OutputFolder, "nglyco" + ".psmtsv");
                WriteFile.WritePsmGlycoToTsv(allPsmsNGly, writtenFileNGlyco, 3);
                FinishedWritingFile(writtenFileNGlyco, new List<string> { taskId });

                var allPsmsOGly = allPsms.Where(p => p.Routes != null).OrderByDescending(p => p.Score).ToList();
                SingleFDRAnalysis(allPsmsOGly, commonParameters, new List<string> { taskId });

                var writtenFileOGlyco = Path.Combine(OutputFolder, "oglyco" + ".psmtsv");
                WriteFile.WritePsmGlycoToTsv(allPsmsOGly, writtenFileOGlyco, 2);
                FinishedWritingFile(writtenFileOGlyco, new List<string> { taskId });

                return MyTaskResults;
            }
        }

        //Calculate the FDR of single peptide FP/TP
        private void SingleFDRAnalysis(List<GlycoSpectralMatch> items, CommonParameters commonParameters, List<string> taskIds)
        {
            // calculate single PSM FDR
            List<PeptideSpectralMatch> psms = items.Select(p => p as PeptideSpectralMatch).ToList();
            new FdrAnalysisEngine(psms, 0, commonParameters, this.FileSpecificParameters, taskIds).Run();

        }

    }
}

