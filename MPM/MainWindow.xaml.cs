using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using DBLibrary;
using Logger;

namespace MPM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LogToFile log;    //объект логгирования в файл
        DBConnection db;  //объект взаимодействия с БД
        
        List<Meet> meetingsList;  // список встреч из БД
        ObservableCollection<Meet> meetings;   // обновляемая коллекция для вывода в ListView

        int idMeet;
        string changeContent;
        string changeNotice;

        DispatcherTimer timer;
        public Task taskNotice;
        public static event Action<LogType, string> Notify;

        public MainWindow()
        {
            InitializeComponent();

            db = new DBConnection();
            meetingsList = db.FindAllMeetings();
            meetings = new ObservableCollection<Meet>(meetingsList);
            MeetingsList.ItemsSource = meetings;

            changeContent = string.Empty;
            changeNotice = string.Empty;

            log = new();
            DBConnection.Notify += log.RecordToLog;
            SaveToFile.Notify += ShowNotify;
            Notify += log.RecordToLog;
            Notify?.Invoke(LogType.info, "START");

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(30);
            timer.Tick += timerTick;
            timer.Start();    //запуск проверки уведомлений        
        }
        //фильтр списка встреч за период
        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = ConversionToDateTime(TextBox_FStart.Text);
            DateTime end = ConversionToDateTime(TextBox_FEnd.Text);
            meetingsList = db.FindMeetingsByPeriod(start, end);
            if (meetingsList != null) MeetingsList.ItemsSource = new ObservableCollection<Meet>(meetingsList);
        }
        //очистка фильтра встреч
        private void СlearFilter_Click(object sender, RoutedEventArgs e)
        {            
            meetingsList = db.FindAllMeetings();
            MeetingsList.ItemsSource = new ObservableCollection<Meet>(meetingsList);
            State.Text = "Фильтр очищен";
        }
        //запись списка отобранного встреч в файл
        private void Discharge_Click(object sender, RoutedEventArgs e)
        {
            SaveToFile.RecordToFile(meetingsList);
            State.Text = "Встречи записаны в файл";
        }
        //сортировка списка-не реализована
        private void SortStartDate_Click(object sender, RoutedEventArgs e)
        {
            // а нужна ли она??
            State.Text = "Сортировка не реализована";
        }
        //запись новой встречи
        private void SaveMeet_Click(object sender, RoutedEventArgs e)
        {            
            var meet = CheсkMeet();
            if (meet !=null)
            {
                db.RecordMeet(meet);
                //meetings.Add(meet);
                meetingsList = db.FindAllMeetings();
                MeetingsList.ItemsSource = new ObservableCollection<Meet>(meetingsList);
            }
        }
        //изменение встречи
        private void ChangeMeet_Click(object sender, RoutedEventArgs e)
        {
            var meet = CheсkMeet();
            if (meet != null)
            {
                meet.Id = idMeet;
                db.UpdateMeet(meet);
            }
            else
            {
                if (changeContent != string.Empty)
                {
                    db.UpdateMeet(idMeet, changeContent);
                    changeContent = string.Empty;
                }
                if (changeNotice != string.Empty)
                {
                    db.UpdateMeet(idMeet, ConversionToDateTime(changeNotice));
                    changeNotice = string.Empty;
                }
            }
            meetingsList = db.FindAllMeetings();
            MeetingsList.ItemsSource = new ObservableCollection<Meet>(meetingsList);
        }
        //очиствка формы записи встречи
        private void СlearForm_Click(object sender, RoutedEventArgs e)
        {
            TextBox_MeetContent.Text = "";
            TextBox_Start.Text = TextBox_Start.PlaceHolderText;
            TextBox_Ending.Text = TextBox_Ending.PlaceHolderText;
            TextBox_Notice.Text = TextBox_Notice.PlaceHolderText;
            changeContent = string.Empty;
            State.Text = "Форма очищена";
        }
        //изменение текста контента
        private void TextBox_MeetContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            changeContent = TextBox_MeetContent.Text.ToString();
        }
        //изменение уведомления
        private void TextBox_Notice_TextChanged(object sender, TextChangedEventArgs e)
        {
            changeNotice = TextBox_Notice.Text.ToString();
        }
        //изменить встречу в списке
        private void MenuItem_Click_Change(object sender, RoutedEventArgs e)
        {
            var meet = MeetingsList.SelectedItem as Meet;
            TextBox_MeetContent.Text = meet.Content;
            TextBox_Start.Text = meet.Start.ToString("yyyy:MM:dd:hh:mm"); 
            TextBox_Ending.Text = meet.Ending.ToString("yyyy:MM:dd:hh:mm");
            TextBox_Notice.Text = meet.Notice.ToString("yyyy:MM:dd:hh:mm");
            idMeet = meet.Id;
            State.Text = idMeet.ToString();
        }
        //удалить встречу из списка
        private void MenuItem_Click_Delete(object sender, RoutedEventArgs e)
        {
            var meet = MeetingsList.SelectedItem as Meet;
            MessageBox.Show(meet.Id.ToString());
            db.DeletMeet(meet.Id);
            meetings = new ObservableCollection<Meet>(db.FindAllMeetings());
            MeetingsList.ItemsSource = meetings;
            State.Text = "Встреча удалена";
        }

        //проверка введенных данных о встрече       
        private Meet CheсkMeet()
        {
            // довести до ума
            var meet = new Meet();
            if (TextBox_MeetContent.Text != string.Empty)
                meet.Content = TextBox_MeetContent.Text;
            else
            {
                MessageBox.Show("Контент нужно заполнить");
                return null;
            }
            meet.Start = ConversionToDateTime(TextBox_Start.Text);
            meet.Ending = ConversionToDateTime(TextBox_Ending.Text);
            if (meet.Start <= DateTime.Now || meet.Start == DateTime.MinValue)
            {
                MessageBox.Show("Планирование начала встречи возможно только на будущее время");
                return null;
            }
            if (meet.Ending <= meet.Start)
            {
                MessageBox.Show("Окончание встречи возможно только после ее начала");
                return null;
            }
            if (TextBox_Notice.Text != string.Empty)
            {
                meet.Notice = ConversionToDateTime(TextBox_Notice.Text);
                if (meet.Notice <= DateTime.Now || meet.Notice >= meet.Start)
                {
                    MessageBox.Show("Планирование уведомления возможно только на будущее время и до начала встречи");
                    return null;
                }
            }
            
            if (db.CheckPeriod(meet.Start, meet.Ending)) return meet;
            else return null;
           
        }
        //преобразование даты
        private static DateTime ConversionToDateTime(string time)
        {
            DateTime dateTime = DateTime.MinValue;
            string[] timeArr = time.Split(':');
            if (timeArr.Length != 5) { MessageBox.Show($"Неправильно введена дата: {time}"); }
            else
            {
                try
                {
                    int ye = Convert.ToInt32(timeArr[0]);
                    int mo = Convert.ToInt32(timeArr[1]);
                    int da = Convert.ToInt32(timeArr[2]);
                    int ho = Convert.ToInt32(timeArr[3]);
                    int mi = Convert.ToInt32(timeArr[4]);
                    dateTime = new DateTime(ye, mo, da, ho, mi, 00);
                }
                catch { MessageBox.Show($"Неправильно введена дата: {time}"); }
            }
            return dateTime;
        }

        //запуск проверки уведомлений     
        private void timerTick(object sender, EventArgs e)
        {
            timer.IsEnabled = true;
            if (timer.IsEnabled)
            {
                taskNotice = new(() => { FindNotice(); });
                taskNotice.Start();
                //State.Text = "Старт проверки уведомлений";
                //Notify?.Invoke(LogType.info, "Старт проверки уведомлений");
            }
            else Notify?.Invoke(LogType.info, "Не запущена проверка уведомлений"); 
        }
        //поиск актуальных уведомлений и вывод на страницу уведомлений
        private void FindNotice()
        {
            Dispatcher.Invoke(new Action(() =>
            {               
            Notify?.Invoke(LogType.info, "FindNotice начато");
            string type = "no";
            var deadlineList = db.CheckDeadlineNotice();
            if (deadlineList != null && deadlineList.Count > 0) type = "yes";
            if (type == "no")
            {
                Notify?.Invoke(LogType.info, "Нет уведомлений о начале встреч");
                State.Text = "Нет уведомлений о начале встреч";
            }
            if (type == "yes")
            {
                NoticeMeetings.Background = Brushes.Lavender;
                NoticeMeetings.Text = Meet.NoticeToString(deadlineList);
                MessageBox.Show("Есть уведомления о начале встреч");
                Notify?.Invoke(LogType.info, "Есть уведомления о начале встреч");
            }
        }), null);
        }

        //вывод сообщений о событиях в строку состояния
        private void ShowNotify(LogType type, string message)
        {
            State.Text = message;
        }

    }
}

/*  TO DO
connectionString  получать из файла json
Сделать валидацию для дат
Сделать так, чтобы время минимальное не записывалось-не отображалось
*/