using System;
using System.Data;
using System.Collections.Generic;
using System.Diagnostics;
using Axicultus.Util;
using DbCommand = System.Data.SQLite.SQLiteCommand;
using DbCommandBuilder = System.Data.SQLite.SQLiteCommandBuilder;
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataAdapter = System.Data.SQLite.SQLiteDataAdapter;
using DbDataReader = System.Data.SQLite.SQLiteDataReader;
using DbTransaction = System.Data.SQLite.SQLiteTransaction;


namespace Axicultus.SQLite;

	class DbController
	{
		internal static string ConnectionStringForFile(string path, bool readOnly = false) =>
			new System.Data.SQLite.SQLiteConnectionStringBuilder { DataSource = path, ReadOnly = readOnly, FailIfMissing = readOnly }.ConnectionString;

		internal static string QuoteIdentifier(string identifier)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
			if (identifier.Contains('\0'))
				throw new ArgumentException("SQLite identifiers cannot contain NUL.", nameof(identifier));
			return "\"" + identifier.Replace("\"", "\"\"") + "\"";
		}

		internal static List<string> GetColumnNames(DbConnection connection, string table)
		{
			using var command = CreateCommand(connection, "SELECT name FROM pragma_table_info(@table) ORDER BY cid", ("@table", table));
			using var reader = command.ExecuteReader();
			var columns = new List<string>();
			while (reader.Read())
				columns.Add(reader.GetString(0));
			return columns;
		}

		internal static DbCommand CreateCommand(DbConnection connection, string sql,
			params (string Name, object Value)[] parameters)
		{
			var command = new DbCommand(sql, connection);
			try
			{
				foreach (var (name, value) in parameters)
					command.Parameters.AddWithValue(name, value ?? DBNull.Value);
				return command;
			}
			catch
			{
				command.Dispose();
				throw;
			}
		}

		public static void CreateField(ref DbConnection connection, string tableName, string fieldName, int fieldType)
		{
			CreateField(ref connection, tableName, fieldName, fieldType, 50);
		}

		public static void CreateField(ref DbConnection connection, string tableName, string fieldName, int fieldType, int fieldSize)
		{
			string sqlType = fieldType switch
			{
				0 => $"TEXT({fieldSize})",
				1 => "INT",
				2 => "DATE",
				4 => "FLOAT",
				5 => "TEXT",
				_ => null
			};
			if (sqlType is null) return;
			if (connection.State != ConnectionState.Open) connection.Open();

			using var transaction = connection.BeginTransaction();
			var existing = new HashSet<string>(GetColumnNames(connection, tableName), StringComparer.OrdinalIgnoreCase);
			using DbCommand command = new(connection);
			command.Transaction = transaction;
			void AddMissingColumn(string name, string type)
			{
				if (!existing.Add(name)) return;
				command.CommandText = $"ALTER TABLE {QuoteIdentifier(tableName)} ADD COLUMN {QuoteIdentifier(name)} {type}";
				command.ExecuteNonQuery();
			}

			AddMissingColumn(fieldName, sqlType);
			if (fieldType == 0 && string.Equals(fieldName, "TIMING", StringComparison.OrdinalIgnoreCase))
				AddMissingColumn("MSC", "TEXT");
			transaction.Commit();
		}

		public static DbConnection GetDbConnection(string ConnectString)
		{
			DbConnection connection = null;
			try
			{
				connection = new DbConnection(ConnectString);
				connection.Open();
				return connection;
			}
			catch (Exception ex)
			{
				connection?.Dispose();
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
				return null;
			}
		}

		public static DataTable GetRecordSet(DbConnection connection, string sql) => GetDataTable(connection, sql);

		// Obsolete: Use GetDataAdapter instead
		[System.Obsolete("Use GetDataAdapter with PascalCase naming.")]
		public static (DbDataAdapter, DataTable) getDataAdapter(DbConnection connection, string FullSearchString)
		{
			return GetDataAdapter(connection, FullSearchString);
		}

		public static (DbDataAdapter, DataTable) GetDataAdapter(DbConnection connection, string FullSearchString)
		{
			try
			{
				DataTable dt = new();
				DbDataAdapter da = new(FullSearchString, connection);
				using (DbCommandBuilder cmdBuilder = new(da))
				{
					da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
					da.FillLoadOption = LoadOption.PreserveChanges;
					da.Fill(dt);
				}
				return (da, dt);
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
				return (null, null);
			}
		}

		public static DataTable GetDataTable(DbConnection connection, string sql, params (string Name, object Value)[] parameters)
		{
			try
			{
				using var command = CreateCommand(connection, sql, parameters);
				using var reader = command.ExecuteReader();
				var table = new DataTable();
				table.Load(reader);
				return table;
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
				return null;
			}
		}

		public static DataTable GetDataTable(string connectionString, string sql, params (string Name, object Value)[] parameters)
		{
			using var connection = GetDbConnection(connectionString);
			return GetDataTable(connection, sql, parameters);
		}

		public static DbDataReader GetDataReader(DbConnection connection, string sql, params (string Name, object Value)[] parameters)
		{
			try
			{
				using var command = CreateCommand(connection, sql, parameters);
				return command.ExecuteReader();
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
				return null;
			}
		}

		public static (DbConnection, DbDataReader) GetDataReader(string connectionString, string sql,
			params (string Name, object Value)[] parameters)
		{
			var connection = GetDbConnection(connectionString);
			var reader = GetDataReader(connection, sql, parameters);
			if (reader != null)
				return (connection, reader);

			connection?.Dispose();
			return (null, null);
		}

		public static DataRow GetDataRowScalar(DbConnection connection, string sql, params (string Name, object Value)[] parameters)
		{
			using var table = GetDataTable(connection, sql, parameters);
			return table?.Rows.Count > 0 ? table.Rows[0] : null;
		}

		public static void UpdateTable(DbConnection connection, string selectQuery, DataTable dataTable)
		{
			try
			{
				using DbTransaction dbTransaction = connection.BeginTransaction();
				using DbCommand command = connection.CreateCommand();
				command.Transaction = dbTransaction;
				command.CommandText = selectQuery;
				using DbDataAdapter da = new(command);

				// 자동으로 쿼리가 생성 되려면
				// 반드시 PK로 설정되어 있는 컬럼이 존재 해야 한다.
				using DbCommandBuilder cb = new(da);

				cb.SetAllValues = false;
				cb.ConflictOption = ConflictOption.OverwriteChanges;
				da.UpdateCommand = cb.GetUpdateCommand();
				da.InsertCommand = cb.GetInsertCommand();
				da.DeleteCommand = cb.GetDeleteCommand();
				da.Update(dataTable);

				dbTransaction.Commit();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}
	}
