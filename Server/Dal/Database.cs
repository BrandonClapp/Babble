using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Server.Dal
{
    class Database
    {
        const string DbFileName = "ServerDb.sqlite";
        static readonly string DbConnectionString = string.Format("Data Source={0};Version=3;", DbFileName);

        public static bool Exists
        {
            get { return File.Exists(DbFileName); }
        }

        public static void CreateDefaultDatabase()
        {
            if (Exists)
            {
                return;
            }

            InitNewDatabase();
        }

        public static SQLiteConnection CreateConnection()
        {
            return new SQLiteConnection(DbConnectionString);
        }

        private static void InitNewDatabase()
        {
            const string sql = @"
create table Users
(
    Id integer primary key autoincrement,
    UserName varchar(50) not null unique,
    Password varchar(1024) not null
);

create table Channels
(
    Id integer primary key autoincrement,
    Name varchar(50)
);

insert into Channels (Name) values ('Lobby Channel');
insert into Channels (Name) values ('Casual Channel');
insert into Channels (Name) values ('Smack Talk Channel');
";
            using (var conn = CreateConnection())
            {
                conn.Execute(sql);
            }
        }
    }
}