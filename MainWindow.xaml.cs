using System;
using System.Collections.Generic;
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
using System.Timers;
using System.IO.Ports;
namespace WpfApp21
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort serialPort;
        string melody = "D4 D4 D4 D4 A4 A4 B4 B4 B4 A4 A4 D4 D4 D4 A4 A4 G4 G4 D4 D4 C4 C4 A4 A4 D4 D4 F4 F4 F4 D4 D4 D4 S4 S4 G4 G4";
       
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SerialPorts();
        }

        private void SerialPorts()
        {
            string[] ports = SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                serialPort = new SerialPort(port, 57600);
                serialPort.DataReceived += SerialPort_DataReceived;

                try
                {
                    serialPort.Open();
                    serialPort.WriteLine("Info from WPF"); // Отправляем запрос на Arduino
                    System.Threading.Thread.Sleep(1000); // Ждем 1 секунду на ответ от Arduino

                    if (label.Content.ToString() == "Info from Arduino")
                    {
                        label.Content = $"Arduino подключена к порту {port}";
                        break;
                    }

                    serialPort.Close();
                }
                catch (Exception ex)
                {
                    // Обработка ошибок
                }
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort.ReadLine();
            label.Dispatcher.Invoke(() => label.Content = data);
        }

        private void S1_Click(object sender, RoutedEventArgs e)
        {
            label.Content = "Нажата первая кнопка";
        }

        private void S2_Click(object sender, RoutedEventArgs e)
        {
            label.Content = "Нажата вторая кнопка";
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            string selectedInterval = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
            double interval = double.Parse(selectedInterval.Split(' ')[0]);

            while (true)
            {
                await Task.Delay((int)(interval * 1000));
                serialPort.WriteLine("LED_ON");
                await Task.Delay((int)(interval * 1000));
                serialPort.WriteLine("LED_OFF");
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            PlayMelody();
        }

        private async void PlayMelody()
        {
            string[] melodyNotes = melody.Split(' ');

            foreach (string note in melodyNotes)
            {
                int frequency;
                switch (note)
                {
                    case "D4":
                        frequency = 31;
                        break;
                    case "A4":
                        frequency = 33;
                        break;
                    case "G4":
                        frequency = 35;
                        break;
                    case "B4":
                        frequency = 37;
                        break;
                    case "C4":
                        frequency = 39;
                        break;
                    case "F4":
                        frequency = 41;
                        break;
                    case "S4":
                        frequency = 44;
                        break;
                    default:
                        frequency = 0;
                        break;
                }

                if (frequency != 0)
                {
                    serialPort.WriteLine($"TONE {frequency}");
                    await Task.Delay(500); // Проигрываем каждую ноту в течение 500 миллисекунд
                    serialPort.WriteLine("NOTONE");
                    await Task.Delay(50); // Пауза между нотами
                }
            }
        }
    }
}
