using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Panoramas_Editor
{
    internal class ExecutionVM : ObservableObject
    {
        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                IsRunningChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }
        public event EventHandler? IsRunningChanged;

        public Task Execution { get; set; }
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;
        
        public ExecutionVM()
        {
            IsRunning = false;
            RunCommand = new RelayCommand(Run);
            StopCommand = new RelayCommand(Stop);
            Log = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\nDuis ultricies lacus sed turpis tincidunt id aliquet risus feugiat.\nMolestie ac feugiat sed lectus vestibulum mattis ullamcorper.\nCommodo sed egestas egestas fringilla phasellus faucibus scelerisque eleifend donec.\nMollis nunc sed id semper risus in hendrerit gravida.\nScelerisque viverra mauris in aliquam sem.\nQuam quisque id diam vel quam elementum pulvinar etiam non.\nMauris pharetra et ultrices neque ornare aenean.\nEst lorem ipsum dolor sit amet consectetur.\nScelerisque purus semper eget duis.\nEu nisl nunc mi ipsum faucibus vitae aliquet nec.\nPhasellus egestas tellus rutrum tellus pellentesque.\nEgestas congue quisque egestas diam in arcu cursus euismod.\nAnte in nibh mauris cursus mattis molestie a iaculis.\nNeque sodales ut etiam sit amet nisl purus in mollis.\nMattis aliquam faucibus purus in massa tempor nec feugiat nisl.\nMi quis hendrerit dolor magna eget.\nPorta lorem mollis aliquam ut porttitor leo a.\nTempus imperdiet nulla malesuada pellentesque elit.\nSemper auctor neque vitae tempus quam pellentesque nec nam aliquam.\nVulputate eu scelerisque felis imperdiet proin fermentum leo vel orci.\nNon curabitur gravida arcu ac tortor dignissim.\nDiam quis enim lobortis scelerisque fermentum dui faucibus in ornare.\nEt malesuada fames ac turpis.\nJusto laoreet sit amet cursus sit amet dictum sit.";
        }

        public string Log { get; set; }

        public IRelayCommand RunCommand { get; }
        public IRelayCommand StopCommand { get; }

        public void Run()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            Execution = Task.Run(async () =>
            {
                IsRunning = true;

                await Task.Run(() =>
                {
                    try
                    {
                        while (true)
                        {
                            if (_cancellationToken.IsCancellationRequested)
                            {
                                _cancellationToken.ThrowIfCancellationRequested();
                            }
                            Thread.Sleep(3000);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        
                    }
                    catch (Exception)
                    {
                        //CustomMessageBox.ShowError("Ошибка");
                    }
                    finally
                    {
                        _cancellationTokenSource.Dispose();
                    }
                }, _cancellationToken);

                IsRunning = false;

            }, _cancellationToken);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
