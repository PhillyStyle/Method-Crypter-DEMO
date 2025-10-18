using dnlib.DotNet;
using dnlib.DotNet.Writer;
using MethodInjector;
using MethodRenamer;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;



namespace MethodInterleaving
{
    public static class MethodInterleaving
    {
        public static void DoInterleaving(string exePath, string outputExePath, List<MethodReplacement> renameList, bool useNewNames)
        {
            string loadEXEPath = "";
            if (File.Exists(outputExePath)) loadEXEPath = outputExePath;
            else loadEXEPath = exePath;

            ModuleDefMD module = ModuleDefMD.Load(loadEXEPath);

            DoInterleaving(module, renameList, useNewNames);

            string tempPath = outputExePath + ".interleaving.tmp.exe";
            try
            {
                var writerOpts = new ModuleWriterOptions(module) { WritePdb = false };
                writerOpts.MetadataOptions.Flags |= dnlib.DotNet.Writer.MetadataFlags.KeepOldMaxStack;
                module.Write(tempPath, writerOpts);
                if (File.Exists(outputExePath)) File.Delete(outputExePath);
                File.Move(tempPath, outputExePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Method renaming caused error:\n" + ex.ToString());
            }
        }


        // Insert junk methods immediately AFTER each target method (constructors or renamed methods).
        // This never removes/re-adds original methods, avoiding ctor corruption.
        public static void DoInterleaving(ModuleDefMD module, List<MethodReplacement> replacements, bool useNewNames)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));
            if (replacements == null) throw new ArgumentNullException(nameof(replacements));

            // Single RNG for this run so junk is varied
            var rnd = new Random();

            // Build a set of type names to process once each
            var typeNames = new HashSet<string>();
            foreach (var r in replacements)
                typeNames.Add(r.TypeFullName);

            foreach (var typeName in typeNames)
            {
                var type = module.Find(typeName, isReflectionName: true);
                if (type == null) continue;

                // Snapshot the methods so we can iterate safely in original order
                var methodsSnapshot = type.Methods.ToList();

                // For each method in the snapshot, if it's a target, insert junk right after it
                for (int s = 0; s < methodsSnapshot.Count; s++)
                {
                    var realMethod = methodsSnapshot[s];

                    bool isTarget = false;
                    //// Is this one of the methods we want to target?
                    for (int ri = 0; ri < replacements.Count; ri++)
                    {
                        var r = replacements[ri];

                        if (useNewNames)
                        {
                            if (r.TypeFullName == type.FullName && r.NewName == realMethod.Name)
                            {
                                isTarget = true;
                            }
                            else if (realMethod.IsConstructor && r.TypeFullName == type.FullName && r.OriginalName == "")
                            {
                                isTarget = true;
                            }
                        }
                        else
                        {
                            if (r.TypeFullName == type.FullName && r.OriginalName == realMethod.Name)
                            {
                                isTarget = true;
                            }
                            else if (realMethod.IsConstructor && r.TypeFullName == type.FullName && r.OriginalName == "")
                            {
                                isTarget = true;
                            }
                        }
                    }
                    //}

                    if (!isTarget)
                        continue;

                    // Find current index of the real method in the live type.Methods list
                    int liveIndex = type.Methods.IndexOf(realMethod);
                    if (liveIndex < 0)
                    {
                        // method not found (weird), skip
                        Console.WriteLine($"[WARN] method {realMethod.FullName} not found in live Methods list; skipping junk insertion.");
                        continue;
                    }

                    int nextIndex = liveIndex + 1;

                    // Generate junk for this method. Use prefix "000" for ctors so they sort near .ctor in viewers.
                    string junkPrefix;
                    if (realMethod.IsConstructor)
                    {
                        // always start with "000" so it sorts near .ctor
                        junkPrefix = "000";
                    }
                    else
                    {
                        // normal methods: prepend "Junk_" so it never conflicts
                        junkPrefix = realMethod.Name;
                    }

                    List<MethodDef> junkList = null;
                    try
                    {
                        bool maketiny = true;
                        int encSize = 48;
                        int rndExtra = 0;
                        if (!IsTinyHeader(realMethod))
                        {
                            encSize = 64;
                            rndExtra = 16;
                            maketiny = false;
                        }

                        // GenerateJunkMethodsInType should return MethodDefs that are NOT yet added to any type.
                        // Pass a per-call seed for uniqueness.
                        junkList = JunkMethodInjector.GenerateJunkMethodsInType(
                            module,
                            type.FullName,
                            junkPrefix,
                            count: 1,
                            minEncodedBytes: encSize,
                            randomExtraMax: rndExtra,
                            noInline: false,
                            rndSeed: rnd.Next(),
                            maketiny: maketiny
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WARN] Generating junk for {realMethod.FullName} failed: {ex.Message}");
                        continue;
                    }

                    if (junkList == null || junkList.Count == 0)
                        continue;

                    // Insert each junk method immediately after the real method.
                    // Insert in the live list at (liveIndex + 1), increment position for multiple junk methods.
                    int insertPos = type.Methods.IndexOf(realMethod) + 1;
                    if (insertPos <= 0) insertPos = type.Methods.Count; // fallback: append at end

                    foreach (var junk in junkList)
                    {
                        // Make sure the method is unowned (should be), then assign its DeclaringType and insert.
                        type.Methods.Insert(insertPos, junk);
                        insertPos++;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the method body uses a Tiny header on disk.
        /// Returns false if it's a Fat header or if the method has no body.
        /// </summary>
        public static bool IsTinyHeader(MethodDef method)
        {
            if (method == null || method.Module == null)
                return false;

            // No IL body in the file (abstract, P/Invoke, etc.)
            if (method.RVA == 0)
                return false;

            var md = ((ModuleDefMD)method.Module).Metadata;
            var reader = md.PEImage.CreateReader(method.RVA);

            byte first = reader.ReadByte();

            // lowest 2 bits indicate header type:
            // 0b10 (2) = Tiny, 0b11 (3) = Fat
            return (first & 0x3) == 2;
        }
    }
}

