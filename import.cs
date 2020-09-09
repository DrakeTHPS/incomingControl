using System;
using System.Data.SQLite;
using System.IO;
using System.Data;
using MySql.Data.MySqlClient;

namespace Data
{
    class import
    {
        static SQLiteConnection m_dbConn;
        static SQLiteCommand m_sqlCmd;
        static MySqlConnection con;
        static MySqlCommand cmd;
        static void createTables()
        {
            string line;
            StreamReader querys = new StreamReader("script.txt");
            while((line=querys.ReadLine())!=null)
            {
                cmd.CommandText = line;
                cmd.ExecuteNonQuery();
            }
        }

        static void fillCinemas()
        {          
            DataTable dTable = new DataTable();
            string sqlQuery = "select distinct city, cinemaAddress from sessions";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, m_dbConn);
            adapter.Fill(dTable);

            if (dTable.Rows.Count > 0)
            {
                for (int i = 0; i < dTable.Rows.Count; i++)
                {
                    cmd.CommandText = "insert into cinemas(city, cinema_address) values ('" + dTable.Rows[i]["city"] + "', '" + dTable.Rows[i]["cinemaAddress"] + "')";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        static void fillMovies()
        {
            DataTable dTable = new DataTable();
            string sqlQuery = "select DISTINCT movie, genre, duration, ageLimit from sessions";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, m_dbConn);
            adapter.Fill(dTable);

            if (dTable.Rows.Count > 0)
            {
                for (int i = 0; i < dTable.Rows.Count; i++)
                {
                    cmd.CommandText = "insert into movies(name, genre, duration, age_limit) values ('" + dTable.Rows[i]["movie"] + "', '" + dTable.Rows[i]["genre"] + "', " + dTable.Rows[i]["duration"] + ", " + dTable.Rows[i]["ageLimit"] + ")";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        static void fillHallTypes()
        {
            DataTable dTable = new DataTable();
            string sqlQuery = "select DISTINCT hallType, format, cost from sessions";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, m_dbConn);
            adapter.Fill(dTable);

            if (dTable.Rows.Count > 0)
            {
                for (int i = 0; i < dTable.Rows.Count; i++)
                {
                    cmd.CommandText = "insert into hall_types(type, format, cost) values ('" + dTable.Rows[i]["hallType"] + "', '" + dTable.Rows[i]["format"] + "', " + dTable.Rows[i]["cost"] + ")";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        static void fillHalls()
        {
            DataTable dTable = new DataTable();
            string sqlQuery = "select DISTINCT city, cinemaAddress, hallNumber, hallType, format from sessions";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, m_dbConn);
            adapter.Fill(dTable);

            if (dTable.Rows.Count > 0)
            {
                for (int i = 0; i < dTable.Rows.Count; i++)
                {
                    string sql = "SELECT id FROM cinemas where city='"+ dTable.Rows[i]["city"] + "' and cinema_address='"+ dTable.Rows[i]["cinemaAddress"] + "'";
                    MySqlCommand temp = new MySqlCommand(sql, con);
                    MySqlDataReader rdr = temp.ExecuteReader();
                    rdr.Read();
                    int cinema_id = rdr.GetInt32(0);
                    rdr.Close();
                    sql = "SELECT id FROM hall_types where type='" + dTable.Rows[i]["hallType"] + "' and format='" + dTable.Rows[i]["format"] + "'";
                    temp = new MySqlCommand(sql, con);                
                    rdr = temp.ExecuteReader();
                    rdr.Read();
                    int hall_type_id = rdr.GetInt32(0);
                    rdr.Close();

                    cmd.CommandText = "insert into halls(cinema_id, hall_number, hall_type_id) values (" + cinema_id + ", " + dTable.Rows[i]["hallNumber"] + ", " + hall_type_id + ")";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        static void fillSessions()
        {
            DataTable dTable = new DataTable();
            string sqlQuery = "select DISTINCT city, cinemaAddress, hallNumber, datetime, movie, available from sessions";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, m_dbConn);
            adapter.Fill(dTable);

            if (dTable.Rows.Count > 0)
            {
                for (int i = 0; i < dTable.Rows.Count; i++)
                {
                    string sql = "SELECT h.id FROM halls h inner join cinemas c on h.cinema_id = c.id where c.city='" + dTable.Rows[i]["city"] + "' and c.cinema_address='" + dTable.Rows[i]["cinemaAddress"] + "' and h.hall_number="+ dTable.Rows[i]["hallNumber"];
                    MySqlCommand temp = new MySqlCommand(sql, con);
                    MySqlDataReader rdr = temp.ExecuteReader();
                    rdr.Read();
                    int hall_id = rdr.GetInt32(0);
                    rdr.Close();
                    sql = "SELECT id FROM movies where name='" + dTable.Rows[i]["movie"] + "'";
                    temp = new MySqlCommand(sql, con);
                    rdr = temp.ExecuteReader();
                    rdr.Read();
                    int movie_id = rdr.GetInt32(0);
                    rdr.Close();

                    cmd.CommandText = "insert into sessions(hall_id, datetime, movie_id, available) values (" + hall_id + ", '" + dTable.Rows[i]["datetime"].ToString().Substring(0,18) + "', " + movie_id + ", " + dTable.Rows[i]["available"] + ")";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        static void Main(string[] args)
        {           
            string path = "C:\\Users\\DrakeTHPS\\SQLite dbs\\sessions.db";
            if (File.Exists(path))
            {
                m_dbConn = new SQLiteConnection();
                m_sqlCmd = new SQLiteCommand();
                m_dbConn = new SQLiteConnection("Data Source=" + path + ";Version=3;");
                m_dbConn.Open();
                m_sqlCmd.Connection = m_dbConn;

                con = new MySqlConnection("server=localhost;userid=root;password=2358;database=sessions;charset=utf8");
                con.Open();
                cmd = new MySqlCommand();
                cmd.Connection = con;

                createTables();
                Console.WriteLine("Нормализованная БД создана");
                fillCinemas();
                fillMovies();
                fillHallTypes();
                fillHalls();
                fillSessions();
                Console.WriteLine("Данные переданы из ненормализованной в нормализованную БД");
                exportToExcel.export();
                Console.WriteLine("Нормализованная БД экспортирована в Excel");
            }
            else
            {
                Console.WriteLine("no such file");
            }
            
        }
    }
}
