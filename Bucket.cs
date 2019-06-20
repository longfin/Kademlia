using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kademlia
{
	class Bucket
	{
		private const int k_SizeOfBucket = 8;
		private List<KademliaNode> _contents;

		public Bucket()
		{
			_contents = new List<KademliaNode>();
		}

		public bool Add(KademliaNode node)
		{
			if (_contents.Count < k_SizeOfBucket)
			{
				_contents.Add(node);
				return true;
			}

			if (_contents.First().Respond())
			{
				// discard apped
				return false;
			}
			else
			{
				// evict oldest node
				_contents.Add(node);
				_contents.RemoveAt(0);
				return true;
			}
		}

		public KademliaNode GetRandomNode()
		{
			int size = _contents.Count;

			if (size == 0)
				return null;

			Random r = new Random();
			int index = (int)(r.NextDouble() * size);
			return _contents[index];
		}
	}
}
