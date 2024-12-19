using UnityEngine;
using System.Collections.Generic;

public class CommandConsole_Controller_OldInput : MonoBehaviour
{
    /// <summary>
    /// NOT WORK AND NOT UP TO DATE
    /// </summary>

    [Header("UI Settings")]
    public float viewportHeight = 100f; // Fixed height for viewport
    public float inputHeight = 30f; // Height for the input field
    public Color autocompleteBackgroundColor = Color.black; // Background color for autocomplete suggestions
    public Color autocompleteTextColor = Color.white; // Text color for autocomplete suggestions
    public Color autocompleteHighlightColor = Color.gray; // Highlight color for selected autocomplete suggestion

    [Header("Input Settings")]
    public KeyCode toggleConsoleKey = KeyCode.BackQuote; // Key to toggle the console
    public KeyCode confirmCommandKey = KeyCode.Return; // Key to confirm commands
    public KeyCode autocompleteKey = KeyCode.Tab; // Key to trigger autocomplete

    private bool showConsole = false; // Is the console visible?
    private bool showHelp = false; // Is the help menu visible?
    private bool showDebugMessages = false; // Are debug messages visible?
    private string input = ""; // User input for commands

    private Vector2 scroll; // Scroll position
    private List<(string message, LogType type)> debugMessages = new List<(string, LogType)>(); // Stores debug messages and their types
    private List<string> autocompleteSuggestions = new List<string>(); // Stores autocomplete suggestions
    private int autocompleteIndex = 0; // Index of the currently selected autocomplete suggestion

    // Command declarations
    public static Command ShowHelp;
    public static Command ClearConsole;
    public static Command ShowDebug;
    public static Command AddMoney;
    public static Command<int> AddMoneySpecific;
    public static Command<int> SetSpecificMoney;
    public static Command SayHello;

    private List<object> commandList; // List of all available commands

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLogMessage; // Subscribe to Unity's debug logging
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLogMessage; // Unsubscribe from Unity's debug logging
    }

    private void HandleLogMessage(string condition, string stackTrace, LogType type)
    {
        debugMessages.Add((condition, type)); // Add debug message with its type
    }

    private void Awake()
    {
        // Command: Show help commands
        ShowHelp = new Command(
            "help",
            "Show a list of all commands.",
            "help",
            () =>
            {
                showHelp = !showHelp;
                showDebugMessages = false; // Disable debug view when showing help
            }
        );

        // Command: Clear the console
        ClearConsole = new Command(
            "clear",
            "Clears the console.",
            "clear",
            () =>
            {
                input = "";
                debugMessages.Clear(); // Clear debug messages
                showHelp = false;
                showDebugMessages = false;
                Debug.Log("Console cleared.");
            }
        );

        // Command: Show debug messages
        ShowDebug = new Command(
            "show_debug",
            "Shows all debug messages in the console.",
            "show_debug",
            () =>
            {
                showHelp = false; // Disable help view when showing debug
                showDebugMessages = !showDebugMessages;
                Debug.Log("Debug messages " + (showDebugMessages ? "enabled" : "disabled"));
            }
        );

        // Command: Add 10 money
        AddMoney = new Command(
            "add_money",
            "Adds 10 money to the player.",
            "add_money",
            () =>
            {
                Controller.Instance.Money += 10;
                Debug.Log($"Added 10 money. Total: {Controller.Instance.Money}");
            }
        );

        // Command: Add specific amount of money
        AddMoneySpecific = new Command<int>(
            "add_money_specific",
            "Adds a specific amount of money to the player.",
            "add_money_specific <amount>",
            (amount) =>
            {
                Controller.Instance.Money += amount;
                Debug.Log($"Added {amount} money. Total: {Controller.Instance.Money}");
            }
        );

        // Command: Set money to a specific value
        SetSpecificMoney = new Command<int>(
            "set_money",
            "Sets the player's money to a specific value.",
            "set_money <amount>",
            (amount) =>
            {
                Controller.Instance.Money = amount;
                Debug.Log($"Money set to: {Controller.Instance.Money}");
            }
        );

        // Command: Say hello
        SayHello = new Command(
            "say_hello",
            "Prints 'Hello' in the console.",
            "say_hello",
            () =>
            {
                Controller.Instance.SayHello();
            }
        );

        // Add commands to the list
        commandList = new List<object>
        {
            ShowHelp,
            ClearConsole,
            ShowDebug,
            AddMoney,
            AddMoneySpecific,
            SetSpecificMoney,
            SayHello
        };
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleConsoleKey))
        {
            showConsole = !showConsole;
            input = ""; // Reset input field
            autocompleteSuggestions.Clear(); // Clear autocomplete suggestions
        }

        if (showConsole && Input.GetKeyDown(confirmCommandKey))
        {
            HandleInput();
            input = ""; // Clear input field
            autocompleteSuggestions.Clear(); // Clear autocomplete suggestions
        }

        if (showConsole && Input.GetKeyDown(autocompleteKey))
        {
            ApplyAutocomplete();
        }
    }

    private void OnGUI()
    {
        if (!showConsole) return;

        float y = 0f;

        // Help menu
        if (showHelp)
        {
            GUI.Box(new Rect(0, y, Screen.width, viewportHeight), "");

            Rect viewport = new Rect(0, 0, Screen.width - 30, 20 * commandList.Count);

            scroll = GUI.BeginScrollView(new Rect(0, y + 5f, Screen.width, viewportHeight - 10), scroll, viewport);

            for (int i = 0; i < commandList.Count; i++)
            {
                CommandBase command = commandList[i] as CommandBase;

                string label = $"{command.CommandFormat} - {command.CommandDescription}";
                Rect labelRect = new Rect(5, 20 * i, viewport.width - 100, 20);

                GUI.Label(labelRect, label);
            }

            GUI.EndScrollView();
            y += viewportHeight + 5;
        }

        // Input field
        GUI.Box(new Rect(0, y, Screen.width, inputHeight), "");
        string previousInput = input; // Save the previous input
        input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, inputHeight - 10), input);

        // Autocomplete suggestions
        if (input != previousInput)
        {
            UpdateAutocompleteSuggestions(input);
        }

        if (autocompleteSuggestions.Count > 0)
        {
            y += inputHeight;
            GUI.Box(new Rect(0, y, Screen.width, autocompleteSuggestions.Count * 20), "", GUIStyle.none);
            for (int i = 0; i < autocompleteSuggestions.Count; i++)
            {
                Rect suggestionRect = new Rect(5, y + (i * 20), Screen.width - 10, 20);

                // Highlight the selected suggestion
                if (i == autocompleteIndex)
                {
                    GUI.backgroundColor = autocompleteHighlightColor;
                    GUI.Box(suggestionRect, GUIContent.none);
                }

                GUI.contentColor = autocompleteTextColor;
                GUI.Label(suggestionRect, autocompleteSuggestions[i]);
            }
            GUI.backgroundColor = Color.black;
            GUI.contentColor = Color.white;
        }
    }

    private void UpdateAutocompleteSuggestions(string input)
    {
        autocompleteSuggestions.Clear();

        if (string.IsNullOrEmpty(input)) return;

        foreach (var commandObject in commandList)
        {
            CommandBase commandBase = commandObject as CommandBase;

            if (commandBase.CommandId.StartsWith(input, System.StringComparison.OrdinalIgnoreCase))
            {
                autocompleteSuggestions.Add(commandBase.CommandId);
            }
        }

        autocompleteIndex = 0;
    }

    private void ApplyAutocomplete()
    {
        if (autocompleteSuggestions.Count == 0) return;

        input = autocompleteSuggestions[autocompleteIndex];
        autocompleteIndex = (autocompleteIndex + 1) % autocompleteSuggestions.Count;
    }

    private void HandleInput()
    {
        string[] properties = input.Split(' ');

        if (properties.Length == 0) return;

        foreach (var commandObject in commandList)
        {
            CommandBase commandBase = commandObject as CommandBase;

            if (properties[0].Equals(commandBase.CommandId, System.StringComparison.OrdinalIgnoreCase))
            {
                if (properties.Length == 2 && commandObject is Command<int> intCommand)
                {
                    if (int.TryParse(properties[1], out int value))
                    {
                        intCommand.Invoke(value);
                        return;
                    }

                    Debug.LogError($"Invalid parameter for command '{commandBase.CommandId}'. Expected a number.");
                    return;
                }

                if (properties.Length == 1 && commandObject is Command command)
                {
                    command.Invoke();
                    return;
                }

                Debug.LogError($"Invalid usage for command: {commandBase.CommandId}. Format: {commandBase.CommandFormat}");
                return;
            }
        }

        Debug.LogError($"Command not recognized: {properties[0]}");
    }
}
