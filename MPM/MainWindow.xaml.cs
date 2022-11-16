using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public MainWindow()
        {
            InitializeComponent();

            TextBox_Start.Text = "ГГГГ:ММ:ДД:ЧЧ:ММ";
            TextBox_Ending.Text = "ГГГГ:ММ:ДД:ЧЧ:ММ";
            TextBox_Notice.Text = "ГГГГ:ММ:ДД:ЧЧ:ММ";

            db = new DBConnection();
            meetingsList = db.FindAllMeetings();
            meetings = new ObservableCollection<Meet>(meetingsList);
            MeetingsList.ItemsSource = meetings;

            DBConnection.Notify += ShowNotify;
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
            TextBox_FStart.Text="";
            TextBox_FEnd.Text = "";
            meetingsList = db.FindAllMeetings();
            MeetingsList.ItemsSource = new ObservableCollection<Meet>(meetingsList);
            State.Text = "Фильтр очищен";
        }

        private void Discharge_Click(object sender, RoutedEventArgs e)
        {
            SaveToFile.RecordToFile(meetingsList);
            State.Text = "Встречи записаны в файл";
        }       

        private void SaveMeet_Click(object sender, RoutedEventArgs e)
        {            
            var meet = CheсkMeet();
            if (meet !=null)
            {
                db.RecordMeet(meet);
                meetings.Add(meet);
                State.Text = "Встреча записана";
            }
        }

        private void ChangeMeet_Click(object sender, RoutedEventArgs e)
        {
            var meet = CheсkMeet();
            if (meet != null)
            {                
                db.UpdateMeet(meet);
                meetingsList = db.FindAllMeetings();
                MeetingsList.ItemsSource = new ObservableCollection<Meet>(meetingsList);                
                State.Text = "Встреча изменена";
            }
        }

        private void СlearForm_Click(object sender, RoutedEventArgs e)
        {
            TextBox_MeetContent.Text = "";
            TextBox_Start.Text = "ГГГГ:ММ:ДД:ЧЧ:ММ";
            TextBox_Ending.Text = "ГГГГ:ММ:ДД:ЧЧ:ММ";
            TextBox_Notice.Text = "ГГГГ:ММ:ДД:ЧЧ:ММ";            
        }

        private void TextBox_MeetContent_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void MenuItem_Click_Change(object sender, RoutedEventArgs e)
        {
            var meet = MeetingsList.SelectedItem as Meet;
            TextBox_MeetContent.Text = meet.Content;
            TextBox_Start.Text = meet.Start.ToString();
            TextBox_Ending.Text = meet.Ending.ToString();
            TextBox_Notice.Text = meet.Notice.ToString();
            idMeet = meet.Id;
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

        private Meet CheсkMeet()
        {
            // довести до ума
            // 2022:12:20:08:00
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
                meet.Notice = ConversionToDateTime(TextBox_Notice.Text);
            if (meet.Notice <= DateTime.Now || meet.Notice >= meet.Start)
            {
                MessageBox.Show("Планирование уведомления возможно только на будущее время и до начала встречи");
                return null;
            }
            return meet;
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
При этом встречи не должны пересекаться.
Сделать так, чтобы время минимальное не записывалось-не отображалось

При наступлении времени напоминания приложение информирует пользователя о предстоящей встрече и времени ее начала.

*/