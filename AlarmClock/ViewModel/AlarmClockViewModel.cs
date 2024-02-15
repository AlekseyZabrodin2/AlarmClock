using AlarmClock.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AlarmClock.ViewModel
{
    public partial class AlarmClockViewModel : ObservableObject
    {
        private string? _musicLocationShow;

        [ObservableProperty]
        private string _timeWakeUp;

        [ObservableProperty]
        private string _currentTime;

        [ObservableProperty]
        private string? _clockStatus;

        [ObservableProperty]
        private string? _musicLocation;

        public string? MusicLocationShow
        {
            get => _musicLocationShow;
            set
            {
                if (SetProperty(ref _musicLocationShow, value))
                {
                    if (string.IsNullOrEmpty(_musicLocationShow))
                    {
                        CreateButtonEnabled = false;
                    }
                }
            }
        }

        [ObservableProperty]
        private bool _alarmClockIsSelected;

        [ObservableProperty]
        private bool _startButtonEnabled = true;

        [ObservableProperty]
        private bool? _setOffButtonEnabled = false;

        [ObservableProperty]
        private bool _stopButtonEnabled = false;

        [ObservableProperty]
        private bool _createButtonEnabled = false;
        
        [ObservableProperty]
        private bool _isDropDownOpen = false;

        [ObservableProperty]
        private bool _isToggleButtonChecked = false;

        [ObservableProperty]
        private int _timeToPostpone = 0;

        [ObservableProperty]
        private ObservableCollection<AlarmClockModel> _observedAlarms;

        [ObservableProperty]
        private AlarmClockModel? _alarmClockModel;

        [ObservableProperty]
        private DispatcherTimer? _timePicker;

        [ObservableProperty]
        private DispatcherTimer? _timerAlarm;

        [ObservableProperty]
        private DispatcherTimer? _timerSetOff;

        [ObservableProperty]
        private MediaPlayer? _soundPlayer;
                

        public AlarmClockViewModel()
        {
            ObservedAlarms = new ObservableCollection<AlarmClockModel>();

            var wakeUp = DateTime.Now.ToLocalTime();
            TimeWakeUp = wakeUp.ToString("HH:mm:ss");
            CurrentTime = wakeUp.ToString("HH:mm:ss");

            TimePicker = new DispatcherTimer();
            TimePicker.Interval = TimeSpan.FromSeconds(1);
            TimePicker.Tick += TimePickerEvent!;
            TimePicker.Start();
        }

        [RelayCommand]
        private void CreateAlarm()
        {            
            var alarmCloclModel = new AlarmClockModel();
            UpdateAlarmClockModel(alarmCloclModel);

            ObservedAlarms.Add(alarmCloclModel);

            ClearFialds();
        }

        private void ClearFialds()
        {
            MusicLocation = string.Empty;
            MusicLocationShow = string.Empty;
            TimeToPostpone = 0;
        }

        private void UpdateAlarmClockModel(AlarmClockModel alarmClockModel)
        {
            alarmClockModel.TimeWakeUpModel = TimeWakeUp;
            alarmClockModel.CurrentTimeModel = CurrentTime;
            alarmClockModel.ClockStatusModel = ClockStatus;
            alarmClockModel.IsToggleButtonCheckedModel = IsToggleButtonChecked;
            alarmClockModel.TimeToPostponeModel = TimeToPostpone;
            alarmClockModel.TimerAlarmModel = new DispatcherTimer();
            alarmClockModel.TimerSetOffModel = new DispatcherTimer();
            alarmClockModel.SoundPlayerModel = new MediaPlayer();
            alarmClockModel.SetOffButtonEnabledModel = SetOffButtonEnabled;
            alarmClockModel.StopButtonEnabledModel = StopButtonEnabled;
            alarmClockModel.MusicLocationModel = MusicLocation;
            alarmClockModel.MusicLocationShowModel = MusicLocationShow;
        }

        [RelayCommand]
        private void DeleteAlarm()
        {
            var selectedAlarms = ObservedAlarms.Where(alarm => alarm.AlarmClockIsSelectedModel).ToList();
            foreach (var alarm in selectedAlarms)
            {
                alarm.TimerAlarmModel!.Stop();
                alarm.TimerSetOffModel!.Stop();
                alarm.SoundPlayerModel!.Stop();
                ObservedAlarms.Remove(alarm);               
            }
        }


        [RelayCommand]
        private void StartAlarm()
        {
            

            if (!IsToggleButtonChecked)
            {
                TimerAlarm!.Stop();
                return;
            }
            TimerAlarm!.Start();
        }

        [RelayCommand]
        private void StartAlarmClock()
        {
            ClockStatus = $" Pognali \r\n ostanovlus v - {TimeWakeUp},\r\n tekushchee vremia -";
            StartButtonEnabled = false;
            StopButtonEnabled = true;
            //IsDropDownOpen = true;

            TimerAlarm!.Interval = TimeSpan.FromSeconds(1);
            TimerAlarm.Tick += AlarmStart!;
            TimerAlarm.Start();
        }

        [RelayCommand]
        private void StopAlarmClock()
        {
            ClockStatus = "Stape";
            StartButtonEnabled = true;
            StopButtonEnabled = false;
            IsDropDownOpen = false;

            TimerSetOff!.Stop();
            SoundPlayer!.Stop();
        }

        [RelayCommand]
        private void SetOffTheAlarm()
        {
            ClockStatus = $"The alarm has been postponed for {TimeToPostpone} minuts";

            //StartButtonEnabled = true;
            //StopButtonEnabled = false;
            //IsDropDownOpen = false;
            
            SoundPlayer?.Stop();

            
            TimerSetOff!.Interval = TimeSpan.FromSeconds(TimeToPostpone);
            TimerSetOff.Tick += AlarmSound;
            TimerSetOff.Start();
        }

        [RelayCommand]
        private void SelectMediaFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Media Files (*.mp3;*.wav)|*.mp3;*.wav|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                MusicLocation = openFileDialog.FileName;
                MusicLocationShow = Path.GetFileName(openFileDialog.FileName);
                CreateButtonEnabled = true;
            }
        }





        private void TimePickerEvent(object sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        }


        private void AlarmStart(object sender, EventArgs e)
        {
            var wakeUpTime = DateTime.Parse(TimeWakeUp);
            var currentTime = DateTime.Now;
            CurrentTime = currentTime.ToString("HH:mm:ss");

            if (currentTime.Hour == wakeUpTime.Hour && currentTime.Minute == wakeUpTime.Minute && currentTime.Second == wakeUpTime.Second)
            {
                TimerSetOff = new DispatcherTimer();
                AlarmSoundPlayer();
            }
        }

        private void AlarmSound(object? sender, EventArgs e)
        {
            AlarmSoundPlayer();
        }

        private void AlarmSoundPlayer()
        {
            SoundPlayer = new MediaPlayer();
            SoundPlayer.Open(new Uri(MusicLocation!.Trim('"')));
            SoundPlayer.MediaEnded += (sender, e) =>
            {
                SoundPlayer.Position = TimeSpan.Zero;
                SoundPlayer.Play();
            };
        }


    }
}
