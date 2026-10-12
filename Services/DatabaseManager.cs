using Dapper;
using ElkollegeGuidanceBot.Models.Database;
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
                DataJson TEXT    NOT NULL CHECK(json_valid(DataJson))
            );

            CREATE TABLE IF NOT EXISTS results (
                Id            INTEGER PRIMARY KEY,
                UserId        INTEGER NOT NULL,
                FullName      TEXT,
                PhoneNumber   TEXT,
                UserStatus    TEXT,
                Institution   TEXT,
                CurrentCourse TEXT,
                PossibleType  TEXT    NOT NULL,
                TypeRatings   TEXT    NOT NULL,
                Created       TEXT    NOT NULL
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
                    UserStatus,
                    Institution,
                    CurrentCourse,
                    PossibleType,
                    TypeRatings,
                    Created
                )
                VALUES (
                    @UserId,
                    SUBSTR(@FullName,      1, 100),
                    SUBSTR(@PhoneNumber,   1, 50 ),
                    SUBSTR(@UserStatus,    1, 100),
                    SUBSTR(@Institution,   1, 100),
                    SUBSTR(@CurrentCourse, 1, 100),
                    @PossibleType,
                    @TypeRatings,
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
