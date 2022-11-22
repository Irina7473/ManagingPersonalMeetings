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
        /* Не получилось        
        private const string jsonFile = "pathDB.json";
        private string connectionString = ConnectStringSet();
        private static string ConnectStringSet()
        {
            string text = "";
            try
            {
                text = File.ReadAllText(jsonFile);
                Notify?.Invoke(LogType.info, $"{jsonFile} прочитан.  {text}");
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

            try
            {
                string constr = JsonSerializer.Deserialize<ConStr>(text).ToString();
                return constr;
            }

            catch (JsonException)
            {
                Notify?.Invoke(LogType.error, $"Недопустимый JSON {jsonFile}");
                return null;
            }
            catch (NotSupportedException)
            {
                Notify?.Invoke(LogType.error, "Совместимые объекты JsonConverter для TValue или его сериализуемых членов отсутствуют.");
                return null;
            }
            catch
            {
                Notify?.Invoke(LogType.error, $"Неизвестная ошибка при сериализации {jsonFile}");
                return null;
            }
        }*/
        
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

        // Запись встречи в БД
        public void RecordMeet(Meet meet)
        {           
            _query.Parameters.Clear();
            string content = String.IsNullOrEmpty(meet.Content) ? "добавлю позже" : meet.Content;
            var contentParam = new MySqlParameter("@content", content);
            _query.Parameters.Add(contentParam);
            var startParam = new MySqlParameter("@start", meet.Start);
            _query.Parameters.Add(startParam);
            var endingParam = new MySqlParameter("@ending", meet.Ending);
            _query.Parameters.Add(endingParam);
            
                Open();
                if (meet.Notice > DateTime.MinValue)
                {
                    var noticeParam = new MySqlParameter("@notice", meet.Notice);
                    _query.Parameters.Add(noticeParam);
                    _query.CommandText = $"INSERT INTO meetingschedule (content, start, ending, notice)" +
                        $"VALUES (@content, @start, @ending, @notice)";
                }
                else
                {
                    _query.CommandText = $"INSERT INTO meetingschedule (content, start, ending)" +
                        $"VALUES (@content, @start, @ending)";
                }
                try
                {
                    _query.ExecuteNonQuery();
                    Notify?.Invoke(LogType.info, $"Запись в бд сделана: {meet.Content}");
                }
                catch (Exception e) { Notify?.Invoke(LogType.error, e.ToString()); }
                Close();                        
        }
        // Изменение записи
        public void UpdateMeet(int id, string content)
        {
            _query.Parameters.Clear();
            _query.Parameters.Add(new MySqlParameter("@content", content));
            _query.CommandText = $"UPDATE meetingschedule SET content=@content WHERE id='{id}';";
            try
            {
                Open();
                _query.ExecuteNonQuery();
                Notify?.Invoke(LogType.info, $"Контент встречи изменен: {id}");
                Close();

            }
            catch (Exception e) { Notify?.Invoke(LogType.error, e.ToString()); }
        }
        public void UpdateMeet(int id, DateTime notice)
        {
            _query.Parameters.Clear();
            _query.Parameters.Add(new MySqlParameter("@notice", notice));
            _query.CommandText = $"UPDATE meetingschedule SET notice=@notice WHERE id='{id}';";
            try
            {
                Open();
                _query.ExecuteNonQuery();
                Notify?.Invoke(LogType.info, $"Уведомление о встрече изменено: {id}");
                Close();

            }
            catch (Exception e) { Notify?.Invoke(LogType.error, e.ToString()); }
        }
        public void UpdateMeet(Meet meet)
        {
            if (!ExistMeet(meet.Id)) Notify?.Invoke(LogType.warn, "Нет данных для изменения");
            else
            {
                _query.Parameters.Clear();
                string content = String.IsNullOrEmpty(meet.Content) ? "добавлю позже" : meet.Content;
                //var contentParam = new MySqlParameter("@content", content);
                _query.Parameters.Add(new MySqlParameter("@content", content));
                //var startParam = new MySqlParameter("@start", meet.Start);
                _query.Parameters.Add(new MySqlParameter("@start", meet.Start));
                //var endingParam = new MySqlParameter("@ending", meet.Ending);
                _query.Parameters.Add(new MySqlParameter("@ending", meet.Ending));
                if (meet.Notice > DateTime.MinValue)
                {
                    //var noticeParam = new MySqlParameter("@notice", meet.Notice);
                    _query.Parameters.Add(new MySqlParameter("@notice", meet.Notice));
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
                    Open();
                    _query.ExecuteNonQuery();
                    Notify?.Invoke(LogType.info, $"Данные в бд изменены: {meet.Id}");
                    Close();
                    
                }
                catch (Exception e) { Notify?.Invoke(LogType.error, e.ToString()); }
            }
        }
        // Проверка возможности записи встреч в заданный период
        public Boolean CheckPeriod(string comText)
        {
            Open();
            _query.CommandText = comText;
            var result = _query.ExecuteReader();            
            if (!result.HasRows)
            {
                Close();
                return true;
            }
            else
            {
                Close();
                Notify?.Invoke(LogType.warn, "На этот период уже назначены встречи 3");
                return false;
            }            
        }
        public Boolean CheckPeriod(DateTime begPer, DateTime endPer)
        {
            _query.Parameters.Clear();
            var startParam = new MySqlParameter("@start", begPer);
            _query.Parameters.Add(startParam);
            var endingParam = new MySqlParameter("@ending", endPer);
            _query.Parameters.Add(endingParam);
            var comText = $"SELECT *FROM meetingschedule " +
                $"WHERE (start >= @start AND start <= @ending) " +
                $"OR (ending >= @start AND ending <= @ending) " +
                $"OR (start <= @start AND ending >= @ending) ";
            if (CheckPeriod(comText))
            {
                return true;
            }
            else
            {
                Notify?.Invoke(LogType.warn, "На этот период уже назначены встречи 4");
                return false;
            }
        }
        // Удаление записи
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
        // Проверка наличия записи
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
        // Поиск записи по id
        public Meet FindMeet(int id)
        {            
            Open();
            _query.CommandText = $"SELECT *FROM meetingschedule WHERE id='{id}';";
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
        // Получение списка всех записей, соответствующих запросу
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
                //Notify?.Invoke(LogType.info, $"Список встреч создан");
                return meetings;
            }
        }
        // Получение списка всех записей
        public List<Meet> FindAllMeetings()
        {
            string comText = $"SELECT * FROM meetingschedule;";
            return FindMeetings(comText);
        }
        // Получение списка записей за период
        public List<Meet> FindMeetingsByPeriod(DateTime begPer, DateTime endPer)
        {
            _query.Parameters.Clear();
            var begParam = new MySqlParameter("@begPer", begPer);
            _query.Parameters.Add(begParam);
            var endParam = new MySqlParameter("@endPer", endPer);
            _query.Parameters.Add(endParam);
            string comText = $"SELECT * FROM meetingschedule WHERE start>@begPer AND start<@endPer;";
            return FindMeetings(comText);            
        }
        // Получение списка по части контента
        public List<Meet> FindMeetingsByLine(string line)
        {
            string comText = "SELECT * FROM meetingschedule WHERE content LIKE '%" + line + "%';";
            return FindMeetings(comText);
        }
        // Проверка уведомлений о начале встреч
        public List<Meet> CheckDeadlineNotice()
        {
            DateTime deadline = DateTime.Now;
            _query.Parameters.Clear();
            var deadlineParam = new MySqlParameter("@deadline", deadline);
            _query.Parameters.Add(deadlineParam);
            string comText = $"SELECT * FROM meetingschedule WHERE notice=@deadline;";
            return FindMeetings(comText);
        }
    }

    /*
   public  class ConStr
    {
        string server; 
        int port; 
        string username; 
        string password;
        string database;

        public ConStr(string server, int port, string username, string password,string database)
        {
            this.server = server;
            this.port = port;
            this.username = username;
            this.password = password;
            this.database = database;
        }
        public string ToString()
        {
            return "server="+server+";port="+port+";username="+username+";password="+password+";database="+database;
        }
    }
    */
}
