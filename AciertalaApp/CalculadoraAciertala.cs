using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

    public partial class CalculadoraAciertala : Form
    {
        private Button[,] buttonGrid; // Arreglo de botones
        private double resultadoActual = 0; // Guarda el resultado de la última operación
        private bool nuevaOperacion = true; // Controla si se inicia una nueva operación

        public CalculadoraAciertala()
        {
            InitializeComponent();
            CrearBotonesCalculadora(); // Crear los botones
            this.KeyPreview = true; // Habilitar la captura de teclas para el formulario
            this.KeyDown += CalculadoraAciertala_KeyDown; // Vincular el evento KeyDown
        }

        private void CrearBotonesCalculadora()
        {
            string[,] buttons = new string[4, 4]
            {
                { "7", "8", "9", "/" },
                { "4", "5", "6", "*" },
                { "1", "2", "3", "-" },
                { "C", "0", "=", "+" }
            };

            buttonGrid = new Button[buttons.GetLength(0), buttons.GetLength(1)];

            for (int row = 0; row < buttons.GetLength(0); row++)
            {
                for (int col = 0; col < buttons.GetLength(1); col++)
                {
                    if (buttons[row, col] != null)
                    {
                        var button = new Button
                        {
                            Text = buttons[row, col],
                            Font = new Font("Segoe UI", 14, FontStyle.Bold),
                            FlatStyle = FlatStyle.Flat,
                            BackColor = Color.LightGray
                        };
                        button.Click += Button_Click; // Asigna el controlador de eventos
                        buttonGrid[row, col] = button; // Guarda el botón en la matriz
                        this.Controls.Add(button);
                    }
                }
            }

            AjustarBotones(); // Ajusta los botones al tamaño actual del formulario
        }

        private void AjustarBotones()
        {
            if (buttonGrid == null) return;

            int buttonRows = buttonGrid.GetLength(0);
            int buttonCols = buttonGrid.GetLength(1);

            int margin = 10;

            textBoxResult.Width = this.ClientSize.Width - (2 * margin);
            textBoxResult.Height = (int)(this.ClientSize.Height * 0.25); // 25% de la altura del formulario

            int totalWidth = this.ClientSize.Width - (margin * (buttonCols + 1));
            int totalHeight = this.ClientSize.Height - textBoxResult.Height - (margin * (buttonRows + 2));

            int buttonWidth = totalWidth / buttonCols;
            int buttonHeight = totalHeight / buttonRows;

            int startX = margin;
            int startY = textBoxResult.Bottom + margin;

            for (int row = 0; row < buttonRows; row++)
            {
                for (int col = 0; col < buttonCols; col++)
                {
                    var button = buttonGrid[row, col];
                    if (button != null)
                    {
                        button.Size = new Size(buttonWidth, buttonHeight);
                        button.Location = new Point(startX + col * (buttonWidth + margin), startY + row * (buttonHeight + margin));
                    }
                }
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;

            if (button == null) return;

            ProcesarEntrada(button.Text);
        }

        private void CalculadoraAciertala_KeyDown(object sender, KeyEventArgs e)
        {
            string entrada = null;

            // Asignar la entrada según la tecla presionada
            if (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0) entrada = "0";
            else if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1) entrada = "1";
            else if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2) entrada = "2";
            else if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3) entrada = "3";
            else if (e.KeyCode == Keys.D4 || e.KeyCode == Keys.NumPad4) entrada = "4";
            else if (e.KeyCode == Keys.D5 || e.KeyCode == Keys.NumPad5) entrada = "5";
            else if (e.KeyCode == Keys.D6 || e.KeyCode == Keys.NumPad6) entrada = "6";
            else if (e.KeyCode == Keys.D7 || e.KeyCode == Keys.NumPad7) entrada = "7";
            else if (e.KeyCode == Keys.D8 || e.KeyCode == Keys.NumPad8) entrada = "8";
            else if (e.KeyCode == Keys.D9 || e.KeyCode == Keys.NumPad9) entrada = "9";
            else if (e.KeyCode == Keys.Add) entrada = "+";
            else if (e.KeyCode == Keys.Subtract) entrada = "-";
            else if (e.KeyCode == Keys.Multiply) entrada = "*";
            else if (e.KeyCode == Keys.Divide) entrada = "/";
            else if (e.KeyCode == Keys.Enter) entrada = "=";
            else if (e.KeyCode == Keys.Back) entrada = "C";

            if (entrada != null)
            {
                ProcesarEntrada(entrada);
                e.Handled = true; // Evitar que la tecla haga algo más
            }
        }

        private void ProcesarEntrada(string entrada)
        {
            if (double.TryParse(entrada, out double _)) // Números
            {
                if (nuevaOperacion)
                {
                    textBoxResult.Text = entrada;
                    nuevaOperacion = false;
                }
                else
                {
                    textBoxResult.AppendText(entrada);
                }
            }
            else if (entrada == "C") // Limpiar
            {
                textBoxResult.Clear();
                textBoxResult.Text = "0";
                resultadoActual = 0;
                nuevaOperacion = true;
            }
            else if (entrada == "=") // Calcular
            {
                try
                {
                    var result = new DataTable().Compute(textBoxResult.Text, null);
                    resultadoActual = Convert.ToDouble(result);

                    textBoxResult.AppendText($" = {resultadoActual}\r\n");
                    nuevaOperacion = true;
                }
                catch
                {
                    textBoxResult.AppendText(" = Error\r\n");
                    nuevaOperacion = true;
                }
            }
            else // Operaciones
            {
                if (nuevaOperacion)
                {
                    textBoxResult.Text = $"{resultadoActual}{entrada}";
                    nuevaOperacion = false;
                }
                else if (!textBoxResult.Text.EndsWith("+") && !textBoxResult.Text.EndsWith("-") &&
                         !textBoxResult.Text.EndsWith("*") && !textBoxResult.Text.EndsWith("/"))
                {
                    textBoxResult.AppendText(entrada);
                }
            }
        }

        private void CalculadoraAciertala_Resize(object sender, EventArgs e)
        {
            AjustarBotones(); // Reajusta los botones y el cuadro de texto cuando se redimensiona el formulario
        }
    }