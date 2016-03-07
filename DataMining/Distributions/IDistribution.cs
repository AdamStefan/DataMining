namespace DataMining.Distributions
{
    public interface IDistribution
    {
        double GetLogProbability(double value);
        double GetExpectation();
    }
}