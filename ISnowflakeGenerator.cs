namespace SnowflakeGenerators;

public interface ISnowflakeGenerator
{
    DateTime GetDateCreation(long snowflakeId);
}
