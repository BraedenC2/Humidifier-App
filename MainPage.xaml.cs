using Microsoft.Maui.Dispatching;
using System.Diagnostics;
namespace Humidifier {

    public partial class MainPage : ContentPage {
        private const int TotalTimeInSeconds = 43200; // 12 hours
        private const double TankHeight = 400;
        private IDispatcherTimer uiUpdateTimer;
        private bool isRunning;

        // Persistent preferences key
        private const string TimerStartKey = "TimerStart";
        private const string TimerActiveKey = "TimerActive";

        public MainPage() {
            InitializeComponent();
            SetupTimer();
            LoadTimerState();
        }

        private void SetupTimer() {
            // Create UI update timer that checks every second
            uiUpdateTimer = Application.Current.Dispatcher.CreateTimer();
            uiUpdateTimer.Interval = TimeSpan.FromSeconds(1);
            uiUpdateTimer.Tick += Timer_Tick;
        }

        private void LoadTimerState() {
            var preferences = Preferences.Default;
            long storedStartTime = preferences.Get(TimerStartKey, 0L);
            bool isTimerActive = preferences.Get(TimerActiveKey, false);

            if (isTimerActive && storedStartTime != 0) {
                var startTime = DateTime.FromBinary(storedStartTime);
                var elapsed = DateTime.Now - startTime;

                // If timer should have finished while app was closed
                if (elapsed.TotalSeconds >= TotalTimeInSeconds) {
                    StopTimer();
                    UpdateDisplay(0);
                } else {
                    isRunning = true;
                    uiUpdateTimer.Start();
                    UpdateDisplay(TotalTimeInSeconds - elapsed.TotalSeconds);
                }
            } else {
                UpdateDisplay(TotalTimeInSeconds);
            }
        }

        private void StartTimer() {
            if (!isRunning) {
                var preferences = Preferences.Default;
                preferences.Set(TimerStartKey, DateTime.Now.ToBinary());
                preferences.Set(TimerActiveKey, true);

                isRunning = true;
                uiUpdateTimer.Start();
                UpdateDisplay(TotalTimeInSeconds);
            }
        }

        private void StopTimer() {
            var preferences = Preferences.Default;
            preferences.Set(TimerActiveKey, false);
            preferences.Set(TimerStartKey, 0L);

            isRunning = false;
            uiUpdateTimer.Stop();
        }

        private void Timer_Tick(object sender, EventArgs e) {
            var preferences = Preferences.Default;
            long storedStartTime = preferences.Get(TimerStartKey, 0L);

            if (storedStartTime != 0) {
                var startTime = DateTime.FromBinary(storedStartTime);
                var elapsed = DateTime.Now - startTime;

                if (elapsed.TotalSeconds >= TotalTimeInSeconds) {
                    StopTimer();
                    UpdateDisplay(0);

                    // Notify user that time is up
                    MainThread.BeginInvokeOnMainThread(async () => {
                        try {
                            await DisplayAlert("Time's Up!", "The timer has completed.", "OK");
                        } catch (Exception ex) {
                            Debug.WriteLine($"Error showing alert: {ex.Message}");
                        }
                    });
                } else {
                    UpdateDisplay(TotalTimeInSeconds - elapsed.TotalSeconds);
                }
            }
        }

        private void UpdateDisplay(double remainingSeconds) {
            // Calculate percentage
            double percentage = (remainingSeconds / TotalTimeInSeconds) * 100;
            percentage = Math.Max(0, Math.Min(100, percentage)); // Clamp between 0 and 100

            // Update water level visualization
            double height = (percentage / 100) * TankHeight;
            WaterLevel.HeightRequest = height;

            // Update percentage text
            PercentageLabel.Text = $"{percentage:F0}%";

            // Update time label
            TimeSpan time = TimeSpan.FromSeconds(remainingSeconds);
            // Modified to show hours for better readability with 12-hour timer
            TimeLabel.Text = $"Time Remaining: {time.Hours}:{time.Minutes:D2}:{time.Seconds:D2}";
        }

        private void OnCheckTimeClicked(object sender, EventArgs e) {
            var preferences = Preferences.Default;
            long storedStartTime = preferences.Get(TimerStartKey, 0L);

            if (storedStartTime != 0) {
                var startTime = DateTime.FromBinary(storedStartTime);
                var elapsed = DateTime.Now - startTime;
                var remaining = Math.Max(0, TotalTimeInSeconds - elapsed.TotalSeconds);

                TimeSpan time = TimeSpan.FromSeconds(remaining);
                DisplayAlert("Time Remaining",
                    $"Time left: {time.Hours}:{time.Minutes:D2}:{time.Seconds:D2}\n" +
                    $"Tank level: {(remaining / TotalTimeInSeconds * 100):F0}%",
                    "OK");
            } else {
                DisplayAlert("Timer Not Running",
                    "The timer is not currently active. Press 'Refill Tank' to start.",
                    "OK");
            }
        }

        private void StopClicked(object sender, EventArgs e) {
            StopTimer();
        }

        private void OnRefillClicked(object sender, EventArgs e) {
            StopTimer();
            StartTimer();
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            // Stop the UI update timer when the page disappears
            uiUpdateTimer.Stop();
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            // Restart the UI update timer when the page appears
            if (isRunning) {
                uiUpdateTimer.Start();
            }
            LoadTimerState();
        }

        private void Button_Clicked(object sender, EventArgs e) {

        }
    }
}