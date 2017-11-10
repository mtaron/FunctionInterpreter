using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionInterpreter.Test
{
    [TestClass]
    public class GraphTests
    {
        [TestMethod]
        public void TopologicalSort_EmptyGraph()
        {
            Graph<int> graph = new Graph<int>();

            graph.TopologicalSort().Should().BeEmpty();
        }

        [TestMethod]
        public void TopologicalSort_SingleNode()
        {
            var graph = new Graph<string>();
            string node = "";
            graph.AddNode(node);

            graph.TopologicalSort().Should().ContainSingle(n => n == node);
        }

        [TestMethod]
        public void TopologicalSort_SingleNodeLoop_ReturnsNull()
        {
            var graph = new Graph<string>();
            string node = string.Empty;
            graph.AddNode(node);
            graph.AddEdge(node, node);

            graph.TopologicalSort().Should().BeNull();
        }

        [TestMethod]
        public void TopologicalSort_DisconnectedNodes()
        {
            var graph = new Graph<int>();
            graph.AddNode(0);
            graph.AddNode(1);
            graph.AddNode(2);
            graph.AddNode(3);

            int[] sorted = graph.TopologicalSort().ToArray();

            sorted.Length.Should().Be(4);
            sorted.Should().Contain(new int[] { 0, 1, 2, 3 });
        }

        [TestMethod]
        public void TopologicalSort_List()
        {
            var graph = new Graph<int>();
            graph.AddNode(3);
            graph.AddNode(2);
            graph.AddNode(1);
            graph.AddNode(0);

            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);

            graph.TopologicalSort().Should().ContainInOrder(new int[] { 0, 1, 2, 3 });
        }

        [TestMethod]
        public void TopologicalSort_ComplexGraphNoLoop()
        {
            var graph = new Graph<int>();
            for (int node = 0; node <= 5; node++)
            {
                graph.AddNode(node);
            }

            graph.AddEdge(0, 1);
            graph.AddEdge(0, 2);
            graph.AddEdge(0, 4);

            graph.AddEdge(1, 2);
            graph.AddEdge(1, 3);
            graph.AddEdge(1, 4);

            graph.AddEdge(2, 3);
            graph.AddEdge(2, 4);
            graph.AddEdge(2, 5);

            graph.AddEdge(3, 4);
            graph.AddEdge(3, 5);

            graph.AddEdge(4, 5);

            graph.TopologicalSort().Should().BeInAscendingOrder();
        }

        [TestMethod]
        public void TopologicalSort_ComplexGraphWithLoop()
        {
            var graph = new Graph<int>();
            for (int node = 1; node < 10; node++)
            {
                graph.AddNode(node);
            }

            graph.AddEdge(1, 2);
            graph.AddEdge(1, 3);
            graph.AddEdge(1, 4);
            graph.AddEdge(2, 4);
            graph.AddEdge(2, 5);
            graph.AddEdge(3, 9);
            graph.AddEdge(4, 6);
            graph.AddEdge(4, 7);
            graph.AddEdge(6, 3);
            graph.AddEdge(7, 8);
            graph.AddEdge(7, 9);
            graph.AddEdge(8, 3);
            graph.AddEdge(8, 9);
            graph.AddEdge(9, 1);

            graph.TopologicalSort().Should().BeNull();
        }

        [TestMethod]
        public void GetTargetClosure_Null_Throws()
        {
            var graph = new Graph<string>();
            Action action = () => graph.GetClosure(null).First();

            action.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void GetClosure_EmptyGraph()
        {
            var graph = new Graph<string>();

            graph.GetClosure(string.Empty).Should().BeEmpty();
        }

        [TestMethod]
        public void GetClosure_SingleNode()
        {
            var graph = new Graph<string>();
            string node = "x";
            graph.AddNode(node);

            graph.GetClosure(node).Single().Should().Be(node);
        }

        [DataTestMethod]
        [DataRow(3, new int[] { 3, 4, 5, 6, 7, 8 })]
        [DataRow(6, new int[] { 6, 8 })]
        [DataRow(2, new int[] { 2, 3, 4, 5, 6, 7, 8 })]
        public void GetClosure_Graph(int startNode, int[] expectedNodes)
        {
            Graph<int> graph = CreateGraph();

            graph.GetClosure(startNode).Should().BeEquivalentTo(expectedNodes);
        }

        private static Graph<int> CreateGraph()
        {
            var graph = new Graph<int>();
            graph.AddNode(1);
            graph.AddNode(2);
            graph.AddNode(3);
            graph.AddNode(4);
            graph.AddNode(5);
            graph.AddNode(6);
            graph.AddNode(7);
            graph.AddNode(8);

            graph.AddEdge(1, 3);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 4);
            graph.AddEdge(3, 5);
            graph.AddEdge(4, 5);
            graph.AddEdge(5, 6);
            graph.AddEdge(5, 7);
            graph.AddEdge(5, 8);
            graph.AddEdge(6, 8);
            graph.AddEdge(7, 8);

            return graph;
        }
    }
}
