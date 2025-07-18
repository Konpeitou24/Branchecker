using Branchecker.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Branchecker.Shells {
    /// <summary>
    /// The main ViewModel for the branch name checking application.
    /// Handles AI calls, regex validation, and input debounce logic.
    /// </summary>
    public partial class MainWindowViewModel : ObservableObject {
        private readonly HttpClient httpClient;
        private readonly IConfiguration config;
        private readonly AsyncCallbackTimer inputTimer;
        private readonly Func<Task>[] UpdateTasks;
        [ObservableProperty]
        private string? remainSeconds = "0";

        /// <summary>
        /// The main header title of the branch checker tool.
        /// </summary>
        [ObservableProperty]
        private string? branchCheckToolHeaderName = "ブランチチェッカー";

        /// <summary>
        /// Label text prompting the user to enter a branch name.
        /// </summary>
        [ObservableProperty]
        private string? askBranchLabelTitle = "ブランチ名を入力してください";

        /// <summary>
        /// The branch name input by the user.
        /// </summary>
        [ObservableProperty]
        private string? askBranchText;

        /// <summary>
        /// Label text prompting the user to enter an optional description.
        /// </summary>
        [ObservableProperty]
        private string? askDescriptionLableTitle = "補足説明（任意）";

        /// <summary>
        /// The optional description input by the user.
        /// </summary>
        [ObservableProperty]
        private string? askDescriptionText;

        /// <summary>
        /// Status of the regex check result (e.g., 未判定 / OK / NG).
        /// </summary>
        [ObservableProperty]
        private string? branchCheckStatus = "未判定";

        /// <summary>
        /// Label indicating the section for AI's suggested branch names.
        /// </summary>
        [ObservableProperty]
        private string? aiResponseLabelTitle = "AIの応答";

        /// <summary>
        /// The list of AI-generated branch name suggestions.
        /// </summary>
        [ObservableProperty]
        private List<string> aiResponseList = [];

        /// <summary>
        /// Label indicating the section for AI-inferred tasks.
        /// </summary>
        [ObservableProperty]
        private string? aiInferredTasksLabelTitle = "AIが推定したタスク";

        /// <summary>
        /// The list of tasks inferred by AI from the branch name.
        /// </summary>
        [ObservableProperty]
        private List<string> aiInferredTasksList = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client used for AI communication.</param>
        /// <param name="config">The configuration provider (for API keys, regex, etc.).</param>
        public MainWindowViewModel(HttpClient httpClient, IConfiguration config) {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.config = config ?? throw new ArgumentNullException(nameof(config));

            UpdateTasks = [CheckBranchFromAi, CheckInferredFromAi, CheckBranchFromRegex];

            inputTimer = new AsyncCallbackTimer(
                float.Parse(config["ResponseTime"] ?? "3"),
                1, UpdateTasks
            );

            // 残り秒数を更新するコールバックをセット
            inputTimer.OnTickUpdate = sec => {
                RemainSeconds = $"{sec:F1}";
            };
            inputTimer.Start();
        }


        /// <summary>
        /// Sends the user input to Gemini AI and retrieves suggested branch names.
        /// </summary>
        public async Task CheckBranchFromAi() {
            if (string.IsNullOrWhiteSpace(AskBranchText)) return;
            AiResponseList = ["考え中..."];
            var ai = new GeminiApiClient(httpClient, config);
            var result = await ai.GenerateTextAsync(config["Prompts:AiSuggest"] ?? string.Empty) ?? string.Empty;
            AiResponseList = [.. result.Split(' ', StringSplitOptions.RemoveEmptyEntries)];
        }

        /// <summary>
        /// Sends the user input to Gemini AI and retrieves inferred tasks.
        /// </summary>
        public async Task CheckInferredFromAi() {
            if (string.IsNullOrWhiteSpace(AskBranchText)) return;
            AiInferredTasksList = ["考え中..."];
            var ai = new GeminiApiClient(httpClient, config);
            var result = await ai.GenerateTextAsync(config["Prompts:AiInferred"] ?? string.Empty) ?? string.Empty;
            AiInferredTasksList = [.. result.Split(' ', StringSplitOptions.RemoveEmptyEntries)];
        }

        /// <summary>
        /// Validates the branch name against the regular expression specified in config["Regex"].
        /// Updates <see cref="BranchCheckStatus"/> with the result.
        /// </summary>
        public async Task CheckBranchFromRegex() {
            if (string.IsNullOrWhiteSpace(AskBranchText)) {
                BranchCheckStatus = "ブランチ名が未入力です";
                return;
            }

            var pattern = config["Regex"];
            if (string.IsNullOrWhiteSpace(pattern)) {
                BranchCheckStatus = "判定不可";
                return;
            }
            try {
                bool isMatch = await Task.Run(() => Regex.IsMatch(AskBranchText!, pattern));
                BranchCheckStatus = isMatch ? "✅ 正常なブランチ名" : "❌ 不正なブランチ名";
            } catch (Exception ex) {
                BranchCheckStatus = $"正規表現が無効です: {ex.Message}";
            }
        }

        /// <summary>
        /// Resets the input timer when the optional description is changed.
        /// </summary>
        /// <param name="value">The new description value.</param>
        partial void OnAskDescriptionTextChanged(string? value) {
            inputTimer?.Reset();
        }

        /// <summary>
        /// Resets the input timer when the branch name is changed.
        /// </summary>
        /// <param name="value">The new branch name.</param>
        partial void OnAskBranchTextChanged(string? value) {
            inputTimer?.Reset();
        }
    }
}
