using System;
using System.Collections;
using System.Collections.Generic;

namespace Backend
{
	class WeightedList<T> : List<T>
	{
		public delegate int GetWeightDelegate(T item);
		private GetWeightDelegate mWeightCallback;
		public WeightedList(GetWeightDelegate callback)
		{
			mWeightCallback = callback;
        }

		public T GetRandom(Random random)
		{
			int totalWeight = 0;

			this.ForEach((item) => totalWeight += mWeightCallback(item));

			int num = random.Next(0, totalWeight - 1);
			int curWeight = 0;
			foreach( var item in this)
			{
				curWeight += mWeightCallback(item);
				if (curWeight > num)
					return item;
			}
			throw new Exception("This should never happen");
		}
		
	}
}