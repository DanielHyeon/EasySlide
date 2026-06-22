using System;
using System.Data;
using System.Data.SQLite;
using System.Text;

namespace Easislides.Util
{
    class Program
    {
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

			///////////////////////////////////////////////////////////////////////////////////
			//SQLiteCommandBuilder를 활용한 데이터 변경 : Insert 예제
			/*
			SQLiteConnection sqConn = new SQLiteConnection();
			sqConn.ConnectionString = @"Data Source = C:\EasiSlides\Admin\Database\EasiSlidesDb.db";
			sqConn.Open();

			//Data Adapter 생성
			SQLiteDataAdapter adapter = new SQLiteDataAdapter();

			DataTable data = new DataTable();
			data.AcceptChanges();

			//select query 생성 Fill시 사용
			adapter.SelectCommand = new SQLiteCommand("SELECT * from TEST", sqConn);

			//insert query 생성할 CommandBuilder 생성 (select query를 바탕으로 자동으로 생성)
			SQLiteCommandBuilder sqCB = new SQLiteCommandBuilder(adapter);
			adapter.InsertCommand = sqCB.GetInsertCommand();

			adapter.Fill(data);

			DataRow dr = data.NewRow();
            dr["Field0"] = 201;
            dr["TIMING"] = "aaa";
            dr["MSC"] = "aaa";

			data.Rows.Add(dr);

			adapter.Update(data);  // Its also update in database.		

			sqConn.Close();
			*/
			//////////////////////////////////////////////////////////////////////////////////
			///
			/// 검색
			///
			/*
			SQLiteConnection sQLiteConnection = new SQLiteConnection();
			sQLiteConnection.ConnectionString = @"Data Source = C:\EasiSlides\Admin\Database\EasiSlidesDb.db";
			sQLiteConnection.Open();

			DataTable dt = new DataTable();

			string FullSearchString = "Select * from SONG";

			SQLiteCommand scdCommand = new SQLiteCommand(FullSearchString, sQLiteConnection);
			SQLiteDataReader sqliteDataReader = scdCommand.ExecuteReader();

			dt.Load(sqliteDataReader);

			DataColumn[] primarykey = new DataColumn[] { dt.Columns["SONGID"] };
			dt.PrimaryKey = primarykey;

			DataRow dr = dt.Rows.Find("10");

			Console.WriteLine(dr["TITLE_1"]);
			*/
			/////////////////////////////////////////////////////////////////////////////////
			///
			/*
			string text = "select * from TEST";

			using SQLiteConnection sQLiteConnection = new SQLiteConnection();
			sQLiteConnection.ConnectionString = @"Data Source = C:\EasiSlides\Admin\Database\EasiSlidesDb.db";
			sQLiteConnection.Open();

			SQLiteDataAdapter sQLiteDataAdapter = null;
			DataTable dt = null;


			(sQLiteDataAdapter, dt) = getDataAdapter(sQLiteConnection, text);
			

			if (dt != null)
            {
                DataColumn[] primarykey = new DataColumn[] { dt.Columns["Field0"] };
                dt.PrimaryKey = primarykey;
            }

            DataRow dr = dt.Rows.Find($"{5}");
			int index = dt.Rows.IndexOf(dr);

			dt.AcceptChanges();
			dt.Rows[index].BeginEdit();
			dt.Rows[index]["TIMING"] = "ccc";
			dt.Rows[index].EndEdit();
			*/

			/////////////////////////////////////////////////




			/////////////////////////////////////////////////////////////////////////////////
			// SQLiteCommand 를 활용한 Insert 예제

			/*

			SQLiteConnection sqConn = new SQLiteConnection();
			sqConn.ConnectionString = @"Data Source = C:\EasiSlides\Admin\Database\EasiSlidesDb.db";
			sqConn.Open();

			SQLiteTransaction transaction = sqConn.BeginTransaction();

			SQLiteCommand cmd = new SQLiteCommand(sqConn);

			cmd.Transaction = transaction;

			StringBuilder sb = new StringBuilder();

			try
			{
				for (int i = 101; i < 105; i++)
				{
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = $"INSERT INTO TEST(Field0, TIMING, MSC) VALUES ({i} , 'ddd', 'cc');";
					cmd.ExecuteNonQuery();
				}

				transaction.Commit();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				transaction.Rollback();
			}

			sqConn.Close();
			*/

			//////////////////////////////////////////////////////////
			///UPDATE
			//
			/*
			SQLiteConnection sqConn = new SQLiteConnection();
			sqConn.ConnectionString = @"Data Source = C:\EasiSlides\Admin\Database\EasiSlidesDb.db";
			sqConn.Open();

			SQLiteTransaction transaction = sqConn.BeginTransaction();

			SQLiteCommand cmd = new SQLiteCommand(sqConn);

			cmd.Transaction = transaction;

			StringBuilder sb = new StringBuilder();

			try
			{
				for (int i = 101; i < 105; i++)
				{
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = $"UPDATE TEST SET TIMING = 'eee{i}', MSC = 'fff{i}' where Field0 = {i}";
					cmd.ExecuteNonQuery();
				}

				transaction.Commit();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				transaction.Rollback();
			}

			sqConn.Close();
			*/

			/////////////////////////////////////////////////////////
			///UPDATE
			//UPDATE();

			//DataTable dt = new DataTable();

			//SQLiteConnection sqConn = new SQLiteConnection();
			//sqConn.ConnectionString = @"Data Source = C:\EasiSlides\Admin\Database\EasiSlidesDb.db";
			//sqConn.Open();

			//// 出力データ取得
			//using (SQLiteCommand command = sqConn.CreateCommand())
			//{
			//	command.CommandText = "SELECT * FROM TEST Where Field0 = 105";
			//	using (SQLiteDataAdapter da = new SQLiteDataAdapter(command))
			//	{
			//		da.Fill(dt);
			//	}
			//}

			//foreach (DataRow dr in dt.Rows)
			//{
			//	dr["TIMING"] = "park2";
			//	dr["MSC"] = "daniel2";
			//}

			//string selectQuery = "Select * FROM TEST";

			//UpdateTest(sqConn, selectQuery, dt);

			///////////////////////////////////////////////////////////
			///UPDATE TABLE
			//DataTable dt = new DataTable();

			//SQLiteConnection sqConn = new SQLiteConnection();
			//sqConn.ConnectionString = @"Data Source = C:\EasiSlides\Admin\Database\EasiSlidesDb.db";
			//sqConn.Open();

			//// 出力データ取得
			//using (SQLiteCommand command = sqConn.CreateCommand())
			//{
			//	command.CommandText = "SELECT * FROM TEST";
			//	using (SQLiteDataAdapter da = new SQLiteDataAdapter(command))
			//	{
			//		da.Fill(dt);
			//	}
			//}

			//DataColumn[] primarykey = new DataColumn[] { dt.Columns["Field0"] };
			//dt.PrimaryKey = primarykey;

			//DataRow dr = dt.Rows.Find("105");

			//dr["TIMING"] = "park5";
			//dr["MSC"] = "daniel5";

			//dr = dt.Rows.Find("104");

			//dr["TIMING"] = "park4";
			//dr["MSC"] = "daniel4";

			//string selectQuery = "Select * FROM TEST";

			//UpdateTest(sqConn, selectQuery, dt);

			//////////////////////////////////////////////////////////
			///


			//SQLiteConnection sqConn = new SQLiteConnection();
			//sqConn.ConnectionString = @"Data Source = C:\EasiSlides\Admin\Database\EasiSlidesDb.db";
			//sqConn.Open();

			//SQLiteDataReader dataReader = GetDataReader(sqConn, "select * from TEST");

			//if (dataReader != null && dataReader.HasRows)
			//{
			//	//recordSet.MoveFirst();
			//	while (dataReader.Read())
			//	{
			//		Console.WriteLine($"{dataReader["Field0"]} , {dataReader["TIMING"]}");
			//	}
			//}

			/////////////////////////////////////////////////////////
			///INSERT
			//(SQLiteDataAdapter sQLiteDataAdapter, DataTable dt) = SQLiteController.getDataAdapter(InConnectString, "Select * from SONG");

			//if (dt != null)
			//{
			//	try
			//	{
			//		dt.AcceptChanges();

			//		SQLiteCommandBuilder sqCB = new SQLiteCommandBuilder(sQLiteDataAdapter);
			//		sQLiteDataAdapter.InsertCommand = sqCB.GetInsertCommand();

			//		DataRow dr = dt.NewRow();

			//		dr["Title_1"] = Title_1;
			//		dr["Title_2"] = Title_2;
			//		dr["song_number"] = song_number;
			//		dr["FolderNo"] = FolderNo;
			//		dr["Lyrics"] = Lyrics;
			//		dr["Sequence"] = Sequence;
			//		dr["writer"] = writer;
			//		dr["copyright"] = copyright;
			//		dr["CJK_WordCount"] = value;
			//		dr["CJK_StrokeCount"] = value2;
			//		dr["capo"] = capo;
			//		dr["Timing"] = Timing;
			//		dr["Key"] = InKey;
			//		dr["msc"] = msc;
			//		dr["CATEGORY"] = CATEGORY;
			//		dr["LICENCE_ADMIN1"] = LICENCE_ADMIN1;
			//		dr["LICENCE_ADMIN2"] = LICENCE_ADMIN2;
			//		dr["BOOK_REFERENCE"] = BOOK_REFERENCE;
			//		dr["USER_REFERENCE"] = USER_REFERENCE;
			//		dr["SETTINGS"] = SETTINGS;
			//		dr["FORMATDATA"] = FORMATDATA;
			//		dr["LastModified"] = DateTime.Now.Date;

			//		dt.Rows.Add(dr);

			//		sQLiteDataAdapter.Update(dt);  // Its also update in database.	

			//		return DataUtil.ObjToInt(dr["SongID"]);
			//	}
			//	catch
			//	{
			//		MessageBox.Show("Error encountered whilst adding item to database - item NOT added");
			//	}
			//}

			///////////////////////////////////////////////////////
			///DataReader

			//string sqlSelect = "select * from Test";

			//SQLiteCommand oCmd = new SQLiteCommand(sqlSelect, sqConn);
			//SQLiteDataReader oReader = null;
			//string loginName = string.Empty;
			//string countryName = string.Empty;
			//try
			//{
			//	oReader = oCmd.ExecuteReader();
			//	while (oReader.Read())
			//	{
			//		if (!Convert.IsDBNull(oReader["Field0"]))
			//		{
			//			Console.WriteLine(oReader["Field0"].ToString());
			//		}
			//	}
			//}
			//catch (Exception e)
			//{
			//	Console.WriteLine(e.Message);
			//	Console.WriteLine(e.StackTrace);
			//}

			//sqConn.Close();

			/////////////////////////////////////////////////////////////////////////////////

			/*
			string tablename = "TEST";

			//SQLite 연결
			//SQLiteConnection sqConn = new SQLiteConnection();
			sqConn.ConnectionString = @"Data Source = C:\EasiSlides\Admin\Database\EasiSlidesDb.db";
			sqConn.Open();

			//트랜잭션 시작
			BeginTranaction(sqConn);

			//Data Adapter 생성
			SQLiteDataAdapter adapter = new SQLiteDataAdapter();
			//select query 생성 Fill시 사용
			adapter.SelectCommand = new SQLiteCommand("SELECT * FROM " + tablename, sqConn);

			//insert query 생성할 CommandBuilder 생성 (select query를 바탕으로 자동으로 생성)
			SQLiteCommandBuilder sqCB = new SQLiteCommandBuilder(adapter);
			adapter.InsertCommand = sqCB.GetInsertCommand();

			//다른 테이블에서 데이터를 읽어오도록 설정
			DataTable data = new DataTable();

			adapter.Fill(data);
			//읽어온 데이터의 column을 맞추기 위하여

			//한 행씩 밀어넣는다
			DataRow dr = data.NewRow();
			dr["Field0"] = 4;
			dr["TIMING"] = "ddd";
			dr["MSC"] = "ddd";

			data.Rows.Add(dr);

			dr = data.NewRow();
			dr["Field0"] = 5;
			dr["TIMING"] = "eee";
			dr["MSC"] = "eee";

			data.Rows.Add(dr);

			//변경된 사항을 update한다. (insert, update, delete)
			adapter.Update(data);

			//트랜잭션을 완료한다.
			CommitTranaction(sqConn);

			//사용했던 객체 삭제
			sqCB.Dispose();
			adapter.Dispose();
			sqConn.Close();
			sqConn.Dispose();
			*/
			Console.WriteLine("park" + "\t" + " b");

			//bibleListTest();

			Console.ReadLine();
		}

		private static void bibleListTest()
		{
			using SQLiteConnection connection = new SQLiteConnection();
			connection.ConnectionString = @"Data Source=C:\EasiSlides\Admin-Copy\Database\EsBiblesList.db";
			connection.Open();

			string queryString = "select * from Biblefolder where NAME like \"%\" ";
			DataTable dt = new DataTable();


			SQLiteDataAdapter adapter = new SQLiteDataAdapter();
			adapter.SelectCommand = new SQLiteCommand(queryString, connection);
			SQLiteCommandBuilder builder = new SQLiteCommandBuilder(adapter);

			DataSet dataSet = new DataSet();
			adapter.Fill(dataSet, "Biblefolder");

			dt = dataSet.Tables[0];

			//DataColumn[] primarykey = new DataColumn[1];
			//primarykey[0] = dt.Columns["NAME"];

			//dt.PrimaryKey = primarykey;

			if (dt.Rows.Count > 0)
			{
				foreach (DataRow dr in dt.Rows)
				{
					dr["displayorder"] = -1;
				}
			}

			adapter.Update(dt);

			//UpdateTable(queryString, connection, dt);

			//builder.GetUpdateCommand();

			//Without the SqlCommandBuilder this line would fail
			//adapter.Update(dataSet, "Biblefolder");

		}

		private static void bibleListTest1()
		{
			using SQLiteConnection sqConn = new SQLiteConnection();
			sqConn.ConnectionString = @"Data Source=C:\EasiSlides\Admin-Copy\Database\EsBiblesList.db";
			sqConn.Open();

			try
			{
				//string query = "select * from Biblefolder where NAME like \"%\" ";
				string query = "select * from Biblefolder";

				using DataTable dt = new DataTable("Biblefolder");
				//dt.AcceptChanges();

				using SQLiteDataAdapter sQLiteDataAdapter = new SQLiteDataAdapter(query, sqConn);
				sQLiteDataAdapter.Fill(dt);

				DataColumn[] primarykey = new DataColumn[1];
				primarykey[0] = dt.Columns["NAME"];

				dt.PrimaryKey = primarykey;

				//SQLiteCommandBuilder sqCB = new SQLiteCommandBuilder(sQLiteDataAdapter);
				//sQLiteDataAdapter.UpdateCommand = sqCB.GetUpdateCommand();

				dt.AcceptChanges();

				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						dr["displayorder"] = -1;
					}

					//UpdateTable(query, sqConn, dt);
					sQLiteDataAdapter.Update(dt);
				}
			}
			catch (Exception ex)
            {
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
            }
		}

		public static void UpdateTable(string query, SQLiteConnection connection, DataTable table)
		{
			//object maxId = null;
			//if (query.ToLower().Contains("from traffic_object"))
			//  maxId = GetScalar("Select max(obj_id) From traffic_object", connection);

			using (SQLiteTransaction transaction = connection.BeginTransaction())
			{
				using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection))
				{
					using (SQLiteCommand command = connection.CreateCommand())
					{
						command.Transaction = transaction;
						command.CommandText = query;
						adapter.SelectCommand = command;

						using (SQLiteCommandBuilder builder = new SQLiteCommandBuilder())
						{
							builder.DataAdapter = adapter;
							adapter.Update(table);

							transaction.Commit();
						}
					}
				}
			}
		}
		/// <summary>
		/// Insert TEST Function
		/// </summary>
		/// <param name="conn"></param>
		/// 
		private static void InsertTest()
        {
			SQLiteConnection sqConn = new SQLiteConnection();
			sqConn.ConnectionString = @"Data Source = C:\EasiSlides\Admin\Database\EasiSlidesDb.db";
			sqConn.Open();

			DataTable dt = new DataTable();

			SQLiteDataAdapter sQLiteDataAdapter = new SQLiteDataAdapter("Select * from TEST", sqConn);
			sQLiteDataAdapter.Fill(dt);

			if (dt != null)
			{
				try
				{
					dt.AcceptChanges();

					SQLiteCommandBuilder sqCB = new SQLiteCommandBuilder(sQLiteDataAdapter);
					sQLiteDataAdapter.InsertCommand = sqCB.GetInsertCommand();

					DataRow dr = dt.NewRow();

					dr["Field0"] = 105;
					dr["TIMING"] = "dd";
					dr["MSC"] = "ee";


					dt.Rows.Add(dr);

					sQLiteDataAdapter.Update(dt);  // Its also update in database.	

				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}
		}

		/// <summary>
		/// Update TEST Function
		/// </summary>
		/// <param name="conn"></param>
		/// 
		private static void UpdateTest()
		{
			SQLiteConnection sqConn = new SQLiteConnection();
			sqConn.ConnectionString = @"Data Source = C:\EasiSlides\Admin\Database\EasiSlidesDb.db";
			sqConn.Open();

			DataTable dt = new DataTable();

			SQLiteDataAdapter sQLiteDataAdapter = new SQLiteDataAdapter("Select * from TEST", sqConn);
			sQLiteDataAdapter.Fill(dt);

			if (dt != null)
			{
				try
				{
					dt.AcceptChanges();

					SQLiteCommandBuilder sqCB = new SQLiteCommandBuilder(sQLiteDataAdapter);
					sQLiteDataAdapter.InsertCommand = sqCB.GetUpdateCommand();

					dt.Rows[9]["Field0"] = 105;
					dt.Rows[9]["TIMING"] = "dd";
					dt.Rows[9]["MSC"] = "ee";

					sQLiteDataAdapter.Update(dt);  // Its also update in database.	

				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}
		}

		private static void UpdateTest(SQLiteConnection sQLiteConnection, string selectQuery, DataTable dataTable)
		{
			
			using (SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction())
			{
				using (SQLiteCommand command = sQLiteConnection.CreateCommand())
				{
					command.Transaction = sQLiteTransaction;
					command.CommandText = "SELECT * FROM TEST";
					SQLiteDataAdapter da = new SQLiteDataAdapter(command);

					// 자동으로 쿼리가 생성 되려면
					// 반드시 PK로 설정되어 있는 컬럼이 존재 해야 한다.
					using (SQLiteCommandBuilder cb = new SQLiteCommandBuilder(da))
					{
						cb.SetAllValues = false;
						cb.ConflictOption = ConflictOption.OverwriteChanges;
						da.UpdateCommand = cb.GetUpdateCommand();
						da.InsertCommand = cb.GetInsertCommand();
						da.DeleteCommand = cb.GetDeleteCommand();
						da.Update(dataTable);
					}
				}
				sQLiteTransaction.Commit();
			}

			
			foreach (DataRow dr in dataTable.Rows)
			{
				Console.WriteLine($"{dr["Field0"]},  {dr["TIMING"]}, {dr["MSC"]}");
			}
		}

		public static void UpdateTable()
		{
			//object maxId = null;
			//if (query.ToLower().Contains("from traffic_object"))
			//  maxId = GetScalar("Select max(obj_id) From traffic_object", connection);

			DataTable dt = new DataTable();
			string query = "Select * From Table";

			SQLiteConnection sqConn = new SQLiteConnection();
			sqConn.ConnectionString = @"Data Source = C:\EasiSlides\Admin\Database\EasiSlidesDb.db";
			sqConn.Open();

			using (SQLiteTransaction transaction = sqConn.BeginTransaction())
			{
				using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, sqConn))
				{
					using (SQLiteCommand command = sqConn.CreateCommand())
					{
						command.Transaction = transaction;
						command.CommandText = query;
						adapter.SelectCommand = command;

						using (SQLiteCommandBuilder builder = new SQLiteCommandBuilder())
						{
							builder.DataAdapter = adapter;
							adapter.Update(dt);

							transaction.Commit();
						}
					}
				}
			}
		}

		public static void UpdateTable(SQLiteConnection sQLiteConnection, string selectQuery, DataTable dataTable)
		{
			try
			{
				using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
				using SQLiteCommand command = sQLiteConnection.CreateCommand();
				command.Transaction = sQLiteTransaction;
				command.CommandText = selectQuery;
				using SQLiteDataAdapter da = new SQLiteDataAdapter(command);

				// 자동으로 쿼리가 생성 되려면
				// 반드시 PK로 설정되어 있는 컬럼이 존재 해야 한다.
				using SQLiteCommandBuilder cb = new SQLiteCommandBuilder(da);

				cb.SetAllValues = false;
				cb.ConflictOption = ConflictOption.OverwriteChanges;
				da.UpdateCommand = cb.GetUpdateCommand();
				da.InsertCommand = cb.GetInsertCommand();
				da.DeleteCommand = cb.GetDeleteCommand();
				da.Update(dataTable);

				sQLiteTransaction.Commit();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}

		public static SQLiteDataReader GetDataReader(SQLiteConnection sQLiteConnection, string FullSearchString)
		{
			try
			{

				// DataAdapter는 자동으로 Connection을
				// 핸들링한다. conn.Open() 불필요.
				//string connStr = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\EasiSlides\Admin\Database\EasiSlidesDb.mdb";
				//string sql = "select * from FOLDER where FolderNo >=0 and FolderNo < 41";
				SQLiteCommand scdCommand = new SQLiteCommand(FullSearchString, sQLiteConnection);
				SQLiteDataReader sqliteDataReader = scdCommand.ExecuteReader(CommandBehavior.CloseConnection);

				return sqliteDataReader;
			}
			catch
			{
				return null;
			}
		}

		private static void BeginTranaction(SQLiteConnection conn)
		{
			SQLiteCommand command = new SQLiteCommand("Begin", conn);
			command.ExecuteNonQuery();
			command.Dispose();
		}

		private static void CommitTranaction(SQLiteConnection conn)
		{
			SQLiteCommand command = new SQLiteCommand("Commit", conn);
			command.ExecuteNonQuery();
			command.Dispose();
		}

		public static DataTable GetTableRecordSet(SQLiteConnection sQLiteConnection, string TableName)
		{
			try
			{
				DataTable dt = new DataTable();

				string query = "select * from " + TableName;

				SQLiteCommand scdCommand = new SQLiteCommand(query, sQLiteConnection);
				SQLiteDataReader sqliteDataReader = scdCommand.ExecuteReader();
				dt.Load(sqliteDataReader);

				return dt;
			}
			catch
			{
			}
			return null;
		}

		public static (SQLiteDataAdapter, DataTable) getDataAdapter(string ConnectString, string FullSearchString)
		{
			try
			{
				DataTable dt = new DataTable();

				// DataAdapter는 자동으로 Connection을
				// 핸들링한다. conn.Open() 불필요.
				//string connStr = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\EasiSlides\Admin\Database\EasiSlidesDb.mdb";
				//string sql = "select * from FOLDER where FolderNo >=0 and FolderNo < 41";

				//SQLiteConnection sQLiteConnection = GetSqliteDbConnection(ConnectString);

				SQLiteDataAdapter da = new SQLiteDataAdapter(FullSearchString, ConnectString);
				//아래 구문이 빠질경우 업데이트가 되지 않는다.

				SQLiteCommandBuilder cmdBuilder = new SQLiteCommandBuilder(da);
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

		public static (SQLiteDataAdapter, DataTable) getDataAdapter(SQLiteConnection sQLiteConnection, string FullSearchString)
		{
			try
			{
				DataTable dt = new DataTable();

				// DataAdapter는 자동으로 Connection을
				// 핸들링한다. conn.Open() 불필요.
				//string connStr = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\EasiSlides\Admin\Database\EasiSlidesDb.mdb";
				//string sql = "select * from FOLDER where FolderNo >=0 and FolderNo < 41";

				SQLiteDataAdapter da = new SQLiteDataAdapter(FullSearchString, sQLiteConnection);
				//아래 구문이 빠질경우 업데이트가 되지 않는다.

				SQLiteCommandBuilder cmdBuilder = new SQLiteCommandBuilder(da);
				da.MissingSchemaAction = MissingSchemaAction.AddWithKey;

				da.InsertCommand = cmdBuilder.GetInsertCommand();

				da.FillLoadOption = LoadOption.PreserveChanges;

				da.Fill(dt);

				return (da, dt);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return (null, null);
			}
		}

		public static void CreateFieldSQL(ref SQLiteConnection connection, string tableName, string fieldName, int fieldType, int fieldSize)
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
						text = "ALTER TABLE " + tableName + " ADD COLUMN " + fieldName + " MEMO";
						break;
				}
				if (!(text == ""))
				{

					if (connection.State != ConnectionState.Open)
						connection.Open();

					SQLiteCommand scdCommand = new SQLiteCommand(text, connection);
					scdCommand.ExecuteNonQuery();

					if (fieldType == 0)
					{
						if (fieldName.ToUpper() == "TIMING".ToUpper())
						{
							fieldName = "MSC";
							text = "ALTER TABLE " + tableName + " ADD COLUMN " + fieldName;
							scdCommand = new SQLiteCommand(text, connection);
							scdCommand.ExecuteNonQuery();
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
	}
}
