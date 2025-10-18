using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MethodInjector
{
    public static class JunkMethodInjector
    {
        public static List<MethodDef> GenerateJunkMethodsInType(
        ModuleDefMD module,
        string typeFullName,
        string methodPrefix,
        int count = 1,
        int minEncodedBytes = 64,
        int randomExtraMax = 16,
        bool noInline = false,
        int rndSeed = -1,
        bool maketiny = false)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));
            if (string.IsNullOrEmpty(methodPrefix)) methodPrefix = "JunkMethod";
            if (string.IsNullOrEmpty(typeFullName)) throw new ArgumentNullException(nameof(typeFullName));
            if (count <= 0) return new List<MethodDef>();

            var rnd = (rndSeed >= 0) ? new Random(rndSeed) : new Random();

            // Find or create the type (modification of module.Types is safe here, not during iteration)
            var targetType = FindOrCreateType(module, typeFullName);

            var created = new List<MethodDef>();
            for (int i = 0; i < count; i++)
            {
                string name = $"{methodPrefix}{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                int goal = minEncodedBytes + (randomExtraMax > 0 ? rnd.Next(0, randomExtraMax + 1) : 0);
                var m = CreateJunkMethodUntilSize(module, targetType, name, goal, noInline, rnd, maketiny);
                created.Add(m);
            }

            // Do NOT add to targetType.Methods here! Caller is responsible.
            return created;
        }

        private static TypeDef FindOrCreateType(ModuleDefMD module, string typeFullName)
        {
            var type = module.Find(typeFullName, false) ?? module.Find(typeFullName, true);
            if (type != null)
                return type;

            string ns = "";
            string name = typeFullName;
            int lastDot = typeFullName.LastIndexOf('.');
            if (lastDot >= 0)
            {
                ns = typeFullName.Substring(0, lastDot);
                name = typeFullName.Substring(lastDot + 1);
            }

            var newType = new TypeDefUser(ns, name, module.CorLibTypes.Object.TypeDefOrRef)
            {
                Attributes = TypeAttributes.NotPublic
            };
            module.Types.Add(newType); // safe if called outside iteration
            return newType;
        }

        private static MethodDef CreateJunkMethodUntilSize(ModuleDefMD module, TypeDef ownerType, string methodName, int goalEncodedBytes, bool noInline, Random rnd, bool maketiny = false)
        {
            var sig = MethodSig.CreateStatic(module.CorLibTypes.Void);
            var method = new MethodDefUser(methodName, sig,
                MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig);

            if (noInline)
                method.ImplAttributes |= MethodImplAttributes.NoInlining;

            // Do NOT add to ownerType.Methods here! Caller will add after iteration

            var body = new CilBody();
            method.Body = body;
            body.InitLocals = true;
            body.MaxStack = 4;

            body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            const int MAX_ITER = 5000;
            int iter = 0;

            while (true)
            {
                iter++;
                if (iter > MAX_ITER)
                    throw new InvalidOperationException($"Unable to grow junk method {methodName} to target size after {MAX_ITER} attempts.");

                int encoded = GetEncodedMethodSize(module, method);
                if (encoded >= Math.Max(goalEncodedBytes, 48))
                    break;

                InsertVariedJunkAtStart(body, module, method, rnd, maketiny);
            }

            if (body.Instructions.Count == 0 || body.Instructions.Last().OpCode != OpCodes.Ret)
                body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            EnsureSingleRetAtEnd(body);

            if (body.MaxStack < 1) body.MaxStack = 1;
            return method;
        }

        private static void EnsureSingleRetAtEnd(CilBody body, bool removeInstructionsAfterLastRet = true)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));

            // Find last ret
            int lastRetIndex = -1;
            for (int i = body.Instructions.Count - 1; i >= 0; i--)
            {
                if (body.Instructions[i].OpCode == OpCodes.Ret)
                {
                    lastRetIndex = i;
                    break;
                }
            }

            if (lastRetIndex == -1)
            {
                // No ret at all -> just append one
                body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                return;
            }

            // Optionally remove any instructions that come after the last ret
            if (removeInstructionsAfterLastRet)
            {
                for (int i = body.Instructions.Count - 1; i > lastRetIndex; i--)
                    body.Instructions.RemoveAt(i);
                // lastRetIndex remains index of last ret
            }

            // If the last instruction is already the ret, nothing more to do
            if (lastRetIndex == body.Instructions.Count - 1)
                return;

            // Otherwise move the found ret to the end
            var retInstr = body.Instructions[lastRetIndex];
            body.Instructions.RemoveAt(lastRetIndex);
            body.Instructions.Add(retInstr);
        }


        private static void InsertVariedJunkAtStart(CilBody body, ModuleDefMD module, MethodDef method, Random rnd, bool maketiny)
        {
            if (body == null || method == null || body.Instructions == null) return;

            int idx = 0;
            int choice = rnd.Next(0, 12);
            if (maketiny) choice = rnd.Next(0, 6);

            Local tempLocal = null;

            // helper: lazily create an Int32 local
            Local EnsureIntLocal(ref Local local)
            {
                if (local != null) return local;
                var intSig = module.CorLibTypes.Int32;
                local = new Local(intSig);
                //local.Name = RandomNameGenerator.GetUniqueName(8);
                body.Variables.Add(local);
                body.InitLocals = true;
                return local;
            }

            switch (choice)
            {
                case 0:
                case 1:
                case 2:
                    body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(0, 10)));
                    body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Pop));
                    break;

                case 3:
                case 4:
                    body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(0, 128)));
                    body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(0, 128)));
                    body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Add));
                    body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Pop));
                    break;

                case 5:
                case 6:
                    body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(0, 256)));
                    body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(0, 256)));
                    body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Xor));
                    body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Pop));
                    break;

                case 7:
                    if (!maketiny)
                    {
                        var l7 = EnsureIntLocal(ref tempLocal);
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(0, 128)));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Stloc, l7));  // store to local
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldloc, l7));  // reload
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Pop));
                    }
                    break;

                case 8:
                    body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(1, 10)));
                    body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(1, 10)));
                    body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Mul));
                    body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Pop));
                    break;

                case 9:
                    if (!maketiny)
                    {
                        var l9 = EnsureIntLocal(ref tempLocal);
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(0, 255)));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(0, 255)));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Add));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Stloc, l9));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldloc, l9));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(0, 128)));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Pop));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Pop));
                    }
                    break;

                case 10:
                    if (!maketiny)
                    {
                        var l10 = EnsureIntLocal(ref tempLocal);
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(0, 64)));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(0, 64)));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Add));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Stloc, l10));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(0, 64)));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldloc, l10));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Mul));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Pop));
                    }
                    break;

                default:
                    if (!maketiny)
                    {
                        var lD = EnsureIntLocal(ref tempLocal);
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(0, 256)));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(0, 256)));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4, rnd.Next(0, 256)));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Add));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Stloc, lD));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldloc, lD));
                        body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Pop));
                    }
                    break;
            }

            // recalc offsets
            body.UpdateInstructionOffsets();
        }

        private static int GetEncodedMethodSize(ModuleDefMD module, MethodDef method)
        {
            var tokenProvider = new MethodPadder.MethodPadder.AdvancedTokenProvider();
            var writer = new MethodBodyWriter(tokenProvider, method);
            writer.Write();
            var buf = writer.GetFullMethodBody();
            return buf.Length;
        }
    }
}