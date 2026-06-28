# Snowflake Generators
This snowflake generator is based on [Unique ID Generators](https://medium.com/prepster/unique-id-generators-4e3f898d0999) by [Girish](https://medium.com/@girishkr).

```cs
var snowflake = new SnowflakeGenerator(workerId: 1, epoch: new DateTime(2018, 1, 1));

var id = snowflake.Generate(processId: 1);
var newIdCreationDate = snowflake.GetDateCreation(id);
var oldIdCreationDate = snowflake.GetDateCreation(1123400248501342208);

Console.WriteLine($"Snowflake ID: {id}");
Console.WriteLine($"New Snowflake ID Date Creation: {newId}");
Console.WriteLine($"Old Snowflake ID Date Creation: {oldId}");
```
## Output
```
Snowflake ID: 1123404794023776256
New Snowflake ID Date Creation: 6/28/2026 8:09:34 AM
Old Snowflake ID Date Creation: 6/28/2026 7:51:31 AM
```