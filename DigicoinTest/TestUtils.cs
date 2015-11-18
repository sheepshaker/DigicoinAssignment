using System;
using System.Collections.Generic;
using System.Linq;
using DigicoinService.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DigicoinTest
{
    public static class TestUtils
    {
        public static IDictionary<int, decimal> GetDummyCommisionMap()
        {
            Dictionary<int, decimal> commisionMap = new Dictionary<int, decimal>();

            for (int i = 10; i <= 100; i += 10)
            {
                commisionMap.Add(i, 0.05m);
            }

            return commisionMap;
        }

        public static IDictionary<int, decimal> GetIncompleteCommissionMap()
        {
            Dictionary<int, decimal> commisionMap = new Dictionary<int, decimal>();

            for (int i = 10; i <= 90; i += 10)
            {
                commisionMap.Add(i, 0.05m);
            }

            return commisionMap;
        } 

        public static void CompareOrders(IEnumerable<Order> one, IEnumerable<Order> two)
        {
            CompareIEnumerable(one, two,
                (x, y) =>
                    x.ClientId == y.ClientId && x.TotalPrice == y.TotalPrice && x.TotalVolume == y.TotalVolume &&
                    AreEqual(x.Quotes, y.Quotes,
                        (a, b) => a.BrokerId == b.BrokerId && a.LotSize == b.LotSize && a.Price == b.Price));
        } 

        private static bool AreEqual<T>(IEnumerable<T> one, IEnumerable<T> two, Func<T, T, bool> comparisonFunction)
        {
            var oneArray = one as T[] ?? one.ToArray();
            var twoArray = two as T[] ?? two.ToArray();

            if (oneArray.Length != twoArray.Length)
            {
                return false;
            }

            for (int i = 0; i < oneArray.Length; i++)
            {
                var isEqual = comparisonFunction(oneArray[i], twoArray[i]);
                if (isEqual == false)
                    return false;
            }

            return true;
        }

        private static void CompareIEnumerable<T>(IEnumerable<T> one, IEnumerable<T> two, Func<T, T, bool> comparisonFunction)
        {
            var oneArray = one as T[] ?? one.ToArray();
            var twoArray = two as T[] ?? two.ToArray();

            if (oneArray.Length != twoArray.Length)
            {
                Assert.Fail("Collections are not same length");
            }

            for (int i = 0; i < oneArray.Length; i++)
            {
                var isEqual = comparisonFunction(oneArray[i], twoArray[i]);
                Assert.IsTrue(isEqual);
            }
        }
    }
}
