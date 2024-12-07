using MessageProcessingAnomalyDetection.Statistics;

namespace MessageProcessingAnomalyDetection.Interfaces
{
    public interface IDatabaseMongoDB
    {
        public Task Insert(ServerStatistics statistics);
        public Task<bool> Update(IServerStatistics statistics);
        public Task<bool> Delete(IServerStatistics statistics, bool deleteMany);
        public Task<IEnumerable<IServerStatistics>> GetAllStatistics();
    }
}
