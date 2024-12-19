using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CommandConsole_Controller : MonoBehaviour
{
    [Tooltip("This boolean is triggered by the toggle console action")] public bool showConsole = false; // Whether the console is currently visible
    [Tooltip("This boolean is triggered by the command to show debug window")] public bool showDebugWindow = false; // Whether the debug window is currently visible

    [Header("Console Window Settings")]
    public Vector2 consoleWindowSize = new Vector2(500f, 200f); // Default size for the console window
    public Vector2 consoleWindowPosition = new Vector2(20f, 20f); // Default position for the console window
    private Vector2 initialConsoleWindowSize; // To store initial console size for resetting
    private Vector2 initialconsoleWindowPosition; // To store initial console position for resetting
    public float inputfildHeight = 20f; // Height for the input field
    public float inputfildPositionY = 30f; // Position offset for the input field

    [Header("Debug Window Settings")]
    public Vector2 debugWindowSize = new Vector2(350f, 150f); // Default size for the debug window
    public Vector2 debugWindowPosition = new Vector2(20f, 20f); // Default position for the debug window

    [Header("Input Actions")]
    [SerializeField] private InputActionReference toggleConsoleAction; // Action to toggle the console
    [SerializeField] private InputActionReference confirmCommandAction; // Action to confirm a command
    [SerializeField] private InputActionReference autocompleteAction; // Action to autocomplete

    private string input = ""; // User input from the input field
    private List<string> autocompleteSuggestions = new List<string>(); // Suggestions for autocomplete
    private int autocompleteIndex = 0; // Current selected suggestion for autocomplete

    private Vector2 messageScroll; // Scroll position for the console messages
    private Vector2 debugScroll; // Scroll position for the debug window

    private List<string> consoleMessages = new List<string>(); // Messages displayed in the console
    private List<(string message, LogType type)> debugMessages = new List<(string, LogType)>(); // Debug log messages

    // Command declarations
    public static Command ShowHelp;
    public static Command ResetConsole;
    public static Command ClearConsole;
    public static Command ShowDebugWindow;
    public static Command SayHello;
    public static Command ResetMoney;
    public static Command<int> AddSpecificMoney;

    private List<object> commandList; // List of all available commands

    private void OnEnable()
    {
        // Store the initial window size and position
        initialConsoleWindowSize = consoleWindowSize;
        initialconsoleWindowPosition = consoleWindowPosition;

        // Bind input actions
        toggleConsoleAction.action.performed += OnToggleConsole;
        confirmCommandAction.action.performed += OnConfirmCommand;
        autocompleteAction.action.performed += OnAutocomplete;

        // Bind debug messages
        Application.logMessageReceived += HandleLogMessage;
    }

    private void OnDisable()
    {
        // Unbind input actions
        toggleConsoleAction.action.performed -= OnToggleConsole;
        confirmCommandAction.action.performed -= OnConfirmCommand;
        autocompleteAction.action.performed -= OnAutocomplete;

        // Unbind debug messages
        Application.logMessageReceived -= HandleLogMessage;
    }

    private void Awake()
    {
        // Initialize the list of commands
        InitializeCommands();
    }

    private void InitializeCommands()
    {
        // Add available commands to the console
        ShowHelp = new Command("help", "Show a list of all commands.", "help", () =>
        {
            AddConsoleMessage("Available commands:");
            foreach (var cmd in commandList)
            {
                CommandBase cmdBase = cmd as CommandBase;
                if (cmdBase != null)
                {
                    AddConsoleMessage($"{cmdBase.CommandFormat} - {cmdBase.CommandDescription}");
                }
            }
        });

        ResetConsole = new Command("reset", "Resets the console scale.", "reset", () =>
        {
            consoleWindowSize = initialConsoleWindowSize;
            consoleWindowPosition = initialconsoleWindowPosition;

            AddConsoleMessage("Console reset.");
        });

        ClearConsole = new Command("clear", "Clears the console.", "clear", () =>
        {
            consoleMessages.Clear();
            AddConsoleMessage("Console cleared.");
        });

        ShowDebugWindow = new Command("show_debug_window", "Shows all debug messages in a separate window.", "show_debug_window", () =>
        {
            showConsole = false;
            showDebugWindow = !showDebugWindow;
            Debug.Log("Debug window " + (showDebugWindow ? "enabled" : "disabled"));
        });

        SayHello = new Command("say_hello", "It says hello.", "say_hello", () =>
        {
            AddConsoleMessage("Hello!");
        });

        ResetMoney = new Command("reset_money", "Reset the money.", "reset_money", () =>
        {
            Controller.Instance.Money = 0;
            Controller.Instance.UpdateMoneyText();
        });

        AddSpecificMoney = new Command<int>("add_specific_money", "Adds a specific amount of money.", "add_specific_money <amount>", (amount) =>
        {
            Controller.Instance.Money += amount;
            Controller.Instance.UpdateMoneyText();
        });

        // Add all commands to the list
        commandList = new List<object>
        {
            ShowHelp,
            ResetConsole,
            ClearConsole,
            ShowDebugWindow,
            SayHello,
            ResetMoney,
            AddSpecificMoney
        };
    }

    private void HandleLogMessage(string condition, string stackTrace, LogType type)
    {
        // Add a debug message to the list and adjust scroll
        debugMessages.Add((condition, type));
        debugScroll.y = Mathf.Infinity;
    }

    public void OnToggleConsole(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Toggle console visibility
            input = "";
            autocompleteSuggestions.Clear();

            showConsole = !showConsole;
            if (showConsole) showDebugWindow = false;
        }
    }

    public void OnConfirmCommand(InputAction.CallbackContext context)
    {
        if (context.performed && showConsole)
        {
            // Execute the input command
            HandleInput();
            input = "";
            autocompleteSuggestions.Clear();
        }
    }

    public void OnAutocomplete(InputAction.CallbackContext context)
    {
        if (context.performed && showConsole)
        {
            // Apply autocomplete suggestion
            ApplyAutocomplete();
        }
    }

    private void OnGUI()
    {
        if (showConsole)
        {
            RenderConsoleWindow();
        }
        else if (showDebugWindow)
        {
            RenderDebugWindow();
        }
    }

    private void RenderConsoleWindow()
    {
        // Draw the console window box
        GUI.Box(new Rect(consoleWindowPosition.x, consoleWindowPosition.y, consoleWindowSize.x, consoleWindowSize.y), "Command Console");

        // Define button dimensions for moving the window
        float buttonSize = 12f; // Size of each button
        float centerX = consoleWindowPosition.x + consoleWindowSize.x - 25 - buttonSize / 2; // X-center for move buttons
        float centerY = consoleWindowPosition.y + 5 + buttonSize; // Y-center for move buttons

        // Top Button (Move Up)
        if (GUI.Button(new Rect(centerX, centerY - buttonSize, buttonSize, buttonSize), "▲"))
        {
            // consoleWindowPosition.y = Mathf.Max(5, consoleWindowPosition.y - 10); // Move the console window upwards
            consoleWindowPosition.y = 5; // Snap to screen top
        }

        // Bottom Button (Move Down)
        if (GUI.Button(new Rect(centerX, centerY + buttonSize, buttonSize, buttonSize), "▼"))
        {
            // consoleWindowPosition.y = Mathf.Min(Screen.height - consoleWindowSize.y - 5, consoleWindowPosition.y + 10); // Move the console window downwards
            consoleWindowPosition.y = Screen.height - consoleWindowSize.y - 5; // Snap to screen bottom
        }

        // Left Button (Move Left)
        if (GUI.Button(new Rect(centerX - buttonSize, centerY, buttonSize, buttonSize), "◀"))
        {
            // consoleWindowPosition.x = Mathf.Max(5, consoleWindowPosition.x - 10); // Move the console window to the left
            consoleWindowPosition.x = 5; // Snap to screen left
        }

        // Right Button (Move Right)
        if (GUI.Button(new Rect(centerX + buttonSize, centerY, buttonSize, buttonSize), "▶"))
        {
            // consoleWindowPosition.x = Mathf.Min(Screen.width - consoleWindowSize.x - 5, consoleWindowPosition.x + 10); // Move the console window to the right
            consoleWindowPosition.x = Screen.width - consoleWindowSize.x - 5; // Snap to screen right
        }

        // Define button positions for scaling the window
        float scaleButtonCenterX = consoleWindowPosition.x + 10 + buttonSize / 2; // X-center for scale buttons
        float scaleButtonCenterY = consoleWindowPosition.y + 5 + buttonSize; // Y-center for scale buttons

        // Increase Height Button
        if (GUI.Button(new Rect(scaleButtonCenterX, scaleButtonCenterY - buttonSize, buttonSize, buttonSize), "-H"))
        {
            consoleWindowSize.y = Mathf.Max(100f, consoleWindowSize.y - 20f); // Decrease height, but not below a minimum value
        }

        // Decrease Height Button
        if (GUI.Button(new Rect(scaleButtonCenterX, scaleButtonCenterY + buttonSize, buttonSize, buttonSize), "+H"))
        {
            consoleWindowSize.y = Mathf.Min(Screen.height, consoleWindowSize.y + 20f); // Increase height, but not beyond screen height
        }

        // Increase Width Button
        if (GUI.Button(new Rect(scaleButtonCenterX - buttonSize, scaleButtonCenterY, buttonSize, buttonSize), "-W"))
        {
            consoleWindowSize.x = Mathf.Max(100f, consoleWindowSize.x - 20f); // Decrease width, but not below a minimum value
        }

        // Decrease Width Button
        if (GUI.Button(new Rect(scaleButtonCenterX + buttonSize, scaleButtonCenterY, buttonSize, buttonSize), "+W"))
        {
            consoleWindowSize.x = Mathf.Min(Screen.width, consoleWindowSize.x + 20f); // Increase width, but not beyond screen width
        }

        // Calculate vertical position for the message box below the buttons
        float y = consoleWindowPosition.y + 2 * buttonSize + 25;

        // Render the message box with scrollable content
        Rect messagesViewport = new Rect(0, 0, consoleWindowSize.x - 40, 20 * consoleMessages.Count); // Virtual area for messages
        messageScroll = GUI.BeginScrollView(new Rect(consoleWindowPosition.x + 10, y, consoleWindowSize.x - 20, consoleWindowSize.y - inputfildHeight - 2 * buttonSize - 40), messageScroll, messagesViewport);

        for (int i = 0; i < consoleMessages.Count; i++)
        {
            GUI.Label(new Rect(5, i * 20, messagesViewport.width - 10, 20), consoleMessages[i]); // Display each message
        }

        GUI.EndScrollView();
        y += consoleWindowSize.y - inputfildHeight - 2 * buttonSize - 60;

        // Render the input field for typing commands
        GUI.SetNextControlName("console");
        string previousInput = input;
        input = GUI.TextField(new Rect(consoleWindowPosition.x + 10, y + inputfildPositionY, consoleWindowSize.x - 20, inputfildHeight), input);
        GUI.FocusControl("console");

        if (input != previousInput)
        {
            UpdateAutocompleteSuggestions(input); // Update suggestions when the input changes
        }

        // Render autocomplete suggestions
        if (autocompleteSuggestions.Count > 0)
        {
            for (int i = 0; i < autocompleteSuggestions.Count; i++)
            {
                // Calculate the size of the text for each suggestion
                GUIContent suggestionContent = new GUIContent(autocompleteSuggestions[i]);
                Vector2 textSize = GUI.skin.label.CalcSize(suggestionContent);

                // Adjust the background rectangle to fit the text
                Rect suggestionRect = new Rect(consoleWindowPosition.x + 10, y + inputfildPositionY - inputfildHeight - ((i + 0.1f) * 20), textSize.x + 10, textSize.y);

                // Highlight the selected suggestion
                GUI.backgroundColor = (i == autocompleteIndex) ? Color.yellow : Color.black;
                GUI.Box(suggestionRect, "", GUI.skin.box);

                // Display the suggestion text
                GUI.contentColor = (i == autocompleteIndex) ? Color.yellow : Color.white;
                GUI.Label(suggestionRect, autocompleteSuggestions[i]);
            }

            // Reset GUI colors to default
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
        }
    }

    private void RenderDebugWindow()
    {
        // Draw the main debug window box
        GUI.Box(new Rect(debugWindowPosition.x, debugWindowPosition.y, debugWindowSize.x, debugWindowSize.y), "Debug Window");

        // Define the starting vertical position for the content inside the debug window
        float y = debugWindowPosition.y + 20;

        // Define the virtual area that will hold all debug messages
        Rect debugViewport = new Rect(0, 0, debugWindowSize.x - 40, 20 * debugMessages.Count);

        // Create a scrollable view for the debug messages
        debugScroll = GUI.BeginScrollView(
            new Rect(debugWindowPosition.x + 10, y, debugWindowSize.x - 20, debugWindowSize.y - 30), // Displayed area
            debugScroll, // Current scroll position
            debugViewport // Total area that can be scrolled
        );

        // Iterate over each debug message and render it
        for (int i = 0; i < debugMessages.Count; i++)
        {
            // Set the text color based on the message type
            Color color = debugMessages[i].type switch
            {
                LogType.Warning => Color.yellow, // Warnings in yellow
                LogType.Error => Color.red, // Errors in red
                _ => Color.green, // Other messages in green
            };

            // Apply the color to the GUI content
            GUI.contentColor = color;

            // Render the debug message as a label
            GUI.Label(new Rect(5, i * 20, debugViewport.width - 10, 20), debugMessages[i].message);
        }

        // Reset the GUI content color to default after rendering
        GUI.contentColor = Color.white;

        // End the scrollable view
        GUI.EndScrollView();
    }

    private void HandleInput()
    {
        string[] properties = input.Split(' ');

        foreach (var command in commandList)
        {
            CommandBase commandBase = command as CommandBase;
            if (properties[0].Equals(commandBase.CommandId, System.StringComparison.OrdinalIgnoreCase))
            {
                if (command is Command cmd && properties.Length == 1)
                {
                    cmd.Invoke();
                    return;
                }
                if (command is Command<int> intCmd && properties.Length == 2 && int.TryParse(properties[1], out int value))
                {
                    intCmd.Invoke(value);
                    return;
                }
            }
        }

        AddConsoleMessage($"Command not recognized: {properties[0]}");
    }

    private void AddConsoleMessage(string message)
    {
        // Add a new message to the console
        consoleMessages.Add(message);
        messageScroll.y = Mathf.Infinity;
    }

    /// <summary>
    /// Updates the list of autocomplete suggestions based on the current input.
    /// </summary>
    /// <param name="input">The current text entered by the user.</param>
    private void UpdateAutocompleteSuggestions(string input)
    {
        // Clear the current list of suggestions to prepare for a fresh update
        autocompleteSuggestions.Clear();

        // If the input is empty, there are no suggestions to provide
        if (string.IsNullOrEmpty(input))
            return;

        // Iterate through all registered commands to find matches
        foreach (var commandObject in commandList)
        {
            // Cast the command object to a CommandBase to access command properties
            CommandBase commandBase = commandObject as CommandBase;

            // Check if the command starts with the input string (case-insensitive match)
            if (commandBase != null && commandBase.CommandId.StartsWith(input, System.StringComparison.OrdinalIgnoreCase))
            {
                // Add the matching command's ID to the autocomplete suggestions
                autocompleteSuggestions.Add(commandBase.CommandId);
            }
        }

        // Reset the autocomplete index to the first suggestion
        autocompleteIndex = 0;
    }

    /// <summary>
    /// Applies the selected autocomplete suggestion to the input field.
    /// Cycles through the suggestions if there are multiple options.
    /// </summary>
    private void ApplyAutocomplete()
    {
        // Check if there are any autocomplete suggestions available
        if (autocompleteSuggestions.Count == 0)
            return;

        // Set the current input field text to the selected autocomplete suggestion
        input = autocompleteSuggestions[autocompleteIndex];

        // Move to the next suggestion in the list (cyclic behavior)
        autocompleteIndex = (autocompleteIndex + 1) % autocompleteSuggestions.Count;
    }
}
