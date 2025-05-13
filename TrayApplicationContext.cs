using System.Windows.Forms;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace ScreenCopilot;

public class TrayApplicationContext : ApplicationContext
{
    private NotifyIcon trayIcon;
    private System.Threading.Timer captureTimer;
    private int TIMER_INTERVAL = 60000; // 10 seconds
    private TextBox folderPathTextBox;
    private Button toggleButton;
    private bool isCapturing = false;
    private TextBox apiKeyTextBox;
    private const string ApiKeyFilePath = "api_key.txt";
    private TrackBar timerIntervalSlider;
    private Label timerIntervalLabel;

    public TrayApplicationContext()
    {
        InitializeTrayIcon();
        InitializeTimer();
    }

    private void InitializeTrayIcon()
    {
        trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            ContextMenuStrip = new ContextMenuStrip(),
            Visible = true,
            Text = "ScreenCopilot"
        };

        // Add menu items
        var menuStrip = trayIcon.ContextMenuStrip;
        menuStrip.Items.Add("Show Window", null, (s, e) => ShowWindow());
        menuStrip.Items.Add("Exit", null, (s, e) => ExitApplication());
    }

    private void InitializeTimer()
    {
        isCapturing = false;

        captureTimer = new System.Threading.Timer(
            async _ =>
            {
                if (isCapturing)
                {
                    await CaptureScreenshot();
                }
            },
            null,
            Timeout.InfiniteTimeSpan,
            Timeout.InfiniteTimeSpan
        );
    }

    private async Task CaptureFrame()
    {
        // Call your screen capture code here
        Console.WriteLine("Captured frame at " + DateTime.Now);
        // await SaveCaptureAsync();
    }

    private async Task CaptureScreenshot()
    {
        // Get the bounds of the primary screen
        Rectangle bounds = Screen.PrimaryScreen.Bounds;

        // Create a bitmap to store the screenshot
        using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Copy the screen content to the bitmap
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            }

            // Use the folder path from the text field
            string defaultFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "ScreenCopilot");
            // if (string.IsNullOrWhiteSpace(folderPathTextBox?.Text))
            // {
            //     folderPathTextBox.Text = defaultFolderPath;
            // }

            // Ensure the default folder exists
            Directory.CreateDirectory(defaultFolderPath);

            string folderPath = string.IsNullOrWhiteSpace(folderPathTextBox?.Text) ? defaultFolderPath : folderPathTextBox.Text;

            // Ensure the folder exists
            Directory.CreateDirectory(folderPath);

            // Save the screenshot to the specified folder
            string filePath = Path.Combine(folderPath, $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            bitmap.Save(filePath, ImageFormat.Png);
            Console.WriteLine($"Screenshot saved to {filePath}");

            // Analyze the screenshot
            await AnalyzeScreenshot(bitmap);
        }

        //sShowNotification("Screenshot captured successfully!");
    }

    private async Task AnalyzeScreenshot(Bitmap bitmap)
    {
        using (var memoryStream = new MemoryStream())
        {
            // Convert the bitmap to a byte array
            bitmap.Save(memoryStream, ImageFormat.Png);
            byte[] imageBytes = memoryStream.ToArray();

            // Convert the byte array to a Base64 string
            string base64Image = Convert.ToBase64String(imageBytes);

            // Prepare the request payload
            var payload = new
            {
                model = "gpt-4o",
            messages = new object[]
            {
                new {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = "This is a screenshot from my desktop. If it is not showing a game, just say 'no game'. Otherwise, analyze what is happening in the game and suggest the best next action the user can take. return a simple one liner with maximum 200 char" },
                        new {
                            type = "image_url",
                            image_url = new {
                                url = $"data:image/png;base64,{base64Image}"
                            }
                        }
                    }
                }
            },
            max_tokens = 300
            };

            string jsonPayload = JsonConvert.SerializeObject(payload);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Replace with your ChatGPT API endpoint
                string apiUrl = "https://api.openai.com/v1/chat/completions";

                // Read the API key from the text field
                string apiKey = string.IsNullOrWhiteSpace(apiKeyTextBox?.Text) ? "your-default-api-key" : apiKeyTextBox.Text;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    string contentz = JsonConvert.DeserializeObject<dynamic>(result)?.choices[0]?.message?.content ?? "No content available.";
                    ShowNotification($"Analysis Result: {contentz}");
                }
                else
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    ShowNotification($"Failed to analyze the screenshot. Details: {errorDetails}");
                }
            }
        }
    }

    private void ShowWindow()
    {
        Form window = new Form
        {
            Text = "ScreenCopilot Window",
            Size = new Size(800, 600)
        };

        InitializeFolderPathField(window);
        InitializeToggleButton(window);
        InitializeApiKeyField(window);
        InitializeTimerIntervalSlider(window);
        window.Show();
    }

    private void InitializeFolderPathField(Form window)
    {
        Label folderPathLabel = new Label
        {
            Text = "Folder Path:",
            Location = new Point(10, 10),
            AutoSize = true
        };

        folderPathTextBox = new TextBox
        {
            Location = new Point(100, 10),
            Width = 600
        };

        Button browseButton = new Button
        {
            Text = "Browse",
            Location = new Point(710, 10)
        };

        browseButton.Click += (sender, e) =>
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    folderPathTextBox.Text = dialog.SelectedPath;
                }
            }
        };

        window.Controls.Add(folderPathLabel);
        window.Controls.Add(folderPathTextBox);
        window.Controls.Add(browseButton);
    }

    private void InitializeToggleButton(Form window)
    {
        toggleButton = new Button
        {
            Text = "Start Capturing",
            Location = new Point(10, 50),
            Width = 150
        };

        toggleButton.Click += (sender, e) =>
        {
            isCapturing = !isCapturing;
            toggleButton.Text = isCapturing ? "Pause Capturing" : "Start Capturing";

            if (isCapturing)
            {
                captureTimer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(TIMER_INTERVAL));
            }
            else
            {
                captureTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        };

        window.Controls.Add(toggleButton);
    }

    private void InitializeApiKeyField(Form window)
    {
        Label apiKeyLabel = new Label
        {
            Text = "API Key:",
            Location = new Point(10, 90),
            AutoSize = true
        };

        apiKeyTextBox = new TextBox
        {
            Location = new Point(100, 90),
            Width = 600
        };

        apiKeyTextBox.Text = LoadApiKey();
        apiKeyTextBox.TextChanged += (sender, e) => SaveApiKey(apiKeyTextBox.Text);

        window.Controls.Add(apiKeyLabel);
        window.Controls.Add(apiKeyTextBox);
    }

    private void InitializeTimerIntervalSlider(Form window)
    {
        timerIntervalLabel = new Label
        {
            Text = "Timer Interval (seconds): 60",
            Location = new Point(10, 130),
            AutoSize = true
        };

        timerIntervalSlider = new TrackBar
        {
            Location = new Point(200, 120),
            Width = 400,
            Minimum = 10,
            Maximum = 300,
            Value = TIMER_INTERVAL / 1000,
            TickFrequency = 10
        };

        timerIntervalSlider.ValueChanged += (sender, e) =>
        {
            int interval = timerIntervalSlider.Value * 1000;
            TIMER_INTERVAL = interval;
            timerIntervalLabel.Text = $"Timer Interval (seconds): {timerIntervalSlider.Value}";

            if (isCapturing)
            {
                captureTimer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(TIMER_INTERVAL));
            }
        };

        window.Controls.Add(timerIntervalLabel);
        window.Controls.Add(timerIntervalSlider);
    }

    private void ExitApplication()
    {
        trayIcon.Visible = false;
        Application.Exit();
    }

    private void ShowNotification(string message)
    {
        NotifyIcon notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Information,
            Visible = true
        };

        notifyIcon.ShowBalloonTip(3000, "ScreenCopilot", message, ToolTipIcon.Info);

        // Bring the notification to the top
        IntPtr hWnd = GetForegroundWindow();
        SetForegroundWindow(hWnd);

        // Dispose the notification icon after the balloon tip is shown
        Task.Delay(3000).ContinueWith(_ => notifyIcon.Dispose());
    }

    private void SaveApiKey(string apiKey)
    {
        File.WriteAllText(ApiKeyFilePath, apiKey);
    }

    private string LoadApiKey()
    {
        return File.Exists(ApiKeyFilePath) ? File.ReadAllText(ApiKeyFilePath) : string.Empty;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            captureTimer?.Dispose();
            trayIcon?.Dispose();
        }
        base.Dispose(disposing);
    }
}