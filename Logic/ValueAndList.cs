using ShareCare.Models;

namespace ShareCare.Logic
{
    public class ValueAndList
    {
        public double Value { get; set; }

        public HashSet<Debt> Debts { get; set; } = [];
    }
}
