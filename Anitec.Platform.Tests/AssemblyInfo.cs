// The BDD scenarios share one API and one in-memory SQLite connection, which cannot run transactions in parallel.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
