using dnlib.DotNet;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Crypted_Demo
{
    internal class DecryptMethod
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        public static void DecryptStringArray(string type, string arrayName, byte[] key, byte[] iv)
        {
            if ((type == "EncryptedStrings.EncryptedStrings") && (arrayName == "Strings"))
            {
                for (int i = 0; i < EncryptedStrings.EncryptedStrings.Strings.Length; i++)
                {
                    EncryptedStrings.EncryptedStrings.Strings[i] = DecryptString(EncryptedStrings.EncryptedStrings.Strings[i], key, iv);
                }
            }
        }

        public static void DecryptModule(string moduleName, string methodName, byte[] key, byte[] iv)
        {
            string tmpPath = Path.Combine(Path.GetTempPath(), GenRandomString(12) + ".tmp");

            // Copy so dnlib can load the assembly
            File.Copy(Assembly.GetExecutingAssembly().Location, tmpPath, true);

            ModuleDefMD module = ModuleDefMD.Load(tmpPath);
            TypeDef type = module.Find(moduleName, isReflectionName: true);

            if (type == null)
                throw new Exception($"Type '{moduleName}' not found.");

            // Collect all methods that match methodName (in case of overloaded methods)
            var methods = string.IsNullOrEmpty(methodName)
                ? type.Methods.Where(m => m.Name == ".ctor").ToList()  // constructor case
                : type.Methods.Where(m => m.Name == methodName).ToList();

            if (methods.Count == 0)
                throw new Exception($"No methods named '{methodName}' found in type '{moduleName}'.");

            foreach (var method in methods)
            {
                //Get Main Method
                int mainMethodSize = GetMethodILSize(module, method);
                var virtualAddress = GetMethodILStartVA(module, method, Process.GetCurrentProcess().MainModule.BaseAddress);

                var cryptSize = GetAesEncryptedBufferSize(mainMethodSize);

                const uint PAGE_EXECUTE_READWRITE = 0x40;
                uint oldProtect, oldProtect2;
                VirtualProtect(virtualAddress, (UIntPtr)mainMethodSize, PAGE_EXECUTE_READWRITE, out oldProtect);

                byte[] encryptedBuff = new byte[cryptSize];
                Marshal.Copy(virtualAddress, encryptedBuff, 0, mainMethodSize);

                //Get Junk Method
                var junkMethod = GetNextMethodInType(method); //Get Junk method that follows our real method
                var junkMethodVirtualAddress = GetMethodILStartVA(module, junkMethod, Process.GetCurrentProcess().MainModule.BaseAddress);

                VirtualProtect(junkMethodVirtualAddress, (UIntPtr)cryptSize - mainMethodSize, PAGE_EXECUTE_READWRITE, out oldProtect2);
                Marshal.Copy(junkMethodVirtualAddress, encryptedBuff, mainMethodSize, cryptSize - mainMethodSize);

                byte[] bytesOutput = Decrypt(encryptedBuff, key, iv);
                Marshal.Copy(bytesOutput, 0, virtualAddress, bytesOutput.Length);

                // restore original protection
                VirtualProtect(virtualAddress, (UIntPtr)mainMethodSize, oldProtect, out _);
                VirtualProtect(junkMethodVirtualAddress, (UIntPtr)cryptSize - mainMethodSize, oldProtect2, out _);
            }

            File.Delete(tmpPath);
        }

        public static MethodDef GetNextMethodInType(MethodDef method)
        {
            if (method == null || method.DeclaringType == null)
                return null;

            var methods = method.DeclaringType.Methods;
            int index = methods.IndexOf(method);

            if (index < 0 || index + 1 >= methods.Count)
                return null; // last method or not found

            return methods[index + 1];
        }

        public static int GetAesEncryptedBufferSize(int plainSize, int minSize = 16)
        {
            const int aesBlockSize = 16;

            // Always add at least one block of padding
            int blocks = (plainSize / aesBlockSize) + 1;
            int total = blocks * aesBlockSize;

            if (total < minSize)
                total = minSize;

            return total;
        }

        public static IntPtr GetEOFMemAddress()
        {
            var proc = Process.GetCurrentProcess();
            var mainModule = proc.MainModule; // System.Diagnostics.ProcessModule

            IntPtr baseAddr = mainModule.BaseAddress;
            int moduleSize = mainModule.ModuleMemorySize;

            return baseAddr + moduleSize; // pointer just past the module
        }

        public static IntPtr GetMethodILStartVA(ModuleDefMD module, MethodDef method, IntPtr moduleBase)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (method.RVA == 0 || !method.HasBody) return IntPtr.Zero;

            int ilOffset;
            var reader = module.Metadata.PEImage.CreateReader(method.RVA);

            byte first = reader.ReadByte();

            if ((first & 3) == 2) // tiny header
                ilOffset = (int)method.RVA + 1;
            else // fat header
                ilOffset = (int)method.RVA + method.Body.HeaderSize;

            // Add RVA to module base to get absolute VA in memory
            return IntPtr.Add(moduleBase, ilOffset);
        }

        public static int GetMethodILSize(ModuleDefMD module, MethodDef method)
        {
            if (method == null || method.RVA == 0 || !method.HasBody || method.Body == null)
                return 0;

            var peImage = module.Metadata.PEImage;

            try
            {
                var reader = peImage.CreateReader(method.RVA);
                byte first = reader.ReadByte();

                // Tiny header
                if ((first & 0x3) == 2)
                {
                    return first >> 2; // code size only
                }
                else
                {
                    // Fat header
                    reader.Position--;
                    ushort flagsAndSize = reader.ReadUInt16();
                    int headerSize = (flagsAndSize >> 12) * 4;

                    reader.ReadUInt16(); // maxStack
                    int codeSize = reader.ReadInt32();
                    reader.ReadUInt32(); // localVarSigTok

                    // NO EH sections
                    return codeSize;
                }
            }
            catch
            {
                return 0;
            }
        }

        /// Returns the file offset (bytes from start of file) where the method body header begins.
        /// If you want the offset where the IL code starts, add method.Body.HeaderSize.
        /// Returns -1 if not available.
        public static long GetMethodBodyFileOffset(ModuleDefMD module, MethodDef method)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (method.RVA == 0) return -1;           // no RVA (P/Invoke, extern, etc.)
            if (module.Metadata?.PEImage == null) return -1;

            // Convert RVA -> file offset
            uint rva = (uint)method.RVA;
            long fileOffset = (uint)module.Metadata.PEImage.ToFileOffset((dnlib.PE.RVA)rva);

            return fileOffset;
        }

        /// Returns the file offset where the IL instruction stream starts (header + code offset).
        /// Returns -1 if not available.
        public static long GetMethodILStartFileOffset(ModuleDefMD module, MethodDef method)
        {
            var baseOffset = GetMethodBodyFileOffset(module, method);
            if (baseOffset < 0) return -1;
            if (!method.HasBody || method.Body == null)
            {
                // Still valid to compute header location even if dnlib hasn't parsed a body,
                // but method.Body may be null in some scenarios.
                return baseOffset;
            }

            // The header size (tiny vs fat) is available from dnlib's parsed body:
            int headerSize = method.Body.HeaderSize; // number of bytes of method header
            return baseOffset + headerSize;
        }

        public static byte[] Decrypt(byte[] encr, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(encr, 0, encr.Length);
                    }

                    return memoryStream.ToArray();
                }
            }
        }

        private static readonly char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

        private static readonly Random random = new Random();

        public static string GenRandomString(int length)
        {
            if (length < 0) throw new ArgumentException("Length must be non-negative", nameof(length));

            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                char c = chars[random.Next(chars.Length)];
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static string DecryptString(string encryptedstr, byte[] key, byte[] iv)
        {
            if (string.IsNullOrEmpty(encryptedstr)) return "";
            byte[] encrypted = Convert.FromBase64String(encryptedstr);
            if (encrypted == null) return "";
            if (encrypted.Length == 0) return "";


            return DecryptStringFromBytes(encrypted, key, iv);
        }

        static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));

            return Encoding.UTF8.GetString(Decrypt(cipherText, key, iv));
        }
    }
}
