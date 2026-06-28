namespace SnowflakeGenerators;

public interface ISnowflakeGenerator
{
    long Generate(int processId);
    DateTime GetDateCreation(long snowflakeId);
}
