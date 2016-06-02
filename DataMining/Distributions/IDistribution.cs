namespace DataMining.Distributions
{    
    public interface IDistribution
    {
        double GetLogProbability(double value);
        double GetProbability(double value);
        double GetExpectation();
    }
}