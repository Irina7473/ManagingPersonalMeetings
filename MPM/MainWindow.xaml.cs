using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        ObservableCollection<Meet> meetings;
        List<Meet> meetingsList;
        DBConnection db;
        int idMeet;
        string changeContent;
        string changeNotice;
        DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            TextBox_Start.Text = "ГГГГ:М:Д:Ч:М";
            TextBox_Ending.Text = "ГГГГ:М:Д:Ч:М";
            TextBox_Notice.Text = "ГГГГ:М:Д:Ч:М";
            TextBox_FStart.Text = "ГГГГ:М:Д:Ч:М";
            TextBox_FEnd.Text = "ГГГГ:М:Д:Ч:М";

            db = new DBConnection();
            meetingsList = db.FindAllMeetings();
            meetings = new ObservableCollection<Meet>(meetingsList);
            MeetingsList.ItemsSource = meetings;

            changeContent = string.Empty;
            changeNotice = string.Empty;

            DBConnection.Notify += ShowNotify;            
            SaveToFile.Notify += ShowNotify;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += timer_Tick;
            //timer.Start();     //программа зависает после нахождения уведомления.
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = ConversionToDateTime(TextBox_FStart.Text);
            DateTime end = ConversionToDateTime(TextBox_FEnd.Text);
            meetingsList = db.FindMeetingsByPeriod(start, end);
            if (meetingsList != null) MeetingsList.ItemsSource = new ObservableCollection<Meet>(meetingsList);
        }

        private void СlearFilter_Click(object sender, RoutedEventArgs e)
        {
            TextBox_FStart.Text= "ГГГГ:М:Д:Ч:М";
            TextBox_FEnd.Text = "ГГГГ:М:Д:Ч:М";
            meetingsList = db.FindAllMeetings();
            MeetingsList.ItemsSource = new ObservableCollection<Meet>(meetingsList);
            State.Text = "Фильтр очищен";
        }

        private void Discharge_Click(object sender, RoutedEventArgs e)
        {
            SaveToFile.RecordToFile(meetingsList);
            State.Text = "Встречи записаны в файл";
        }

        private void StartNotice_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
            State.Text = "Старт2 проверки уведомлений";
        }

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

        private void СlearForm_Click(object sender, RoutedEventArgs e)
        {
            TextBox_MeetContent.Text = "";
            TextBox_Start.Text = "ГГГГ:М:Д:Ч:М";
            TextBox_Ending.Text = "ГГГГ:М:Д:Ч:М";
            TextBox_Notice.Text = "ГГГГ:М:Д:Ч:М";
            changeContent = string.Empty;
            State.Text = "Форма очищена";
        }

        private void TextBox_MeetContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            changeContent = TextBox_MeetContent.Text.ToString();
        }

        private void TextBox_Notice_TextChanged(object sender, TextChangedEventArgs e)
        {
            changeNotice = TextBox_Notice.Text.ToString();
        }

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

        private void MenuItem_Click_Delete(object sender, RoutedEventArgs e)
        {
            var meet = MeetingsList.SelectedItem as Meet;
            MessageBox.Show(meet.Id.ToString());
            db.DeletMeet(meet.Id);
            meetings = new ObservableCollection<Meet>(db.FindAllMeetings());
            MeetingsList.ItemsSource = meetings;
            State.Text = "Встреча удалена";
        }
               
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
               
        private void timer_Tick(object sender, EventArgs e)
        {
            timer.IsEnabled = true;
            if (timer.IsEnabled)
            {
                State.Text = "Старт1 проверки уведомлений";
                while (true)
                {
                    var deadlineList = db.CheckDeadlineNotice();
                    if (deadlineList != null)
                    {
                        NoticeMeetings.Background = Brushes.DarkViolet;
                        NoticeMeetings.Text = Meet.NoticeToString(deadlineList);
                        State.Text = "Есть уведомления о начале встреч";
                    }
                    else State.Text = "Нет уведомлений о начале встреч";
                }
            }
            else State.Text = "Не запущена проверка уведомлений";
        }

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

Доработать изменение уведомления

Уведомление пользователя программу вводит в ступор после нахождения уведомления.

*/