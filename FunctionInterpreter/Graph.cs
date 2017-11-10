using System;
using System.Collections.Generic;

namespace FunctionInterpreter
{
    internal sealed class Graph<T> where T : IEquatable<T>
    {
        private readonly Dictionary<T, List<T>> _adjacencyList = new Dictionary<T, List<T>>();

        public bool Contains(T node)
        {
            return _adjacencyList.ContainsKey(node);
        }

        public void AddNode(T node)
        {
            if (!_adjacencyList.ContainsKey(node))
            {
                _adjacencyList.Add(node, new List<T>());
            }
        }

        public void AddEdge(T source, T target)
        {
            AddNode(source);
            AddNode(target);

            _adjacencyList[source].Add(target);
        }

        public IEnumerable<T> GetClosure(T node)
        {
            if (!_adjacencyList.ContainsKey(node))
            {
                yield break;
            }

            var visitedNodes = new HashSet<T>();
            var stack = new Stack<T>();

            stack.Push(node);

            while (stack.Count != 0)
            {
                T current = stack.Pop();

                if (!visitedNodes.Add(current))
                {
                    continue;
                }

                yield return current;

                foreach (T adjacentNode in _adjacencyList[current])
                {
                    if (!visitedNodes.Contains(adjacentNode))
                    {
                        stack.Push(adjacentNode);
                    }
                }
            }
        }

        public IEnumerable<T> TopologicalSort()
        {
            var visitedNodes = new HashSet<T>();
            var finishedNodes = new Stack<T>(_adjacencyList.Count);

            foreach (T node in _adjacencyList.Keys)
            {
                if (visitedNodes.Contains(node) || finishedNodes.Contains(node))
                {
                    continue;
                }

                if (!Visit(node, visitedNodes, finishedNodes))
                {
                    return null;
                }
            }

            return finishedNodes;
        }

        private bool Visit(T node, HashSet<T> visitedNodes, Stack<T> finishedNodes)
        {
            if (visitedNodes.Contains(node))
            {
                // The graph is cyclic.
                return false;
            }

            visitedNodes.Add(node);

            foreach (T adjacentNode in _adjacencyList[node])
            {
                if (!finishedNodes.Contains(adjacentNode))
                {
                    if (!Visit(adjacentNode, visitedNodes, finishedNodes))
                    {
                        return false;
                    }
                }
            }

            finishedNodes.Push(node);
            return true;
        }
    }
}
