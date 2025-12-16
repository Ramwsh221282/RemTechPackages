using System.Data.Common;
using Dapper;
using Npgsql;
using Serilog;

namespace ParserSubscriber.SubscribtionContext;

public sealed class SubscriptionStorage(NpgSqlProvider npgSqlProvider, ILogger? logger)
{
    private ILogger? Logger { get; } = logger?.ForContext<SubscriptionStorage>();
    
    private string ConnectionString => npgSqlProvider.ConnectionString;
    private string SchemaName { get; set; } = string.Empty;

    public async Task<ParserSubscribtion?> GetSubscription(CancellationToken ct = default)
    {
        NpgsqlDataSourceBuilder builder = new(ConnectionString);
        await using var dataSource = builder.Build();
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        Logger?.Information("Schema name: {Schema}", SchemaName);
        var sql =
            $"""
             SELECT 
                 id as id, 
                 domain as domain, 
                 type as type, 
                 created as created
             FROM {SchemaName}.subscriptions
             """;
        
        CommandDefinition command = new(sql, cancellationToken: ct);
        await using DbDataReader reader = await connection.ExecuteReaderAsync(command);
        ParserSubscribtion? result = null;
        
        while (await reader.ReadAsync(ct))
        {
            Guid id = reader.GetGuid(reader.GetOrdinal("id"));
            string domain = reader.GetString(reader.GetOrdinal("domain"));
            string type = reader.GetString(reader.GetOrdinal("type"));
            DateTime created = reader.GetDateTime(reader.GetOrdinal("created"));
            result = new ParserSubscribtion(id, domain, type, created); 
        }
        
        return result;
    }
    
    public async Task InitializeSubscriptionStorage()
    {
        Logger?.Information("Initializing parsers subscription storage.");
        NpgsqlDataSourceBuilder builder = new(ConnectionString);
        await using var dataSource = builder.Build();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        Logger?.Information("Schema name: {Schema}", SchemaName);
        var schemaSql =
            $"""
             CREATE SCHEMA IF NOT EXISTS {SchemaName}
             """;
        var tableSql =
            $"""
             CREATE TABLE IF NOT EXISTS {SchemaName}.subscriptions
             (
                 id uuid primary key,
                 domain varchar(128) not null,
                 type varchar(128) not null,
                 created timestamptz not null
             )
             """;
        await connection.ExecuteAsync(schemaSql, transaction: transaction);
        await connection.ExecuteAsync(tableSql, transaction: transaction);
        await transaction.CommitAsync();
        Logger?.Information("Initialized parsers subscription storage.");
    }

    public async Task SaveSubscription(ParserSubscribtion subscribtion)
    {
        await InitializeSubscriptionStorage();
        Logger?.Information("Saving parser subscription.");
        Logger?.Information("Schema name: {Schema}", SchemaName);
        object parameters = new
        {
            id = subscribtion.Id,
            domain = subscribtion.Domain,
            type = subscribtion.Type,
            created = subscribtion.Subscribed
        };
        NpgsqlDataSourceBuilder builder = new(ConnectionString);
        await using var dataSource = builder.Build();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        var sql =
            $"""
             INSERT INTO {SchemaName}.subscriptions (id, domain, type, created)
             VALUES (@id, @domain, @type, @created) ON CONFLICT (id) DO NOTHING
             """;
        var affected = await connection.ExecuteAsync(sql, parameters, transaction);
        await transaction.CommitAsync();
        if (affected == 0) Logger?.Warning("Subscription already exists.");
        Logger?.Information(
            """ 
            Saved parser subscription.
            Id: {Id}
            Domain: {Domain}
            Type: {Type}
            """, subscribtion.Id, subscribtion.Domain, subscribtion.Type);
    }

    internal void SetSchema(string schema)
    {
        Logger?.Information("Setting schema name: {Schema}", schema);
        SchemaName = schema;
        Logger?.Information("Schema name: {Schema}", SchemaName);
    }
}