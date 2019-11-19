using System;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using dnlib.DotNet.Emit;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace SizeOf_Replacer
{
    public partial class Form1 : Form
    {
        string directoryName = "";
        string filePath = "";
        static ModuleDefMD module = null;
        public Thread thr;

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Label2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Rhotav");
        }

        private void Panel1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                Array array = (Array)e.Data.GetData(DataFormats.FileDrop);
                if (array != null)
                {
                    string text = array.GetValue(0).ToString();
                    int num = text.LastIndexOf(".");
                    if (num != -1)
                    {
                        string text2 = text.Substring(num);
                        text2 = text2.ToLower();
                        if (text2 == ".exe" || text2 == ".dll")
                        {
                            Activate();
                            int num2 = text.LastIndexOf("\\");
                            if (num2 != -1)
                            {
                                directoryName = text.Remove(num2, text.Length - num2);
                            }
                            if (directoryName.Length == 2)
                            {
                                directoryName += "\\";
                            }
                            module = ModuleDefMD.Load(text);
                            filePath = text;
                            pictureBox1.BackColor = Color.Lime;
                            label4.Text = "Loaded !";
                            label4.ForeColor = Color.Lime;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                filePath = "";
                module = null;
                MessageBox.Show(ex.Message, "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                pictureBox1.BackColor = Color.Red;
                label4.Text = "Not Loaded !";
                label4.ForeColor = Color.Lime;
            }
        }

        private void Panel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            if (filePath == string.Empty) return;

            MessageBox.Show(filePath,"SizeOf Replacer",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Executable Files|*.exe|DLL Files |*.dll";
                if (open.ShowDialog() == DialogResult.OK)
                {
                    module = ModuleDefMD.Load(open.FileName);
                    filePath = open.FileName;
                    pictureBox1.BackColor = Color.Lime;
                    label4.Text = "Loaded !";
                    label4.ForeColor = Color.Lime;
                }
            }
            catch(Exception ex)
            {
                filePath = "";
                module = null;
                MessageBox.Show(ex.Message , "Error !" , MessageBoxButtons.OK , MessageBoxIcon.Error);
                pictureBox1.BackColor = Color.Red;
                label4.Text = "Not Loaded !";
                label4.ForeColor = Color.Lime;
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if(filePath != string.Empty && module != null)
            {
                thr = new Thread(new ThreadStart(CodeBlock));
                thr.Start();
            }
        }
        public void CodeBlock()
        {
            try
            {
                int decrypted = 0;

                foreach (TypeDef type in module.Types)
                {
                    if (!type.HasMethods) continue;
                    foreach (MethodDef method in type.Methods)
                    {
                        if (!method.HasBody) continue;

                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if (method.Body.Instructions[i].OpCode == OpCodes.Sizeof)
                            {
                                int value = ReturnSizeOf(method.Body.Instructions[i].Operand.ToString());
                                method.Body.Instructions[i].OpCode = OpCodes.Ldc_I4;
                                method.Body.Instructions[i].Operand = value;

                                i += 1;
                                decrypted++;
                            }
                        }

                    }
                }
                SaveAssembly();
                MessageBox.Show("Replaced " + decrypted.ToString() + " sizeOf !", "SizeOf Replacer", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        static void SaveAssembly()
        {
            var writerOptions = new NativeModuleWriterOptions(module, true);
            writerOptions.Logger = DummyLogger.NoThrowInstance;
            writerOptions.MetadataOptions.Flags = (MetadataFlags.PreserveTypeRefRids | MetadataFlags.PreserveTypeDefRids | MetadataFlags.PreserveFieldRids | MetadataFlags.PreserveMethodRids | MetadataFlags.PreserveParamRids | MetadataFlags.PreserveMemberRefRids | MetadataFlags.PreserveStandAloneSigRids | MetadataFlags.PreserveEventRids | MetadataFlags.PreservePropertyRids | MetadataFlags.PreserveTypeSpecRids | MetadataFlags.PreserveMethodSpecRids | MetadataFlags.PreserveStringsOffsets | MetadataFlags.PreserveUSOffsets | MetadataFlags.PreserveBlobOffsets | MetadataFlags.PreserveAll | MetadataFlags.AlwaysCreateGuidHeap | MetadataFlags.PreserveExtraSignatureData | MetadataFlags.KeepOldMaxStack);
            module.NativeWrite(Path.GetDirectoryName(module.Location) + @"\" + Path.GetFileNameWithoutExtension(module.Location) + "_sizeOf.exe", writerOptions);
        }
        public static int ReturnSizeOf(string deger)
        {
            switch (deger)
            {
                case "System.SByte":
                    return 1;
                case "System.Boolean":
                    return 1;
                case "System.Decimal":
                    return 16;
                case "System.Int16":
                    return 2;
                case "System.Int32":
                    return 4;
                case "System.Byte":
                    return 1;
                case "System.Int64":
                    return 8;
                case "System.Single":
                    return 4;
            }
            return 0;
        }
    }
}
