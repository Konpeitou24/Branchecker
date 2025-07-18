using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Branchecker.Core {
    /// <summary>
    /// Executes one or more asynchronous callback actions once, after a specified duration has elapsed.
    /// The timer runs on the WPF UI thread using DispatcherTimer.
    /// </summary>
    public class AsyncCallbackTimer {
        public Action<float>? OnTickUpdate { get; set; }
        private readonly float executionSeconds;
        private readonly float step;
        private readonly Func<Task>[] callbacks;
        private readonly DispatcherTimer timer;
        private bool executedCallback = false;
        public float RemainSeconds { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCallbackTimer"/> class.
        /// </summary>
        /// <param name="executionSeconds">The number of seconds to wait before executing callbacks.</param>
        /// <param name="step">The interval in seconds at which the timer ticks. Default is 1.</param>
        /// <param name="callbacks">One or more asynchronous actions to execute when the timer completes.</param>
        public AsyncCallbackTimer(float executionSeconds, float step = 1, params Func<Task>[] callbacks) {
            this.executionSeconds = executionSeconds;
            this.step = step;
            this.callbacks = callbacks;
            RemainSeconds = 0;

            timer = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(step)
            };
            timer.Tick += Tick;
        }

        /// <summary>
        /// Starts the timer. The callbacks will be executed once after the specified time elapses.
        /// </summary>
        public void Start() {
            executedCallback = false;
            RemainSeconds = 0;
            timer.Start();
        }

        /// <summary>
        /// Stops the timer immediately. The callbacks will not be executed if the time has not elapsed.
        /// </summary>
        public void Stop() {
            timer.Stop();
        }

        /// <summary>
        /// Resets the timer's elapsed time to zero and marks it as not yet executed.
        /// </summary>
        public void Reset() {
            executedCallback = false;
            RemainSeconds = 0;
        }

        /// <summary>
        /// Called at each timer tick. Increments elapsed time and executes callbacks if needed.
        /// </summary>
        private async void Tick(object? sender, EventArgs e) {
            OnTickUpdate?.Invoke(RemainSeconds);
            if (RemainSeconds < executionSeconds) {
                RemainSeconds += step;
            } else if (!executedCallback) {
                executedCallback = true;
                await ExecuteAsync();
            }
        }

        /// <summary>
        /// Invokes all registered asynchronous callback actions in parallel and awaits their completion.
        /// </summary>
        private async Task ExecuteAsync() {
            try {
                var tasks = callbacks.Select(cb => Task.Run(cb)).ToArray();
                await Task.WhenAll(tasks);
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"並列実行中にエラー: {ex.Message}");
                throw;
            }
        }
    }
}
