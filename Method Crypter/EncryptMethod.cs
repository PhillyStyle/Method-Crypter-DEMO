using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Method_Crypter
{
    internal class EncryptMethod
    {
        public static void EncryptModule(string moduleName, string methodName, string cryptedPath, byte[] key, byte[] iv)
        {
            ModuleDefMD module = ModuleDefMD.Load(cryptedPath);
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
                int cryptSize = GetMethodILSize(module, method);
                var fileoffset = GetMethodILStartFileOffset(module, method);

                //cryptSize = MultipleOf32(cryptSize,32);
                int encryptedSizeShouldBe = GetAesEncryptedBufferSize(cryptSize);

                byte[] buffer = new byte[cryptSize];
                using (FileStream fs = new FileStream(cryptedPath, FileMode.Open, FileAccess.ReadWrite))
                {
                    fs.Seek(fileoffset, SeekOrigin.Begin);
                    int bytesRead = fs.Read(buffer, 0, cryptSize);
                    var encryptedData = Encrypt(buffer, key, iv);

                    if (encryptedSizeShouldBe != encryptedData.Length)
                        throw new Exception($"Encrypted Size Should Be {encryptedSizeShouldBe}\nEncrypted Size Actually is {encryptedData.Length}.");

                    fs.Seek(fileoffset, SeekOrigin.Begin);
                    fs.Write(encryptedData, 0, cryptSize); //First put what will fit, back where it came from

                    var junkMethod = GetNextMethodInType(method); //Get Junk method that follows our real method
                    var junkMethodFileoffset = GetMethodILStartFileOffset(module, junkMethod);
                    fs.Seek(junkMethodFileoffset, SeekOrigin.Begin);
                    fs.Write(encryptedData, cryptSize, encryptedData.Length-cryptSize); //Write the remaining
                }
            }
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

        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Key = key;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                    }
                    return ms.ToArray();
                }
            }
        }

        public static string EncryptString(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException(nameof(Key));
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException(nameof(IV));

            byte[] encrypted;

            // Create an Aes object with the specified key and IV
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // Write all data to the stream
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes
            return Convert.ToBase64String(encrypted);
        }

        public static class RandomNameGenerator
        {
            private static readonly HashSet<string> usedNames = new HashSet<string>();

            public static string GetUniqueName(int length = 12)
            {
                string candidate;
                do
                {
                    candidate = Guid.NewGuid().ToString("N").Substring(0, length);
                } while (!usedNames.Add(candidate));
                return candidate;
            }
        }
    }
}
