using igCauldron3.Frames.TextEdit.Editor;
using igCauldron3.Frames.TextEdit.Input;
using igCauldron3.Frames.TextEdit.Syntax;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace igCauldron3.Frames.TextEdit;

/// <summary>Text editor component that provides functionality for editing text with syntax highlighting, undo/redo, selection, and more.</summary>
public class TextEditor
{
    internal TextEditorUndoStack UndoStack { get; }
    internal TextEditorColor Color { get; }
    internal TextEditorText Text { get; }

    /// <summary>Gets the selection manager, allowing for text selection and manipulation.</summary>
    public TextEditorSelection Selection { get; }

    /// <summary>Gets the options for configuring the text editor's behavior and appearance.</summary>
    public TextEditorOptions Options { get; }

    /// <summary>Gets the breakpoints manager, allowing for setting and managing breakpoints in the text.</summary>
    public TextEditorBreakpoints Breakpoints { get; }

    /// <summary>Gets the error markers manager, allowing for setting and managing error markers in the text.</summary>
    public TextEditorErrorMarkers ErrorMarkers { get; }

    /// <summary>Gets the renderer, responsible for rendering the text and its syntax highlighting.</summary>
    public TextEditorRenderer Renderer { get; }

    /// <summary>Gets the movement manager, allowing for cursor movement and navigation within the text.</summary>
    public TextEditorMovement Movement { get; }

    /// <summary>Initializes a new instance of the <see cref="TextEditor"/> class with default options and configurations.</summary>
    public TextEditor()
    {
        Options = new();
        Text = new(Options);
        Selection = new(Text);
        Breakpoints = new(Text);
        ErrorMarkers = new(Text);
        Color = new(Options, Text);
        Movement = new(Selection, Text);
        UndoStack = new(Text, Options);
        Renderer = new(this, Palettes.Dark)
        {
            KeyboardInput = new StandardKeyboardInput(this),
            MouseInput = new StandardMouseInput(this),
        };
    }

    /// <summary>Gets the total number of lines in the text editor, excluding the last empty line.</summary>
    public int TotalLines => Text.LineCount;

    /// <summary>Gets or sets the complete text content of the editor, including all lines.</summary>
    public string AllText
    {
        get => Text.GetText((0, 0), (Text.LineCount, 0));
        set => Text.SetText(value);
    }

    /// <summary>Gets or sets the lines of text in the editor.</summary>
    public IList<string> TextLines
    {
        get => Text.TextLines;
        set => Text.TextLines = value;
    }

    public bool searching = false;
    private int savedPos = 0;
    private string searchQuery = "";
    private bool hasAResult = false;
    private int skipIndex = 0;
    private bool lastSearchWasForward = true;
    public class OtherSelection
    {
        public Coordinates Start;
        public Coordinates End;
        public bool searching = false;
    }
    public void SearchQuery()
    {
        TextEditor editor = this;
        ImGui.InputText(string.Empty, ref searchQuery, 0x100, ImGuiInputTextFlags.EnterReturnsTrue);
        ImGui.SameLine();
        if (ImGui.Button("Cancel"))
        {
            OtherSelection cancelSel = new OtherSelection();
            cancelSel.searching = false;
            Renderer._query = cancelSel;
            searching = false;
            savedPos = 0;
            searchQuery = "";
            hasAResult = false;
            return;
        }
        bool f3pressed = ImGui.IsKeyPressed(ImGuiKey.F3);
        if (searchQuery != string.Empty && ImGui.IsKeyDown(ImGuiKey.LeftShift) && ImGui.IsKeyPressed(ImGuiKey.F3))
        {
            if (lastSearchWasForward)
            {
                skipIndex -= searchQuery.Length;
            }
            lastSearchWasForward = false;
            if (savedPos < 0) savedPos = editor.Text._lines.Count - 1;
            for (int j = savedPos; j >= 0; j--)
            {
                int scrollTo = -1;
                string curLine = editor.Text.GetLineText(j);
                if (skipIndex > curLine.Length) skipIndex = curLine.Length;
                string curLineSkipped = curLine.Substring(0, skipIndex);
                if (curLineSkipped.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    Coordinates qStart = new Coordinates();
                    OtherSelection querySel = new OtherSelection();
                    qStart.Line = j;
                    qStart.Column = curLine.LastIndexOf(searchQuery, skipIndex, StringComparison.OrdinalIgnoreCase);
                    skipIndex = qStart.Column;
                    querySel.Start = qStart;
                    Coordinates qEnd = new Coordinates();
                    qEnd.Line = j;
                    qEnd.Column = qStart.Column + searchQuery.Length;
                    querySel.End = qEnd;
                    hasAResult = true;
                    savedPos = j;
                    scrollTo = j;
                    editor.ScrollToLine(scrollTo);
                    Renderer._query = querySel;
                    break;
                }
                else
                {
                    skipIndex = int.MaxValue;
                    savedPos = j - 1;
                }
                if (hasAResult && savedPos < 0)
                {
                    skipIndex = int.MaxValue;
                    savedPos = editor.Text._lines.Count - 1;
                    j = savedPos + 1;
                }
            }
        }
        else if (searchQuery != string.Empty && ImGui.IsKeyPressed(ImGuiKey.F3))
        {
            if (!lastSearchWasForward)
            {
                skipIndex += searchQuery.Length;
            }
            lastSearchWasForward = true;
            if (savedPos == editor.Text._lines.Count && hasAResult) savedPos = 0;
            for (int j = savedPos; j < editor.Text._lines.Count; j++)
            {
                int scrollTo = -1;
                string curLine = editor.Text.GetLineText(j);
                if (skipIndex > curLine.Length) skipIndex = 0;
                string curLineSkipped = curLine.Substring(skipIndex);
                if (curLineSkipped.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    Coordinates qStart = new Coordinates();
                    OtherSelection querySel = new OtherSelection();
                    qStart.Line = j;
                    qStart.Column = curLine.IndexOf(searchQuery, skipIndex, StringComparison.OrdinalIgnoreCase);
                    skipIndex = qStart.Column + searchQuery.Length;
                    querySel.Start = qStart;
                    Coordinates qEnd = new Coordinates();
                    qEnd.Line = j;
                    qEnd.Column = qStart.Column + searchQuery.Length;
                    querySel.End = qEnd;
                    string testString = curLine.Substring(qStart.Column, searchQuery.Length);
                    hasAResult = true;
                    savedPos = j;
                    scrollTo = j;
                    editor.ScrollToLine(scrollTo);
                    Renderer._query = querySel;
                    break;
                }
                else
                {
                    skipIndex = 0;
                    savedPos = j + 1;
                }
                if (hasAResult && savedPos >= editor.Text._lines.Count)
                {
                    skipIndex = 0;
                    savedPos = 0;
                    j = savedPos - 1;
                }
            }
        }
    }

    /// <summary>Appends a line of text to the end of the editor.</summary>
    public void AppendLine(string text)
    {
        UndoStack.Clear();
        Text.InsertLine(Text.LineCount - 1, text);
    }

    /// <summary>Appends a line of text with a specific color to the end of the editor.</summary>
    public void AppendLine(string text, PaletteIndex color)
    {
        UndoStack.Clear();
        Text.InsertLine(Text.LineCount - 1, text, color);
    }

    /// <summary>Appends a span of text to the end of the editor.</summary>
    public void Append(ReadOnlySpan<char> text, PaletteIndex color) => Text.Append(text, color);

    /// <summary>Appends a line of text represented by a <see cref="Line"/> object to the end of the editor.</summary>
    public void AppendLine(Line line)
    {
        UndoStack.Clear();
        Text.InsertLine(Text.LineCount - 1, line);
    }

    /// <summary>Gets or sets the syntax highlighter used for syntax highlighting in the text editor.</summary>
    public ISyntaxHighlighter SyntaxHighlighter
    {
        get => Color.SyntaxHighlighter;
        set => Color.SyntaxHighlighter = value;
    }

    /// <summary>Sets the color for a specific palette index in the text editor.</summary>
    public void SetColor(PaletteIndex color, uint abgr) => Renderer.SetColor(color, abgr);

    /// <summary>Gets the text of the current line where the cursor is located.</summary>
    public string GetCurrentLineText()
    {
        var lineLength = Text.GetLineMaxColumn(Selection.Cursor.Line);
        return Text.GetText((Selection.Cursor.Line, 0), (Selection.Cursor.Line, lineLength));
    }

    /// <summary>Gets or sets the current cursor position in the text editor.</summary>
    public Coordinates CursorPosition
    {
        get => Selection.GetActualCursorCoordinates();
        set
        {
            if (Selection.Cursor == value)
                return;

            Selection.Cursor = value;
            Selection.Select(value, value);
            ScrollToLine(value.Line);
        }
    }

    /// <summary>Renders the text editor with the specified title and size. Returns true if the text has changed.</summary>
    public bool Render(string title, Vector2 size = new())
    {
        if (searching) SearchQuery();
        long initialVersion = Text.Version;
        Renderer.Render(title, size);
        return initialVersion != Text.Version;
    }

    /// <summary>The version of the text content, for detecting changes.</summary>
    public long Version => Text.Version;

    /// <summary>Undoes the last action in the text editor, allowing for reverting changes made to the text.</summary>
    public void Undo() => UndoStack.Undo(this);

    /// <summary>Redoes the last undone action in the text editor, allowing for reapplying changes that were previously undone.</summary>
    public void Redo() => UndoStack.Redo(this);

    /// <summary>Gets the number of actions that can be undone in the text editor.</summary>
    public int UndoCount => UndoStack.UndoCount;

    /// <summary>Gets the index of the current undo action in the undo stack.</summary>
    public int UndoIndex => UndoStack.UndoIndex;

    /// <summary>Serializes the current state of the text editor, including options, selection, breakpoints, error markers, and text lines, to a JSON string.</summary>
    public string SerializeState()
    {
        var state = new
        {
            Options,
            Selection = Selection.SerializeState(),
            Breakpoints = Breakpoints.SerializeState(),
            ErrorMarkers = ErrorMarkers.SerializeState(),
            Text = TextLines,
        };

        return JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>Scrolls the text editor to a specific line number, making it visible in the viewport.</summary>
    public void ScrollToLine(int lineNumber) => Text.PendingScrollRequest = lineNumber;
}
