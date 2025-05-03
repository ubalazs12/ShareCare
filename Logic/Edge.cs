using ShareCare.Models;

namespace ShareCare.Logic
{
    public class Edge
    {
        public int From { get; set; }
        public int To { get; set; }
        public Edge? Residual { get; set; }
        public double Flow { get; set; }
        public double Cost { get; set; }
        public double Capacity { get; set; }
        public int OriginalCost { get; set; }

        public HashSet<Debt> Debts { get; set; } = [];

        public Edge(int from, int to, double capacity, double cost = 0)
        {  From = from;
            To = to;
            Capacity = capacity;
            Cost = cost;
        }

        public bool isResidual()
        {
            return Capacity.Equals(0);
        }

        public double remainingCapacity()
        {
            return Capacity - Flow;
        }

        public void augment(double bottleNeck)
        {
            Flow += bottleNeck;
            if (Residual != null)
            {
                Residual.Flow -= bottleNeck;
            }
        }
    }
}
