using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;
using System.IO;
using System.Text.Json;

namespace DBLibrary
{
    public class DBConnection
    {
        public static event Action<LogType, string> Notify;

        //Переделать получать из json
        private string connectionString = "server=localhost;port=3306;username=root;password=server;database=dbeventlist";
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
        public void RecordMeet(Meet meet)
        {
            string content = String.IsNullOrEmpty(meet.Content) ? "добавлю позже" : meet.Content;
            var contentParam = new MySqlParameter("@content", content);
            _query.Parameters.Add(contentParam);
            var startParam = new MySqlParameter("@start", meet.Start);
            _query.Parameters.Add(startParam);
            var endingParam = new MySqlParameter("@ending", meet.Ending);
            _query.Parameters.Add(endingParam);
            if (meet.Notice > DateTime.MinValue)
            {
                var noticeParam = new MySqlParameter("@notice", meet.Notice);
                _query.Parameters.Add(noticeParam);
                Open();
                _query.CommandText = $"INSERT INTO meetingschedule (content, start, ending, notice)" +
                    $"VALUES (@content, @start, @ending, @notice)";
            }
            else
            {
                Open();
                _query.CommandText = $"INSERT INTO meetingschedule (content, start, ending)" +
                    $"VALUES (@content, @start, @ending)";
            }
            try { 
                _query.ExecuteNonQuery();
                Notify?.Invoke(LogType.info, $"Запись в бд сделана: {meet.ToString()}");;
            }
            catch (Exception e) { Notify?.Invoke(LogType.error, e.ToString()); }
            Close();
        }

        public void UpdateMeet(Meet meet)
        {
            if (!ExistMeet(meet.Id)) Notify?.Invoke(LogType.warn, "Нет данных для изменения");
            else
            {
                Open();
                string content = String.IsNullOrEmpty(meet.Content) ? "добавлю позже" : meet.Content;
                var contentParam = new MySqlParameter("@content", content);
                _query.Parameters.Add(contentParam);
                var startParam = new MySqlParameter("@start", meet.Start);
                _query.Parameters.Add(startParam);
                var endingParam = new MySqlParameter("@ending", meet.Ending);
                _query.Parameters.Add(endingParam);
                if (meet.Notice > DateTime.MinValue)
                {
                    var noticeParam = new MySqlParameter("@notice", meet.Notice);
                    _query.Parameters.Add(noticeParam);
                    _query.CommandText = $"UPDATE meetingschedule SET content=@content, " +
                        $"start=@start, ending=@ending, notice=@notice  WHERE id='{meet.Id}'; ";                       
                }
                else
                {
                    _query.CommandText = $"UPDATE meetingschedule SET content=@content, " +
                        $"start=@start, ending=@ending, notice=NULL  WHERE id='{meet.Id}';";
                }
                try
                {
                    _query.ExecuteNonQuery();
                    Notify?.Invoke(LogType.info, $"Данные в бд изменены: {meet.ToString()}"); ;
                }
                catch (Exception e) { Notify?.Invoke(LogType.error, e.ToString()); }
                Close();
            }
        }

        public void DeletMeet(int id)
        {
            if (!ExistMeet(id)) Notify?.Invoke(LogType.warn, "Нет данных для удаления");
            else
            {
                Open();
                _query.CommandText = $"DELETE FROM meetingschedule where id='{id}';";
                int rowCount = _query.ExecuteNonQuery();
                Notify?.Invoke(LogType.info, $"Данные о встрече удалены, id={id}");
                Close();
            }
        }

        public bool ExistMeet(int id)
        {
            Open();
            _query.CommandText = $"SELECT *FROM meetingschedule where id='{id}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(LogType.warn, $"Нет данных о встрече, id={id}");
                Close();
                return false;
            }
            else
            {                
                Close();
                return true;
            }
        }

        public Meet FindMeet(int id)
        {            
            Open();
            _query.CommandText = $"SELECT *FROM meetingschedule where id='{id}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows) 
            { 
                Notify?.Invoke(LogType.warn, $"Нет данных о встрече, id={id}");
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
                Close();
                return meet;
            }
        }

        public List<Meet> FindMeetings(string comText)
        {
            var meetings = new List<Meet>();
            Open();            
            _query.CommandText = comText;
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(LogType.warn, $"Нет встреч в списке");
                Close();
                return null;
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
                Notify?.Invoke(LogType.info, $"Список встреч создан");
                return meetings;
            }
        }

        public List<Meet> FindAllMeetings()
        {
            string comText = $"SELECT * FROM meetingschedule;";
            return FindMeetings(comText);
        }

        public List<Meet> FindMeetingsByPeriod(DateTime begPer, DateTime endPer)
        {            
            var begParam = new MySqlParameter("@begPer", begPer);
            _query.Parameters.Add(begParam);
            var endParam = new MySqlParameter("@endPer", endPer);
            _query.Parameters.Add(endParam);
            string comText = $"SELECT * FROM meetingschedule WHERE start>@begPer AND start<@endPer;";
            return FindMeetings(comText);            
        }

        public List<Meet> FindMeetingsByLine(string line)
        {
            string comText = "SELECT * FROM meetingschedule WHERE content LIKE '%" + line + "%';";
            return FindMeetings(comText);
        }

    }
}

/*
 * Этот код не работает - не находит файл
 *  private const string jsonFile = "pathsDB.json";
        private string connectionString = ConnectStringSet();

        public static string ConnectStringSet()
        {
            //string text = "";
            try
            {
                string text = File.ReadAllText(jsonFile);
                Notify?.Invoke(LogType.info, $"{jsonFile} прочитан.");
                return text;
            }
            catch (FileNotFoundException)
            {
                Notify?.Invoke(LogType.error, $"{jsonFile} не найден.");
                return null;
            }
            catch (IOException)
            {
                Notify?.Invoke(LogType.error, $"При открытии файла {jsonFile} произошла ошибка ввода-вывода.");
                return null;
            }
            catch (NotSupportedException)
            {
                Notify?.Invoke(LogType.error, $"Параметр {jsonFile} задан в недопустимом формате.");
                return null;
            }
            catch
            {
                Notify?.Invoke(LogType.error, $"Неизвестная ошибка при чтении {jsonFile}");
                return null;
            }          
        }
*/