using Dapper;
using ElkollegeGuidanceBot.Models;
using Microsoft.Data.Sqlite;

namespace ElkollegeGuidanceBot.Services;

public class DatabaseManager
{
    private readonly string _sqliteConnectionString;

    public DatabaseManager()
    {
        _sqliteConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = "Application.db",
            DefaultTimeout = 5,
            ForeignKeys = true
        }.ToString();

        using var connection = new SqliteConnection(_sqliteConnectionString);

        connection.Query(
            """
            PRAGMA journal_mode = WAL;

            CREATE TABLE IF NOT EXISTS states (
                UserId   INTEGER NOT NULL PRIMARY KEY,
                Type     INTEGER NOT NULL,
                DataJson TEXT NOT NULL CHECK(json_valid(DataJson))
            );

            CREATE TABLE IF NOT EXISTS results (
                Id            INTEGER PRIMARY KEY,
                UserId        INTEGER NOT NULL,
                FullName      TEXT NOT NULL,
                PhoneNumber   TEXT NOT NULL,
                UserType      TEXT NOT NULL,
                Institution   TEXT NOT NULL,
                CurrentCourse TEXT NOT NULL,
                PossibleType  TEXT NOT NULL,
                Created       TEXT NOT NULL
            );
            """
        );
    }

    public async Task<State> UpsertStateAsync(State state, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_sqliteConnectionString);

        return await connection.QuerySingleAsync<State>(
            new CommandDefinition(
                """
                INSERT INTO states (UserId, Type, DataJson)
                VALUES (@UserId, @Type, @DataJson)
                ON CONFLICT(UserId) DO UPDATE SET
                    Type     = excluded.Type,
                    DataJson = excluded.DataJson
                RETURNING *;
                """,
                state,
                cancellationToken: cancellationToken
            )
        );
    }

    public async Task<State> GetUserStateAsync(long userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_sqliteConnectionString);

        var state = await connection.QuerySingleOrDefaultAsync<State>(
            new CommandDefinition(
                "SELECT * FROM states WHERE UserId = @UserId",
                new { UserId = userId },
                cancellationToken: cancellationToken
            )
        );

        return state ?? await UpsertStateAsync(new State { UserId = userId }, cancellationToken);
    }

    public async Task InsertResultAsync(Result result, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_sqliteConnectionString);

        await connection.QueryAsync(
            new CommandDefinition(
                """
                INSERT INTO results (
                    UserId,
                    FullName,
                    PhoneNumber,
                    UserType,
                    Institution,
                    CurrentCourse,
                    PossibleType,
                    Created
                )
                VALUES (
                    @UserId,
                    SUBSTR(@FullName, 1, 100),
                    SUBSTR(@PhoneNumber, 1, 50),
                    SUBSTR(@UserType, 1, 50),
                    SUBSTR(@Institution, 1, 100),
                    SUBSTR(@CurrentCourse, 1, 100),
                    SUBSTR(@PossibleType, 1, 100),
                    @Created
                );
                """,
                result,
                cancellationToken: cancellationToken
            )
        );
    }

    public async Task<Result[]> GetAllResultsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_sqliteConnectionString);

        await using var gridReader = await connection.QueryMultipleAsync(
            new CommandDefinition(
                "SELECT * FROM results",
                cancellationToken: cancellationToken
            )
        );

        return [.. await gridReader.ReadAsync<Result>()];
    }
}
