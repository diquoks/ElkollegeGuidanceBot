using System.Data;
using System.Runtime.CompilerServices;
using System.Text.Json;
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
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            PRAGMA journal_mode = WAL;

            CREATE TABLE IF NOT EXISTS states (
                user_id       INTEGER NOT NULL PRIMARY KEY,
                last_callback TEXT NOT NULL,
                type          INTEGER NOT NULL,
                data          TEXT NOT NULL CHECK(json_valid(data))
            );

            CREATE TABLE IF NOT EXISTS results (
                id             INTEGER PRIMARY KEY,
                user_id        INTEGER NOT NULL,
                full_name      TEXT NOT NULL,
                phone_number   TEXT NOT NULL,
                user_type      TEXT NOT NULL,
                institution    TEXT NOT NULL,
                current_course TEXT NOT NULL,
                possible_type  TEXT NOT NULL,
                timestamp      INTEGER NOT NULL
            );
            """;

        command.ExecuteNonQuery();
    }

    public async Task<State> UpsertStateAsync(State state, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_sqliteConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO states (user_id, last_callback, type, data)
            VALUES ($user_id, $last_callback, $type, $data)
            ON CONFLICT(user_id) DO UPDATE SET
                type = excluded.type,
                data = excluded.data
            RETURNING *;
            """;
        command.Parameters.AddRange([
            new SqliteParameter("$user_id", state.UserId),
            new SqliteParameter("$last_callback", state.LastCallback),
            new SqliteParameter("$type", JsonSerializer.Serialize(state.Type)),
            new SqliteParameter("$data", JsonSerializer.Serialize(state.Data))
        ]);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return (await ReadStatesAsync(reader, cancellationToken).ToArrayAsync(cancellationToken))
            .Single();
    }

    public async Task<State> GetUserStateAsync(long userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_sqliteConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT * FROM states WHERE user_id = $user_id";
        command.Parameters.Add(new SqliteParameter("$user_id", userId));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return reader.HasRows
            ? (await ReadStatesAsync(reader, cancellationToken).ToArrayAsync(cancellationToken)).Single()
            : await UpsertStateAsync(new State { UserId = userId }, cancellationToken);
    }

    private static async IAsyncEnumerable<State> ReadStatesAsync(
        SqliteDataReader reader,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        while (await reader.ReadAsync(cancellationToken))
            yield return new State
            {
                UserId = reader.GetInt64("user_id"),
                LastCallback = reader.GetString("last_callback"),
                Type = (StateType)reader.GetInt32("type"),
                Data = JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString("data")) ??
                       new Dictionary<string, object>()
            };
    }

    public async Task InsertResultAsync(Result result, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_sqliteConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO results (user_id, full_name, phone_number, user_type, institution, current_course, possible_type, timestamp)
            VALUES ($user_id, $full_name, $phone_number, $user_type, $institution, $current_course, $possible_type, $timestamp);
            """;
        command.Parameters.AddRange([
            new SqliteParameter("$user_id", result.UserId),
            new SqliteParameter("$full_name", result.FullName) { Size = 100 },
            new SqliteParameter("$phone_number", result.PhoneNumber) { Size = 30 },
            new SqliteParameter("$user_type", result.UserType),
            new SqliteParameter("$institution", result.Institution) { Size = 100 },
            new SqliteParameter("$current_course", result.CurrentCourse) { Size = 100 },
            new SqliteParameter("$possible_type", result.PossibleType) { Size = 100 },
            new SqliteParameter("$timestamp", result.Timestamp.ToUnixTimeSeconds())
        ]);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Result[]> GetAllResultsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_sqliteConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT * FROM results";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await ReadResultsAsync(reader, cancellationToken).ToArrayAsync(cancellationToken);
    }

    private static async IAsyncEnumerable<Result> ReadResultsAsync(
        SqliteDataReader reader,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        while (await reader.ReadAsync(cancellationToken))
            yield return new Result
            {
                Id = reader.GetInt32("id"),
                UserId = reader.GetInt64("user_id"),
                FullName = reader.GetString("full_name"),
                PhoneNumber = reader.GetString("phone_number"),
                UserType = reader.GetString("user_type"),
                Institution = reader.GetString("institution"),
                CurrentCourse = reader.GetString("current_course"),
                PossibleType = reader.GetString("possible_type"),
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("timestamp"))
            };
    }
}
