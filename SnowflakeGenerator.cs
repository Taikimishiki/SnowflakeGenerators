namespace SnowflakeGenerators;

public class SnowflakeGenerator : ISnowflakeGenerator
{
    private const int WORKER_ID_BITS = 5;
    private const int PROCESS_ID_BITS = 5;
    private const int SEQUENCE_BITS = 12;
    
    private const int MAX_WORKER_ID = -1 ^ (-1 << WORKER_ID_BITS);
    private const int MAX_PROCESS_ID = -1 ^ (-1 << PROCESS_ID_BITS);
    private const int MAX_SEQUENCE = -1 ^ (-1 << SEQUENCE_BITS);

    private const int WORKER_ID_SHIFT = SEQUENCE_BITS;
    private const int PROCESS_ID_SHIFT = WORKER_ID_BITS + SEQUENCE_BITS;
    private const int TIMESTAMP_SHIFT = WORKER_ID_BITS + PROCESS_ID_BITS + SEQUENCE_BITS;

    private readonly int _workerId;
    private int _sequence;
    
    private readonly long _epoch;
    private long _lastTimestamp = -1;
    
    private readonly Lock _lock = new();

    public SnowflakeGenerator(int workerId, DateTime epoch)
    {
        _epoch = (long) epoch.Subtract(DateTime.UnixEpoch).TotalMilliseconds;
        _workerId = workerId;

        if (_workerId is < 0 or > MAX_WORKER_ID)
        {
            throw new InvalidOperationException($"Worker ID must be between 0 and {MAX_WORKER_ID}");
        }
    }

    public DateTime GetDateCreation(long snowflakeId)
    {
        long timestamp = (snowflakeId >> TIMESTAMP_SHIFT) + _epoch;
        var timeZone = TimeSpan.FromHours(8);
        var absoluteTime = DateTimeOffset
            .FromUnixTimeMilliseconds(timestamp)
            .ToOffset(timeZone);
        
        return absoluteTime.DateTime;
    }

    public static int GetWorkerId(long id) => (int) (id >> SEQUENCE_BITS) & MAX_WORKER_ID;
    public static int GetProcessId(long id) => (int) (id >> (WORKER_ID_BITS + SEQUENCE_BITS)) & MAX_PROCESS_ID;
    
    public long Generate(int processId)
    {
        if (processId is < 0 or > MAX_PROCESS_ID)
        {
            throw new InvalidOperationException($"Process ID must be between 0 and {MAX_PROCESS_ID}");
        }
        
        lock (_lock)
        {
            long timestamp = CurrentTimestamp();

            if (timestamp < _lastTimestamp)
            {
                throw new InvalidOperationException("Refusing to generate ID.");
            }

            if (timestamp == _lastTimestamp)
            {
                _sequence = (_sequence + 1) & MAX_SEQUENCE;

                if (_sequence == 0)
                {
                    timestamp = GetNextTimestamp(_lastTimestamp);
                }
            }
            else
            {
                _sequence = 0;
            }

            _lastTimestamp = timestamp;

            long id = ((timestamp - _epoch) << TIMESTAMP_SHIFT)
                | ((uint) _workerId << WORKER_ID_SHIFT)
                | ((uint) processId << PROCESS_ID_SHIFT)
                | (uint) _sequence;

            return id;
        }
    }

    private static long GetNextTimestamp(long lastTimestamp)
    {
        var timestamp = CurrentTimestamp();
        
        while (timestamp <= lastTimestamp)
        {
            timestamp = CurrentTimestamp();
        }
        
        return timestamp;
    }

    private static long CurrentTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
