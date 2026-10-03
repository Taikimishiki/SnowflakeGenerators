namespace SnowflakeGenerators;

public interface ISnowflakeGenerator
{
    DateTime GetDateCreation(long snowflakeId);
    long Generate(Dictionary<string, int> fields);
}
