using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

public class CustomUpdateDialog : Form
{
    private Label titleLabel;
    private Label messageLabel;
    private Label countdownLabel;
    private ProgressBar progressBar;
    private Button updateButton;
    private Button cancelButton;
    private Timer timer;
    private int countdown = 30; // Cuenta regresiva en segundos

    public bool UserAccepted { get; private set; } = false;

    public CustomUpdateDialog()
    {
        // Configurar el formulario principal
        this.Text = "Actualización Disponible";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Width = 400;
        this.Height = 300;
        this.BackColor = Color.White;

        // Crear y agregar controles
        InitializeControls();

        // Configurar el temporizador
        ConfigureTimer();
    }

    private void InitializeControls()
    {
        // Título
        titleLabel = new Label()
        {
            Text = "¡Nueva versión disponible!",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.DarkBlue,
            AutoSize = true,
            Location = new Point(20, 20)
        };

        // Mensaje
        messageLabel = new Label()
        {
            Text = "Se ha detectado una nueva versión de la aplicación.\nEs recomendable actualizar para acceder a las mejoras.",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Black,
            AutoSize = true,
            MaximumSize = new Size(350, 0),
            Location = new Point(20, 60)
        };

        // Contador regresivo
        countdownLabel = new Label()
        {
            Text = $"Actualización automática en {countdown} segundos...",
            Font = new Font("Segoe UI", 10, FontStyle.Italic),
            ForeColor = Color.Red,
            AutoSize = true,
            Location = new Point(20, 110)
        };

        // Barra de progreso
        progressBar = new ProgressBar()
        {
            Width = 340,
            Height = 20,
            Location = new Point(20, 140),
            Visible = false
        };

        // Botón Actualizar
        updateButton = new Button()
        {
            Text = "Actualizar ahora",
            BackColor = Color.DarkBlue,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Width = 150,
            Height = 35,
            Location = new Point(20, 180)
        };
        updateButton.Click += (s, e) => AcceptUpdate();

        // Botón Cancelar
        cancelButton = new Button()
        {
            Text = "Recordar más tarde",
            BackColor = Color.LightGray,
            Font = new Font("Segoe UI", 10),
            Width = 150,
            Height = 35,
            Location = new Point(200, 180)
        };
        cancelButton.Click += (s, e) => CancelUpdate();

        // Agregar controles al formulario
        this.Controls.Add(titleLabel);
        this.Controls.Add(messageLabel);
        this.Controls.Add(countdownLabel);
        this.Controls.Add(progressBar);
        this.Controls.Add(updateButton);
        this.Controls.Add(cancelButton);
    }

    private void ConfigureTimer()
    {
        timer = new Timer()
        {
            Interval = 1000 // 1 segundo
        };

        timer.Tick += (s, e) =>
        {
            countdown--;
            countdownLabel.Text = $"Actualización automática en {countdown} segundos...";

            if (countdown <= 0)
            {
                timer.Stop();
                AcceptUpdate();
            }
        };

        timer.Start();
    }

    private void AcceptUpdate()
    {
        UserAccepted = true;
        timer.Stop();
        this.Close();
    }

    private void CancelUpdate()
    {
        UserAccepted = false;
        timer.Stop();
        this.Close();
    }
}
