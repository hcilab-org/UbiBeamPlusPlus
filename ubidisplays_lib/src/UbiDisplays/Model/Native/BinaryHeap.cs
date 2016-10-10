using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.Native
{
	class HeapObject
	{
		public Node _0;
		public double _1;

		public HeapObject(Node _0, double _1)
		{
			this._0 = _0;
			this._1 = _1;
		}
	}

	// Binary heap implementation from:
	// http://eloquentjavascript.net/appendix2.html
	class BinaryHeap
	{
		public List<HeapObject> content = new List<HeapObject>();
		public delegate double ScoreFunction(HeapObject obj);
		private ScoreFunction scoreFunction;

		public BinaryHeap(ScoreFunction scoreFunction)
		{
			this.scoreFunction = scoreFunction;
		}

		public void push(HeapObject element)
		{
			this.content.Add(element);
			this.bubbleUp(this.content.Count - 1);
		}

		public HeapObject pop()
		{
			var result = this.content[0];
			var end = this.content.Last();
			this.content.RemoveAt(this.content.Count - 1);
			if (this.content.Count > 0)
			{
				this.content[0] = end;
				this.sinkDown(0);
			}
			return result;
		}

		public HeapObject peek()
		{
			return this.content[0];
		}

		public void remove(HeapObject node)
		{
			var len = this.content.Count;
			for (var i = 0; i < len; i++)
			{
				if (this.content[i] == node)
				{
					var end = this.content.Last();
					this.content.RemoveAt(this.content.Count - 1);
					if (i != len - 1)
					{
						this.content[i] = end;
						if (this.scoreFunction(end) < this.scoreFunction(node))
							this.bubbleUp(i);
						else
							this.sinkDown(i);
					}
					return;
				}
			}
			throw new Exception("Node not found.");
		}

		public int size()
		{
			return this.content.Count;
		}

		public void bubbleUp(int n)
		{
			var element = this.content[n];
			while (n > 0)
			{
				int parentN = (int)Math.Floor((n + 1) / 2.0) - 1;
				HeapObject parent = this.content[parentN];
				if (this.scoreFunction(element) < this.scoreFunction(parent)) {
					this.content[parentN] = element;
					this.content[n] = parent;
					n = parentN;
				}
				else {
					break;
				}
			}
		}

		public void sinkDown(int n)
		{
			int length = this.content.Count;
			HeapObject element = this.content[n];
			double elemScore = this.scoreFunction(element);

			while (true)
			{
				var child2N = (n + 1) * 2;
				var child1N = child2N - 1;
				int swap = 0;
				bool swapped = false;
				double child1Score = 0;
				if (child1N < length)
				{
					var child1 = this.content[child1N];
					child1Score = this.scoreFunction(child1);
					if (child1Score < elemScore)
					{
						swap = child1N;
						swapped = true;
					}
				}
				if (child2N < length)
				{
					var child2 = this.content[child2N];
					double child2Score = this.scoreFunction(child2);
					if (child2Score < (!swapped ? elemScore : child1Score))
					{
						swap = child2N;
						swapped = true;
					}
				}

				if (swapped)
				{
					this.content[n] = this.content[swap];
					this.content[swap] = element;
					n = swap;
				}
				else
				{
					break;
				}
			}
		}
	}
}
