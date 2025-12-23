using Easislides.Util;
//using Microsoft.Office.Interop.Access.Dao;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Data.SQLite;
using System.Threading;
using DbConnection = MySql.Data.MySqlClient.MySqlConnection;
using DbDataAdapter = MySql.Data.MySqlClient.MySqlDataAdapter;
using DbCommandBuilder = MySql.Data.MySqlClient.MySqlCommandBuilder;
using DbCommand = MySql.Data.MySqlClient.MySqlCommand;
using DbDataReader = MySql.Data.MySqlClient.MySqlDataReader;
using DbTransaction = MySql.Data.MySqlClient.MySqlTransaction;

namespace Easislides.MariaDB
{
	class DbController
	{
		const string dbConnectionString = @"Server=127.0.0.1;Database=easislidesdb;Uid =root;Pwd=dnffkffk48;";

		public static void DropTable(ref DbConnection connection, string tableName)
		{
			try
			{
				bool flag = false;
				DataTable dbSchemaTable = connection.GetSchema("Tables", new string[4]
				{
					null,
					null,
					tableName,
					"TABLE"
				});
				foreach (DataRow row in dbSchemaTable.Rows)
				{
					if (tableName.ToUpper() == DataUtil.ObjToString(row["TABLE_NAME"]).ToUpper())
					{
						flag = true;
					}
				}
				if (flag)
				{
					DbCommand command = new DbCommand("DROP TABLE " + tableName, connection);
					command.ExecuteNonQuery();
				}
			}
			catch
			{
			}
		}

		public static void CreateField(ref DbConnection connection, string tableName, string fieldName, int fieldType)
		{
			CreateField(ref connection, tableName, fieldName, fieldType, 50);
		}

		public static void CreateField(ref DbConnection connection, string tableName, string fieldName, int fieldType, int fieldSize)
		{
			string text = "";
			try
			{
				switch (fieldType)
				{
					case 0:
						text = "ALTER TABLE " + tableName + " ADD COLUMN " + fieldName + " TEXT(" + DataUtil.ObjToString(fieldSize) + ") NULL ";
						break;
					case 1:
						text = "ALTER TABLE " + tableName + " ADD COLUMN " + fieldName + " INT";
						break;
					case 2:
						text = "ALTER TABLE " + tableName + " ADD COLUMN " + fieldName + " DATE";
						break;
					case 4:
						text = "ALTER TABLE " + tableName + " ADD COLUMN " + fieldName + " FLOAT";
						break;
					case 5:
						text = "ALTER TABLE " + tableName + " ADD COLUMN " + fieldName + " TEXT";
						break;
				}
				if (!(text == ""))
				{

					if (connection.State != ConnectionState.Open)
						connection.Open();

					DbCommand command = new DbCommand(text, connection);
					command.ExecuteNonQuery();

					if (fieldType == 0)
					{
						if (fieldName.ToUpper() == "TIMING".ToUpper())
						{
							fieldName = "MSC";
							text = "ALTER TABLE " + tableName + " ADD COLUMN " + fieldName;
							command = new DbCommand(text, connection);
							command.ExecuteNonQuery();
						}
					}
				}

			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}

        public static DbConnection GetDbConnection(string ConnectString)
        {
			try
            {
                DbConnection connection = new DbConnection(dbConnectionString);
				connection.Open(); 

				return connection;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        public static DataTable GetRecordSet(DbConnection connection, string FullSearchString)
		{
			try
			{
				DataTable dt = new DataTable();
				DbCommand command = new DbCommand(FullSearchString, connection);
				//DbDataReader dataReader = command.ExecuteReader();
				DbDataReader dataReader = command.ExecuteReader();
				dt.Load(dataReader);

				return dt;
			}
			catch
			{
				return null;
			}
		}

		public static DataTable GetTableRecordSet(string ConnectString, string FullSearchString)
		{
			try
			{
				DataTable dt = new DataTable();
				DbConnection connection = GetDbConnection(ConnectString);
				DbCommand command = new DbCommand(FullSearchString, connection);
				//DbDataReader dataReader = command.ExecuteReader();
				DbDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
				dt.Load(dataReader);
				return dt;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return null;
			}
		}

		public static DataTable GetTableRecordSet(DbConnection connection, string TableName)
		{
			try
			{
				DataTable dt = new DataTable();

				string query = "select * from " + TableName;

				DbCommand command = new DbCommand(query, connection);
				//DbDataReader dataReader = command.ExecuteReader();
				DbDataReader dataReader = command.ExecuteReader();
				dt.Load(dataReader);

				return dt;
			}
			catch
			{
			}
			return null;
		}


		public static (DbDataAdapter, DataTable) getDataAdapter(string ConnectString, string FullSearchString)
		{
			try
			{
				DataTable dt = new DataTable();

				// DataAdapter는 자동으로 Connection을
				// 핸들링한다. connection.Open() 불필요.
				//string connStr = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\EasiSlides\Admin\Database\EasiSlidesDb.mdb";
				//string sql = "select * from FOLDER where FolderNo >=0 and FolderNo < 41";

				DbConnection connection = GetDbConnection(ConnectString);

				DbDataAdapter da = new DbDataAdapter(FullSearchString, connection);
				//아래 구문이 빠질경우 업데이트가 되지 않는다.

				DbCommandBuilder cmdBuilder = new DbCommandBuilder(da);
				da.MissingSchemaAction = MissingSchemaAction.AddWithKey;

				da.FillLoadOption = LoadOption.PreserveChanges;

				da.Fill(dt);

				return (da, dt);
			}
			catch
			{
				return (null, null);
			}
		}

		public static (DbDataAdapter, DataTable) getDataAdapter(DbConnection connection, string FullSearchString)
		{
			try
			{
				DataTable dt = new DataTable();

				// DataAdapter는 자동으로 Connection을
				// 핸들링한다. connection.Open() 불필요.
				//string connStr = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\EasiSlides\Admin\Database\EasiSlidesDb.mdb";
				//string sql = "select * from FOLDER where FolderNo >=0 and FolderNo < 41";

				DbDataAdapter da = new DbDataAdapter(FullSearchString, connection);
				//아래 구문이 빠질경우 업데이트가 되지 않는다.

				DbCommandBuilder cmdBuilder = new DbCommandBuilder(da);
				da.MissingSchemaAction = MissingSchemaAction.AddWithKey;

				da.FillLoadOption = LoadOption.PreserveChanges;

				da.Fill(dt);

				return (da, dt);
			}
			catch
			{
				return (null, null);
			}
		}

		public static DataTable GetDataTable(DbConnection connection, string FullSearchString)
		{
			try
			{

				// DataAdapter는 자동으로 Connection을
				// 핸들링한다. connection.Open() 불필요.
				//string connStr = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\EasiSlides\Admin\Database\EasiSlidesDb.mdb";
				//string sql = "select * from FOLDER where FolderNo >=0 and FolderNo < 41";
				DataTable dt = new DataTable();

				DbCommand command = new DbCommand(FullSearchString, connection);
				//DbDataReader dataReader = command.ExecuteReader();
				DbDataReader dataReader = command.ExecuteReader();

				dt.Load(dataReader);

				return dt;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return null;
			}
		}

		public static DataTable GetDataTable(string ConnectString, string FullSearchString)
		{
			try
			{
				//------------------------------------------------------------------
				DataTable dt = new DataTable();

				using DbConnection connection = GetDbConnection(ConnectString);

				DbCommand command = new DbCommand(FullSearchString, connection);
				//DbDataReader dataReader = command.ExecuteReader();
				DbDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);

				dt.Load(dataReader);

				return dt;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return null;
			}
		}

		public static DbDataReader GetDataReader(DbConnection connection, string FullSearchString)
		{
			try
			{

				// DataAdapter는 자동으로 Connection을
				// 핸들링한다. connection.Open() 불필요.
				//string connStr = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\EasiSlides\Admin\Database\EasiSlidesDb.mdb";
				//string sql = "select * from FOLDER where FolderNo >=0 and FolderNo < 41";
				DbCommand command = new DbCommand(FullSearchString, connection);
				//DbDataReader dataReader = command.ExecuteReader();
				DbDataReader dataReader = command.ExecuteReader();

				return dataReader;
			}
			catch
			{
				return null;
			}
		}

		public static (DbConnection, DbDataReader) GetDataReader(string ConnectString, string FullSearchString)
		{
			try
			{
				//------------------------------------------------------------------
				DbConnection connection = GetDbConnection(ConnectString);
				DbCommand command = new DbCommand(FullSearchString, connection);

				DbDataReader dataReader = command.ExecuteReader();

				return (connection, dataReader);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return (null, null);
			}
		}

		public static DataRow GetDataRowScalar(DbConnection connection, string FullSearchString)
		{
			try
			{
				using DataTable dt = new DataTable();
				DataRow dataRow = null;

				using DbCommand command = new DbCommand(FullSearchString, connection);
				using DbDataReader dataReader = command.ExecuteReader();

				dt.Load(dataReader);

				if (dt != null && dt.Rows.Count > 0)
					dataRow = dt.Rows[0];

				return dataRow;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return null;
			}
		}

		public static (DbConnection, DataRow) GetDataRowScalar(string ConnectString, string FullSearchString)
		{

			try
			{
				using DataTable dt = new DataTable();
				DataRow dataRow = null;

				DbConnection connection = GetDbConnection(ConnectString);
				connection.Open();

				using DbCommand command = new DbCommand(FullSearchString, connection);
				using DbDataReader dataReader = command.ExecuteReader();

				dt.Load(dataReader);

				if (dt != null && dt.Rows.Count > 0)
					dataRow = dt.Rows[0];

				return (connection, dataRow);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return (null, null);
			}
		}

		public static void UpdateTable(DbConnection connection, string selectQuery, DataTable dataTable)
		{
			try
			{
				using DbTransaction dbTransaction = connection.BeginTransaction();
				using DbCommand command = connection.CreateCommand();
				command.Transaction = dbTransaction;
				command.CommandText = selectQuery;
				using DbDataAdapter da = new DbDataAdapter(command);

				// 자동으로 쿼리가 생성 되려면
				// 반드시 PK로 설정되어 있는 컬럼이 존재 해야 한다.
				using DbCommandBuilder cb = new DbCommandBuilder(da);

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
}
