using System;
using System.Windows.Threading;

namespace Branchecker.Core {
    /// <summary>
    /// Executes one or more callback actions once, after a specified duration has elapsed.
    /// The timer runs on the WPF UI thread using DispatcherTimer.
    /// </summary>
    public class CallbackTimer {
        /// <summary>
        /// Tracks the elapsed time in seconds.
        /// </summary>
        private float remain;

        /// <summary>
        /// The total number of seconds to wait before executing the callbacks.
        /// </summary>
        private readonly float executionSeconds;

        /// <summary>
        /// The interval (in seconds) at which the timer ticks.
        /// </summary>
        private readonly float step;

        /// <summary>
        /// The collection of callback actions to execute after the duration elapses.
        /// </summary>
        private readonly Action[] callbacks;

        /// <summary>
        /// The DispatcherTimer instance that drives the tick events.
        /// </summary>
        private readonly DispatcherTimer timer;

        /// <summary>
        /// Indicates whether the callbacks have already been executed.
        /// Prevents multiple executions.
        /// </summary>
        private bool executedCallback = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackTimer"/> class.
        /// </summary>
        /// <param name="executionSeconds">The number of seconds to wait before executing callbacks.</param>
        /// <param name="step">The interval in seconds at which the timer ticks. Default is 1.</param>
        /// <param name="callbacks">One or more actions to execute when the timer completes.</param>
        public CallbackTimer(float executionSeconds, float step = 1, params Action[] callbacks) {
            this.executionSeconds = executionSeconds;
            this.step = step;
            remain = 0;
            this.callbacks = callbacks;
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
        /// This allows reuse of the timer for another countdown cycle.
        /// </summary>
        public void Reset() {
            executedCallback = false;
            remain = 0;
        }

        /// <summary>
        /// Called at each timer tick. Increments elapsed time and executes callbacks if needed.
        /// </summary>
        /// <param name="sender">The timer object.</param>
        /// <param name="e">Event data.</param>
        private void Tick(object? sender, EventArgs e) {
            remain += step;

            if (remain >= executionSeconds && !executedCallback) {
                Execute();
                executedCallback = true;
            }
        }

        /// <summary>
        /// Invokes all registered callback actions.
        /// </summary>
        private void Execute() {
            foreach (var callback in callbacks) {
                callback?.Invoke();
            }
        }
    }
}
