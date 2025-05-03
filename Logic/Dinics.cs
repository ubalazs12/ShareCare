using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using ShareCare.Models;

namespace ShareCare.Logic
{
    public class Dinics
    {
        const long INF = long.MaxValue / 2;

        public string[] VertexLabels { get; set; }

        public int[] Level { get; set; }

        public int NumberOfVertexes { get; set; }
        public int Source { get; set; }
        public int Sink { get; set; }

        public double MaxFlow { get; set; }
        public double MinCost { get; set; }

        public bool[] MinCut { get; set; }
        public List<Edge>[] Graph { get; set; }
        public List<Edge> Edges { get; set; }

        public HashSet<(int, int)> Visited;

        public bool Solved { get; set; }

        public Dinics(int numberOfVertexes, string[] vertexLabels, HashSet<(int, int)> visited)
        {
            NumberOfVertexes = numberOfVertexes;
            InitializeGraph();
            AssignLabelsToVertices(vertexLabels);
            MinCut = new bool[numberOfVertexes];
            Level = new int[numberOfVertexes];
            Edges = new List<Edge>();
            Visited = visited;
        }

        private void InitializeGraph()
        {
            Graph = new List<Edge>[NumberOfVertexes];
            for (int i = 0; i < NumberOfVertexes; i++)
            {
                Graph[i] = new List<Edge>();
            }
        }

        private void AssignLabelsToVertices(string[] vertexLabels)
        {
            if (vertexLabels != null)
            {
                VertexLabels = vertexLabels;
            }
        }

        public void AddEdges(List<Edge> edges)
        {
            if (edges != null)
            {
                foreach (Edge e in edges)
                {
                    AddEdge(e.From, e.To, e.Capacity, e.Debts);
                }
            }
        }

        public void AddEdge(int from, int to, double capacity, HashSet<Debt> Debts)
        {
            Edge e1 = new Edge(from, to, capacity);
            Edge e2 = new Edge(to, from, 0);
            e1.Residual = e2;
            e2.Residual = e1;
            e1.Debts = Debts;
            e2.Debts = Debts;
            Graph[from].Add(e1);
            Graph[to].Add(e2);
            Edges.Add(e1);
        }

        public List<Edge>[] GetSolvedGraph()
        {
            execute();
            return Graph;
        }

        public double GetSolvedMaxFlow()
        {
            execute();
            return MaxFlow;
        }

        private void execute()
        {
            if (Solved) return;
            Solved = true;
            solve();
        }

        public void Recompute()
        {
            for (int i = 0; i < NumberOfVertexes; i++)
            {
                for (int j = 0; j < NumberOfVertexes; j++)
                {
                    if (i != j && Edges.FirstOrDefault(e => (e.From == i && e.To == j)) == null)
                    {
                        AddEdge(i, j, 0, []);
                    }
                }
            }
            Solved = false;
        }

        public void solve()
        {
            int[] next = new int[NumberOfVertexes];

            while (BFS())
            {
                Array.Fill(next, 0);

                for (double f = DFS(Source, next, INF); !f.Equals(0); f = DFS(Source, next, INF))
                {
                    MaxFlow += f;
                }
            }

            for (int i = 0; i < NumberOfVertexes; i++)
            {
                if (Level[i] != -1)
                {
                    MinCut[i] = true;
                }
            }
        }

        private bool BFS()
        {
            Array.Fill(Level, -1);
            Level[Source] = 0;
            Queue<int> q = new Queue<int>(NumberOfVertexes);
            q.Enqueue(Source);

            while (q.Count > 0)
            {
                int node = q.Dequeue();
                foreach (Edge edge in Graph[node])
                {
                    double cap = edge.remainingCapacity();
                    if (cap > 0 && Level[edge.To] == -1)
                    {
                        Level[edge.To] = Level[node] + 1;
                        q.Enqueue(edge.To);
                    }
                }
            }
            return Level[Sink] != -1;
        }

        private double DFS(int at, int[] next, double flow)
        {
            if (at == Sink) return flow;
            int numEdges = Graph[at].Count;

            for (; next[at] < numEdges; next[at]++)
            {
                Edge edge = Graph[at][next[at]];
                double cap = edge.remainingCapacity();
                if (cap > 0 && Level[edge.To] == Level[at] + 1)
                {
                    double bottleNeck = DFS(edge.To, next, Math.Min(flow, cap));
                    if (bottleNeck > 0)
                    {
                        edge.augment(bottleNeck);
                        return bottleNeck;
                    }
                }
            }
            return 0;
        }

        public void AddToVisited(Edge edge)
        {
            Visited.Add((edge.From, edge.To));
        }
        public Edge GetNonVisitedEdge()
        {
            return Edges.LastOrDefault(e => !Visited.Contains((e.From, e.To)));
        }

        public string PrintEdges()
        {
            string res = "";
            foreach (Edge edge in Edges)
            {
                Console.WriteLine(VertexLabels[edge.From] + " ----" + edge.Capacity + "----> " + VertexLabels[edge.To]);
                res += VertexLabels[edge.From] + " ----" + edge.Capacity + "----> " + VertexLabels[edge.To] + Environment.NewLine;
            }
            return res;
        }
    }
}
