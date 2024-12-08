using Rabbit_MQ_Client_Library.Statistics;

namespace Rabbit_MQ_Client_Library.Interfaces
{
    public interface IDatabaseMongoDB
    {
        public Task Insert(ServerStatistics statistics);
        public Task<bool> Update(IServerStatistics statistics);
        public Task<bool> Delete(IServerStatistics statistics, bool deleteMany);
        public Task<IEnumerable<IServerStatistics>> GetAllStatistics();
    }
}