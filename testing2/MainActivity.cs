using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using Java.Lang;


namespace testing2
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]

    public class MainActivity : AppCompatActivity
    {
        ImageView slot1, slot2, slot3;
        Random random = new Random();

        // Add your theme symbol arrays and background here
        int[] theme1Symbols = {
        
        Resource.Drawable.slotsymbol1,
        Resource.Drawable.slotsymbol2,
        Resource.Drawable.slotsymbol3,
        Resource.Drawable.slotsymbol4
    };

        int[] theme2Symbols = {
        
        Resource.Drawable.slotsymbol1,
        Resource.Drawable.slotsymbol2,
        Resource.Drawable.slotsymbol3,
        Resource.Drawable.slotsymbol4
    };

        int currentBackground = Resource.Drawable.BGtheme1; // default
        int[] slotSymbols;

        Handler handler = new Handler(Looper.MainLooper);

        bool isSpinning = false;
        bool stopRequested = false;
        bool[] reelStopped = new bool[3];

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            slotSymbols = theme1Symbols; // Set initial theme

            ImageView backgroundImage = FindViewById<ImageView>(Resource.Id.backgroundImage);

            Button theme1Button = FindViewById<Button>(Resource.Id.theme1Button);
            Button theme2Button = FindViewById<Button>(Resource.Id.theme2Button);

            theme1Button.Click += (s, e) =>
            {
                slotSymbols = theme1Symbols;
                currentBackground = Resource.Drawable.BGtheme1;
                backgroundImage.SetImageResource(currentBackground);
            };

            theme2Button.Click += (s, e) =>
            {
                slotSymbols = theme2Symbols;
                currentBackground = Resource.Drawable.BGtheme2;
                backgroundImage.SetImageResource(currentBackground);
            };
            Button stopButton = FindViewById<Button>(Resource.Id.stopButton);
            stopButton.Click += (s, e) =>
            {
                stopRequested = true;
            };

            // background change size via scalee
            var backgroundImages = FindViewById<ImageView>(Resource.Id.backgroundImage);
            if (backgroundImage != null)
            {
                backgroundImage.ScaleX = 3f;
                backgroundImage.ScaleY = 4f;
            }

            slot1 = FindViewById<ImageView>(Resource.Id.slot1);
            slot2 = FindViewById<ImageView>(Resource.Id.slot2);
            slot3 = FindViewById<ImageView>(Resource.Id.slot3);
            //THIS IS THE SLOT IMAGES and how long they spin
            Button spinButton = FindViewById<Button>(Resource.Id.spinButton);
            spinButton.Click += (s, e) =>
            {
                if (isSpinning) return;

                isSpinning = true;
                stopRequested = false;
                finishedSlots = 0;
                reelStopped = new bool[3]; // reset state

                int duration1 = random.Next(2000,10000);
                int duration2 = random.Next(2000, 10000);
                int duration3 = random.Next(2000, 10000);// how long before allowing slowdown
                StartQuickSpin(slot1, duration1, () => CheckSpinComplete(), 0);
                StartQuickSpin(slot2, duration2, () => CheckSpinComplete(), 1);
                StartQuickSpin(slot3, duration3, () => CheckSpinComplete(), 2);
            };


            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
        }


        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (View.IOnClickListener)null).Show();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        //-----------------------------------------------------Trying to animate the slots with a reel effect--------------------------------------------------------//
        void StartQuickSpin(ImageView slot, int duration, Action onFinish, int index)
        {
            int interval = 120; // start speed
            long startTime = Java.Lang.JavaSystem.CurrentTimeMillis();
            int slowdownStep = 20; // how much to slow down each interval
            int maxInterval = 500; // max delay between spins when slowing down

            Runnable spinTask = null;
            spinTask = new Runnable(() =>
            {
                long elapsed = Java.Lang.JavaSystem.CurrentTimeMillis() - startTime;

                if (reelStopped[index]) return;

                // Set new symbol
                int symbol = slotSymbols[random.Next(slotSymbols.Length)];
                slot.SetImageResource(symbol);

                // Animate (fake spin)
                slot.TranslationY = -190;
                slot.Animate().TranslationY(0).SetDuration(80);

                if (!stopRequested && elapsed < duration)
                {
                    handler.PostDelayed(spinTask, interval);
                }
                else if (!reelStopped[index])
                {
                    // Slow down
                    interval += slowdownStep;

                    if (interval >= maxInterval)
                    {
                        reelStopped[index] = true;

                        int final = slotSymbols[random.Next(slotSymbols.Length)];
                        slot.SetImageResource(final);
                        onFinish?.Invoke();
                        return;
                    }

                    handler.PostDelayed(spinTask, interval);
                }
            });

            handler.Post(spinTask);
        }
        int finishedSlots = 0;
        void CheckSpinComplete()
        {
            finishedSlots++;
            if (finishedSlots == 3)
            {
                isSpinning = false;
                finishedSlots = 0;
            }
        }
    }
}
