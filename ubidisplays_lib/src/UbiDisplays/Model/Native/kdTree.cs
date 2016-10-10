using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.Native
{
	/**
	 * @brief kdTree is a basic but super fast JavaScript implementation of
	 * the k-dimensional tree data structure.
	 * @see https://github.com/ubilabs/kd-tree-javascript/blob/master/Readme.md
	 * 
	 * Copyright (c) 2012 Ubilabs
	 * 
	 * Permission is hereby granted, free of charge, to any person obtaining a copy of
	 * this software and associated documentation files (the "Software"), to deal in
	 * the Software without restriction, including without limitation the rights to
	 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
	 * of the Software, and to permit persons to whom the Software is furnished to do
	 * so, subject to the following conditions:
	 * 
	 * The above copyright notice and this permission notice shall be included in all
	 * copies or substantial portions of the Software.
	 * 
	 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	 * THE SOFTWARE.
	 */

	class Node
	{
		public Point obj;
		public Node left;
		public Node right;
		public Node parent;
		public int dimension;

		public Node(Point obj, int d, Node parent)
		{
			this.obj = obj;
			this.left = null;
			this.right = null;
			this.parent = parent;
			this.dimension = d;
		}
	}

	struct PointAndDistance
	{
		public Point _0;
		public double _1;

		public PointAndDistance(Point _0, double _1)
		{
			this._0 = _0;
			this._1 = _1;
		}
	}

	/*class kdTree
	{
		public delegate double MetricDelegate(Point a, Point b);
		private MetricDelegate metric;
		private List<Point> points;

		public kdTree(List<Point> points, MetricDelegate metric, List<string> dimensions)
		{
			this.metric = metric;
			//this.dimensions = dimensions;
			//this.root = buildTree(points, 0, null);
			this.points = points;
		}

		public List<PointAndDistance> nearest(Point point, int maxNodes, double maxDistance)
		{
			//maxDistance /= 4.0;
			var pds = new List<PointAndDistance>();
			foreach (var p in points)
			{
				double distance = metric(point, p);
				if (distance <= maxDistance)
				{
					pds.Add(new PointAndDistance(p, distance));
					//if (pds.Count >= maxNodes) break;
				}
			}
			pds.Sort((PointAndDistance a, PointAndDistance b) => { return (int)(metric(point, a._0) - metric(point, b._0)); });
			var pds2 = new List<PointAndDistance>();
			pds2.AddRange(pds.GetRange(0, Math.Min(maxNodes, pds.Count)));
			return pds2;
		}
	}*/

	class kdTree
	{
		private Node root;
		private List<string> dimensions;
		
		public delegate double MetricDelegate(Point a, Point b);
		private MetricDelegate metric;

		public kdTree(List<Point> points, MetricDelegate metric, List<string> dimensions)
		{
			this.metric = metric;
			this.dimensions = dimensions;
			this.root = buildTree(points, 0, null);
		}

		Node buildTree(List<Point> points, int depth, Node parent)
		{
			var dim = depth % dimensions.Count;
			if (points.Count == 0) return null;
			if (points.Count == 1) return new Node(points[0], dim, parent);

			points.Sort((Point a, Point b) => { return (int)(a.dimension(dim) - b.dimension(dim)); });

			var median = (int)Math.Floor(points.Count / 2.0);
			var node = new Node(points[median], dim, parent);
			node.left = buildTree(points.Slice(0, median), depth+1, node);
			node.right = buildTree(points.Slice(median + 1), depth+1, node);
			return node;
		}

		private Point point;

		void insert(Point point)
		{
			this.point = point;
			var insertPosition = innerSearch(this.root, null);

			if(insertPosition == null) {
				this.root = new Node(point, 0, null);
				return;
			}

			var newNode = new Node(point, (insertPosition.dimension+1)%dimensions.Count, insertPosition);
			var dimension = dimensions[insertPosition.dimension];
			if(point.dimension(dimension) < insertPosition.obj.dimension(dimension)) {
				insertPosition.left = newNode;
			} else {
				insertPosition.right = newNode;
			}
		}

		Node innerSearch(Node node, Node parent)
		{
			if(node == null) return parent;

			var dimension = dimensions[node.dimension];
			if (point.dimension(dimension) < node.obj.dimension(dimension))
			{
				return innerSearch(node.left, node);
			}
			else
			{
				return innerSearch(node.right, node);
			}
		}

		Node nodeSearch(Node node)
		{
			if(node == null) return null;
			if(node.obj == point) return node;

			var dimension = dimensions[node.dimension];
			if (point.dimension(dimension) < node.obj.dimension(dimension))
			{
				return nodeSearch(node.left);//, node);
			}
			else
			{
				return nodeSearch(node.right);//, node);
			}
		}

		void remove(Point point)
		{
			this.point = point;
			var node = nodeSearch(root);
			if (node == null) return;

			removeNode(node);
		}

		void removeNode(Node node)
		{
			if (node.left == null && node.right == null)
			{
				if (node.parent == null)
				{
					root = null;
					return;
				}
				var pDimension = dimensions[node.parent.dimension];
				if (node.obj.dimension(pDimension) < node.parent.obj.dimension(pDimension)) {
					node.parent.left = null;
				} else {
					node.parent.right = null;
				}
				return;
			}

			Node nextNode;
			if (node.left != null) {
				nextNode = findMax(node.left, node.dimension);
			} else {
				nextNode = findMin(node.right, node.dimension);
			}
			var nextObj = nextNode.obj;
			removeNode(nextNode);
			node.obj = nextObj;
		}

		Node findMax(Node node, int dim)
		{
			if (node == null) return null;

			var dimension = dimensions[dim];
			if (node.dimension == dim) {
				if (node.right != null) return findMax(node.right, dim);
				return node;
			}

			var own = node.obj.dimension(dimension);
			var left = findMax(node.left, dim);
			var right = findMax(node.right, dim);
			var max = node;
			if (left != null && left.obj.dimension(dimension) > own) max = left;
			if (right != null && right.obj.dimension(dimension) > max.obj.dimension(dimension)) max = right;
			return max;
		}

		Node findMin(Node node, int dim)
		{
			if (node == null) return null;

			var dimension = dimensions[dim];
			if (node.dimension == dim) {
				if (node.left != null) return findMin(node.left, dim);
				return node;
			}

			var own = node.obj.dimension(dimension);
			var left = findMin(node.left, dim);
			var right = findMin(node.right, dim);
			var min = node;
			if (left != null && left.obj.dimension(dimension) < own) min = left;
			if (right != null && right.obj.dimension(dimension) < min.obj.dimension(dimension)) min = right;
			return min;
		}

		private BinaryHeap bestNodes;
		private int maxNodes;

		public List<PointAndDistance> nearest(Point point, int maxNodes, double maxDistance)
		{
			this.point = point;
			this.maxNodes = maxNodes;
			bestNodes = new BinaryHeap((HeapObject e) => { return -e._1; });
	
			if (maxDistance > 0)
			{
				for (var i = 0; i < maxNodes; i++)
				{
					bestNodes.push(new HeapObject(null, maxDistance));
				}
			}
			nearestSearch(root);

			var result = new List<PointAndDistance>();
			for (var i = 0; i < maxNodes; i++)
			{
				//if (bestNodes.content[i] == undefined) // TODO FIXME
				//	continue;
				if (bestNodes.content[i]._0 != null)
				{ //if(bestNodes.content[i][0]) 
					result.Add(new PointAndDistance(bestNodes.content[i]._0.obj, bestNodes.content[i]._1));
					//console.log("wo");
				}
			}
			return result;
		}

		void nearestSearch(Node node)
		{
			Node bestChild;
			var dimension = dimensions[node.dimension];
			var ownDistance = metric(point, node.obj);

			Point linearPoint = new Point();
			for (var i = 0; i < dimensions.Count; i++)
			{
				if (i == node.dimension)
				{
					linearPoint.setDimension(dimensions[i], point.dimension(dimensions[i]));
				}
				else
				{
					linearPoint.setDimension(dimensions[i], node.obj.dimension(dimensions[i]));
				}
			}
			var linearDistance = metric(linearPoint, node.obj);

			if (node.right == null && node.left == null)
			{
				if(bestNodes.size() < maxNodes || ownDistance < bestNodes.peek()._1)
				{
					saveNode(node, ownDistance);
				}
				return;
			}

			if (node.right == null)
			{
				bestChild = node.left;
			}
			else if (node.left == null)
			{
				bestChild = node.right;
			}
			else
			{
				if (point.dimension(dimension) < node.obj.dimension(dimension))
				{
					bestChild = node.left;
				}
				else
				{
					bestChild = node.right;
				}
			}

			nearestSearch(bestChild);

			if (bestNodes.size() < maxNodes || ownDistance < bestNodes.peek()._1)
			{
				saveNode(node, ownDistance);
			}

			if (bestNodes.size() < maxNodes || Math.Abs(linearDistance) < bestNodes.peek()._1)
			{
				Node otherChild;
				if (bestChild == node.left)
				{
					otherChild = node.right;
				}
				else
				{
					otherChild = node.left;
				}
				if (otherChild != null) nearestSearch(otherChild);
			}
		}

		void saveNode(Node node, double distance)
		{
			bestNodes.push(new HeapObject(node, distance));
			if (bestNodes.size() > maxNodes)
			{
				bestNodes.pop();
			}
		}

		double balanceFactor()
		{
			return height(root) / (Math.Log(count(root)) / Math.Log(2));
		}

		double height(Node node)
		{
			if (node == null) return 0;
			return Math.Max(height(node.left), height(node.right)) + 1;
		}

		int count(Node node)
		{
			if (node == null) return 0;
			return count(node.left) + count(node.right) + 1;
		}
	}
}
