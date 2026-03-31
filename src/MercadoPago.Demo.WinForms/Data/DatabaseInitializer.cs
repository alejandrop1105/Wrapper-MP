using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Serilog;

namespace MercadoPago.Demo.WinForms.Data
{
    /// <summary>
    /// Inicializa la base de datos SQLite y crea las tablas necesarias.
    /// </summary>
    public static class DatabaseInitializer
    {
        private static readonly string DbPath = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            "mpwrapper_demo.db");

        public static string ConnectionString => $"Data Source={DbPath};Version=3;";

        /// <summary>Crea la base de datos y las tablas si no existen.</summary>
        public static void Initialize()
        {
            if (!File.Exists(DbPath))
            {
                SQLiteConnection.CreateFile(DbPath);
                Log.Information("Base de datos SQLite creada: {Path}", DbPath);
            }

            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                ExecuteSchema(conn);
                Log.Information("Esquema de base de datos verificado.");
            }
        }

        public static IDbConnection CreateConnection()
        {
            var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        private static void ExecuteSchema(SQLiteConnection conn)
        {
            var sql = @"
CREATE TABLE IF NOT EXISTS AppConfig (
    Id INTEGER PRIMARY KEY DEFAULT 1,
    AccessToken TEXT NOT NULL DEFAULT '',
    PublicKey TEXT DEFAULT '',
    Environment TEXT DEFAULT 'sandbox',
    WebhookSecret TEXT DEFAULT '',
    WebhookPort INTEGER DEFAULT 5100,
    Country TEXT DEFAULT 'AR',
    UserId TEXT DEFAULT '',
    PlatformId TEXT DEFAULT '',
    ClientId TEXT DEFAULT '',
    ClientSecret TEXT DEFAULT '',
    RefreshToken TEXT DEFAULT ''
);

INSERT OR IGNORE INTO AppConfig (Id) VALUES (1);

CREATE TABLE IF NOT EXISTS OperationLog (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    OperationType TEXT NOT NULL,
    ExternalId TEXT,
    ExternalReference TEXT,
    Status TEXT,
    Amount REAL,
    Currency TEXT DEFAULT 'ARS',
    RequestJson TEXT,
    ResponseJson TEXT,
    ErrorMessage TEXT,
    CreatedAt TEXT DEFAULT (datetime('now','localtime'))
);

CREATE TABLE IF NOT EXISTS Stores (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MpStoreId TEXT,
    Name TEXT NOT NULL,
    ExternalId TEXT,
    Address TEXT,
    SyncedAt TEXT
);

CREATE TABLE IF NOT EXISTS Cashiers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MpCashierId INTEGER,
    StoreId INTEGER REFERENCES Stores(Id),
    Name TEXT NOT NULL,
    ExternalId TEXT,
    QrImage TEXT,
    SyncedAt TEXT
);

CREATE TABLE IF NOT EXISTS Customers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MpCustomerId TEXT,
    Email TEXT,
    FirstName TEXT,
    LastName TEXT,
    Phone TEXT,
    SyncedAt TEXT
);

CREATE TABLE IF NOT EXISTS WebhookLog (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EventType TEXT,
    ResourceId TEXT,
    Action TEXT,
    RawJson TEXT,
    Processed INTEGER DEFAULT 0,
    ReceivedAt TEXT DEFAULT (datetime('now','localtime'))
);
";
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }

            // Migración: agregar columnas nuevas a DB existentes
            MigrateAddColumn(conn, "AppConfig", "PlatformId", "TEXT DEFAULT ''");
            MigrateAddColumn(conn, "AppConfig", "ClientId", "TEXT DEFAULT ''");
            MigrateAddColumn(conn, "AppConfig", "ClientSecret", "TEXT DEFAULT ''");
            MigrateAddColumn(conn, "AppConfig", "RefreshToken", "TEXT DEFAULT ''");
        }

        private static void MigrateAddColumn(
            SQLiteConnection conn, string table, string column, string type)
        {
            try
            {
                using (var cmd = new SQLiteCommand(
                    $"ALTER TABLE {table} ADD COLUMN {column} {type}", conn))
                {
                    cmd.ExecuteNonQuery();
                    Log.Debug("Migración: columna {Column} agregada a {Table}.",
                        column, table);
                }
            }
            catch (Exception)
            {
                // La columna ya existe, ignorar
            }
        }
    }
}
