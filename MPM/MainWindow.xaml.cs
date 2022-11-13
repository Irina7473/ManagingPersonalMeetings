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

namespace MPM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        new ObservableCollection<Meet> meetings;

        public MainWindow()
        {
            InitializeComponent();

            var db = new DBConnection();
            meetings = new ObservableCollection<Meet>(db.FindAllMeetings());
            MeetingsList.ItemsSource = meetings;
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddMeet_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Discharge_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveMeet_Click(object sender, RoutedEventArgs e)
        {
            // исправить преобразование в Datatime
            var meet = new Meet(TextBox_MeetContent.Text, DateTime.Parse(TextBox_Start.Text), DateTime.Parse(TextBox_Limit.Text));
            if (TextBox_Notice.Text != null) meet.Notice = DateTime.Parse(TextBox_Notice.Text);
            meetings.Add(meet);
        }

        private void ChangeMeet_Click(object sender, RoutedEventArgs e)
        {

        }

        private void СlearForm_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_MeetContent_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void MenuItem_Click_Change(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click_Delete(object sender, RoutedEventArgs e)
        {

        }
    }
}


/*
 
namespace TasksListWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>    

    public partial class MainWindow : Window
    {
        //Не годится public ObservableCollection<Objective>, т.к. не дает возможность раскрасить строки каждую в свой цвет
        List<ListViewItem> ITEMS = new List<ListViewItem>();
        Dictionary<int, ImportanceTable> level;

        int importance = 0;
        string taskContent = "";
        string limit = "";
        Objective changeTask = new Objective();        

        
        public MainWindow()
        {
            InitializeComponent();

            SaveTask.IsEnabled = false;
            ChangeTask.IsEnabled = false;
            СlearForm.IsEnabled = false;
            ChangeColor.IsEnabled = false;
            
            
        }

        private void Creating_Click(object sender, RoutedEventArgs e)
        {            
            ITEMS.Clear();
            ObjectiveList.Items.Refresh();
            level = ImportanceTable.CreatTaskList();
            State.Text = "Новый список задач создан";
        }

        private void Addendum_Click(object sender, RoutedEventArgs e)
        {
            if (level == null)
            {
                ImportanceTable.Info = msg => MessageBox.Show(msg);
                level = ImportanceTable.CreatTaskList();
            }
            SaveTask.IsEnabled = true;
            СlearForm.IsEnabled = true;
            State.Text = "Добавление задач активизировано";
        }

        private void Uploading_Click(object sender, RoutedEventArgs e)
        {                       
            ITEMS.Clear();
            ObjectiveList.Items.Refresh();
            level = SaveToFile.ReaderFromFail();
            UpdateObjectiveList();
            State.Text = "Список задач загружен из файла";            
        }

        private void Discharge_Click(object sender, RoutedEventArgs e)
        {
            SaveToFile.Info = msg => MessageBox.Show(msg);
            SaveToFile.RecordToFile(level);    
            if(level==null) State.Text = "Список не существует";
            else State.Text = "Список задач записан в файл";
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            ExitWindow exit = new ExitWindow();
            if (exit.ShowDialog() == true) this.Close();
        } 

       
        private void TaskContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            taskContent = TextBox_TaskContent.Text.ToString();
        }

        private void Limit_TextChanged(object sender, TextChangedEventArgs e)
        {
            limit = TextBox_Limit.Text.ToString();
        }

        private void SaveTask_Click(object sender, RoutedEventArgs e)
        {
            if (importance == 0) MessageBox.Show("Выберите уровень важности задачи");
            else
            {
                var task = new Objective(importance, taskContent, limit);
                level[task.Importance].AddTask(task);
                UpdateObjectiveList();                
            }
        }

        private void ChangeTask_Click(object sender, RoutedEventArgs e)
        {
            foreach(var uptask in level[changeTask.Importance].AnyLevel)
            {
                if (uptask==changeTask)
                {
                    uptask.Importance = importance;
                    uptask.TaskContent = taskContent;
                    uptask.Limit = limit;
                    level[uptask.Importance].AddTask(uptask);
                    level[changeTask.Importance].RemoveTask(uptask);
                    UpdateObjectiveList();
                    State.Text = "Задача изменена";
                    return;
                }
            }
        }

        
        
        private void MenuItem_Click_Change(object sender, RoutedEventArgs e)
        {
            changeTask = (ObjectiveList.SelectedItem as ListViewItem).Content as Objective;

            importance = changeTask.Importance;
            if (importance == 1) RadioButton_Importance1.IsChecked = true;
            if (importance == 2) RadioButton_Importance2.IsChecked = true;
            if (importance == 3) RadioButton_Importance3.IsChecked = true;
            TextBox_TaskContent.Text = changeTask.TaskContent;
            TextBox_Limit.Text = changeTask.Limit;

            ChangeTask.IsEnabled = true;
            СlearForm.IsEnabled = true;            
        }

        private void MenuItem_Click_Delete(object sender, RoutedEventArgs e)
        {
            var task = (ObjectiveList.SelectedItem as ListViewItem).Content as Objective;
            level[task.Importance].RemoveTask(task);
            UpdateObjectiveList();
            State.Text = "Задача удалена";
        }

        private void UpdateObjectiveList()
        {
            ITEMS = new List<ListViewItem>();

            foreach (var k in level.Keys)
            {
                if (level[k].AnyLevel.Count != 0)
                {
                    var taskArr = level[k].AnyLevel.ToArray();
                    foreach (var t in taskArr)
                    {
                        ListViewItem OneItem = new ListViewItem();
                        if (k == 1) OneItem.Background = color1;
                        if (k == 2) OneItem.Background = color2;
                        if (k == 3) OneItem.Background = color3;

                        OneItem.Content = new Objective(k, t.TaskContent, t.Limit);
                        ITEMS.Add(OneItem);
                        ObjectiveList.ItemsSource = ITEMS;
                    }
                }
                ObjectiveList.Items.Refresh();
            }
            State.Text="Список обновлен";
        }

      
    }
}
 */
