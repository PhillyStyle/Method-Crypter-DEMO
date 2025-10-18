using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System;
using System.IO;
using System.Windows.Forms;

namespace Method_Crypter
{
    internal class Misc
    {
        public static void RandomizeAssemblyGuid(string exePath)
        {
            ModuleDefMD module = ModuleDefMD.Load(exePath);

            RandomizeAssemblyGuid(module);

            string tempPath = exePath + ".random.guid.tmp.exe";
            try
            {
                var writerOpts = new ModuleWriterOptions(module) { WritePdb = false };
                writerOpts.MetadataOptions.Flags |= dnlib.DotNet.Writer.MetadataFlags.KeepOldMaxStack;
                module.Write(tempPath, writerOpts);
                File.Delete(exePath);
                File.Move(tempPath, exePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Method renaming caused error:\n" + ex.ToString());
            }
        }

        public static void RandomizeAssemblyGuid(ModuleDefMD module)
        {
            // Get assembly definition
            var asm = module.Assembly;

            // Find GuidAttribute
            var guidAttr = asm.CustomAttributes.Find("System.Runtime.InteropServices.GuidAttribute");

            if (guidAttr != null && guidAttr.ConstructorArguments.Count > 0)
            {
                // Generate new GUID string
                string newGuid = Guid.NewGuid().ToString();

                // Update the attribute argument
                guidAttr.ConstructorArguments[0] = new CAArgument(module.CorLibTypes.String, newGuid);
            }
        }
    }
}
