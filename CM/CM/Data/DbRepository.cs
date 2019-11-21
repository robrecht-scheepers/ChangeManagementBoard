using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CM.Data
{
    public class DbRepository 
    {
        private const string DbFileName = "cm.db3";
        private const string ConnectionStringBase = "Data Source={0}; Version=3;LockingMode=Normal; Synchronous=Off";
        private readonly string _dbPath;
        private readonly string _dbConnectionString;

        public DbRepository(string dbFolderInput)
        {
            string dbFolder = dbFolderInput == "."
                ? Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "db")
                : dbFolderInput;

            if (!Directory.Exists(dbFolder))
                Directory.CreateDirectory(dbFolder);
            _dbPath = Path.Combine(dbFolder, DbFileName);
            _dbConnectionString = string.Format(ConnectionStringBase, _dbPath);

            if (!File.Exists(_dbPath))
            {
                CreateNewDatabase(_dbPath);
            }
        }

        private SQLiteConnection GetConnection() => new SQLiteConnection(_dbConnectionString);

        private void RunSqlScript(string scriptFileName)
        {
            // this method assumes tha script lies in the Sql folder
            string sql;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"CM.Data.{scriptFileName}"))
            {
                if (stream == null)
                    throw new Exception($"Embedded resource for script {scriptFileName} not found");
                using (var reader = new StreamReader(stream))
                {
                    sql = reader.ReadToEnd();
                }
            }
            using (var dbConnection = GetConnection())
            {
                var command = new SQLiteCommand(dbConnection) { CommandText = sql };
                dbConnection.Open();
                command.ExecuteNonQuery(CommandBehavior.CloseConnection);
            }
        }

        private void CreateNewDatabase(string dbPath)
        {
            SQLiteConnection.CreateFile(dbPath);
            RunSqlScript("CreateDatabase.sql");
        }

        public async Task Initialize()
        {
            
        }

        public List<string> GetProjects()
        {
            const string commandText = "SELECT Name FROM Projects";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    dbConnection.Open();
                    var reader = command.ExecuteReader();
                    var projects = new List<string>();
                    if (!reader.HasRows)
                        return projects;
                    while (reader.Read())
                    {
                        projects.Add(reader.GetString(0));
                    }

                    return projects;
                }
            }
        }
        
        public async Task AddProject(string name)
        {
            var id = Guid.NewGuid();
            const string commandText = 
                "INSERT INTO Projects(Id,[Name]) " +
                "VALUES (@id,@name)";
            int lines;
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = commandText})
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@name", name);

                    dbConnection.Open();
                    lines = await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<IEnumerable<Person>> GetParticipants(string projectName)
        {
            var commandText = 
                "SELECT pp.[Name],pp.Phase,pp.Resistance " +
                "FROM ProjectParticipants pp JOIN Projects p ON pp.Project = p.Id " +
                "WHERE p.Name = @projectName";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection){CommandText = commandText})
                {
                    command.Parameters.AddWithValue("@projectName", projectName);
                    
                    dbConnection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var participants = new List<Person>();
                        if (!reader.HasRows)
                            return participants;
                        while (await reader.ReadAsync())
                        {
                            participants.Add(
                                new Person
                                {
                                    Name = reader.GetString(0),
                                    Position = new Position(reader.GetInt32(1), reader.GetInt32(2))
                                });
                        }

                        return participants;
                    }
                }
            }
        }

        public async Task AddParticipant(string projectName, string participantName, int phase = 0, int resistance = 0)
        {
            var id = Guid.NewGuid();
            const string commandText =
                "INSERT INTO ProjectParticipants(Id,Project,[Name], Phase, Resistance) " +
                "VALUES (@id, (SELECT Id FROM Projects WHERE Name = @projectName), @name, @phase,@resistance)";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) { CommandText = commandText })
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@projectName", projectName);
                    command.Parameters.AddWithValue("@name", participantName);
                    command.Parameters.AddWithValue("@phase", phase);
                    command.Parameters.AddWithValue("@resistance", resistance);
                    dbConnection.Open();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateParticipant(string projectName, string participantName, int phase, int resistance)
        {
            var id = Guid.NewGuid();
            const string commandText =
                "UPDATE ProjectParticipants " +
                "SET Phase = @phase, Resistance = @resistance " +
                "WHERE Name = @name AND Project = (SELECT Id FROM Projects WHERE Name = @projectName)";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) { CommandText = commandText })
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@projectName", projectName);
                    command.Parameters.AddWithValue("@name", participantName);
                    command.Parameters.AddWithValue("@phase", phase);
                    command.Parameters.AddWithValue("@resistance", resistance);
                    dbConnection.Open();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteParticipant(string projectName, string participantName)
        {
            var id = Guid.NewGuid();
            const string commandText =
                "DELETE FROM ProjectParticipants " +
                "WHERE Name = @name AND Project = (SELECT Id FROM Projects WHERE Name = @projectName)";
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) { CommandText = commandText })
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@projectName", projectName);
                    command.Parameters.AddWithValue("@name", participantName);
                    dbConnection.Open();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }



    }
}
