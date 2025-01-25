using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;

namespace AciertalaApp
{
    public partial class FormPrintSettings : Form
    {
        private readonly string DefaultLogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logo", "LogoAciertala.png");

        public FormPrintSettings()
        {
            InitializeComponent();
        }

        private void FormPrintSettings_Load(object sender, EventArgs e)
        {
            cmbPrinters.Items.Clear();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                cmbPrinters.Items.Add(printer);
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.PrinterName))
            {
                cmbPrinters.SelectedItem = Properties.Settings.Default.PrinterName;
            }

            if (Properties.Settings.Default.PaperSize == "58mm")
            {
                rb58mm.Checked = true;
            }
            else if (Properties.Settings.Default.PaperSize == "80mm")
            {
                rb80mm.Checked = true;
            }

            LoadLogo();
        }

        private void LoadLogo()
        {
            Debug.WriteLine($"Verificando ruta del logo: {DefaultLogoPath}");

            if (!string.IsNullOrEmpty(Properties.Settings.Default.LogoPath) && File.Exists(Properties.Settings.Default.LogoPath))
            {
                picLogo.Image = Image.FromFile(Properties.Settings.Default.LogoPath);
                Debug.WriteLine($"Logo cargado desde configuración: {Properties.Settings.Default.LogoPath}");
            }
            else if (File.Exists(DefaultLogoPath))
            {
                picLogo.Image = Image.FromFile(DefaultLogoPath);
                Properties.Settings.Default.LogoPath = DefaultLogoPath;
                Properties.Settings.Default.Save();
                Debug.WriteLine($"Logo cargado desde ruta predeterminada: {DefaultLogoPath}");
            }
            else
            {
                picLogo.Image = null;
                Debug.WriteLine("No se encontró el archivo de logo predeterminado.");
                MessageBox.Show("No se encontró un logo predeterminado. Por favor, seleccione un logo.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSeleccionarLogo_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Archivos de Imagen|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Seleccionar Logo de Impresión"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                picLogo.Image = Image.FromFile(ofd.FileName);
                Properties.Settings.Default.LogoPath = ofd.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (cmbPrinters.SelectedItem != null)
            {
                Properties.Settings.Default.PrinterName = cmbPrinters.SelectedItem.ToString();
            }

            if (rb58mm.Checked)
            {
                Properties.Settings.Default.PaperSize = "58mm";
            }
            else if (rb80mm.Checked)
            {
                Properties.Settings.Default.PaperSize = "80mm";
            }

            if (picLogo.Image != null)
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.LogoPath) || !File.Exists(Properties.Settings.Default.LogoPath))
                {
                    if (File.Exists(DefaultLogoPath))
                    {
                        Properties.Settings.Default.LogoPath = DefaultLogoPath;
                    }
                }
                else
                {
                    Properties.Settings.Default.LogoPath = Properties.Settings.Default.LogoPath;
                }
            }
            else
            {
                if (File.Exists(DefaultLogoPath))
                {
                    Properties.Settings.Default.LogoPath = DefaultLogoPath;
                }
            }

            Properties.Settings.Default.Save();
            MessageBox.Show("Configuraciones guardadas correctamente.", "Configuración de Impresión", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Close();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.PrinterName = string.Empty;
            Properties.Settings.Default.PaperSize = "58mm";
            Properties.Settings.Default.LogoPath = DefaultLogoPath;

            Properties.Settings.Default.Save();

            cmbPrinters.SelectedItem = null;
            rb58mm.Checked = true;
            LoadLogo();

            MessageBox.Show("Las configuraciones se han restablecido a los valores predeterminados.", "Configuración de Impresión", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
