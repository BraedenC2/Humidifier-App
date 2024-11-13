using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace Humidifier;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity {
    protected override void OnCreate(Bundle savedInstanceState) {
        base.OnCreate(savedInstanceState);

        // Set status bar color to match app theme
        Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#1A4D2E"));

        // Optional: Make status bar icons light (if your color is dark)
        Window.DecorView.SystemUiVisibility &= ~(StatusBarVisibility)SystemUiFlags.LightStatusBar;
    }
}