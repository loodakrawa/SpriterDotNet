using Android.App;
using Android.Views;
using Android.OS;
using Android.Content.PM;
using Microsoft.Xna.Framework;

namespace SpriterDotNet.MonoGame.Example.Android
{
    [Activity(Label = "SpriterDotNet.MonoGame.Example.Android",
           MainLauncher = true,
           Icon = "@drawable/icon",
           ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden
           )]
    public class MainActivity : AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var g = new SpriterGame();
            SetContentView((View)g.Services.GetService(typeof(View)));
            g.Run();
        }
    }
}

