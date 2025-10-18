using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Forms;


namespace Method_Crypter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listViewMethods.FullRowSelect = true;
            listViewStringArrays.FullRowSelect = true;
            LoadSettings();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void buttonEncrypt_Click(object sender, EventArgs e)
        {
            string exePath = textBoxPathToCryptedDemoEXE.Text.Trim();
            if (!File.Exists(exePath))
            {
                MessageBox.Show("Error Finding Crypted Demo EXE.");
                return;
            }

            string outputExePath = textBoxPathToOutputEXE.Text.Trim();
            if (File.Exists(outputExePath)) File.Delete(outputExePath);
            if (File.Exists(outputExePath))
            {
                MessageBox.Show("Error Deleting Output EXE.  (Is it in use?)");
            }

            // Build rename list from your ListView
            List<MethodRenamer.MethodReplacement> renameList = new List<MethodRenamer.MethodReplacement>();
            foreach (ListViewItem lvi in listViewMethods.Items)
            {
                string typeName = lvi.SubItems[0].Text.Trim();
                string methodName = lvi.SubItems[1].Text.Trim();
                renameList.Add(new MethodRenamer.MethodReplacement(typeName, methodName, EncryptMethod.RandomNameGenerator.GetUniqueName(12)));
            }

            //Method Renaming (also renames structs and fields in effected types) (optional)
            if (checkBoxRandomizeMethodNames.Checked)
                MethodRenamer.MethodRenamer.RenameMethodsOnly(exePath, outputExePath, renameList);

            //Insert Junk code at start of methods (optional)
            if (checkBoxInsertJunkCode.Checked)
                MethodPadder.MethodPadder.InjectJunkCodeStartOfMethodOnly(exePath, outputExePath, renameList, checkBoxRandomizeMethodNames.Checked);

            //Create junk methods and interleave them with encrypted methods (not optional)
            MethodInterleaving.MethodInterleaving.DoInterleaving(exePath, outputExePath, renameList, checkBoxRandomizeMethodNames.Checked);
            
            //Randomize Assmebly GUID (optional)
            if (checkBoxRandomizeGUID.Checked) 
                Misc.RandomizeAssemblyGuid(outputExePath);

            //Next encrypt String Arrays
            var key = Convert.FromBase64String(textBoxAESKey.Text);
            var iv = Convert.FromBase64String(textBoxAESIV.Text);
            StringArrayEncrypter.StringArrayEncrypter.EncryptStringArrays(outputExePath, listViewStringArrays.Items, key, iv);

            //Method Inspector
            if (checkBoxShowMethodInspector.Checked)
            {
                var inspectorForm = new FormMethodInspector();
                try
                {
                    MethodInspector.PopulateListView(outputExePath, inspectorForm.listViewMethodInspector);
                    inspectorForm.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error analyzing methods:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (checkBoxRandomizeMethodNames.Checked)
            {
                foreach (var mr in renameList)
                {
                    if (!string.IsNullOrEmpty(mr.OriginalName)) EncryptMethod.EncryptModule(mr.TypeFullName, mr.NewName, outputExePath, key, iv);
                    else EncryptMethod.EncryptModule(mr.TypeFullName, "", outputExePath, key, iv);
                }
            }
            else
            {
                foreach (var mr in renameList)
                {
                    if (!string.IsNullOrEmpty(mr.OriginalName)) EncryptMethod.EncryptModule(mr.TypeFullName, mr.OriginalName, outputExePath, key, iv);
                    else EncryptMethod.EncryptModule(mr.TypeFullName, "", outputExePath, key, iv);
                }
            }

            SaveServerSettings(renameList, checkBoxRandomizeMethodNames.Checked);

            MessageBox.Show("Done encrypting EXE.");
        }


        private void buttonAddMethod_Click(object sender, EventArgs e)
        {
            string type = textBoxMethodType.Text.Trim();
            string methodName = textBoxMethodName.Text.Trim();

            if (string.IsNullOrEmpty(type))
            {
                MessageBox.Show("Please enter a Type\n(Note: If \"Method Name\" is empty, then it means you are encrypting the constructor.)", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            int lastDot = type.LastIndexOf('.');
            if (lastDot != -1)
            {
                string lastPart = type.Substring(lastDot + 1);
                if (lastPart == methodName)
                {
                    MessageBox.Show("Note: To add a constructor, just enter a type with an empty \"Method Name\".", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Add a new row to the ListView
            var item = new ListViewItem(type);
            item.SubItems.Add(methodName);
            listViewMethods.Items.Add(item);
        }

        private void buttonAddStringArray_Click(object sender, EventArgs e)
        {
            string type = textBoxStringMethodType.Text.Trim();
            string arrayName = textBoxStringMethodName.Text.Trim();

            if (string.IsNullOrEmpty(type))
            {
                MessageBox.Show("Please enter a \"Type\".", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (string.IsNullOrEmpty(arrayName))
            {
                MessageBox.Show("Please enter a \"Array Name\".", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Add a new row to the ListView
            var item = new ListViewItem(type);
            item.SubItems.Add(arrayName);
            listViewStringArrays.Items.Add(item);
        }

        private void buttonPathToCryptedDemoEXE_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select an executable";
                ofd.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
                ofd.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textBoxPathToCryptedDemoEXE.Text = ofd.FileName;
                }
            }
        }

        private void buttonPathToServerEXE_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select an executable";
                ofd.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
                ofd.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textBoxPathToServer.Text = ofd.FileName;
                }
            }
        }

        private void buttonPathToOutputEXE_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Create or select output executable";
                sfd.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
                sfd.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                sfd.FileName = "output.exe"; // optional default name

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    textBoxPathToOutputEXE.Text = sfd.FileName;
                }
            }
        }

        private void buttonClearMethods_Click(object sender, EventArgs e)
        {
            listViewMethods.Items.Clear();
        }

        private void buttonClearStringArrays_Click(object sender, EventArgs e)
        {
            listViewStringArrays.Items.Clear();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewMethods.SelectedItems)
            {
                listViewMethods.Items.Remove(item);
            }
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewStringArrays.SelectedItems)
            {
                listViewStringArrays.Items.Remove(item);
            }
        }

        private void listViewMethods_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewMethods.SelectedItems.Count > 0)
            {
                textBoxMethodType.Text = listViewMethods.SelectedItems[0].Text;
                textBoxMethodName.Text = listViewMethods.SelectedItems[0].SubItems[1].Text;
            }
        }

        private void listViewStringMethods_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewStringArrays.SelectedItems.Count > 0)
            {
                textBoxStringMethodType.Text = listViewStringArrays.SelectedItems[0].Text;
                textBoxStringMethodName.Text = listViewStringArrays.SelectedItems[0].SubItems[1].Text;
            }
        }

        private void listViewMethods_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                foreach (ListViewItem item in listViewMethods.SelectedItems)
                {
                    listViewMethods.Items.Remove(item);
                }
            }
        }

        private void listViewStringMethods_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                foreach (ListViewItem item in listViewStringArrays.SelectedItems)
                {
                    listViewStringArrays.Items.Remove(item);
                }
            }
        }

        private void buttonRndAESKeyAndIV_Click(object sender, EventArgs e)
        {
            // Create a new instance of the Aes class
            using (Aes aes = Aes.Create())
            {
                // Generate a new key
                aes.GenerateKey();
                aes.GenerateIV();

                // Display the key and IV
                textBoxAESKey.Text = Convert.ToBase64String(aes.Key);
                textBoxAESIV.Text = Convert.ToBase64String(aes.IV);
            }
        }

        ///////////////////////////////////////////////
        //
        //              Save/Load Settings
        //
        ///////////////////////////////////////////////

        private void SaveSettings()
        {
            JsonSyncSettings jss = new JsonSyncSettings();
            jss.jsonTextBoxPathToCryptedDemoEXE = textBoxPathToCryptedDemoEXE.Text;
            jss.jsonTextBoxAESKey = textBoxAESKey.Text;
            jss.jsonTextBoxAESIV = textBoxAESIV.Text;
            jss.jsonTextBoxPathToServer = textBoxPathToServer.Text;
            jss.jsonCheckBoxRandomizeMethodNames = checkBoxRandomizeMethodNames.Checked;
            jss.jsonCheckBoxInsertJunkCode = checkBoxInsertJunkCode.Checked;
            jss.jsonCheckBoxRandomizeGUID = checkBoxRandomizeGUID.Checked;
            jss.jsonCheckBoxShowMethodInspector = checkBoxShowMethodInspector.Checked;

            // Save ListView data for Methods
            jss.Items = new List<ListViewEntry>();
            foreach (ListViewItem lvi in listViewMethods.Items)
            {
                jss.Items.Add(new ListViewEntry
                {
                    Type = lvi.SubItems[0].Text,
                    MethodName = lvi.SubItems[1].Text
                });
            }

            // Save ListView data for String Arrays
            jss.Items2 = new List<ListViewEntry2>();
            foreach (ListViewItem lvi in listViewStringArrays.Items)
            {
                jss.Items2.Add(new ListViewEntry2
                {
                    Type = lvi.SubItems[0].Text,
                    ArrayName = lvi.SubItems[1].Text
                });
            }

            string fileName = Path.Combine(
                Path.GetDirectoryName(Application.ExecutablePath),
                "settings.json"
            );

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string jsonText = JsonSerializer.Serialize(jss, options);
            File.WriteAllText(fileName, jsonText);
        }

        private void LoadSettings()
        {
            string fileName = Path.Combine(
                Path.GetDirectoryName(Application.ExecutablePath),
                "settings.json"
            );

            if (File.Exists(fileName))
            {
                string jsonText = File.ReadAllText(fileName);
                JsonSyncSettings jss = JsonSerializer.Deserialize<JsonSyncSettings>(jsonText);

                if (jss != null)
                {
                    textBoxPathToCryptedDemoEXE.Text = jss.jsonTextBoxPathToCryptedDemoEXE;
                    textBoxAESKey.Text = jss.jsonTextBoxAESKey;
                    textBoxAESIV.Text = jss.jsonTextBoxAESIV;
                    textBoxPathToServer.Text = jss.jsonTextBoxPathToServer;
                    checkBoxRandomizeMethodNames.Checked = jss.jsonCheckBoxRandomizeMethodNames;
                    checkBoxInsertJunkCode.Checked = jss.jsonCheckBoxInsertJunkCode;
                    checkBoxRandomizeGUID.Checked = jss.jsonCheckBoxRandomizeGUID;
                    checkBoxShowMethodInspector.Checked = jss.jsonCheckBoxShowMethodInspector;

                    listViewMethods.Items.Clear();
                    if (jss.Items != null)
                    {
                        foreach (var entry in jss.Items)
                        {
                            var item = new ListViewItem(entry.Type);
                            item.SubItems.Add(entry.MethodName);
                            listViewMethods.Items.Add(item);
                        }
                    }

                    listViewStringArrays.Items.Clear();
                    if (jss.Items2 != null)
                    {
                        foreach (var entry in jss.Items2)
                        {
                            var item = new ListViewItem(entry.Type);
                            item.SubItems.Add(entry.ArrayName);
                            listViewStringArrays.Items.Add(item);
                        }
                    }
                }
            }
        }

        public class JsonSyncSettings
        {
            public string jsonTextBoxPathToCryptedDemoEXE { get; set; }
            public string jsonTextBoxAESKey { get; set; }
            public string jsonTextBoxAESIV { get; set; }
            public string jsonTextBoxPathToServer { get; set; }
            public bool jsonCheckBoxRandomizeMethodNames { get; set; }
            public bool jsonCheckBoxInsertJunkCode { get; set; }
            public bool jsonCheckBoxRandomizeGUID { get; set; }
            public bool jsonCheckBoxShowMethodInspector { get; set; }
            public List<ListViewEntry> Items { get; set; }
            public List<ListViewEntry2> Items2 { get; set; }
        }

        public class ListViewEntry
        {
            public string Type { get; set; }
            public string MethodName { get; set; }
        }

        public class ListViewEntry2
        {
            public string Type { get; set; }
            public string ArrayName { get; set; }
        }

        private void SaveServerSettings(List<MethodRenamer.MethodReplacement> renameList, bool useNewNames)
        {
            if (!File.Exists(textBoxPathToServer.Text))
            {
                MessageBox.Show("Error: Could not find Server.exe!");
                return;
            }
            JsonSyncServerSettings jss = new JsonSyncServerSettings();
            jss.jsonkey = textBoxAESKey.Text;
            jss.jsoniv = textBoxAESIV.Text;

            // Save ListView data
            if (useNewNames)
            {
                jss.jsonTypeAndMethod = new List<TypeAndMethodEncryptionEntry>();
                foreach (var mr in renameList)
                {
                    if (!string.IsNullOrEmpty(mr.OriginalName))
                    {
                        jss.jsonTypeAndMethod.Add(new TypeAndMethodEncryptionEntry
                        {
                            Type = mr.TypeFullName,
                            MethodName = mr.NewName
                        });
                    }
                    else
                    {
                        jss.jsonTypeAndMethod.Add(new TypeAndMethodEncryptionEntry
                        {
                            Type = mr.TypeFullName,
                            MethodName = ""
                        });
                    }
                }
            }
            else
            {
                jss.jsonTypeAndMethod = new List<TypeAndMethodEncryptionEntry>();
                foreach (var mr in renameList)
                {
                    if (!string.IsNullOrEmpty(mr.OriginalName))
                    {
                        jss.jsonTypeAndMethod.Add(new TypeAndMethodEncryptionEntry
                        {
                            Type = mr.TypeFullName,
                            MethodName = mr.OriginalName
                        });
                    }
                    else
                    {
                        jss.jsonTypeAndMethod.Add(new TypeAndMethodEncryptionEntry
                        {
                            Type = mr.TypeFullName,
                            MethodName = ""
                        });
                    }
                }
            }

            jss.jsonTypeAndStringArray = new List<TypeAndStringArrayEncryptionEntry>();
            foreach (ListViewItem lvi in listViewStringArrays.Items)
            {
                jss.jsonTypeAndStringArray.Add(new TypeAndStringArrayEncryptionEntry
                {
                    Type = lvi.SubItems[0].Text,
                    ArrayName = lvi.SubItems[1].Text
                });
            }

            string fileName = Path.Combine(
                Path.GetDirectoryName(textBoxPathToServer.Text),
                "server_settings.json"
            );

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string jsonText = JsonSerializer.Serialize(jss, options);
            File.WriteAllText(fileName, jsonText);
        }

        public class JsonSyncServerSettings
        {
            public string jsonkey { get; set; }
            public string jsoniv { get; set; }
            public List<TypeAndMethodEncryptionEntry> jsonTypeAndMethod { get; set; }
            public List<TypeAndStringArrayEncryptionEntry> jsonTypeAndStringArray { get; set; }
        }

        public class TypeAndMethodEncryptionEntry
        {
            public string Type { get; set; }
            public string MethodName { get; set; }
        }

        public class TypeAndStringArrayEncryptionEntry
        {
            public string Type { get; set; }
            public string ArrayName { get; set; }
        }
    }
}
