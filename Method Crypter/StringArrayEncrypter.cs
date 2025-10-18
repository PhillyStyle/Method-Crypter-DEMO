using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;

namespace StringArrayEncrypter
{
    internal class StringArrayEncrypter
    {

        public static void EncryptStringArrays(string exePath, System.Windows.Forms.ListView.ListViewItemCollection lvic, byte[] key, byte[] iv)
        {
            ModuleDefMD module = ModuleDefMD.Load(exePath);

            foreach (ListViewItem lvi in lvic)
            {
                string type = lvi.SubItems[0].Text;
                string stringArray = lvi.SubItems[1].Text;
                EncryptStringArray(module, type, stringArray, key, iv);
            }

            string tempPath = exePath + ".strings.encrypt.tmp.exe";
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


        public static void EncryptStringArray(ModuleDef module, string type, string stringArray, byte[] key, byte[] iv)
        {
            // find the StringResource class
            var srType = module.Types.First(t => t.FullName == type);

            // find the field
            var field = srType.Fields.First(f => f.Name == stringArray);

            // inspect the cctor (.cctor = static constructor)
            var cctor = srType.FindStaticConstructor();

            foreach (var instr in cctor.Body.Instructions)
            {
                if (instr.OpCode == OpCodes.Ldstr)
                {
                    // take the string literal
                    var original = (string)instr.Operand;

                    // encrypt it
                    var encrypted = Method_Crypter.EncryptMethod.EncryptString(original, key, iv);

                    // replace with encrypted value
                    instr.Operand = encrypted;
                }
            }
            return;
        }
    }
}
