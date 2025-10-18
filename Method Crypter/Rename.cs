using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MethodRenamer
{
    public struct MethodReplacement
    {
        public string TypeFullName;   // e.g. "MyNamespace.MyClass"
        public string OriginalName;   // method name
        public string NewName;        // randomized/obfuscated name

        public MethodReplacement(string typeFullName, string originalName, string newName)
        {
            TypeFullName = typeFullName;
            OriginalName = originalName;
            NewName = newName;
        }
    }

    public struct FieldReplacement
    {
        public string TypeFullName { get; }
        public string OriginalName { get; }
        public string NewName { get; }

        public FieldReplacement(string typeFullName, string originalName, string newName)
        {
            TypeFullName = typeFullName;
            OriginalName = originalName;
            NewName = newName;
        }
    }

    public static class MethodRenamer
    {
        public static void PatchNullInstructions(ModuleDefMD module)
        {
            foreach (var type in module.GetTypes())
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    var body = method.Body;

                    bool patched = false;

                    for (int i = 0; i < body.Instructions.Count; i++)
                    {
                        if (body.Instructions[i] == null)
                        {
                            body.Instructions[i] = Instruction.Create(OpCodes.Nop);
                            patched = true;
                        }
                    }

                    // Also patch branch operands
                    foreach (var instr in body.Instructions)
                    {
                        if (instr.Operand is Instruction target && target == null)
                            instr.Operand = body.Instructions[0];

                        if (instr.Operand is Instruction[] targets)
                        {
                            for (int j = 0; j < targets.Length; j++)
                                if (targets[j] == null)
                                    targets[j] = body.Instructions[0];
                        }
                    }

                    // Remove EHs that reference nulls
                    for (int i = body.ExceptionHandlers.Count - 1; i >= 0; i--)
                    {
                        var eh = body.ExceptionHandlers[i];
                        if (eh.TryStart == null || eh.TryEnd == null || eh.HandlerStart == null || eh.HandlerEnd == null)
                            body.ExceptionHandlers.RemoveAt(i);
                    }

                    if (patched)
                        MessageBox.Show($"Patched null instructions in {method.FullName}");
                }
            }
        }

        public static void RenameMethodsOnly(string exePath, string outputExePath, List<MethodReplacement> renameList)
        {
            string loadEXEPath = "";
            if (File.Exists(outputExePath)) loadEXEPath = outputExePath;
            else loadEXEPath = exePath;

            ModuleDefMD module = ModuleDefMD.Load(loadEXEPath);
            PatchNullInstructions(module);

            RenameMethodsOnly(module, renameList);
            RenameStructsAndFields(module, renameList);

            string tempPath = outputExePath + ".methods.tmp.exe";
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

        public static void RenameMethodsOnly(ModuleDefMD module, List<MethodReplacement> replacements)
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
                    if (method.IsConstructor) continue;
                    if (!method.HasBody) continue;
                    if (string.IsNullOrEmpty(mr.OriginalName)) continue;

                    if (method.Name == mr.OriginalName)
                    {
                        method.Name = mr.NewName;
                    }
                }
            }
        }


        /// <summary>
        /// Renames structs and fields recursively for outer types present in renameList.
        /// Outer classes are not renamed.
        /// </summary>
        public static void RenameStructsAndFields(ModuleDefMD module, List<MethodReplacement> renameList)
        {
            foreach (var type in module.Types)
            {
                RenameTypeRecursively(type, renameList, false);
            }
        }

        private static void RenameTypeRecursively(TypeDef type, List<MethodReplacement> renameList, bool ancestorShouldRename)
        {
            // Determine if this outer type is in rename list
            string typePrefix = type.FullName.Split('+')[0];
            bool shouldRenameThisBranch = ancestorShouldRename || (renameList.FindIndex(x => x.TypeFullName == typePrefix) != -1);

            if (shouldRenameThisBranch)
            {
                // Rename the type if it's a struct
                if (type.IsValueType && !type.IsEnum)
                {
                    string oldName = type.Name;
                    type.Name = Method_Crypter.EncryptMethod.RandomNameGenerator.GetUniqueName(12);
                    //Console.WriteLine($"Renamed struct: {oldName} -> {type.Name}");
                }

                // Rename all fields
                foreach (var field in type.Fields)
                {
                    if (!field.IsSpecialName && !field.IsRuntimeSpecialName)
                    {
                        string oldFieldName = field.Name;
                        field.Name = Method_Crypter.EncryptMethod.RandomNameGenerator.GetUniqueName(12);
                        //Console.WriteLine($"Renamed field: {oldFieldName} -> {field.Name}");
                    }
                }
            }

            // Recurse into nested types
            foreach (var nested in type.NestedTypes)
            {
                RenameTypeRecursively(nested, renameList, shouldRenameThisBranch);
            }
        }
    }
}