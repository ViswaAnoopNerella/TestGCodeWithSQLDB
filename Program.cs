using System.Data.SqlClient;

class Program
{
	private static object _server = "DCZZ189776_OLD\\SQLEXPRESS";
	private static object _dataBase = "GVFCProduction";
	private static object _password = "RemotePassword";
	private static object _user = "RemoteUser";

	static void Main()
	{
		try
		{
			// See https://aka.ms/new-console-template for more information
			Console.WriteLine("Hello, World!");
			//string connectionString = "YourConnectionString"; // Replace with your actual connection string.
			int gCodeId = 2; // Replace with the ID of the G-Code you want to retrieve.

			var connectionString = $"Data Source = {_server};" + $"\nInitial Catalog = {_dataBase};" +
							  "\nPersist Security Info = True;" + $"\nUser ID = {_user};" +
							  $"\nPassword = {_password};";

			//SendGCodeFileToDB_Test1(connectionString);


			GetGCodeFileFromDB(gCodeId, connectionString);
		}
		catch (SqlException sqlex)
		{
			Console.WriteLine(sqlex.Message);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
		Console.ReadKey();

		static void SendGCodeFileToDB_Test1(string connectionString)
		{
			//string connectionString = "YourConnectionString"; // Your actual connection string
			string tableName = "SampleGCodeTesting.dbo.GCodeBin"; // Your actual table name
			string fileNameColumn = "misc"; // Your actual column name for the file name
			string gCodeContentColumn = "GCodeBin"; // Your actual column name for the G-Code content
			string filePath = @"C:\MSDApps\sample_gcode_1.gcode"; // Path to your G-Code file

			bool bresult = InsertGCodeFile(connectionString, tableName, fileNameColumn, gCodeContentColumn, filePath);
			Console.WriteLine($"Insert of GCode File {filePath} is {bresult}");


			//string connectionString = "YourConnectionString"; // Your actual connection string
			//string tableName = "YourTableName"; // Your actual table name
			//string gCodeContentColumn = "GCodeBin"; // Your actual column name for the G-Code content
			//string fileNameColumn = "misc"; // Your actual column name for the file name
			//string filePath = @"C:\path\to\your\gcodefile.gcode"; // Path to your G-Code file

			//InsertGCodeFile(connectionString, tableName, gCodeContentColumn, fileNameColumn, filePath);


		}
	}

	private static bool InsertGCodeFile(string connectionString, string tableName, string fileNameColumn, string gCodeContentColumn, string filePath)
	{
		try
		{
			string fileName = Path.GetFileName(filePath);
			byte[] gCodeContent = File.ReadAllBytes(filePath); // Read the file as binary data

			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();

				// Retrieve the maximum ID currently in the table
				string maxIdCommandText = $"SELECT ISNULL(MAX(ID), 0) FROM {tableName}";
				var maxIdCommand = new SqlCommand(maxIdCommandText, connection);
				int maxId = (int)maxIdCommand.ExecuteScalar();
				int newId = maxId + 1;

				// Check if a file with the same name already exists in the database.
				string checkCommandText = $"SELECT COUNT(*) FROM {tableName} WHERE {fileNameColumn} = @FileName";
				var checkCommand = new SqlCommand(checkCommandText, connection);
				checkCommand.Parameters.AddWithValue("@FileName", fileName);

				int fileCount = (int)checkCommand.ExecuteScalar();

				if (fileCount > 0)
				{
					Console.WriteLine("A file with the same name already exists in the database.");
					return false;

				}

				// Insert the new file with the new ID.
				string insertCommandText = $"INSERT INTO {tableName} (ID, {gCodeContentColumn}, {fileNameColumn}) VALUES (@NewID, @GCodeContent, @FileName)";
				var insertCommand = new SqlCommand(insertCommandText, connection);

				insertCommand.Parameters.AddWithValue("@NewID", newId);
				insertCommand.Parameters.AddWithValue("@GCodeContent", gCodeContent);
				insertCommand.Parameters.AddWithValue("@FileName", "test" + newId);

				insertCommand.ExecuteNonQuery();
				connection.Close();
				return true;
			}
		}
		catch (SqlException sqlex)
		{
			Console.WriteLine(" SQL Exception message is " + sqlex.Message);
			return false;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			return false;
		}
	}


	private static void GetGCodeFileFromDB(int gCodeId, string connectionString)
	{
		try
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				string sql = "SELECT [GCodeBin] FROM [SampleGCodeTesting].[dbo].[GCodeBin] WHERE [ID] = @ID";
				using (SqlCommand command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@ID", gCodeId);
					connection.Open();

					using (SqlDataReader reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							byte[] gCodeBytes = reader["GCodeBin"] as byte[];
							if (gCodeBytes != null)
							{
								string filePath = @$"C:\MSDApps\retrieved_gcode{gCodeId}.gcode"; // Specify your file path here.
								File.WriteAllBytes(filePath, gCodeBytes);
								Console.WriteLine("G-Code file retrieved and saved to " + filePath);
							}
						}
					}
				}
			}
		}
		catch (SqlException sqlex)
		{
			Console.WriteLine(sqlex.Message);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}
}
