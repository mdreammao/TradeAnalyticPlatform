'''
测试pyfolio包
create_full_tear_sheet

 Parameters
    ----------
    returns : pd.Series
        Daily returns of the strategy, noncumulative.
         - Time series with decimal returns.
         - Example:
            2015-07-16    -0.012143
            2015-07-17    0.045350
            2015-07-20    0.030957
            2015-07-21    0.004902
    positions : pd.DataFrame, optional
        Daily net position values.
         - Time series of dollar amount invested in each position and cash.
         - Days where stocks are not held can be represented by 0 or NaN.
         - Non-working capital is labelled 'cash'
         - Example:
            index         'AAPL'         'MSFT'          cash
            2004-01-09    13939.3800     -14012.9930     711.5585
            2004-01-12    14492.6300     -14624.8700     27.1821
            2004-01-13    -13853.2800    13653.6400      -43.6375
    transactions : pd.DataFrame, optional
        Executed trade volumes and fill prices.
        - One row per trade.
        - Trades on different names that occur at the
          same time will have identical indicies.
        - Example:
            index                  amount   price    symbol
            2004-01-09 12:18:01    483      324.12   'AAPL'
            2004-01-09 12:18:01    122      83.10    'MSFT'
            2004-01-13 14:12:23    -75      340.43   'AAPL'
    market_data : pd.Panel, optional
        Panel with items axis of 'price' and 'volume' DataFrames.
        The major and minor axes should match those of the
        the passed positions DataFrame (same dates and symbols).
    gross_lev : pd.Series, optional
        The leverage of a strategy.
         - Time series of the sum of long and short exposure per share
            divided by net asset value.
         - Example:
            2009-12-04    0.999932
            2009-12-07    0.999783
            2009-12-08    0.999880
            2009-12-09    1.000283
    slippage : int/float, optional
        Basis points of slippage to apply to returns before generating
        tearsheet stats and plots.
        If a value is provided, slippage parameter sweep
        plots will be generated from the unadjusted returns.
        Transactions and positions must also be passed.
        - See txn.adjust_returns_for_slippage for more details.
    live_start_date : datetime, optional
        The point in time when the strategy began live trading,
        after its backtest period. This datetime should be normalized.
    hide_positions : bool, optional
        If True, will not output any symbol names.
    bayesian: boolean, optional
        If True, causes the generation of a Bayesian tear sheet.
    round_trips: boolean, optional
        If True, causes the generation of a round trip tear sheet.
    cone_std : float, or tuple, optional
        If float, The standard deviation to use for the cone plots.
        If tuple, Tuple of standard deviation values to use for the cone plots
         - The cone is a normal distribution with this standard deviation
             centered around a linear regression.
    bootstrap : boolean (optional)
        Whether to perform bootstrap analysis for the performance
        metrics. Takes a few minutes longer.
    set_context : boolean, optional
        If True, set default plotting style context.
         - See plotting.context().

'''

import pyfolio as pf
import pandas as pd
import numpy as np
import pytz
from pyfolio import plotting
import matplotlib.pyplot as plt
from empyrical_stats import aggregate_returns
from matplotlib.ticker import FuncFormatter
from pyfolio import utils


def plot_daily_returns_dist(returns, ax=None, **kwargs):
    """
    Plots a distribution of monthly returns.
    Parameters
    ----------
    returns : pd.Series
        Daily returns of the strategy, noncumulative.
         - See full explanation in tears.create_full_tear_sheet.
    ax : matplotlib.Axes, optional
        Axes upon which to plot.
    **kwargs, optional
        Passed to plotting function.
    Returns
    -------
    ax : matplotlib.Axes
        The axes that were plotted on.
    """

    if ax is None:
        ax = plt.gca()

    x_axis_formatter = FuncFormatter(utils.percentage)
    ax.xaxis.set_major_formatter(FuncFormatter(x_axis_formatter))
    ax.tick_params(axis='x', which='major', labelsize=10)

    #daily_ret_table = aggregate_returns(returns, 'daily')

    ax.hist(
        100 * returns,
        color='orangered',
        alpha=0.80,
        bins=20,
        **kwargs)

    ax.axvline(
        100 * returns.mean(),
        color='gold',
        linestyle='--',
        lw=4,
        alpha=1.0)

    ax.axvline(0.0, color='black', linestyle='-', lw=3, alpha=0.75)
    ax.legend(['mean'])
    ax.set_ylabel('Number of days')
    ax.set_xlabel('Returns')
    ax.set_title("Distribution of daily returns")
    return ax


def GetPythonChart(path):

    testFile = pd.read_csv(path)
    keys = testFile.keys()
    date  = pd.to_datetime(testFile[keys[0]])
    print(date[0].tz)
    returns = np.array(testFile[keys[1]])
    print(returns,date)

    totalReturn = pd.Series(returns, index = date)
    print(totalReturn)

    #returns
    #pf.plot_rolling_returns(totalReturn.tz_localize(pytz.utc))
    #plt.show()

    #monthly return 直方图
    #pf.plot_monthly_returns_dist(totalReturn.tz_localize(pytz.utc))
    #plt.show()

    #daily return 直方图
    plot_daily_returns_dist(totalReturn.tz_localize(pytz.utc))
    plt.show()

    #return sharp ratio
    #pf.plot_rolling_sharpe(totalReturn)
    #plt.show()

    #pf.create_full_tear_sheet(totalReturn.tz_localize(pytz.utc))