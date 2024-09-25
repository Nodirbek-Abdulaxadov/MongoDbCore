using System.Diagnostics;

public static class Program
{
    public static async Task Main()
    {
        List<double> AddResults = new();
        List<double> GetResults = new();
        List<double> UpdateResults = new();
        List<double> DeleteResults = new();

        Stopwatch stopwatch = new();

        for (int i = 0; i < 1_000; i++)
        {
            CRUD_Postgres_EFCore_Async cRUD_Postgres = new();
            await cRUD_Postgres.Setup();
            stopwatch.Start();
            await cRUD_Postgres.AddBook();
            stopwatch.Stop();
            AddResults.Add(stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Restart();
            await cRUD_Postgres.RetrieveBooks();
            stopwatch.Stop();
            GetResults.Add(stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Restart();
            await cRUD_Postgres.UpdateBook();
            stopwatch.Stop();
            UpdateResults.Add(stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Restart();
            await cRUD_Postgres.DeleteBook();
            stopwatch.Stop();
            DeleteResults.Add(stopwatch.Elapsed.TotalMilliseconds);
        }

        List<double> AddResults2 = new();
        List<double> GetResults2 = new();
        List<double> UpdateResults2 = new();
        List<double> DeleteResults2 = new();
        Console.WriteLine();
        Console.WriteLine();
        for (int i = 0; i < 1_000; i++)
        {
            CRUD_MongoDbCore_Async cRUD_MongoDb = new();
            await cRUD_MongoDb.Setup();
            stopwatch.Start();
            await cRUD_MongoDb.AddBook();
            stopwatch.Stop();
            AddResults2.Add(stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Restart();
            await cRUD_MongoDb.RetrieveBooks();
            stopwatch.Stop();
            GetResults2.Add(stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Restart();
            await cRUD_MongoDb.UpdateBook();
            stopwatch.Stop();
            UpdateResults2.Add(stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Restart();
            await cRUD_MongoDb.DeleteBook();
            stopwatch.Stop();
            DeleteResults2.Add(stopwatch.Elapsed.TotalMilliseconds);
        }

        string summary = $"""
        MethodName                |  Min      | Max         |  Average
        --------------------------------------------------------------
        AddBooks_Postgresql       | {AddResults.Min().ToString("F4")}   | {AddResults.Max().ToString("F4")}   | {AddResults.Average().ToString("F4")}
        AddBooks_MongoDbCore      | {AddResults2.Min().ToString("F4")}   | {AddResults2.Max().ToString("F4")}   | {AddResults2.Average().ToString("F4")}
        
        RetrieveBooks_Postgresql  | {GetResults.Min().ToString("F4")}   | {GetResults.Max().ToString("F4")}   | {GetResults.Average().ToString("F4")}
        RetrieveBooks_MongoDbCore | {GetResults2.Min().ToString("F4")}   | {GetResults2.Max().ToString("F4")}   | {GetResults2.Average().ToString("F4")}        
        
        UpdateBooks_Postgresql    | {UpdateResults.Min().ToString("F4")}   | {UpdateResults.Max().ToString("F4")}   | {UpdateResults.Average().ToString("F4")}
        UpdateBooks_MongoDbCore   | {UpdateResults2.Min().ToString("F4")}   | {UpdateResults2.Max().ToString("F4")}   | {UpdateResults2.Average().ToString("F4")}        
        
        DeleteBooks_Postgresql    | {DeleteResults.Min().ToString("F4")}   | {DeleteResults.Max().ToString("F4")}   | {DeleteResults.Average().ToString("F4")}
        DeleteBooks_MongoDbCore   | {DeleteResults2.Min().ToString("F4")}   | {DeleteResults2.Max().ToString("F4")}   | {DeleteResults2.Average().ToString("F4")}
        """;
        Console.WriteLine(summary);


        /*// Run the MongoDB benchmark
        var mongoSummary = BenchmarkRunner.Run<CRUD_MongoDbCore_Async>();

        // Run the PostgreSQL benchmark
        var postgresSummary = BenchmarkRunner.Run<CRUD_Postgres_EFCore_Async>();*/
    }
}

/* Custom test results time:
    MethodName                |   Min      |  Max         |   Average
    ----------------------------------------------------------------------
    AddBooks_Postgresql       |  4.3099 ms |  147.3738 ms |  33.1545 ms
    AddBooks_MongoDbCore      |  4.3138 ms |   69.4084 ms |   6.7356 ms
    ----------------------------------------------------------------------
    RetrieveBooks_Postgresql  |  2.5040 ms | 1385.3620 ms | 260.0881 ms
    RetrieveBooks_MongoDbCore |  4.2880 ms |  926.6556 ms | 240.6899 ms
    ----------------------------------------------------------------------
    UpdateBooks_Postgresql    |  2.6386 ms |  417.3581 ms |  56.4922 ms
    UpdateBooks_MongoDbCore   |  4.0518 ms |  135.4579 ms |   6.2540 ms
    ----------------------------------------------------------------------
    DeleteBooks_Postgresql    |  2.9195 ms |  145.5029 ms |  31.5612 ms
    DeleteBooks_MongoDbCore   |  2.6309 ms |   66.5493 ms |   4.6263 ms
 */

/* Benchmark Results:
| Method                 | Mean         | Error       | StdDev       | Median       | Allocated    |
|----------------------- |-------------:|------------:|-------------:|-------------:|-------------:|
| AddBook_Mongo          |    0.2456 ms |  0.00482 ms |   0.00996 ms |    0.2461 ms |     23.46 KB |
| AddBooks_Postgres      |     9.005 ms |   1.1771 ms |    3.4706 ms |     8.675 ms |  11199.62 KB |

| RetrieveBooks_Mongo    |  500.6659 ms |  9.62977 ms |  19.00823 ms | 498.6263 ms  | 152272.41 KB |
| RetrieveBooks_Postgres |    25.448 ms |   0.4946 ms |    0.5080 ms |   25.215 ms  |   9248.93 KB |

| UpdateBook_Mongo       |    4.4934 ms |  0.80384 ms |   2.35752 ms |   2.7977 ms  |   2006.87 KB |
| UpdateBook_Postgres    |     1.201 ms |   0.0531 ms |    0.1514 ms |    1.243 ms  |    637.21 KB |

| DeleteBook_Mongo       |    1.1366 ms |  0.02229 ms |   0.04186 ms |   1.1388 ms  |    169.36 KB |
| DeleteBook_Postgres    |           NA |          NA |           NA |          NA  |           NA | (was exception)
 */