using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;

namespace DBLibrary
{
    public class DBConnection
    {
        public static event Action<LogType, string> Notify;
        //Переделать получать из json
        string connectionString = "server=localhost;port=3306;username=root;password=server;database=dbeventlist";
        private MySqlConnection _connection;
        private MySqlCommand _query;
        public DBConnection()
        {
            _connection = new MySqlConnection(connectionString);
            _query = new MySqlCommand
            {
                Connection = _connection
            };
        }

        public void Open()
        {
            try
            {
                _connection.Open();
            }
            catch (InvalidOperationException)
            {
                Notify?.Invoke(LogType.error, "Ошибка открытия БД");
            }
            catch (MySqlException)
            {
                Notify?.Invoke(LogType.error, "Подключаемся к уже открытой БД");
            }
            catch (Exception)
            {
                Notify?.Invoke(LogType.error, "Путь к базе данных не найден");
            }
        }
        public void Close()
        {
            _connection.Close();
        }
        
        // Запись в БД
        public void RecordToMeet(Meet meet)
        {
            var contentParam = new MySqlParameter("@content", "добавлю позже"); ;
            if (meet.Content != null) contentParam = new MySqlParameter("@content", meet.Content);
            _query.Parameters.Add(contentParam);
            var startParam = new MySqlParameter("@start", meet.Start);
            _query.Parameters.Add(startParam);
            var endingParam = new MySqlParameter("@ending", meet.Ending);
            _query.Parameters.Add(endingParam);
            var noticeParam = new MySqlParameter("@notice", meet.Notice);
            _query.Parameters.Add(noticeParam);

            Open();
            _query.CommandText = $"INSERT INTO meetingschedule (content, start, ending, notice)" +
                    $"VALUES (@content, @start, @ending, @notice)";
            try { 
                _query.ExecuteNonQuery();
                Notify?.Invoke(LogType.info, "Запись в бд сделана");
            }
            catch (Exception e) { Notify?.Invoke(LogType.error, e.ToString()); }
            Close();
        }

        public Meet FindMeet(int id)
        {
            Open();
            _query.CommandText = $"SELECT *FROM meetingschedule where id='{id}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(LogType.warn, "Нет данных о встрече");
                Close();
                return null;
            }
            else
            {
                var meet = new Meet();
                while (result.Read())
                {
                    meet.Id = result.GetInt32(0);
                    meet.Content = result.GetString(1);
                    meet.Start = result.GetDateTime(2);
                    meet.Ending = result.GetDateTime(3);
                    if (!result.IsDBNull(4)) meet.Notice = result.GetDateTime(4);
                }
                //result.Read();                
                //string content = result.GetString(1);
                Close();
                //if (content == string.Empty) Notify?.Invoke(LogType.warn, $"не задано");
                //else Notify?.Invoke(LogType.info, $"найдено");
                //return content;
                return meet;
            }            
        }


        public List<Meet> FindMeetings()
        {
            var meetings = new List<Meet>();
            Open();
            _query.CommandText = $"SELECT * FROM meetingschedule;";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(LogType.warn, $"Нет встреч в списке");
                Close();
                return meetings;
            }
            else
            {
                while (result.Read())
                {
                    var meet = new Meet();
                    meet.Id = result.GetInt32(0);
                    meet.Content = result.GetString(1);
                    meet.Start = result.GetDateTime(2);
                    meet.Ending = result.GetDateTime(3);
                    if (!result.IsDBNull(4)) meet.Notice = result.GetDateTime(4);
                    meetings.Add(meet);
                }
                Close();
                return meetings;
            }
        }
    }
}
