﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System;

namespace OneStarCalculator
{
	public class SeedSearcher
	{
		// モード
		public enum Mode {
			Star12,
			Star35_5,
			Star35_6
		};
		Mode m_Mode;

		public SeedSearcher(Mode mode)
		{
			m_Mode = mode;
		}

		// 結果
		public List<ulong> Result { get; } = new List<ulong>();

		// ★1～2検索
		[DllImport("OneStarCalculatorLib.dll")]
		static extern void Prepare(int rerolls);

		[DllImport("OneStarCalculatorLib.dll")]
		public static extern void SetFirstCondition(int iv0, int iv1, int iv2, int iv3, int iv4, int iv5, int fixedIV, int flawlessIDX, int ability, int nature, int characteristic, bool noGender, bool isDream);

		[DllImport("OneStarCalculatorLib.dll")]
		public static extern void SetNextCondition(int iv0, int iv1, int iv2, int iv3, int iv4, int iv5, int fixedIV, int ability, int nature, int characteristic, bool noGender, bool isDream);

		[DllImport("OneStarCalculatorLib.dll")]
		public static extern void SetThirdCondition(int iv0, int iv1, int iv2, int iv3, int iv4, int iv5, int fixedIV, int ability, int nature, int characteristic, bool noGender, bool isDream);

		[DllImport("OneStarCalculatorLib.dll")]
		public static extern void SetLSB(int bit);

		[DllImport("OneStarCalculatorLib.dll")]
		static extern ulong Search(ulong ivs);

		[DllImport("OneStarCalculatorLib.dll")]
		static extern uint TestSeed(ulong seed);

		// ★3～5検索
		[DllImport("OneStarCalculatorLib.dll")]
		static extern void PrepareSix(int ivOffset);

		[DllImport("OneStarCalculatorLib.dll")]
		public static extern void SetSixFirstCondition(int iv0, int iv1, int iv2, int iv3, int iv4, int iv5, int fixedIV, int ability, int nature, int characteristic, bool noGender, bool isDream);

		[DllImport("OneStarCalculatorLib.dll")]
		public static extern void SetSixSecondCondition(int iv0, int iv1, int iv2, int iv3, int iv4, int iv5, int fixedIV, int ability, int nature, int characteristic, bool noGender, bool isDream);

		[DllImport("OneStarCalculatorLib.dll")]
		public static extern void SetSixThirdCondition(int iv0, int iv1, int iv2, int iv3, int iv4, int iv5, int fixedIV, int ability, int nature, int characteristic, bool noGender, bool isDream);

		[DllImport("OneStarCalculatorLib.dll")]
		public static extern void SetSixFourthCondition(int iv0, int iv1, int iv2, int iv3, int iv4, int iv5, int fixedIV, int ability, int nature, int characteristic, bool noGender, bool isDream);

		[DllImport("OneStarCalculatorLib.dll")]
		public static extern void SetTargetCondition6(int iv0, int iv1, int iv2, int iv3, int iv4, int iv5);

		[DllImport("OneStarCalculatorLib.dll")]
		public static extern void SetTargetCondition5(int iv0, int iv1, int iv2, int iv3, int iv4);

		[DllImport("OneStarCalculatorLib.dll")]
		public static extern void SetSixLSB(int bit);

		[DllImport("OneStarCalculatorLib.dll")]
		static extern ulong SearchSix(ulong ivs);

		[DllImport("OneStarCalculatorLib.dll")]
		static extern uint TestSixSeed(ulong seed);

		static readonly ulong shift = 0x7817eba09827c0eful;

		public void Calculate(int minRerolls, int maxRerolls, Label updateLbl)
		{
			Result.Clear();

			if (m_Mode == Mode.Star12)
			{
				if(TestSeed(0) != 7)
				{
					// 探索範囲
					int searchLower = 0;
					int searchUpper = 0xFFFFFFF;

					for (int i = minRerolls; i <= maxRerolls; ++i)
					{
						updateLbl.Text = i.ToString();
						// C++ライブラリ側の事前計算
						Prepare(i);
						// 中断あり
						Parallel.For(searchLower, searchUpper, (ivs, state) =>
						{
							ulong result = Search((ulong)ivs);
							if (result != 0)
							{
								Result.Add(result);
								state.Stop();
							}
						});
						if (Result.Count > 0)
						{
							break;
						}
					}
				} else
				{
					Result.Add(0);
				}
			}
			else
			{
				if (TestSixSeed(0) != 11)
				{
					// 探索範囲
					int searchLower = 0;
					int searchUpper = (m_Mode == Mode.Star35_5 ? 0x1FFFFFF : 0x3FFFFFFF);
					for (int i = 0; i <= maxRerolls; ++i)
					{
						updateLbl.Text = i.ToString();
						// C++ライブラリ側の事前計算
						PrepareSix(i);
						// 並列探索
						Parallel.For(searchLower, searchUpper, (ivs, state) =>
						{
							ulong result = SearchSix((ulong)ivs);
							if (result != 0)
							{
								Result.Add(result+shift);
								state.Stop();
							}
						});
						if (Result.Count > 0)
						{
							break;
						}
					}
				} else
				{
					Result.Add(shift);
				}
			}
		}
	}
}
