﻿// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Android.App;
using Android.Views;
using Android.OS;
using Android.Content.PM;
using Microsoft.Xna.Framework;

namespace SpriterDotNet.Example.MonoGame.Android
{
    [Activity(Label = "SpriterDotNet.Example.MonoGame.Android",
           MainLauncher = true,
           Icon = "@drawable/icon",
           ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden
           )]
    public class MainActivity : AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var g = new ExampleGame();
            SetContentView((View)g.Services.GetService(typeof(View)));
            g.Run();
        }
    }
}

