using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CloudSecurityTest
{


    class TCPConnectionsTest : SecurityTest, ISecurityTest
    {

        public bool IsLowerScoreBetter
        {
            get { return true; }
        }

        public string Description
        {
            get { return "Test if an app cycles are balanced using process priorities."; }
        }

        public string Metric
        {
            get { return "Process priority"; }
        }

        public void RunTest()
        {
            Stopwatch start = Stopwatch.StartNew();
            Integers.IsPrime("999999999999999989");
            start.Stop();
            Message(start.ElapsedMilliseconds.ToString());
            Score(Process.GetCurrentProcess().BasePriority);
        }
 
        public static partial class Integers
        {
            #region Prime Numbers <100
            private static readonly int[] Primes =
            new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23,
                    29, 31, 37, 41, 43, 47, 53, 59,
                    61, 67, 71, 73, 79, 83, 89, 97 };
            #endregion
            // starting number for iterative factorization
            private const int _startNum = 101;
            #region IsPrime: primality Check
            /// <summary>
            /// Check if the number is Prime
            /// </summary>
            /// <param name="Num">Int64</param>
            /// <returns>bool</returns>
            public static bool IsPrime(Int64 Num)
            {
                int j;
                bool ret;
                Int64 _upMargin = (Int64)Math.Sqrt(Num) + 1; ;
                // Check if number is in Prime Array
                for (int i = 0; i < Primes.Length; i++)
                {
                    if (Num == Primes[i]) { return true; }
                }
                // Check divisibility w/Prime Array
                for (int i = 0; i < Primes.Length; i++)
                {
                    if (Num % Primes[i] == 0) return false;
                }
                // Main iteration for Primality check
                _upMargin = (Int64)Math.Sqrt(Num) + 1;
                j = _startNum;
                ret = true;
                while (j <= _upMargin)
                {
                    if (Num % j == 0) { ret = false; break; }
                    else { j++; j++; }
                }
                return ret;
            }
            /// <summary>
            /// Check if number-string is Prime
            /// </summary>
            /// <param name="Num">string</param>
            /// <returns>bool</returns>
            public static bool IsPrime(string StringNum)
            {
                return IsPrime(Int64.Parse(StringNum));
            }
            #endregion
            #region Fast Factorization
            /// <summary>
            /// Factorize string converted to long integers
            /// </summary>
            /// <param name="StringNum">string</param>
            /// <returns>Int64[]</returns>
            public static Int64[] FactorizeFast(string StringNum)
            {
                return FactorizeFast(Int64.Parse(StringNum));
            }
            /// <summary>
            /// Factorize long integers: speed optimized
            /// </summary>
            /// <param name="Num">Int64</param>
            /// <returns>Int64[]</returns>
            public static Int64[] FactorizeFast(Int64 Num)
            {
                #region vars
                // list of Factors
                List<Int64> _arrFactors = new List<Int64>();
                // temp variable
                Int64 _num = Num;
                #endregion
                #region Check if the number is Prime (<100)
                for (int k = 0; k < Primes.Length; k++)
                {
                    if (_num == Primes[k])
                    {
                        _arrFactors.Add(Primes[k]);
                        return _arrFactors.ToArray();
                    }
                }
                #endregion
                #region Try to factorize using Primes Array
                for (int k = 0; k < Primes.Length; k++)
                {
                    int m = Primes[k];
                    if (_num < m) break;
                    while (_num % m == 0)
                    {
                        _arrFactors.Add(m);
                        _num = (Int64)_num / m;
                    }
                }
                if (_num < _startNum)
                {
                    _arrFactors.Sort();
                    return _arrFactors.ToArray();
                }
                #endregion
                #region Main Factorization Algorithm
                Int64 _upMargin = (Int64)Math.Sqrt(_num) + 1;
                Int64 i = _startNum;
                while (i <= _upMargin)
                {
                    if (_num % i == 0)
                    {
                        _arrFactors.Add(i);
                        _num = _num / i;
                        _upMargin = (Int64)Math.Sqrt(_num) + 1;
                        i = _startNum;
                    }
                    else { i++; i++; }
                }
                _arrFactors.Add(_num);
                _arrFactors.Sort();
                return _arrFactors.ToArray();
                #endregion
            }
            #endregion
        }
    }
}
