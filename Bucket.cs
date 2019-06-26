using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kademlia
{
	class Bucket
	{
		public const int BucketSize = 16;
		public List<KademliaNode> _contents { get; private set; }
		private Random _rand;

		public Bucket()
		{
			_rand = new Random();
			_contents = new List<KademliaNode>();
		}

		public bool Add(KademliaNode node)
		{
			if (_contents.Contains(node))
			{
				// move node to most recently used
				_contents.Remove(node);
				_contents.Add(node);
				return false;
			}

			if (_contents.Count < BucketSize)
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
				// evict least recently used node 
				_contents.RemoveAt(0);
				_contents.Add(node);
				return true;
			}
		}

		public void Remove(KademliaNode node)
		{
			if (_contents.Contains(node))
			{
				_contents.Remove(node);
			}
		}

		public KademliaNode GetRandomNode()
		{
			int size = _contents.Count;

			if (size == 0)
				return null;

			int index = _rand.Next(size);
			return _contents[index];
		}
	}
}
