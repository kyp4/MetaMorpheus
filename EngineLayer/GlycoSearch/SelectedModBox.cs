﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chemistry;
using System;
using Proteomics;
using MassSpectrometry;
using Proteomics.ProteolyticDigestion;
using Proteomics.Fragmentation;

namespace EngineLayer
{
    public class SelectedModBox : ModBox
    {
        public static Modification[] SelectedModifications;

        public static SelectedModBox[] ModBoxes;

        public static Glycan[] Global_NOGlycans { get; set; }

        public static IEnumerable<SelectedModBox> BuildModBoxes(int maxNum)
        {
            for (int i = 1; i <= maxNum; i++)
            {
                foreach (var idCombine in Glycan.GetKCombsWithRept(Enumerable.Range(0, SelectedModifications.Length), i))
                {
                    SelectedModBox modBox = new SelectedModBox(idCombine.ToArray());

                    yield return modBox;
                }
            }
        }


        public static int[] GetAllPossibleModSites(PeptideWithSetModifications peptide, SelectedModBox modBox)
        {
            List<int> allPossibleModSites = new List<int>();

            foreach (var mn in modBox.MotifNeeded)
            {
                List<int> possibleModSites = new List<int>();
                ModificationMotif.TryGetMotif(mn.Key, out ModificationMotif motif);
                Modification modWithMotif = new Modification(_target: motif, _locationRestriction: "Anywhere.");

                for (int r = 0; r < peptide.Length; r++)
                {
                    if (peptide.AllModsOneIsNterminus.Keys.Contains(r + 2))
                    {
                        continue;
                    }

                    if (ModificationLocalization.ModFits(modWithMotif, peptide.BaseSequence, r + 1, peptide.Length, r + 1))
                    {
                        possibleModSites.Add(r + 2);
                    }
                }

                if (possibleModSites.Count < mn.Value.Count)
                {
                    return null;
                }

                allPossibleModSites.AddRange(possibleModSites);
            }
            return allPossibleModSites.OrderBy(p => p).ToArray();
        }

        public static PeptideWithSetModifications GetTheoreticalPeptide(Tuple<int, int, double>[] theModPositions, PeptideWithSetModifications peptide, SelectedModBox modBox)
        {
            Dictionary<int, Modification> testMods = new Dictionary<int, Modification>();
            foreach (var mod in peptide.AllModsOneIsNterminus)
            {
                testMods.Add(mod.Key, mod.Value);
            }

            for (int i = 0; i < theModPositions.Count(); i++)
            {
                testMods.Add(theModPositions.ElementAt(i).Item1, SelectedModifications[theModPositions.ElementAt(i).Item2]);
            }

            var testPeptide = new PeptideWithSetModifications(peptide.Protein, peptide.DigestionParams, peptide.OneBasedStartResidueInProtein,
                peptide.OneBasedEndResidueInProtein, peptide.CleavageSpecificityForFdrCategory, peptide.PeptideDescription, peptide.MissedCleavages, testMods, peptide.NumFixedMods);

            return testPeptide;
        }

        public static PeptideWithSetModifications GetTheoreticalPeptide(int[] theModPositions, PeptideWithSetModifications peptide, SelectedModBox modBox)
        {
            Modification[] modifications = new Modification[modBox.NumberOfMods];
            for (int i = 0; i < modBox.NumberOfMods; i++)
            {
                modifications[i] = SelectedModBox.SelectedModifications[modBox.ModIds.ElementAt(i)];
            }

            Dictionary<int, Modification> testMods = new Dictionary<int, Modification>();
            foreach (var mod in peptide.AllModsOneIsNterminus)
            {
                testMods.Add(mod.Key, mod.Value);
            }

            for (int i = 0; i < theModPositions.Count(); i++)
            {
                testMods.Add(theModPositions.ElementAt(i), modifications[i]);
            }

            var testPeptide = new PeptideWithSetModifications(peptide.Protein, peptide.DigestionParams, peptide.OneBasedStartResidueInProtein,
                peptide.OneBasedEndResidueInProtein, peptide.CleavageSpecificityForFdrCategory, peptide.PeptideDescription, peptide.MissedCleavages, testMods, peptide.NumFixedMods);

            return testPeptide;
        }

        public static int[] GetLocalFragmentHash(List<Product> products, int peptideLength, int[] modPoses, int modInd, ModBox TotalBox, ModBox localBox, int FragmentBinsPerDalton)
        {
            List<double> newFragments = new List<double>();
            var local_c_fragments = products.Where(p => p.ProductType == ProductType.b && p.AminoAcidPosition >= modPoses[modInd] - 1 && p.AminoAcidPosition < modPoses[modInd + 1] - 1).ToList();

            foreach (var c in local_c_fragments)
            {
                var newMass = c.NeutralMass + localBox.Mass;
                newFragments.Add(newMass);
            }

            var local_z_fragments = products.Where(p => p.ProductType == ProductType.y && p.AminoAcidPosition >= modPoses[modInd] && p.AminoAcidPosition < modPoses[modInd + 1]).ToList();

            foreach (var z in local_z_fragments)
            {
                var newMass = z.NeutralMass + (TotalBox.Mass - localBox.Mass);
                newFragments.Add(newMass);
            }


            int[] fragmentHash = new int[newFragments.Count];
            for (int i = 0; i < newFragments.Count; i++)
            {
                fragmentHash[i] = (int)Math.Round(newFragments[i] * FragmentBinsPerDalton);
            }
            return fragmentHash;
        }

        public static IEnumerable<SelectedModBox> BuildChildModBoxes(int maxNum, int[] modIds)
        {
            yield return new SelectedModBox(new int[0]);
            HashSet<string> seen = new HashSet<string>();
            for (int i = 1; i <= maxNum; i++)
            {
                foreach (var idCombine in Glycan.GetKCombs(Enumerable.Range(0, maxNum), i))
                {
                    List<int> ids = new List<int>();
                    foreach (var id in idCombine)
                    {
                        ids.Add(modIds[id]);
                    }

                    if (!seen.Contains(string.Join(",", ids.Select(p => p.ToString()))))
                    {
                        seen.Add(string.Join(",", ids.Select(p => p.ToString())));

                        SelectedModBox modBox = new SelectedModBox(ids.ToArray());

                        yield return modBox;
                    }

                }
            }
        }

        public static Dictionary<string, List<int>> GenerateMotifNeeded(int[] ModIds)
        {
            Dictionary<string, List<int>> aa = new Dictionary<string, List<int>>();
            foreach (var id in ModIds)
            {
                var mod = SelectedModifications[id];
                if (aa.ContainsKey(mod.Target.ToString()))
                {
                    aa[mod.Target.ToString()].Add(id);
                }
                else
                {
                    aa.Add(mod.Target.ToString(), new List<int> { id });
                }
            }
            return aa;
        }


        #region GlycoRelated function

        public static IEnumerable<SelectedModBox> Build_NOGlycanBoxes(int OGlycoMaxNum, int NGlycoMaxNum, int GlobalOGlycanNumber, int GlobalNGlycoNumber)
        {
            for (int i = 1; i <= OGlycoMaxNum; i++)
            {
                foreach (var idCombine in Glycan.GetKCombsWithRept(Enumerable.Range(0, GlobalOGlycanNumber), i))
                {
                    SelectedModBox o_modBox = new SelectedModBox(idCombine.ToArray(), 1);

                    yield return o_modBox;

                    for (int j = 1; j <= NGlycoMaxNum; j++)
                    {
                        foreach (var jdCombine in Glycan.GetKCombsWithRept(Enumerable.Range(GlobalOGlycanNumber, GlobalNGlycoNumber), j))
                        {
                            SelectedModBox n_modBox = new SelectedModBox(jdCombine.ToArray(), 2);

                            yield return n_modBox;

                            var ijdCombine = idCombine.Concat(jdCombine);

                            SelectedModBox on_modBox = new SelectedModBox(ijdCombine.ToArray(), 3);

                            yield return on_modBox;

                        }
                    }
                }
            }

        }

        #endregion

        public SelectedModBox(int[] ids) : base(ids)
        {
            double mass = 0;
            foreach (var id in ModIds)
            {
                mass += SelectedModifications[id].MonoisotopicMass.Value;
            }
            Mass = mass;

            MotifNeeded = GenerateMotifNeeded(ids);
        }

        public SelectedModBox(int[] ids, int glycoFlag) : base(ids)
        {
            byte[] kind = new byte[Glycan.SugarLength];
            foreach (var id in ModIds)
            {
                for (int i = 0; i < kind.Length; i++)
                {
                    kind[i] += Global_NOGlycans[id].Kind[i];
                }
            }
            Kind = kind;

            Mass = (double)Glycan.GetMass(Kind) / 1E5;
            
            GlycoFlag = glycoFlag;
        }

        public int GlycoFlag { get; set; }

        public byte[] Kind { get; private set; }

        //key: motif, value: all ids for this motif
        public Dictionary<string, List<int>> MotifNeeded { get; }


    }
}