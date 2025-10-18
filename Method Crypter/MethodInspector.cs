using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System;
using System.Windows.Forms;

public static class MethodInspector
{
    // Populates a ListView instead of showing a MessageBox
    public static void PopulateListView(string exePath, ListView listView)
    {
        if (exePath == null)
            throw new ArgumentNullException(nameof(exePath));
        if (listView == null)
            throw new ArgumentNullException(nameof(listView));

        var module = ModuleDefMD.Load(exePath);

        listView.BeginUpdate();
        listView.Items.Clear();

        foreach (var type in module.GetTypes()) // includes nested types
        {
            foreach (var method in type.Methods)
            {
                try
                {
                    uint rva = (uint)method.RVA;
                    int encodedSize = GetEncodedMethodSize(module, method);
                    string rvaHex = rva != 0 ? $"0x{rva:X8}" : "0x00000000";
                    string token = method.MDToken.ToString();

                    var item = new ListViewItem(type.FullName);
                    item.SubItems.Add(method.Name);
                    item.SubItems.Add(token);
                    item.SubItems.Add(rvaHex);
                    item.SubItems.Add(encodedSize.ToString());

                    listView.Items.Add(item);
                }
                catch (Exception ex)
                {
                    var item = new ListViewItem(type.FullName);
                    item.SubItems.Add(method.Name);
                    item.SubItems.Add("ERROR");
                    item.SubItems.Add("-");
                    item.SubItems.Add(ex.Message);

                    listView.Items.Add(item);
                }
            }
        }

        listView.EndUpdate();
        AutoResizeColumns(listView);
    }

    // Resizes columns to fit content neatly
    private static void AutoResizeColumns(ListView listView)
    {
        foreach (ColumnHeader column in listView.Columns)
            column.Width = -2; // Auto size to content
    }

    // Same encoded size logic from before
    private static int GetEncodedMethodSize(ModuleDefMD module, MethodDef method)
    {
        if (method == null || !method.HasBody)
            return 0;

        var tokenProvider = new MethodPadder.MethodPadder.AdvancedTokenProvider(); // your existing one
        var writer = new MethodBodyWriter(tokenProvider, method);
        writer.Write();
        var buf = writer.GetFullMethodBody();
        return buf?.Length ?? 0;
    }
}
