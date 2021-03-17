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

                //group psms by unique glycan
                //var uniqueOGlycan = allPsms.Where(p => p.Routes != null).GroupBy(p => p.LocalizedGlycan).ToList();
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

                //var glyco_mass_file = Path.Combine(OutputFolder, "glyco_mass_file" + ".tsv");
                //WriteFile.GlycoMassSummaryTSV(ProteinLevelLocalization, glyco_mass_file);
                //FinishedWritingFile(glyco_mass_file, new List<string> { taskId });

                /*
                //Unique O-Glycan Output
                var localizedGlycanPsms = allPsmsGly.Where(p => p.LocalizedGlycan != null && !p.IsDecoy).ToList();
                //localizedGlycanPsms[1].GlycanScore


                List<int> glycanIds = new List<int>();
                foreach (GlycoSpectralMatch gsm in localizedGlycanPsms)
                {
                    foreach (Tuple<int, int, bool> glycanId in gsm.LocalizedGlycan)
                    {
                        if (glycanId.Item3 == true)
                        {
                            glycanIds.Add(glycanId.Item2);
                        }
                    }
                }
                glycanIds = glycanIds.Distinct().OrderBy(i => i).ToList();
                Dictionary<int, List<GlycoSpectralMatch>> glycanOutDictionary = new Dictionary<int, List<GlycoSpectralMatch>>();
                Dictionary<int, List<string>> glycanSequencesDictionary = new Dictionary<int, List<string>>();
                Dictionary<int, List<string>> glycanProteinOutDictionary = new Dictionary<int, List<string>>();
                foreach (int glycanId in glycanIds)
                {
                    List<GlycoSpectralMatch> gsmsWithId = new List<GlycoSpectralMatch>();
                    gsmsWithId = localizedGlycanPsms.Where(g => g.LocalizedGlycan.Any(p => p.Item2 == glycanId && p.Item3 == true)).ToList();
                    glycanOutDictionary.Add(glycanId, gsmsWithId);
                    glycanSequencesDictionary.Add(glycanId, gsmsWithId.Select(s => s.BaseSequence).Distinct().OrderBy(b => b).ToList());
                    glycanProteinOutDictionary.Add(glycanId, gsmsWithId.Select(p => p.ProteinAccession).Distinct().OrderBy(a => a).ToList());
                }
                //End Unique O-Glycan Output

                var unique_oglyco_localization_file = Path.Combine(OutputFolder, "unique_oglyco_localization" + ".tsv");
                WriteFile.WriteUniqueGlyco(glycanOutDictionary, glycanSequencesDictionary, glycanProteinOutDictionary, unique_oglyco_localization_file);
                FinishedWritingFile(unique_oglyco_localization_file, new List<string> { taskId });

                var unique_oglyco_localization_file2 = Path.Combine(OutputFolder, "unique_oglyco_localization2" + ".tsv");
                WriteFile.WriteUniqueGlyco2(localizedGlycanPsms, unique_oglyco_localization_file2);
                FinishedWritingFile(unique_oglyco_localization_file2, new List<string> { taskId });

                */

                //GlycoPepMix_35trig_EThcD35_Glycans
                var nonDecoyGlycanPsms = allPsmsGly.Where(p => p.LocalizedGlycan != null && !p.IsDecoy).ToList();
                //localizedGlycanPsms[1].GlycanScore


                List<int> glycanIds = new List<int>();
                List<int> glycanIdsLocalized = new List<int>();
                foreach (GlycoSpectralMatch gsm in nonDecoyGlycanPsms)
                {
                    foreach (Tuple<int, int, bool> glycanId in gsm.LocalizedGlycan)
                    {
                 
                         glycanIds.Add(glycanId.Item2);

                        if (glycanId.Item3 == true)
                        {
                            glycanIdsLocalized.Add(glycanId.Item2);
                        }
                    }
                }
                glycanIds = glycanIds.Distinct().OrderBy(i => i).ToList();
                Dictionary<int, List<GlycoSpectralMatch>> glycanOutDictionary = new Dictionary<int, List<GlycoSpectralMatch>>();         
                Dictionary<int, List<string>> glycanProteinOutDictionary = new Dictionary<int, List<string>>();
                Dictionary<int, List<string>> glycanSequencesDictionary = new Dictionary<int, List<string>>();
                Dictionary<int, List<string>> glycanLocalizedDictionary = new Dictionary<int, List<string>>();

                //Dictionary<int, List<string>> glycanNameDictionary = new Dictionary<int, List<string>>();

                Dictionary<int, List<GlycoSpectralMatch>> glycanLocalizedOutDictionary = new Dictionary<int, List<GlycoSpectralMatch>>();
                Dictionary<int, List<string>> glycanLocalizedProteinOutDictionary = new Dictionary<int, List<string>>();
                Dictionary<int, List<string>> glycanLocalizedSequencesDictionary = new Dictionary<int, List<string>>();

                foreach (int glycanId in glycanIds)
                {
                    List<GlycoSpectralMatch> gsmsWithId = new List<GlycoSpectralMatch>();
                    gsmsWithId = nonDecoyGlycanPsms.Where(g => g.LocalizedGlycan.Any(p => p.Item2 == glycanId)).ToList();
                    glycanOutDictionary.Add(glycanId, gsmsWithId);
                    glycanProteinOutDictionary.Add(glycanId, gsmsWithId.Select(p => p.ProteinAccession).Distinct().OrderBy(a => a).ToList());
                    glycanSequencesDictionary.Add(glycanId, gsmsWithId.Select(s => s.BaseSequence).Distinct().OrderBy(b => b).ToList());

                    //glycanNameDictionary.Add(glycanId, gsmsWithId.Select(s => s.LocalizedGlycan).Distinct().OrderBy(b => b).ToList());


                    // glycanSequencesDictionary.Add(glycanId, gsmsWithId.Select(s => s.LocalizedGlycan.Any(predicate.)).Distinct().OrderBy(b => b).ToList());

                }
                
                foreach (int glycanIdLocalized in glycanIdsLocalized.Distinct())
                {
                    List<GlycoSpectralMatch> gsmsWithIdLocalized = new List<GlycoSpectralMatch>();
                    gsmsWithIdLocalized = nonDecoyGlycanPsms.Where(g => g.LocalizedGlycan.Any(p => p.Item2 == glycanIdLocalized && p.Item3 == true)).ToList();
                    glycanLocalizedOutDictionary.Add(glycanIdLocalized, gsmsWithIdLocalized);
                    glycanLocalizedProteinOutDictionary.Add(glycanIdLocalized, gsmsWithIdLocalized.Select(p => p.ProteinAccession).Distinct().OrderBy(a => a).ToList());
                    glycanLocalizedSequencesDictionary.Add(glycanIdLocalized, gsmsWithIdLocalized.Select(s => s.BaseSequence).Distinct().OrderBy(b => b).ToList());
                }

                

                var unique_oglyco_localization_file = Path.Combine(OutputFolder, "unique_oglyco_localization" + ".tsv");
                WriteFile.WriteUniqueGlyco(glycanOutDictionary, glycanSequencesDictionary, glycanProteinOutDictionary, glycanLocalizedOutDictionary, glycanLocalizedSequencesDictionary, glycanLocalizedProteinOutDictionary, unique_oglyco_localization_file);
                FinishedWritingFile(unique_oglyco_localization_file, new List<string> { taskId });

                //Dictionary<int, List<float>> glycanMassDictionary = new Dictionary<int, List<float>>();

                //GlycoPepMix_35trig_EThcD35_GlycoProteins
                //glycanIds
          
                List<string> glycoProtIds = new List<string>();
                List<string> glycoProtIdsLocalized = new List<string>();
                foreach (GlycoSpectralMatch gsm in nonDecoyGlycanPsms)
                {

                    glycoProtIds.Add(gsm.ProteinAccession);

                    //foreach (Tuple<int, int, bool> glycoProtId in gsm.LocalizedGlycan)
                    //{

                        //glycoProtIdsLocalized.Add(glycoProtId.ProteinAccession);

                        // if (glycoProtId.Item3 == true)
                        // {
                         //    glycoProtIdsLocalized.Add(glycoProtId.ProteinAccession);
                         //}
                    //}
                }
                glycoProtIds = glycoProtIds.Distinct().OrderBy(i => i).ToList();
                glycoProtIdsLocalized = glycoProtIdsLocalized.Distinct().OrderBy(i => i).ToList();
                Dictionary<string, List<string>> glycoProtDescription = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> glycoProtUniprotGlycoProt = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> glycoProtPSMLocalized = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> glycoProtUniquePepLocalized = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> glycoProtUniqueSeqLocalized = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> glycoProtGlycoSitesLocalized = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> glycoProtNSitesLocalized = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> glycoProtOSitesLocalized = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> glycoProtNumLocalizedGlycans = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> glycoProtLocalizedGlycans = new Dictionary<string, List<string>>();
               
                Dictionary<string, List<int?>> glycoProtPSM = new Dictionary<string, List<int?>>();
                Dictionary<string, List<double?>> glycoProtPeps = new Dictionary<string, List<double?>>();
                Dictionary<string, List<string>> glycoProtSeq = new Dictionary<string, List<string>>();
                Dictionary<string, List<List<Tuple<int, int, bool>>>> glycoProtGlycoSites = new Dictionary<string, List<List<Tuple<int, int, bool>>>>(); //how do I get this?
                Dictionary<string, List<List<Glycan>>> glycoProtNSites = new Dictionary<string, List<List<Glycan>>>();
                Dictionary<string, List<string>> glycoProtOSites = new Dictionary<string, List<string>>(); //how do I get this?
                //Dictionary<string, List<string>> glycoProtNumGlycans = new Dictionary<string, List<string>>();
                Dictionary<string, List<int>> glycoProtGlycans = new Dictionary<string, List<int>>();

                foreach (string glycoProtId in glycoProtIds)
                {
                    List<GlycoSpectralMatch> gsmsWithId = new List<GlycoSpectralMatch>();
                    //gsmsWithId = nonDecoyGlycanPsms.Where(g => g.ProteinAccession.Any(p => p.Equals(glycoProtId))).ToList();
                    //gsmsWithId = nonDecoyGlycanPsms.Where(p => glycoProtIds.Any(q => p.ProteinAccession.Contains(q))).ToList(); // this was counting Seq that match to ALL uniprot IDs in list
                    gsmsWithId = nonDecoyGlycanPsms.Where(g => g.ProteinAccession.Equals(glycoProtId)).ToList();

                    //glycanOutDictionary.Add(glycoProtId, gsmsWithId);
                    //glycanProteinOutDictionary.Add(glycanId, gsmsWithId.Select(p => p.ProteinAccession).Distinct().OrderBy(a => a).ToList());

                    glycoProtDescription.Add(glycoProtId, gsmsWithId.Select(s => s.Organism).Distinct().OrderBy(b => b).ToList());

                    glycoProtPSM.Add(glycoProtId, gsmsWithId.Select(s => s.PrecursorScanNumber).Distinct().OrderBy(b => b).ToList());
                    glycoProtPeps.Add(glycoProtId, gsmsWithId.Select(s => s.PeptideMonisotopicMass).Distinct().OrderBy(b => b).ToList());
                    glycoProtSeq.Add(glycoProtId, gsmsWithId.Select(s => s.BaseSequence).Distinct().OrderBy(b => b).ToList());
                    //glycoProtGlycoSites.Add(glycoProtId, gsmsWithId.Select(s => s.LocalizedGlycan).Distinct().OrderBy(b => b).ToList());
                    glycoProtNSites.Add(glycoProtId, gsmsWithId.Select(s => s.NGlycan).Distinct().OrderBy(b => b).ToList());
                    //glycoProtOSites.Add(glycoProtId, gsmsWithId.Select(s => s.NGlycan).Distinct().OrderBy(b => b).ToList());
                    //glycoProtGlycans.Add(glycoProtId, gsmsWithId.Select(s => s.LocalizedGlycan).Distinct().OrderBy(b => b).ToList()); //throwing an error
                    //glycoProtGlycans.Add(glycoProtId, gsmsWithId.Select(s => s.LocalizedGlycan.Any(p => p.Item2)).Distinct().OrderBy(b => b).ToList());


                    //glycoProtGlycans.Add(glycoProtId, gsmsWithId.Select(s => s.LocalizedGlycan.Select(p => p.Item2)).Distinct().OrderBy(b => b).ToList());

                    //glycoProtGlycans.Add(glycoProtId, gsmsWithId.Select(s => s.LocalizedGlycan[0].Item2).Distinct().OrderBy(b => b).ToList()); //works

                    //glycoProtGlycans.Add(glycoProtId, gsmsWithId.Select(s => s.LocalizedGlycan.Add(p => p.Item2)).Distinct().OrderBy(b => b).ToList());

                    //foreach (List<Tuple<int, int, bool>> glycan in gsmsWithId.Select(s => s.LocalizedGlycan)) 
                    //{
                       // List<int> tempGlycoProt = new List<int>();
                        //tempGlycoProt.Add(glycan.Item2);
                        //glycoProtGlycans.Add(glycoProtId, glycan.Item2);
                        
                   // }

                    


                    //glycoProtGlycans.Add(glycoProtId, gsmsWithId.Select(s => s.LocalizedGlycan).Distinct().OrderBy(b => b).ToList());

                }

                var unique_glyco_prot_localization_file = Path.Combine(OutputFolder, "unique_glyco_prot_localization" + ".tsv");
                //WriteFile.WriteUniqueGlycoProt(glycoProtIds, glycoProtDescription, glycoProtPSM, glycoProtPeps, glycoProtSeq, glycoProtNSites, glycoProtGlycans, unique_glyco_prot_localization_file);
                WriteFile.WriteUniqueGlycoProt(glycoProtIds, glycoProtDescription, glycoProtPSM, glycoProtPeps, glycoProtSeq, glycoProtNSites, glycoProtGlycans, unique_glyco_prot_localization_file);
                FinishedWritingFile(unique_glyco_prot_localization_file, new List<string> { taskId });



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

