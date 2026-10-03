namespace SnowflakeGenerators;

public class SnowflakeGenerator(DateTime epoch)
{
    private int _timestampShift;
    private int _currentFieldShifts;
    
    private int _sequenceBits = 12;
    private long _sequence;
    
    private readonly long _epoch = (long) epoch.Subtract(DateTime.UnixEpoch).TotalMilliseconds;
    private long _lastTimestamp = -1;
    
    private readonly Lock _lock = new();
    private readonly Dictionary<string, int> _bitAllocation = [ ];
    private readonly Dictionary<string, int> _fieldShifts = [ ];
    
    private long MaxSequence => -1L ^ (-1L << _sequenceBits);
    
    public SnowflakeGenerator AddFieldBits(string name, int bits)
    {
        if (_timestampShift == 0)
        {
            _timestampShift = _sequenceBits;
        }
        
        _currentFieldShifts += bits;
        _fieldShifts[name] = _currentFieldShifts;
        
        _timestampShift += bits;
        _bitAllocation[name] = bits;
        return this;
    }
    
    public SnowflakeGenerator SetSequenceBits(int bits)
    {
        _sequenceBits = bits;
        return this;
    }

    public DateTime GetDateCreation(long snowflakeId)
    {
        long timestamp = (snowflakeId >> _timestampShift) + _epoch;
        var timeZone = TimeSpan.FromHours(8);
        var absoluteTime = DateTimeOffset
            .FromUnixTimeMilliseconds(timestamp)
            .ToOffset(timeZone);
        
        return absoluteTime.DateTime;
    }
    
    public long GetFieldValue(long id, string name)
    {
        if (!_bitAllocation.TryGetValue(name, out var bits) || !_fieldShifts.TryGetValue(name, out var shift))
        {
            throw new InvalidOperationException($"Field {name} not found.");
        }
        
        return (id >> shift) & (1L << bits) - 1;
    }

    
    public long Generate(Dictionary<string, int> fields)
    {
        if (fields.Count == 0)
        {
            throw new InvalidOperationException("No fields specified.");
        }
        
        foreach (var field in fields.Reverse())
        {
            if (!_bitAllocation.TryGetValue(field.Key, out var bits))
            {
                continue;
            }
            
            long max = -1L - (-1L << bits);
            
            if (field.Value < 0 || field.Value > max)
            {
                throw new InvalidOperationException($"Field must be between 0 and {max}");
            }
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
                _sequence++;

                if (_sequence > MaxSequence)
                {
                    timestamp = GetNextTimestamp(_lastTimestamp);
                    _sequence = 0;
                }
            }
            else
            {
                _sequence = 0;
            }

            _lastTimestamp = timestamp;
            
            long id = (timestamp - _epoch) << _timestampShift;

            id = _fieldShifts.Aggregate(id, (current, fieldShift) =>
                current | (uint) fields[fieldShift.Key] << fieldShift.Value);

            id |= _sequence;

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
