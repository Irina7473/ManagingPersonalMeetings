using System;
using System.Windows.Controls;
using System.Windows.Media;


namespace MPM
{
    public class PlaceHolderTextBox : TextBox
    {
        bool isPlaceHolder = true;
        string _placeHolderText;
        public string PlaceHolderText
        {
            get { return _placeHolderText; }
            set
            {
                _placeHolderText = value;
                setPlaceholder();
            }
        }
        public new string Text
        {
            get => isPlaceHolder ? string.Empty : base.Text;
            set => base.Text = value;
        }
        
        public PlaceHolderTextBox()
        {
            GotFocus += removePlaceHolder;
            LostFocus += setPlaceholder;
        }
        //когда элемент управления теряет фокус, отображается заполнитель placeholder
        private void setPlaceholder(object sender, EventArgs e)
        {
            setPlaceholder();            
        }
        //когда элемент управления сфокусирован, удаляется placeholder
        private void removePlaceHolder(object sender, EventArgs e)
        {
            removePlaceHolder();            
        }
        
        //когда элемент управления теряет фокус, отображается заполнитель placeholder
        private void setPlaceholder()
        {
            if (string.IsNullOrEmpty(base.Text))
            {
                base.Text = PlaceHolderText;
                this.Foreground = Brushes.Gray;
                isPlaceHolder = true;
            }
        }

        //когда элемент управления сфокусирован, удаляется placeholder
        private void removePlaceHolder()
        {
            if (isPlaceHolder)
            {
                base.Text = "";
                this.Foreground = Brushes.Black;
                isPlaceHolder = false;
            }
        }
    }
}
