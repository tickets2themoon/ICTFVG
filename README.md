# ICTFVG
Simple indicator implementation of The Inner Circle Trader's (ICT - http://theinnercircletrader.com) Fair Value Gap (FVG) for the Ninjatrader V8 platform.

The indicator has the following parameters:

**Max Lookback Bars** : This is the maximum number of prior bars to use for FVG detection. Defaults to 500.

**ATR Period** : The indicator uses the ATR to determine if there has been an impulse move. This is the number of bars to use for ATR calculation. Defaults to 10.

**ATRs in Impulse Move** : The number of ATRs in a single bar to be considered an "Impulse" move. Defaults to 1.1.

**Display / hide filled FVGs**: Display or hide FVGs that are considered filled.

**FVG fill condition options**
- Pierce through (FVG is marked filled if price _pierces_ through, and not necessarily _close_ through)
- Close through (FVG is marked filled only if price _closes_ through)
