# Slot Machine

A 3x3 slot machine game for Android, built in C# with Xamarin.Android. Deposit play money, place a bet, spin, and win on any matching row, column or diagonal. Made as a team for a mobile development assessment.

![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![Xamarin](https://img.shields.io/badge/Xamarin.Android-3498DB?logo=xamarin&logoColor=white)
![Android](https://img.shields.io/badge/Android-3DDC84?logo=android&logoColor=white)

<p>
  <img src="SlotMachine/Resources/drawable/slotsymbol1.png" width="48">
  <img src="SlotMachine/Resources/drawable/slotsymbol2.png" width="48">
  <img src="SlotMachine/Resources/drawable/slotsymbol3.png" width="48">
  <img src="SlotMachine/Resources/drawable/slotsymbol4.png" width="48">
  &nbsp;&nbsp;
  <img src="SlotMachine/Resources/drawable/symbol1.png" width="48">
  <img src="SlotMachine/Resources/drawable/symbol2.png" width="48">
  <img src="SlotMachine/Resources/drawable/symbol3.png" width="48">
  <img src="SlotMachine/Resources/drawable/symbol4.png" width="48">
</p>

*The two symbol themes.*

| | |
|---|---|
| **Stack** | C#, Xamarin.Android (Android 13 target), AndroidX AppCompat |
| **Team** | Built with a classmate; work was done on feature branches merged through pull requests |
| **Highlights** | Reel animation that slows each column down before it stops, 8 win lines, bank and wallet betting flow, switchable themes |

## Features

- **3x3 reels** that scroll column by column and ease to a stop; a Stop button ends the spin early
- **Win check on 8 lines:** 3 rows, 3 columns and 2 diagonals. Winning cells are highlighted and pay 2x the bet
- **Betting:** deposit into a bank, move money from the bank to the machine wallet, then enter a bet before each spin, with checks for invalid amounts and insufficient funds
- **Two themes** that swap both the symbols and the background

## Project structure

```
Slot_machine2/
├── SlotMachine.sln
└── SlotMachine/
    ├── MainActivity.cs              all game logic: reels, spin timing, win check, betting
    ├── Resources/layout/            screen layout (grid, buttons, balances)
    ├── Resources/drawable/          symbols and theme backgrounds
    └── Properties/AndroidManifest.xml
```

## Running it

Requires Visual Studio 2022 with the **Mobile development with .NET** (Xamarin) workload and an Android emulator.

1. Open `SlotMachine.sln`.
2. Select an Android emulator and press **F5**.

Note: Microsoft ended Xamarin support in May 2024. It needs Visual Studio 2022 with the Xamarin workload installed; moving it to .NET for Android would be the next step.
