using System;
using Android.App;
using Android.Icu.Util;
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
        //bool[,] reelStopped = new bool[3, 3];
        
        //DECLARE AMOUNTS
        TextView dollarAmountText;
        TextView bankAmountText;

        bool autoSpinActive = false;
        bool autoSpinStopping = false;
        int columnsStopped = 0;

        decimal currentAmount = 0m;
        decimal bankAmount = 0m;
        decimal currentBet = 0m;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            slotSymbols = theme1Symbols; // Set initial theme
            dollarAmountText = FindViewById<TextView>(Resource.Id.dollaramount);

            dollarAmountText.Text = $"${currentAmount:0.00}";

            ImageView backgroundImage = FindViewById<ImageView>(Resource.Id.backgroundImage);

            Button theme1Button = FindViewById<Button>(Resource.Id.theme1Button);
            Button theme2Button = FindViewById<Button>(Resource.Id.theme2Button);

            

            theme1Button.Click += (s, e) =>
            {
                slotSymbols = theme1Symbols;
                currentBackground = Resource.Drawable.BGtheme1_1;
                backgroundImage.SetImageResource(currentBackground);
            };

            theme2Button.Click += (s, e) =>
            {
                slotSymbols = theme2Symbols;
                currentBackground = Resource.Drawable.BGTheme2_1;
                backgroundImage.SetImageResource(currentBackground);
            };

            Button stopButton = FindViewById<Button>(Resource.Id.stopButton);
            stopButton.Click += (s, e) =>
            {
                if (autoSpinActive && !autoSpinStopping)
                {
                    autoSpinStopping = true;
                }
                stopRequested = true;
            };




            //depositing and betting buttons and functionality

            bankAmountText = FindViewById<TextView>(Resource.Id.bankAmount);
            var addAmountButton = FindViewById<Button>(Resource.Id.addAmountButton); // Transfer bank → wallet
            var addBankButton = FindViewById<Button>(Resource.Id.addBankButton);     // Deposit to bank





            

            


            //Add to bank 
            addBankButton.Click += (s, e) =>
            {
                var input = new EditText(this)
                {
                    InputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal
                };

                new Android.App.AlertDialog.Builder(this)
                    .SetTitle("Deposit to Bank")
                    .SetMessage("Enter deposit amount:")
                    .SetView(input)
                    .SetPositiveButton("Deposit", (sender, args) =>
                    {
                        if (decimal.TryParse(input.Text, out decimal value) && value > 0)
                        {
                            bankAmount += value;
                            bankAmountText.Text = $"Bank: ${bankAmount:0.00}";
                        }
                        else
                        {
                            Toast.MakeText(this, "Invalid amount", ToastLength.Short).Show();
                        }
                    })
                    .SetNegativeButton("Cancel", (sender, args) => { })
                    .Show();
            };

            //Transfer from Bank to Machine (Wallet)
            addAmountButton.Click += (s, e) =>
            {
                var input = new EditText(this)
                {
                    InputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal
                };

                new Android.App.AlertDialog.Builder(this)
                    .SetTitle("Transfer to Wallet")
                    .SetMessage("Enter amount to move from Bank to Wallet:")
                    .SetView(input)
                    .SetPositiveButton("Transfer", (sender, args) =>
                    {
                        if (decimal.TryParse(input.Text, out decimal value) && value > 0)
                        {
                            if (bankAmount >= value)
                            {
                                bankAmount -= value;
                                currentAmount += value;

                                // Update UI
                                bankAmountText.Text = $"Bank: ${bankAmount:0.00}";
                                dollarAmountText.Text = $"${currentAmount:0.00}";
                            }
                            else
                            {
                                Toast.MakeText(this, "Not enough in Wallet", ToastLength.Short).Show();
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, "Invalid amount", ToastLength.Short).Show();
                        }
                    })
                    .SetNegativeButton("Cancel", (sender, args) => { })
                    .Show();
            };

            //make sure na may bet amount befoore starting
            void PromptForBetAndSpin(Action onBetAccepted)
            {
                var input = new EditText(this)
                {
                    InputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal
                };

                new Android.App.AlertDialog.Builder(this)
                    .SetTitle("Place Your Bet")
                    .SetMessage("Enter bet amount:")
                    .SetView(input)
                    .SetPositiveButton("Bet", (sender, args) =>
                    {
                        if (decimal.TryParse(input.Text, out decimal bet) && bet > 0)
                        {
                            if (currentAmount >= bet)
                            {
                                currentBet = bet;
                                currentAmount -= bet;
                                dollarAmountText.Text = $"${currentAmount:0.00}";
                                onBetAccepted?.Invoke();
                            }
                            else
                            {
                                Toast.MakeText(this, "Not enough in Wallet", ToastLength.Short).Show();
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, "Invalid bet", ToastLength.Short).Show();
                        }
                    })
                    .SetNegativeButton("Cancel", (sender, args) => { })
                    .Show();
            }




            // Initialize the 3x3 grid of slot ImageViews
            InitializeSlotGrid();

            // Spin button setup
            Button spinButton = FindViewById<Button>(Resource.Id.spinButton);
            spinButton.Click += (s, e) =>
            {
                if (isSpinning) return;

                // Reset all highlights BEFORE spinning
                for (int row = 0; row < 3; row++)
                    for (int col = 0; col < 3; col++)
                        slotGrid[row, col].Alpha = 1.0f;

                PromptForBetAndSpin(() =>
                {
                    StartAutoSpin();
                    for (int row = 0; row < 3; row++)
                        for (int col = 0; col < 3; col++)
                            slotGrid[row, col].ClearColorFilter();
                });
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


        //void StartQuickSpin(ImageView slot, int duration, Action onFinish, int row, int col)
        //{
        //    int interval = 120; // start speed
        //    long startTime = Java.Lang.JavaSystem.CurrentTimeMillis();
        //    int slowdownStep = 20; // how much to slow down each interval
        //    int maxInterval = 500; // max delay between spins when slowing down

        //    Runnable spinTask = null;
        //    spinTask = new Runnable(() =>
        //    {
        //        long elapsed = Java.Lang.JavaSystem.CurrentTimeMillis() - startTime;

        //        if (reelStopped[row, col]) return;

        //        // Set new symbol
        //        int symbol = slotSymbols[random.Next(slotSymbols.Length)];
        //        slot.SetImageResource(symbol);

        //        // Animate (fake spin)
        //        slot.TranslationY = -100;
        //        slot.Animate().TranslationY(0).SetDuration(80);

        //        if (!stopRequested && elapsed < duration)
        //        {
        //            handler.PostDelayed(spinTask, interval);
        //        }
        //        else if (!reelStopped[row, col])
        //        {
        //            // Slow down
        //            interval += slowdownStep;

        //            if (interval >= maxInterval)
        //            {
        //                reelStopped[row, col] = true;

        //                int finalIndex = random.Next(slotSymbols.Length);
        //                slot.SetImageResource(slotSymbols[finalIndex]);
        //                currentSymbols[row, col] = finalIndex; // Store index
        //                onFinish?.Invoke();
        //                return;
        //            }

        //            handler.PostDelayed(spinTask, interval);
        //        }
        //    });

        //    handler.Post(spinTask);
        //}


        void StartAutoSpin()
        {
            if (isSpinning) return;

            isSpinning = true;
            autoSpinActive = true;
            autoSpinStopping = false;
            columnsStopped = 0;

            int[] spinIntervals = { 80, 80, 80 }; // initial fast interval for each column
            int[] spins = { 0, 0, 0 };

            Action<int> slowDownColumn = null;
            slowDownColumn = (col) =>
            {
                if (spinIntervals[col] < 400)
                {
                    spinIntervals[col] += 20;
                    handler.PostDelayed(() => slowDownColumn(col), 100);
                }
            };

            Action<int> spinColumn = null;
            spinColumn = (col) =>
            {
                if (!autoSpinActive || columnsStopped == 3) return;

                ScrollColumn(col);
                spins[col]++;

                if (autoSpinStopping)
                {
                    // Gradually slow down
                    if (spinIntervals[col] < 400)
                    {
                        spinIntervals[col] += 20;
                    }
                    else
                    {
                        columnsStopped++;
                        if (columnsStopped == 3)
                        {
                            isSpinning = false;
                            autoSpinActive = false;
                            autoSpinStopping = false;
                            CheckForWins();
                        }
                        return;
                    }
                }

                handler.PostDelayed(() => spinColumn(col), spinIntervals[col]);
            };

            // Start all columns spinning
            for (int col = 0; col < 3; col++)
            {
                spinColumn(col);
            }

            // After 10 seconds, trigger gradual stop if not already stopping
            handler.PostDelayed(() =>
            {
                if (autoSpinActive && !autoSpinStopping)
                {
                    autoSpinStopping = true;
                }
            }, 10000);
        }

        void ScrollColumn(int col)
        {
            // Save the bottom symbol (not used, but for clarity)
            int prevSymbol = currentSymbols[2, col];

            // Animate and shift symbols down in data and UI
            for (int row = 2; row > 0; row--)
            {
                currentSymbols[row, col] = currentSymbols[row - 1, col];
                slotGrid[row, col].SetImageResource(slotSymbols[currentSymbols[row, col]]);
                slotGrid[row, col].Alpha = 0.7f;
                slotGrid[row, col].TranslationY = -80;
                slotGrid[row, col].Animate().TranslationY(0).Alpha(1f).SetDuration(60);
            }

            // Insert a new random symbol at the top
            int newSymbolIndex = random.Next(slotSymbols.Length);
            currentSymbols[0, col] = newSymbolIndex;
            slotGrid[0, col].SetImageResource(slotSymbols[newSymbolIndex]);
            slotGrid[0, col].Alpha = 0.7f;
            slotGrid[0, col].TranslationY = -80;
            slotGrid[0, col].Animate().TranslationY(0).Alpha(1f).SetDuration(60);
        }


        //void StartColumnScroll(int col, int spins, int interval, Action onFinish)
        //{
        //    int count = 0;
        //    Runnable scrollTask = null;
        //    scrollTask = new Runnable(() =>
        //    {
        //        ScrollColumn(col);
        //        count++;
        //        if (count < spins)
        //        {
        //            handler.PostDelayed(scrollTask, interval);
        //        }
        //        else
        //        {
        //            onFinish?.Invoke();
        //        }
        //    });
        //    handler.Post(scrollTask);
        //}


        //int finishedSlots = 0;
        //void CheckSpinComplete()
        //{
        //    finishedSlots++;
        //    if (finishedSlots == 9)
        //    {
        //        isSpinning = false;
        //        finishedSlots = 0;


        //        CheckForWins();
        //    }
        //}
        private void HighlightWinningSlots(int r1, int c1, int r2, int c2, int r3, int c3)
        {
            var highlightColor = Android.Graphics.Color.Argb(128, 255, 255, 0); // semi-transparent yellow
            slotGrid[r1, c1].SetColorFilter(highlightColor);
            slotGrid[r2, c2].SetColorFilter(highlightColor);
            slotGrid[r3, c3].SetColorFilter(highlightColor);
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
                decimal winnings = currentBet * 2;
                currentAmount += winnings;
                dollarAmountText.Text = $"${currentAmount:0.00}";
                Toast.MakeText(this, $"You Win! +${winnings:0.00}", ToastLength.Long).Show();
            }
            else
            {
                Toast.MakeText(this, $"No Win! -${currentBet:0.00}", ToastLength.Short).Show();
            }
            currentBet = 0m;
            
        }

    }
}