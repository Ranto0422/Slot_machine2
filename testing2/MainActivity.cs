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
        // 3x3 grid of ImageViews
        ImageView[,] slotGrid = new ImageView[3, 3];
        int[,] currentSymbols = new int[3, 3];

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
        bool[,] reelStopped = new bool[3, 3];

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

            // Background change size via scale
            if (backgroundImage != null)
            {
                backgroundImage.ScaleX = 3f;
                backgroundImage.ScaleY = 4f;
            }

            // Initialize the 3x3 grid of slot ImageViews
            InitializeSlotGrid();

            // Spin button setup
            Button spinButton = FindViewById<Button>(Resource.Id.spinButton);
            spinButton.Click += (s, e) =>
            {
                if (isSpinning) return;

                isSpinning = true;
                stopRequested = false;
                finishedSlots = 0;
                reelStopped = new bool[3, 3]; // reset state
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        slotGrid[row, col].Alpha = 1.0f;
                    }
                }

                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        int duration = random.Next(2000, 10000);
                        int currentRow = row;
                        int currentCol = col;
                        StartQuickSpin(slotGrid[row, col], duration, () => CheckSpinComplete(), currentRow, currentCol);
                    }
                }
            };

            
        }

        private void InitializeSlotGrid()
        {

            slotGrid[0, 0] = FindViewById<ImageView>(Resource.Id.slot1_1);
            slotGrid[0, 1] = FindViewById<ImageView>(Resource.Id.slot1_2);
            slotGrid[0, 2] = FindViewById<ImageView>(Resource.Id.slot1_3);
            slotGrid[1, 0] = FindViewById<ImageView>(Resource.Id.slot2_1);
            slotGrid[1, 1] = FindViewById<ImageView>(Resource.Id.slot2_2);
            slotGrid[1, 2] = FindViewById<ImageView>(Resource.Id.slot2_3);
            slotGrid[2, 0] = FindViewById<ImageView>(Resource.Id.slot3_1);
            slotGrid[2, 1] = FindViewById<ImageView>(Resource.Id.slot3_2);
            slotGrid[2, 2] = FindViewById<ImageView>(Resource.Id.slot3_3);


            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (slotGrid[row, col] != null)
                    {
                        int initialSymbol = slotSymbols[random.Next(slotSymbols.Length)];
                        slotGrid[row, col].SetImageResource(initialSymbol);
                    }
                }
            }
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


        void StartQuickSpin(ImageView slot, int duration, Action onFinish, int row, int col)
        {
            int interval = 120; // start speed
            long startTime = Java.Lang.JavaSystem.CurrentTimeMillis();
            int slowdownStep = 20; // how much to slow down each interval
            int maxInterval = 500; // max delay between spins when slowing down

            Runnable spinTask = null;
            spinTask = new Runnable(() =>
            {
                long elapsed = Java.Lang.JavaSystem.CurrentTimeMillis() - startTime;

                if (reelStopped[row, col]) return;

                // Set new symbol
                int symbol = slotSymbols[random.Next(slotSymbols.Length)];
                slot.SetImageResource(symbol);

                // Animate (fake spin)
                slot.TranslationY = -100;
                slot.Animate().TranslationY(0).SetDuration(80);

                if (!stopRequested && elapsed < duration)
                {
                    handler.PostDelayed(spinTask, interval);
                }
                else if (!reelStopped[row, col])
                {
                    // Slow down
                    interval += slowdownStep;

                    if (interval >= maxInterval)
                    {
                        reelStopped[row, col] = true;

                        int finalIndex = random.Next(slotSymbols.Length);
                        slot.SetImageResource(slotSymbols[finalIndex]);
                        currentSymbols[row, col] = finalIndex; // Store index
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
            if (finishedSlots == 9)
            {
                isSpinning = false;
                finishedSlots = 0;


                CheckForWins();
            }
        }
        private void HighlightWinningSlots(int r1, int c1, int r2, int c2, int r3, int c3)
        {
            slotGrid[r1, c1].Alpha = 0.5f;
            slotGrid[r2, c2].Alpha = 0.5f;
            slotGrid[r3, c3].Alpha = 0.5f;


        }


        private void CheckForWins()
        {
            bool win = false;

            // Check rows
            for (int row = 0; row < 3; row++)
            {
                if (currentSymbols[row, 0] == currentSymbols[row, 1] &&
                    currentSymbols[row, 1] == currentSymbols[row, 2])
                {
                    win = true;
                    HighlightWinningSlots(row, 0, row, 1, row, 2);
                }
            }

            // Check columns
            for (int col = 0; col < 3; col++)
            {
                if (currentSymbols[0, col] == currentSymbols[1, col] &&
                    currentSymbols[1, col] == currentSymbols[2, col])
                {
                    win = true;
                    HighlightWinningSlots(0, col, 1, col, 2, col);
                }
            }

            // Check diagonals
            if (currentSymbols[0, 0] == currentSymbols[1, 1] &&
                currentSymbols[1, 1] == currentSymbols[2, 2])
            {
                win = true;
                HighlightWinningSlots(0, 0, 1, 1, 2, 2);
            }

            if (currentSymbols[0, 2] == currentSymbols[1, 1] &&
                currentSymbols[1, 1] == currentSymbols[2, 0])
            {
                win = true;
                HighlightWinningSlots(0, 2, 1, 1, 2, 0);
            }

            if (win)
            {
                Toast.MakeText(this, "You Win!", ToastLength.Long).Show();
            }
            else
            {
                Toast.MakeText(this, "No Win", ToastLength.Short).Show();
            }
        }

    }
}