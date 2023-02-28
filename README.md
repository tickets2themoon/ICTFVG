# ICTFVG
Simple indicator implementation of The Inner Circle Trader's (ICT - http://theinnercircletrader.com) Fair Value Gap (FVG) for the Ninjatrader V8 platform.

The indicator has the following parameters:

**Max Lookback Bars** : This is the maximum number of prior bars to use for FVG detection. Defaults to 500.

**ATR Period** : The indicator uses the ATR to determine if there has been an impulse move. This is the number of bars to use for ATR calculation. Defaults to 10.

**ATRs in Impulse Move** : The number of ATRs in a single bar to be considered an "Impulse" move. Defaults to 1.1.

**Minimum FVG Size (Points)** : Filter FVGs based on gap size 

**Display Consequent Encroachment Line** : FVG mid-point line

**Display / hide filled FVGs**: Display or hide FVGs that are considered filled.

**FVG fill condition options**
- Pierce through (FVG is marked filled if price _pierces_ through, and not necessarily _close_ through)
- Close through (FVG is marked filled only if price _closes_ through)

## Detect FVGs in multiple underlying bar types
FVGs can be detected in a configurable underlying bar type, irrespective of what the chart's data series is. 

For instance, you can display 15 minute FVGs on an hourly chart, or display 5 minute FVGs on a 1 minute chart, and so on.

Supported underlying Data Series Types:
- Tick
- Volume
- Second
- Minute
- Day
- Week
- Month
- Year

## Sample Screenshots

### 3-Minute FVGs on a 8 Renko Chart
![3MinFVG_On_8Renko](https://github.com/OrderFlowTools/screenshots/blob/main/ictfvg/3minFVG_on_8renko.PNG)

### 5-Minute FVGs on a 30 Minute Chart
![5MinFVG_On_30Min](https://github.com/OrderFlowTools/screenshots/blob/main/ictfvg/5minFVG_on_30minute.PNG)

### 6000-Tick FVGs on a 60 Minute Chart
![6000TickFVG_On_60Min](https://github.com/OrderFlowTools/screenshots/blob/main/ictfvg/6000tickFVG_on_60min.PNG)

