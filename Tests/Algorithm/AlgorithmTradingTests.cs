/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QuantConnect.Algorithm;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Securities;

namespace QuantConnect.Tests.Algorithm
{
    [TestFixture]
    public class AlgorithmTradingTests
    {
        /*****************************************************/
        //  Isostatic market conditions tests.
        /*****************************************************/
        [Test]
        public void SetHoldings_ZeroToLong()
        {
            var algo = GetAlgorithm();
            //Set price to $25 & Target 50%
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
            var actual = algo.CalculateOrderQuantity("MSFT", 0.5m);
            Assert.AreEqual(2000, actual);
        }

        [Test]
        public void SetHoldings_ZeroToShort()
        {
            var algo = GetAlgorithm();
            //Set price to $25 & Target 50%
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
            var actual = algo.CalculateOrderQuantity("MSFT", -0.5m);
            Assert.AreEqual(-2000, actual);
        }

        [Test]
        public void SetHoldings_LongToLonger()
        {
            var algo = GetAlgorithm();
            //Set price to $25
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
            //Half cash spent on 2000 MSFT shares.
            algo.Portfolio.SetCash(50000);
            algo.Portfolio["MSFT"].SetHoldings(25, 2000);
            //Calculate the new holdings:
            var actual = algo.CalculateOrderQuantity("MSFT", 0.75m);
            Assert.AreEqual(1000, actual);
        }

        [Test]
        public void SetHoldings_LongerToLong()
        {
            var algo = GetAlgorithm();
            //Set price to $25
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
            //75% cash spent on 3000 MSFT shares.
            algo.Portfolio.SetCash(25000);
            algo.Portfolio["MSFT"].SetHoldings(25, 3000);
            //Sell all 2000 held:
            var actual = algo.CalculateOrderQuantity("MSFT", 0.5m);
            Assert.AreEqual(-1000, actual);
        }

        [Test]
        public void SetHoldings_LongToZero()
        {
            var algo = GetAlgorithm();
            //Set price to $25
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
            //Half cash spent on 2000 MSFT shares.
            algo.Portfolio.SetCash(50000);
            algo.Portfolio["MSFT"].SetHoldings(25, 2000);
            //Sell all 2000 held:
            var actual = algo.CalculateOrderQuantity("MSFT", 0m);
            Assert.AreEqual(-2000, actual);
        }

        [Test]
        public void SetHoldings_LongToShort()
        {
            var algo = GetAlgorithm();
            //Set price to $25
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
            //Half cash spent on 2000 MSFT shares.
            algo.Portfolio.SetCash(50000);
            algo.Portfolio["MSFT"].SetHoldings(25, 2000);

            //Sell all 2000 held + -2000 to get to -50%
            var actual = algo.CalculateOrderQuantity("MSFT", -0.5m);
            Assert.AreEqual(-4000, actual);
        }

        [Test]
        public void SetHoldings_ShortToZero()
        {
            var algo = GetAlgorithm();
            //Set price to $25
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
            //Half cash spent on 2000 MSFT shares.
            algo.Portfolio.SetCash(50000);
            algo.Portfolio["MSFT"].SetHoldings(25, -2000);
            //Buy 2000 to get to 0 holdings.
            var actual = algo.CalculateOrderQuantity("MSFT", 0m);
            Assert.AreEqual(2000, actual);
        }

        [Test]
        public void SetHoldings_ShortToShorter()
        {
            var algo = GetAlgorithm();
            //Set price to $25
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
            //Half cash spent on -2000 MSFT shares.
            algo.Portfolio.SetCash(50000);
            algo.Portfolio["MSFT"].SetHoldings(25, -2000);
            //Sell 1000 to get to -3000 holdings.
            var actual = algo.CalculateOrderQuantity("MSFT", -0.75m);
            Assert.AreEqual(-1000, actual);
        }

        [Test]
        public void SetHoldings_ShortToLong()
        {
            var algo = GetAlgorithm();
            //Set price to $25
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
            //Half cash spent on -2000 MSFT shares.
            algo.Portfolio.SetCash(50000);
            algo.Portfolio["MSFT"].SetHoldings(25, -2000);
            //Sell 1000 to get to -3000 holdings.
            var actual = algo.CalculateOrderQuantity("MSFT", 0.5m);
            Assert.AreEqual(4000, actual);
        }


        /*****************************************************/
        //  Rising market conditions tests.
        /*****************************************************/
        [Test]
        public void SetHoldings_LongFixed_PriceRise()
        {
            var algo = GetAlgorithm();
            //Set price to $25
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
            
            //Half cash spent on 2000 MSFT shares.
            algo.Portfolio.SetCash(50000);
            algo.Portfolio["MSFT"].SetHoldings(25, 2000);

            //Price rises to $50.
            algo.Securities.Update(DateTime.Now, Update("MSFT", 50));

            //Now: 2000 * 50 = $100k Holdings, $50k Cash: $150k.
            //Calculate the new holdings for 50% MSFT::
            var actual = algo.CalculateOrderQuantity("MSFT", 0.5m);
            
            //Need to sell $25k so 50% of $150k: $25k / $50-share = -500 shares
            Assert.AreEqual(-500, actual);
        }
        
        
        [Test]
        public void SetHoldings_LongToLonger_PriceRise()
        {
            var algo = GetAlgorithm();
            //Set price to $25
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
            //Half cash spent on 2000 MSFT shares.
            algo.Portfolio.SetCash(50000);
            algo.Portfolio["MSFT"].SetHoldings(25, 2000);

            //Price rises to $50.
            algo.Securities.Update(DateTime.Now, Update("MSFT", 50));

            //Now: 2000 * 50 = $100k Holdings, $50k Cash: $150k. MSFT is already 66% of holdings.
            //Calculate the order for 75% MSFT:
            var actual = algo.CalculateOrderQuantity("MSFT", 0.75m);

            //Need to buy to make position $112.5k == $12.5k / 50 = 250 shares
            Assert.AreEqual(250, actual);
        }

        [Test]
        public void SetHoldings_LongerToLong_PriceRise()
        {
            var algo = GetAlgorithm();
            //Set price to $25
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));

            //75% cash spent on 3000 MSFT shares.
            algo.Portfolio.SetCash(25000);
            algo.Portfolio["MSFT"].SetHoldings(25, 3000);

            //Price rises to $50.
            algo.Securities.Update(DateTime.Now, Update("MSFT", 50));

            //Now: 3000 * 50 = $150k Holdings, $25k Cash: $175k. MSFT is 86% of holdings.
            //Calculate the order for 50% MSFT:
            var actual = algo.CalculateOrderQuantity("MSFT", 0.5m);

            //Need to sell to 50% = 87.5k target from $150k = 62.5 / $50-share = 1250
            Assert.AreEqual(-1250, actual);
        }


        [Test]
        public void SetHoldings_LongToShort_PriceRise()
        {
            var algo = GetAlgorithm();
            //Set price to $25
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
            //Half cash spent on 2000 MSFT shares.
            algo.Portfolio.SetCash(50000);
            algo.Portfolio["MSFT"].SetHoldings(25, 2000);

            //Price rises to $50.
            algo.Securities.Update(DateTime.Now, Update("MSFT", 50));

            //Now: 2000 * 50 = $100k Holdings, $50k Cash: $150k. MSFT is 66% of holdings.
            var actual = algo.CalculateOrderQuantity("MSFT", -0.5m);

            // Need to hold -75k from $100k = delta: $175k / $50-share = -3500 shares.
            Assert.AreEqual(-3500, actual);
        }

        [Test]
        public void SetHoldings_ShortToShorter_PriceRise()
        {
            var algo = GetAlgorithm();
            //Set price to $25
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
            //Half cash spent on -2000 MSFT shares.
            algo.Portfolio.SetCash(50000);
            algo.Portfolio["MSFT"].SetHoldings(25, -2000);

            //Price rises to $50.
            algo.Securities.Update(DateTime.Now, Update("MSFT", 50));

            //Now: 2000 * 50 = $0k Net Holdings, $50k Cash: $50k. MSFT is 0% of holdings.
            var actual = algo.CalculateOrderQuantity("MSFT", -0.75m);

            //Want to hold -75% of MSFT: 50k total, -37.5k / $50-share = -750 TOTAL. 
            // Currently -2000, so net order +1250.
            Assert.AreEqual(1250, actual);
        }


        [Test]
        public void SetHoldings_ShortToLong_PriceRise_ZeroValue()
        {
            var algo = GetAlgorithm();
            //Set price to $25
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
            //Half cash spent on -2000 MSFT shares.
            algo.Portfolio.SetCash(50000);
            algo.Portfolio["MSFT"].SetHoldings(25, -2000);

            //Price rises to $50: holdings now worthless.
            algo.Securities.Update(DateTime.Now, Update("MSFT", 50m));

            //Now: 2000 * 50 = $0k Net Holdings, $50k Cash: $50k. MSFT is 0% of holdings.
            var actual = algo.CalculateOrderQuantity("MSFT", 0.5m);

            //We want to be 50% long, this is currently +2000 holdings + 50% 50k = $25k/ $50-share=500
            Assert.AreEqual(2500, actual);
        }

        [Test]
        public void SetHoldings_ShortToLong_PriceRise()
        {
            var algo = GetAlgorithm();
            //Set price to $25
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
            //Half cash spent on -2000 MSFT shares.
            algo.Portfolio.SetCash(50000);
            algo.Portfolio["MSFT"].SetHoldings(25, -2000);

            //Price rises to $37.50 - half the holdings value
            algo.Securities.Update(DateTime.Now, Update("MSFT", 37.5m));

            //Now: 2000 * 50 = $25k Net Holdings, $50k Cash: $75k. MSFT is 33% of holdings.
            var actual = algo.CalculateOrderQuantity("MSFT", 0.5m);

            //We want to be +50% long, starting -2000 holdings, + 50% of 75k = $37.5k/ $50-share=750
            Assert.AreEqual(2750, actual);
        }



        /*****************************************************/
        //  Falling market conditions tests.
        /*****************************************************/
        [Test]
        public void SetHoldings_ShortFixed_PriceFall()
        {
            var algo = GetAlgorithm();
            //Set price to $25
            algo.Securities.Update(DateTime.Now, Update("MSFT", 25));

            //Half cash spent on 2000 MSFT shares.
            algo.Portfolio.SetCash(50000);
            algo.Portfolio["MSFT"].SetHoldings(25, -2000);

            //Price rises to $50.
            algo.Securities.Update(DateTime.Now, Update("MSFT", 12.5m));

            //Now: 2000 * 12.5 = +$25k Holdings, $50k Original, $50k Cash: Total $125k.
            //Calculate the new holdings for 50% MSFT::
            var actual = algo.CalculateOrderQuantity("MSFT", -0.5m);

            // Hold -$75k, want to hold $125k/2 = -$62.5k: Delta +12.5k / $12.5-share 
            // = +1000 shares to reduce short position
            Assert.AreEqual(1000, actual);
        }


        //[Test]
        //public void SetHoldings_LongToLonger_PriceRise()
        //{
        //    var algo = GetAlgorithm();
        //    //Set price to $25
        //    algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
        //    //Half cash spent on 2000 MSFT shares.
        //    algo.Portfolio.SetCash(50000);
        //    algo.Portfolio["MSFT"].SetHoldings(25, 2000);

        //    //Price rises to $50.
        //    algo.Securities.Update(DateTime.Now, Update("MSFT", 50));

        //    //Now: 2000 * 50 = $100k Holdings, $50k Cash: $150k. MSFT is already 66% of holdings.
        //    //Calculate the order for 75% MSFT:
        //    var actual = algo.CalculateOrderQuantity("MSFT", 0.75m);

        //    //Need to buy to make position $112.5k == $12.5k / 50 = 250 shares
        //    Assert.AreEqual(250, actual);
        //}

        //[Test]
        //public void SetHoldings_LongerToLong_PriceRise()
        //{
        //    var algo = GetAlgorithm();
        //    //Set price to $25
        //    algo.Securities.Update(DateTime.Now, Update("MSFT", 25));

        //    //75% cash spent on 3000 MSFT shares.
        //    algo.Portfolio.SetCash(25000);
        //    algo.Portfolio["MSFT"].SetHoldings(25, 3000);

        //    //Price rises to $50.
        //    algo.Securities.Update(DateTime.Now, Update("MSFT", 50));

        //    //Now: 3000 * 50 = $150k Holdings, $25k Cash: $175k. MSFT is 86% of holdings.
        //    //Calculate the order for 50% MSFT:
        //    var actual = algo.CalculateOrderQuantity("MSFT", 0.5m);

        //    //Need to sell to 50% = 87.5k target from $150k = 62.5 / $50-share = 1250
        //    Assert.AreEqual(-1250, actual);
        //}


        //[Test]
        //public void SetHoldings_LongToShort_PriceRise()
        //{
        //    var algo = GetAlgorithm();
        //    //Set price to $25
        //    algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
        //    //Half cash spent on 2000 MSFT shares.
        //    algo.Portfolio.SetCash(50000);
        //    algo.Portfolio["MSFT"].SetHoldings(25, 2000);

        //    //Price rises to $50.
        //    algo.Securities.Update(DateTime.Now, Update("MSFT", 50));

        //    //Now: 2000 * 50 = $100k Holdings, $50k Cash: $150k. MSFT is 66% of holdings.
        //    var actual = algo.CalculateOrderQuantity("MSFT", -0.5m);

        //    // Need to hold -75k from $100k = delta: $175k / $50-share = -3500 shares.
        //    Assert.AreEqual(-3500, actual);
        //}

        //[Test]
        //public void SetHoldings_ShortToShorter_PriceRise()
        //{
        //    var algo = GetAlgorithm();
        //    //Set price to $25
        //    algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
        //    //Half cash spent on -2000 MSFT shares.
        //    algo.Portfolio.SetCash(50000);
        //    algo.Portfolio["MSFT"].SetHoldings(25, -2000);

        //    //Price rises to $50.
        //    algo.Securities.Update(DateTime.Now, Update("MSFT", 50));

        //    //Now: 2000 * 50 = $0k Net Holdings, $50k Cash: $50k. MSFT is 0% of holdings.
        //    var actual = algo.CalculateOrderQuantity("MSFT", -0.75m);

        //    //Want to hold -75% of MSFT: 50k total, -37.5k / $50-share = -750 TOTAL. 
        //    // Currently -2000, so net order +1250.
        //    Assert.AreEqual(1250, actual);
        //}

        //[Test]
        //public void SetHoldings_ShortToLong_PriceRise()
        //{
        //    var algo = GetAlgorithm();
        //    //Set price to $25
        //    algo.Securities.Update(DateTime.Now, Update("MSFT", 25));
        //    //Half cash spent on -2000 MSFT shares.
        //    algo.Portfolio.SetCash(50000);
        //    algo.Portfolio["MSFT"].SetHoldings(25, -2000);

        //    //Price rises to $50.
        //    algo.Securities.Update(DateTime.Now, Update("MSFT", 50));

        //    //Now: 2000 * 50 = $0k Net Holdings, $50k Cash: $50k. MSFT is 0% of holdings.
        //    var actual = algo.CalculateOrderQuantity("MSFT", 0.5m);

        //    //We want to be 50% long, this is currently +2000 holdings + 50% 50k = $25k/ $50-share=500
        //    Assert.AreEqual(2500, actual);
        //}

















        private QCAlgorithm GetAlgorithm()
        {
            //Initialize algorithm
            var algo = new QCAlgorithm();
            algo.AddSecurity(SecurityType.Equity, "MSFT");
            algo.SetCash(100000);
            algo.Securities["MSFT"].TransactionModel = new ConstantFeeTransactionModel(0);
            return algo;
        }


        private Dictionary<int, List<BaseData>> Update(string symbol, decimal close)
        {
            return new Dictionary<int, List<BaseData>>()
            {
                { 
                    0, new List<BaseData>() { new TradeBar()
                    {
                        Time = DateTime.Now,
                        Symbol = symbol,
                        Open = close,
                        High = close,
                        Low = close,
                        Close = close
                    }}
                }
            };
        }
    }
}
