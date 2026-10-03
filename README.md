# Snowflake Generators
This snowflake generator is based on [Unique ID Generators](https://medium.com/prepster/unique-id-generators-4e3f898d0999) by [Girish](https://medium.com/@girishkr).

```cs
var generator = new SnowflakeGenerator(new DateTime(2018, 1, 1))
    .AddFieldBits("Worker", 10);

var id = generator.Generate(new Dictionary<string, int>
{
    { "Worker", 0 }
});

var id2 = generator.Generate(new Dictionary<string, int>
{
    { "Worker", 1 }
});

long oldId = 1158692301086655488;

Console.WriteLine($"{oldId}'s Date: {generator.GetDateCreation(oldId)}");
Console.WriteLine($"{oldId}'s Worker: {generator.GetFieldValue(oldId, "Worker")}");
Console.WriteLine($"Worker 0: {id}");
Console.WriteLine($"Worker 1: {id2}");
```
## Output
```
1158692301086655488's Date: 10/3/2026 5:09:32 PM
1158692301086655488's Worker: 1
Worker 0: 1158694677084372992
Worker 1: 1158694677092762624
```