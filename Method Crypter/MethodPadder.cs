using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using MethodRenamer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MethodPadder
{
    public static class MethodPadder
    {
        public static void InjectJunkCodeStartOfMethodOnly(string exePath, string outputExePath, List<MethodReplacement> renameList, bool useNewNames)
        {
            string loadEXEPath = "";
            if (File.Exists(outputExePath)) loadEXEPath = outputExePath;
            else loadEXEPath = exePath;

            ModuleDefMD module = ModuleDefMD.Load(loadEXEPath);
            InjectJunkCodeStartOfMethodOnly(module, renameList, useNewNames);

            string tempPath = outputExePath + ".methods.junk.tmp.exe";
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

        public static void InjectJunkCodeStartOfMethodOnly(ModuleDefMD module, List<MethodReplacement> replacements, bool useNewNames)
        {
            Random rnd = new Random();

            foreach (var mr in replacements)
            {
                var type = module.Find(mr.TypeFullName, isReflectionName: true);
                if (type == null) continue;

                // Take a snapshot of methods first
                var methodsSnapshot = type.Methods.ToList();

                foreach (var method in methodsSnapshot)
                {
                    if (method.IsConstructor)
                    {
                        //PadMethodToOver64Bytes(method, module); //Can't currently add junk to constructors.
                        continue;
                    }

                    if (!method.HasBody) continue;

                    if (useNewNames)
                    {
                        if (string.IsNullOrEmpty(mr.NewName)) continue;

                        if (method.Name == mr.NewName)
                        {
                            PadMethodToOver64Bytes(method, module);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(mr.OriginalName)) continue;

                        if (method.Name == mr.OriginalName)
                        {
                            PadMethodToOver64Bytes(method, module);
                        }
                    }
                }
            }
        }

        public class AdvancedTokenProvider : ITokenProvider, IWriterError
        {
            private int _nextToken = 1; // Start token from 1
            private readonly Dictionary<object, MDToken> _objectTokens = new Dictionary<object, MDToken>();

            public MDToken GetToken()
            {
                return new MDToken(Table.Module, (uint)_nextToken++);
            }

            public MDToken GetToken(object obj)
            {
                if (obj == null)
                    throw new ArgumentNullException(nameof(obj));

                // Check if the object already has a token
                if (_objectTokens.TryGetValue(obj, out MDToken token))
                {
                    return token; // Return existing token
                }

                // Generate a new token for the object
                token = GetToken();
                _objectTokens[obj] = token; // Store the token
                return token;
            }

            public MDToken GetToken(IList<TypeSig> typeSigs, uint baseToken)
            {
                // For simplicity, we can just return a new token based on the baseToken
                // In a real implementation, you might want to handle type signatures more carefully
                return new MDToken(Table.TypeDef, baseToken + (uint)typeSigs.Count); // Example logic
            }

            public void FreeToken(MDToken token)
            {
                // In this simple implementation, we won't reuse tokens
            }

            public void Error(string message)
            {
                // Handle the error (e.g., log it or throw an exception)
                Console.WriteLine($"Error: {message}");
            }
        }

        // Pads a method by inserting junk at method start so the encoded size increases
        // by at least 64 bytes plus an optional random extra (0..randomExtraMax).
        // rndSeed: -1 for non-deterministic randomness, otherwise reproducible.
        public static void PadMethodToOver64Bytes(MethodDef method, ModuleDef module, int randomExtraMax = 32, int rndSeed = -1)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (!method.HasBody || method.Body == null || method.Body.Instructions.Count == 0)
                return;

            var body = method.Body;
            body.InitLocals = true;
            if (body.MaxStack < 4) body.MaxStack = 4;

            int GetEncodedSize()
            {
                var tokenProvider = new AdvancedTokenProvider();
                var writer = new MethodBodyWriter(tokenProvider, method);
                writer.Write();
                return writer.GetFullMethodBody().Length;
            }

            var rnd = (rndSeed >= 0) ? new Random(rndSeed) : new Random();
            int initialEncoded = GetEncodedSize();
            int extra = (randomExtraMax > 0) ? rnd.Next(0, randomExtraMax + 1) : 0;
            int targetSize = initialEncoded + 64 + extra;

            const int MAX_ITER = 2000;
            int iter = 0;

            // Determine safe insertion index
            int insertIdx = FindSafeInjectionPoint(method);

            while (GetEncodedSize() < targetSize)
            {
                iter++;
                if (iter > MAX_ITER)
                    throw new InvalidOperationException($"Pad loop exceeded {MAX_ITER} iterations for {method.FullName}");

                insertIdx = InsertSmartJunk(body, module, method, rnd, insertIdx);
            }

            // sanity check
            int finalSize = GetEncodedSize();
            if (finalSize < targetSize)
                throw new InvalidOperationException($"Sanity check failed: method {method.FullName} encoded size {finalSize} < target {targetSize}");
        }

        private static int InsertSmartJunk(CilBody body, ModuleDef module, MethodDef method, Random rnd, int idx)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            var instrs = body.Instructions;
            if (instrs == null) return idx;
            if (idx < 0) idx = 0;
            if (idx > instrs.Count) idx = instrs.Count;

            int insertIdx = idx;

            // Keep a small set of locals to use for some junk sequences
            Local[] locals = new Local[3];

            Local EnsureIntLocal(int i)
            {
                if (locals[i] != null) return locals[i];
                var l = new Local(module.CorLibTypes.Int32);
                //{
                //    Name = RandomNameGenerator.GetUniqueName(8)
                //};
                locals[i] = l;
                body.Variables.Add(l);
                body.InitLocals = true;
                return l;
            }

            // small helper: insert push/pop sequence
            void PushPop(int min = 0, int max = 10)
            {
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(min, max)));
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Pop));
            }

            // push two constants, add, pop
            void AddPop(int min = 0, int max = 128)
            {
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(min, max)));
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(min, max)));
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Add));
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Pop));
            }

            // push two constants, xor, pop
            void XorPop(int min = 0, int max = 256)
            {
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(min, max)));
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(min, max)));
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Xor));
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Pop));
            }

            // push two constants, mul, pop
            void MulPop(int min = 1, int max = 10)
            {
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(min, max)));
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(min, max)));
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Mul));
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Pop));
            }

            // store/load using local
            void StoreLoadPop(int localIndex = 0, int min = 0, int max = 128)
            {
                var l = EnsureIntLocal(localIndex);
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(min, max)));
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Stloc, l));
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Ldloc, l));
                instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Pop));
            }

            // insert a small NOP sometimes
            void MaybeNop()
            {
                if (rnd.NextDouble() < 0.3)
                    instrs.Insert(insertIdx++, Instruction.Create(OpCodes.Nop));
            }

            // choose a random junk pattern
            int choice = rnd.Next(0, 10);
            switch (choice)
            {
                case 0:
                case 1:
                    PushPop();
                    break;
                case 2:
                case 3:
                    AddPop();
                    break;
                case 4:
                case 5:
                    XorPop();
                    break;
                case 6:
                    MulPop();
                    break;
                case 7:
                    StoreLoadPop(0);
                    break;
                case 8:
                    StoreLoadPop(1, 1, 20);
                    break;
                case 9:
                    // combined: push/add/store/load/pop sequence
                    AddPop();
                    StoreLoadPop(2);
                    break;
            }

            // sprinkle some NOPs to break monotony
            MaybeNop();
            MaybeNop();

            body.UpdateInstructionOffsets();

            return insertIdx;
        }

        /// <summary>
        /// Returns an Instruction to insert *after*. -1 means "no safe insertion point found"
        /// (e.g. abstract, pinvoke, no-body).
        /// </summary>
        public static int FindSafeInjectionPoint(MethodDef method)
        {
            if (method == null) return -1;
            if (method.RVA == 0 || !method.HasBody) return -1;

            var instrs = method.Body.Instructions;
            if (instrs == null || instrs.Count == 0) return -1;

            // Helper: "prolog-like" instructions we can skip over when searching for injection point
            bool IsPrologInstruction(Instruction ins)
            {
                var oc = ins.OpCode;

                // common prolog patterns:
                // - initial nops
                // - argument loads (ldarg.*, ldarg.s)
                // - local stores often used to initialize locals (stloc.*, stloc.s)
                // - local loads (ldloc.*, ldloc.s) used in simple initializers
                // - short loads/stores and dup that might be part of init patterns
                // - initobj, newobj is sometimes part of initialization — be conservative and skip newobj?
                if (oc == OpCodes.Nop) return true;

                if (oc == OpCodes.Ldarg || oc == OpCodes.Ldarg_S ||
                    oc == OpCodes.Ldarg_0 || oc == OpCodes.Ldarg_1 ||
                    oc == OpCodes.Ldarg_2 || oc == OpCodes.Ldarg_3)
                    return true;

                if (oc == OpCodes.Stloc || oc == OpCodes.Stloc_S ||
                    oc == OpCodes.Stloc_0 || oc == OpCodes.Stloc_1 ||
                    oc == OpCodes.Stloc_2 || oc == OpCodes.Stloc_3)
                    return true;

                if (oc == OpCodes.Ldloc || oc == OpCodes.Ldloc_S ||
                    oc == OpCodes.Ldloc_0 || oc == OpCodes.Ldloc_1 ||
                    oc == OpCodes.Ldloc_2 || oc == OpCodes.Ldloc_3)
                    return true;

                if (oc == OpCodes.Dup || oc == OpCodes.Initobj || oc == OpCodes.Newobj)
                    return true;

                // small constant pushes often appear in prolog initialization sequences
                if (oc == OpCodes.Ldc_I4 || oc == OpCodes.Ldc_I4_S ||
                    oc == OpCodes.Ldc_I4_0 || oc == OpCodes.Ldc_I4_1 ||
                    oc == OpCodes.Ldc_I4_2 || oc == OpCodes.Ldc_I4_3 ||
                    oc == OpCodes.Ldc_I4_4 || oc == OpCodes.Ldc_I4_5 ||
                    oc == OpCodes.Ldc_I4_6 || oc == OpCodes.Ldc_I4_7 ||
                    oc == OpCodes.Ldc_I4_8 || oc == OpCodes.Ldc_I4_M1)
                    return true;

                return false;
            }

            // Special handling for instance constructors (.ctor)
            if (method.IsConstructor && !method.IsStatic)
            {
                // Find the last base .ctor call early in the method:
                // we look for a 'call' or 'callvirt' whose operand is a MethodDef/IMethod and Name == ".ctor"
                int baseCtorIndex = -1;
                for (int i = 0; i < instrs.Count; i++)
                {
                    var ins = instrs[i];
                    if ((ins.OpCode == OpCodes.Call || ins.OpCode == OpCodes.Callvirt) && ins.Operand is IMethod m)
                    {
                        if (m.Name == ".ctor")
                        {
                            // heuristics: prefer a ctor call that targets a base type (not this type),
                            // but if none, accept the first .ctor call (it might be a chained ctor)
                            baseCtorIndex = i;
                            // keep scanning so we end up with the last .ctor call in the prolog area
                        }
                    }

                    // If we hit instructions that are clearly beyond prolog (like a branch target,
                    // a field access, or a method call that isn't .ctor), we can stop scanning for base ctor.
                    if (ins.OpCode.FlowControl == FlowControl.Cond_Branch ||
                        ins.OpCode.FlowControl == FlowControl.Branch ||
                        ins.OpCode.FlowControl == FlowControl.Return)
                    {
                        // break out; we've likely passed the prolog-ish region
                        break;
                    }
                }

                if (baseCtorIndex >= 0)
                {
                    // start scanning forward from after the identified base ctor call
                    int i = baseCtorIndex + 1;
                    // skip prolog-like instructions that commonly follow the ctor call
                    while (i < instrs.Count && IsPrologInstruction(instrs[i]))
                        i++;

                    // If we've found a non-prolog instruction, insert *before* it,
                    // which means return the previous instruction to insert AFTER.
                    if (i < instrs.Count)
                        return i; // insert after this
                                      // fallback: if we reached end, return last instr
                    return instrs.Count - 1;
                }

                // If we couldn't find a base ctor call, fall through to general scanning below.
            }

            // General method: skip common prolog instructions from the start of the method
            int idx = 0;
            while (idx < instrs.Count && IsPrologInstruction(instrs[idx]))
                idx++;

            if (idx < instrs.Count)
            {
                // return the instruction to insert AFTER: if idx==0 we return null previous -> choose first instr
                if (idx == 0)
                    return 0; // insert after instrs[0] (i.e., before next)
                return idx - 1;
            }

            // fallback (method all prolog-like or trivial): insert before the final ret if possible
            for (int i = instrs.Count - 1; i >= 0; i--)
            {
                if (instrs[i].OpCode == OpCodes.Ret)
                {
                    // return previous to ret if exists
                    if (i - 1 >= 0) return i - 1;
                    return i;
                }
            }

            // last resort: return the last instruction
            return instrs.Count - 1;
        }

    }
}