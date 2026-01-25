using Xunit;

namespace Test;

// This collection definition ensures that all tests in the "Redis Tests" collection
// run sequentially rather than in parallel, preventing race conditions with shared Redis state
[CollectionDefinition("Redis Tests", DisableParallelization = true)]
public class RedisTestsCollection
{
}
