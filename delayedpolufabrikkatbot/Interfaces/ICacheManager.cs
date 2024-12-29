using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace delayedpolufabrikkatbot.Interfaces
{
	public interface ICacheManager
	{
		public void Add<T>(object key, T value, TimeSpan? timeout = default);
		bool TryGet<T>(object key, out T value);
		void Remove(object key);
	}
}
