using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MachineControlBase
{
    /// <summary>
    /// TimePicker.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TimePicker : UserControl
    {
        public TimePicker()
        {
            InitializeComponent();
        }

        public DateTime? DateTimeValue
        {
            get
            {
                string hours = this.txtHours.Text;
                string minutes = this.txtMinutes.Text;
                string seconds = this.txtSeconds.Text;

                //string amPm = this.txtAmPm.Text;
                if (!string.IsNullOrWhiteSpace(hours)
                    && !string.IsNullOrWhiteSpace(minutes)
                    && !string.IsNullOrWhiteSpace(seconds))
                {
                    string Value = string.Format("{0}:{1}:{2}", hours, minutes, seconds);
                    DateTime time = DateTime.Parse(Value);
                    return time;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                DateTime? time = value;
                if (time.HasValue)
                {
                    string timeString = time.Value.ToShortTimeString();
                    //9:54 AM
                    string[] values = timeString.Split(':', ':', ':');
                    if (values.Length == 3)
                    {
                        this.txtHours.Text = values[0];
                        this.txtMinutes.Text = values[1];
                        this.txtSeconds.Text = values[2];
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the time span value.
        /// </summary>
        /// <value>The time span value.</value>
        public TimeSpan? TimeSpanValue
        {
            get
            {
                DateTime? time = this.DateTimeValue;
                if (time.HasValue)
                {
                    return new TimeSpan(time.Value.Ticks);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                TimeSpan? timeSpan = value;
                if (timeSpan.HasValue)
                {
                    this.DateTimeValue = new DateTime(timeSpan.Value.Ticks);
                }
            }
        }

        #region Event Subscriptions

        /// <summary>
        /// Handles the Click event of the btnDown control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            string controlId = this.GetControlWithFocus().Name;
            if ("txtHours" == controlId)
            {
                this.ChangeHours(false);
            }
            else if ("txtMinutes" == controlId)
            {
                this.ChangeMinutes(false);
            }
            else if ("txtSeconds" == controlId)
            {
                this.ChangeSeconds(false);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnUp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            string controlId = this.GetControlWithFocus().Name;
            if ("txtHours" == controlId)
            {
                this.ChangeHours(true);
            }
            else if ("txtMinutes" == controlId)
            {
                this.ChangeMinutes(true);
            }
            else if ("txtSeconds" == controlId)
            {
                this.ChangeSeconds(true);
            }
        }

        /// <summary>
        /// Handles the PreviewTextInput event of the txtAmPm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.TextCompositionEventArgs"/> instance containing the event data.</param>
        private void txtAmPm_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // prevent users to type text
            e.Handled = true;
        }

        /// <summary>
        /// Handles the KeyUp event of the txt control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void txt_KeyUp(object sender, KeyEventArgs e)
        {
            // check for up and down keyboard presses
            if (Key.Up.Equals(e.Key))
            {
                btnUp_Click(this, null);
            }
            else if (Key.Down.Equals(e.Key))
            {
                btnDown_Click(this, null);
            }
        }

        /// <summary>
        /// Handles the MouseWheel event of the txt control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseWheelEventArgs"/> instance containing the event data.</param>
        private void txt_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                btnUp_Click(this, null);
            }
            else
            {
                btnDown_Click(this, null);
            }
        }

        /// <summary>
        /// Handles the PreviewKeyUp event of the txt control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void txt_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            // make sure all characters are number
            bool allNumbers = textBox.Text.All(Char.IsNumber);
            if (!allNumbers)
            {
                e.Handled = true;
                return;
            }

            // make sure user did not enter values out of range
            int value;
            int.TryParse(textBox.Text, out value);
            if ("txtHours" == textBox.Name && value > 23)
            {
                EnforceLimits(e, textBox);
            }
            else if ("txtMinutes" == textBox.Name && value > 59)
            {
                EnforceLimits(e, textBox);
            }
            else if ("txtSeconds" == textBox.Name && value > 59)
            {
                EnforceLimits(e, textBox);
            }
        }

        #endregion Event Subscriptions

        #region Methods

        /// <summary>
        /// Changes the hours.
        /// </summary>
        /// <param name="isUp">if set to <c>true</c> [is up].</param>
        private void ChangeHours(bool isUp)
        {
            int value = Convert.ToInt32(this.txtHours.Text);
            if (isUp)
            {
                value += 1;
                if (value == 24)
                {
                    value = 0;
                }
            }
            else
            {
                value -= 1;
                if (value == -1)
                {
                    value = 23;
                }
            }
            this.txtHours.Text = Convert.ToString(value);
        }

        /// <summary>
        /// Changes the minutes.
        /// </summary>
        /// <param name="isUp">if set to <c>true</c> [is up].</param>
        private void ChangeMinutes(bool isUp)
        {
            int value = Convert.ToInt32(this.txtMinutes.Text);
            if (isUp)
            {
                value += 1;
                if (value == 60)
                {
                    value = 0;
                }
            }
            else
            {
                value -= 1;
                if (value == -1)
                {
                    value = 59;
                }
            }

            string textValue = Convert.ToString(value);
            if (value < 10)
            {
                textValue = "0" + Convert.ToString(value);
            }
            this.txtMinutes.Text = textValue;
        }

        private void ChangeSeconds(bool isUp)
        {
            int value = Convert.ToInt32(this.txtSeconds.Text);
            if (isUp)
            {
                value += 1;
                if (value == 60)
                {
                    value = 0;
                }
            }
            else
            {
                value -= 1;
                if (value == -1)
                {
                    value = 59;
                }
            }

            string textValue = Convert.ToString(value);
            if (value < 10)
            {
                textValue = "0" + Convert.ToString(value);
            }
            this.txtSeconds.Text = textValue;
        }

        /// <summary>
        /// Enforces the limits.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        /// <param name="textBox">The text box.</param>
        /// <param name="enteredValue">The entered value.</param>
        private static void EnforceLimits(KeyEventArgs e, TextBox textBox)
        {
            string enteredValue = GetEnteredValue(e.Key);
            string text = textBox.Text.Replace(enteredValue, "");
            if (string.IsNullOrEmpty(text))
            {
                text = enteredValue;
            }
            textBox.Text = text;
            e.Handled = true;
        }

        /// <summary>
        /// Gets the control with focus.
        /// </summary>
        /// <returns></returns>
        private TextBox GetControlWithFocus()
        {
            TextBox txt = new TextBox();
            if (this.txtHours.IsFocused)
            {
                txt = this.txtHours;
            }
            else if (this.txtMinutes.IsFocused)
            {
                txt = this.txtMinutes;
            }
            else if (this.txtSeconds.IsFocused)
            {
                txt = this.txtSeconds;
            }
            return txt;
        }

        /// <summary>
        /// Gets the entered value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private static string GetEnteredValue(Key key)
        {
            string value = string.Empty;
            switch (key)
            {
                case Key.D0:
                case Key.NumPad0:
                    value = "0";
                    break;

                case Key.D1:
                case Key.NumPad1:
                    value = "1";
                    break;

                case Key.D2:
                case Key.NumPad2:
                    value = "2";
                    break;

                case Key.D3:
                case Key.NumPad3:
                    value = "3";
                    break;

                case Key.D4:
                case Key.NumPad4:
                    value = "4";
                    break;

                case Key.D5:
                case Key.NumPad5:
                    value = "5";
                    break;

                case Key.D6:
                case Key.NumPad6:
                    value = "6";
                    break;

                case Key.D7:
                case Key.NumPad7:
                    value = "7";
                    break;

                case Key.D8:
                case Key.NumPad8:
                    value = "8";
                    break;

                case Key.D9:
                case Key.NumPad9:
                    value = "9";
                    break;
            }
            return value;
        }

        #endregion Methods
    }
}