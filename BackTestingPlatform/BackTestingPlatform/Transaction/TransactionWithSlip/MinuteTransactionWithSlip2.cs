using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using BackTestingPlatform.Utilities.TimeList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Transaction.TransactionWithSlip
{
    public static class MinuteTransactionWithSlip2
    {
        //佣金和手续费字典，品种字符对应手续费，期权单位为“x元/张”，股票及期货为成交金额的“x%”
        //实际交易每个品种都有差异，不同期货品种不同，此处暂定1%%
        public static Dictionary<string, double> brokerFeeRatio = new Dictionary<string, double> { { "option", 5 }, { "stock", 0.0005 }, { "futures", 0.0001 } };
        //期权合约倍数
        public static int optionContractTimes = 10000;

        //根据signal进行成交判断
        public static DateTime computeMinutePositions2(Dictionary<string, MinuteSignal> signal, Dictionary<string, List<KLine>> data, ref SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions, ref BasicAccount myAccount, DateTime now, double slipPoint = 0.003)
        {
            //若signal为空或无信号，返回下一时刻时间
            if (signal == null || signal.Count == 0)
            {
                return now.AddMinutes(1);
            }
            //否则在信号价格上，朝不利方向加一个滑点成交
            Dictionary<string, PositionsWithDetail> positionShot = new Dictionary<string, PositionsWithDetail>();
            Dictionary<string, PositionsWithDetail> positionLast = (positions.Count == 0 ? null : positions[positions.Keys.Last()]);
            //成交价初始化，在当前模式下为信号价格朝不利方向加一个滑点
            double transactionPrice = 0;
            //成交数量初始化，此方法下等于信号值（全部成交）
            double transactionVolume = 0;
            //当前成交成本初始化
            double nowTransactionCost = 0;
            //当前后续费初始化
            double nowBrokerFeeRatio = 0;
            //若上一时刻持仓不为空，上刻持仓先赋给此刻持仓，再根据信号调仓
            if (positionLast != null)
            {
                positionShot = new Dictionary<string, PositionsWithDetail>(positionLast);
            }

            foreach (var signal0 in signal.Values)
            {
                //当前信号委托数量不为0，需进行下单操作
                if (signal0.volume != 0)
                {
                    //委托时间
                    now = (signal0.time > now) ? signal0.time : now;
                    //当前临时头寸
                    PositionsWithDetail position0 = new PositionsWithDetail();
                    //多空持仓初始化
                    position0.LongPosition = new PositionDetail();
                    position0.ShortPosition = new PositionDetail();
                    //当前信号多空标志，
                    int longShortFlag = (signal0.volume > 0) ? 1 : -1;
                    //当前信号证券代码
                    position0.code = signal0.code;
                    //将当前证券持仓情况赋给
                    //  position0 = positionShot[position0.code];
                    //当前成交价，信号价格加滑点---注：此模型下信号价格即为现价
                    transactionPrice = signal0.price * (1 + slipPoint * longShortFlag);
                    //当前可成交量
                    transactionVolume = signal0.volume;
                    //当前成交成本（交易费+佣金）
                    nowTransactionCost = 0;
                    //获取当前品种手续费
                    nowBrokerFeeRatio = brokerFeeRatio[signal0.tradingVarieties];
                    //当前证券已有持仓
                    if (positionLast != null && positionLast.ContainsKey(position0.code))
                    {
                        //将当前证券持仓情况赋给临时持仓变量
                        position0 = positionShot[position0.code];
                        //当前为多头持仓
                        if (position0.volume > 0)
                        {
                            //若signal为long，则多头头寸增加
                            if (longShortFlag > 0)
                            {
                                //多头头寸更新
                                position0.LongPosition.averagePrice = (position0.LongPosition.averagePrice * position0.LongPosition.volume + transactionPrice * transactionVolume) / (position0.LongPosition.volume + transactionVolume);
                                position0.LongPosition.volume = position0.LongPosition.volume + transactionVolume;
                                position0.LongPosition.totalCost = position0.LongPosition.averagePrice * position0.LongPosition.volume;
                            }
                            //若signal为short，先平多，若不足则开空(注：short则成交量为负)
                            else
                            {
                                // 若信号空头量小于等于持仓多头，则多头持仓减小
                                if ((position0.LongPosition.volume + transactionVolume) >= 0)
                                {
                                    //多头头寸更新
                                    //position价格不变,方便记录真实持仓成本
                                    position0.LongPosition.volume = position0.LongPosition.volume + transactionVolume;
                                    position0.LongPosition.totalCost = position0.LongPosition.averagePrice * position0.LongPosition.volume;
                                }
                                else if ((position0.LongPosition.volume + transactionVolume) < 0)
                                {
                                    //多头头寸更新，平多头
                                    position0.LongPosition.averagePrice = 0;
                                    position0.LongPosition.volume = 0;
                                    position0.LongPosition.totalCost = 0;
                                    transactionVolume = position0.LongPosition.volume + transactionVolume;
                                    //空头头寸更新，开空头
                                    position0.ShortPosition.averagePrice = transactionPrice;
                                    position0.ShortPosition.volume = transactionVolume;
                                    position0.ShortPosition.totalCost = position0.ShortPosition.averagePrice * position0.ShortPosition.volume;
                                }
                            }
                        }
                        //当前为空头持仓 positon0.volume < 0
                        else
                        {
                            //若signal为short，则空头头寸增加
                            if (longShortFlag < 0)
                            {
                                //空头头寸更新
                                position0.ShortPosition.averagePrice = (position0.ShortPosition.averagePrice * position0.ShortPosition.volume + transactionPrice * transactionVolume) / (position0.ShortPosition.volume + transactionVolume);
                                position0.ShortPosition.volume = position0.ShortPosition.volume + transactionVolume;
                                position0.ShortPosition.totalCost = position0.ShortPosition.averagePrice * position0.ShortPosition.volume;
                            }
                            //若signal为long，先平空，若不足则开多(注：short则成交量为负)
                            else
                            {
                                // 若信号多头量小于等于持仓空头，则空头持仓减小
                                if ((position0.ShortPosition.volume + transactionVolume) <= 0)
                                {
                                    //空头头寸更新
                                    //position价格不变,方便记录真实持仓成本
                                    position0.ShortPosition.volume = position0.ShortPosition.volume + transactionVolume;
                                    position0.ShortPosition.totalCost = position0.ShortPosition.averagePrice * position0.ShortPosition.volume;
                                }
                                else if ((position0.ShortPosition.volume + transactionVolume) > 0)
                                {
                                    //空头头寸更新，平空头
                                    position0.ShortPosition.averagePrice = 0;
                                    position0.ShortPosition.volume = 0;
                                    position0.ShortPosition.totalCost = 0;
                                    transactionVolume += position0.ShortPosition.volume;
                                    //多头头寸更新，开多头
                                    position0.LongPosition.averagePrice = transactionPrice;
                                    position0.LongPosition.volume = transactionVolume;
                                    position0.LongPosition.totalCost = position0.LongPosition.averagePrice * position0.LongPosition.volume;
                                }
                            }
                        }
                    }
                    //当前无证券持仓
                    else
                    {

                        //若为多头开仓，更新多头头寸
                        if (longShortFlag > 0)
                        {
                            position0.LongPosition.averagePrice = transactionPrice;
                            position0.LongPosition.volume = transactionVolume;
                            position0.LongPosition.totalCost = position0.LongPosition.averagePrice * position0.LongPosition.averagePrice;
                        }
                        //若为空头开仓，更新空头头寸
                        else
                        {
                            position0.ShortPosition.averagePrice = transactionPrice;
                            position0.ShortPosition.volume = transactionVolume;
                            position0.ShortPosition.totalCost = position0.ShortPosition.averagePrice * position0.ShortPosition.averagePrice;
                        }
                    }
                    //持仓汇总信息记录
                    //当前时间
                    position0.time = now;
                    //当前品种
                    position0.tradingVarieties = signal0.tradingVarieties;
                    //当前价
                    position0.currentPrice = signal0.price;
                    //当前持仓量
                    position0.volume = position0.LongPosition.volume + position0.ShortPosition.volume;
                    //当前权益（实时）
                    position0.totalAmt = position0.currentPrice * position0.volume;
                    //手续费
                    //手续费计算,期权
                    if (signal0.tradingVarieties.Equals("option"))
                    {
                        //若为short信号，开空的部分手续费为0
                        //若信号为short，且调整持仓后交易总量大于等于空头持仓量,说明是先平多再开空，只收取平多部分手续费
                        if (longShortFlag < 0 && Math.Abs(transactionVolume) >= Math.Abs(position0.ShortPosition.volume))
                            //平多合约张数 * 手续费
                            nowTransactionCost = Math.Abs((transactionVolume - position0.ShortPosition.volume) / optionContractTimes * nowBrokerFeeRatio);
                        //若信号为short，且调整持仓后交易总量小于空头持仓量,说明是继续开空，无手续费
                        else if (longShortFlag < 0 && Math.Abs(transactionVolume) < Math.Abs(position0.ShortPosition.volume))
                            nowTransactionCost = 0;
                        //若信号为long，正常收取手续费
                        else
                            //合约张数 * 手续费
                            nowTransactionCost = Math.Abs(transactionVolume / optionContractTimes * nowBrokerFeeRatio);
                    }
                    else if (signal0.tradingVarieties.Equals("stock"))
                    {
                        //若信号为short，正常收取手续费
                        if (longShortFlag < 0)
                            //成交金额 * 手续费率
                            nowTransactionCost = Math.Abs(transactionPrice * transactionVolume * nowBrokerFeeRatio);
                        //若信号为long，无手续费
                        else
                            nowTransactionCost = 0;
                    }
                    //实际中不同期货品种手续费差异大，有的按手有的按成交额比率，有的单边有的双边，此处简单处理按单边
                    else if (signal0.tradingVarieties.Equals("futures"))
                    {
                        //若信号为short，正常收取手续费
                        if (longShortFlag < 0)
                            //成交金额 * 手续费率
                            nowTransactionCost = Math.Abs(transactionPrice * transactionVolume * nowBrokerFeeRatio);
                        //若信号为long，无手续费
                        else
                            nowTransactionCost = 0;
                    }
                    //总手续费、持仓成本更新  
                    //手续费，持续累加
                    position0.transactionCost += nowTransactionCost;
                    //当前持仓总成本:头寸总价值+当前手续费
                    position0.totalCost += (position0.volume > 0 ? position0.LongPosition.totalCost : position0.ShortPosition.totalCost) + nowTransactionCost;
                    //交易记录添加
                    position0.record = new List<TransactionRecord>();
                    position0.record.Add(new TransactionRecord
                    {
                        time = now,
                        volume = transactionVolume,
                        price = transactionPrice
                    });
                    //存储当前持仓信息
                    if (positionShot.ContainsKey(position0.code))
                    {
                        positionShot[position0.code] = position0;
                    }
                    else
                    {
                        positionShot.Add(signal0.code, position0);
                    }
                    //账户信息更新
                    //根据当前交易记录和持仓情况更新账户
                    AccountUpdating.computeAccountUpdating(ref myAccount, positionShot, now, ref data);
                }

            }
            positions.Add(now, positionShot);

            return now.AddMinutes(1);
        }
    }
}
