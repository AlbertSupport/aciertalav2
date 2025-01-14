using System;
using System.Windows.Forms;

namespace TerminalV2
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Configuración inicial de la aplicación
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Ejecutar el formulario principal (Form1)
            Application.Run(new Form1());
        }
    }
}